#region
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using AL.APIClient.Model;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Json.SystemTextJson;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
using SocketIO.Core;
using SocketIO.Serializer.SystemTextJson;
using SocketIOClient;
using SocketIOClient.Transport;
#endregion

namespace AL.SocketClient;

/// <summary>
///     Provides a basic implementation for interacting with the Adventure Land socket server.
/// </summary>
/// <seealso cref="FormattedLogger" />
/// <seealso cref="IAsyncDisposable" />
public sealed class ALSocketClient : IALSocketClient
{
    private readonly IFormattedLogger Logger;
    private readonly ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList> Subscriptions;
    private bool Disposed;
    private SocketIOClient.SocketIO Socket = null!;

    /// <summary>Frames waiting to be handled, in the order the transport read them off the wire.</summary>
    private readonly Channel<QueuedFrame> Frames;

    /// <summary>The single consumer of <see cref="Frames" />. One per client, which is what makes the order one.</summary>
    private readonly Task Pump;

    /// <summary>
    ///     How long a frame may sit behind the one in front before that is worth a line.
    /// </summary>
    /// <remarks>
    ///     There is no budget being enforced here and nothing is dropped when it is exceeded. It exists because the
    ///     cost of ordering is exactly this wait, and a number nobody can see is a number nobody can argue about -
    ///     the last attempt at ordering was reverted on an impression rather than a reading.
    /// </remarks>
    private static readonly TimeSpan QUEUE_LAG_WARN = TimeSpan.FromMilliseconds(50);

    /// <summary>One frame, decoded and waiting its turn.</summary>
    /// <remarks>
    ///     Decoding happens on the transport's own callback, before the frame is queued, so the queue holds work that
    ///     is already done rather than json waiting to be parsed. What waits here is only the handler call.
    /// </remarks>
    private readonly record struct QueuedFrame(
        ALSocketMessageType MessageType,
        ALSocketSubscriptionList Subscriptions,
        object Data,
        string EventName,
        long EnqueuedAt);

    /// <summary>
    ///     Whether or not this socket is currently connected.
    /// </summary>
    public bool Connected { get; private set; }

    /// <inheritdoc />
    public string? LastDisconnectReason { get; private set; }

    /// <summary>
    ///     Whether to connect over TLS. True for the public game host, which sets its auth cookie with the "secure" flag. Set
    ///     to false to reach a locally hosted server.
    /// </summary>
    public static bool UseSecureTransport { get; set; } = true;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ALSocketClient" /> class.
    /// </summary>
    /// <param name="logger">
    ///     The prefixed logged to log messages to.
    /// </param>
    public ALSocketClient(IFormattedLogger logger)
    {
        Logger = logger;
        Subscriptions = new ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList>();

        //a client is single use - DisconnectAsync marks it disposed and ConnectAsync refuses a disposed one - so the
        //queue and its consumer live as long as the instance and there is no per-connection lifetime to get wrong
        Frames = Channel.CreateUnbounded<QueuedFrame>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,

                //the writer is the transport's receive callback; letting a continuation run inline on it would put
                //a handler back on the thread the ordering is meant to keep clear
                AllowSynchronousContinuations = false
            });

        Pump = PumpAsync();
    }

    /// <summary>
    ///     Hands each frame to its subscribers, one at a time, in the order the transport read them.
    /// </summary>
    /// <remarks>
    ///     <b>No subscriber may await a server response.</b> Its answer arrives as a frame, and that frame queues
    ///     behind the subscriber waiting for it, so the wait never ends. Nothing does this today - the only
    ///     <c>async</c> subscriber in the client awaits nothing at all - and a new one that did would not fail
    ///     visibly, it would stop the socket. Register a callback that records what it saw and returns.
    ///     <br />
    ///     Hitchhiked events are not affected: they arrive inside a frame already being handled and dispatch through
    ///     <see cref="HandleEventAsync" /> inline, which is the order they belong in anyway.
    /// </remarks>
    private async Task PumpAsync()
    {
        var previousEvent = "nothing";

        await foreach (var frame in Frames.Reader.ReadAllAsync()
                                          .ConfigureAwait(false))
        {
            var waited = Stopwatch.GetElapsedTime(frame.EnqueuedAt);

            //names the frame ahead of this one, because that is the one whose handler held the line. The pair of
            //warnings is what identifies the offender: this line says the queue backed up, the one below says who
            if (waited > QUEUE_LAG_WARN)
                Logger.Warn(
                    $"Frame \"{frame.EventName}\" waited {waited.TotalMilliseconds:N0}ms to be handled, behind \"{previousEvent}\".");

            var startedAt = Stopwatch.GetTimestamp();

            try
            {
                await InvokeAsync(frame.MessageType, frame.Subscriptions, frame.Data)
                    .ConfigureAwait(false);
            } catch (Exception e)
            {
                //one frame's handler must not end the pump, which would leave the socket connected and deaf
                Logger.Error($"Handler for \"{frame.EventName}\" threw. {e}");
            }

            var handling = Stopwatch.GetElapsedTime(startedAt);

            //a slow handler with nothing queued behind it delays the next frame just as much and warns nowhere else
            if (handling > QUEUE_LAG_WARN)
                Logger.Warn($"Handler for \"{frame.EventName}\" took {handling.TotalMilliseconds:N0}ms.");

            previousEvent = frame.EventName;
        }
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Socket is already open.
    /// </exception>
    public async Task ConnectAsync(Server server)
    {
        if (Connected)
            throw new InvalidOperationException("Socket is already open.");

        if (Disposed)
            throw new ObjectDisposedException(nameof(ALSocketClient));

        var host = $"{(UseSecureTransport ? "wss" : "ws")}://{server.Address}";

        var options = new SocketIOOptions
        {
            Transport = TransportProtocol.WebSocket,

            //ALClient.ReconnectAsync is the only thing allowed to reconnect; a second
            //authority races it and leaves an unauthenticated observer session behind
            Reconnection = false
        };

        //the engine.io mount path is per-server config, not the socket.io default. the server
        //publishes it with a trailing slash but the library appends "/?EIO=..." to whatever it
        //is given, so normalize to the same form as the library's own default
        if (!string.IsNullOrEmpty(server.Path))
            options.Path = server.Path.TrimEnd('/');

        Logger.Info($"Connecting to {host}{options.Path}");
        Socket = new SocketIOClient.SocketIO(host, options);
        Socket.Serializer = new SystemTextJsonSerializer(SocketJson.Options);
        Socket.OnDisconnected += DisconnectedEvent;
        Socket.OnAny(OnAny);

        //the server emits disconnect_reason (and, on a rate-limit kick, limitdcreport) immediately before
        //it drops the connection. capture them on the underlying socket's own dispatch, which runs inline on
        //the receive loop - routing through OnAny's Task.Run would race the transport disconnect and let
        //ReconnectAsync read a null reason. both bodies deserialize server input, so guard them: an unhandled
        //throw on the receive thread must not take down the loop, and losing the value degrades gracefully.
        Socket.On(
            "disconnect_reason",
            response =>
            {
                try
                {
                    LastDisconnectReason = response.GetValue<string>();
                } catch (Exception e)
                {
                    Logger.Error($"Failed to read disconnect_reason. {e}");
                }
            });

        Socket.On(
            "limitdcreport",
            response =>
            {
                try
                {
                    var report = response.GetValue<LimitDcReportData>();

                    Logger.Warn(
                        $"Rate-limited: {report.TotalCalls} total calls, exceeded a call-cost limit of {report.CallLimit} in 4s. {report.Calls?.ToJsonString()}");

                    OnLimitDcReport?.Invoke(this, report);
                } catch (Exception e)
                {
                    Logger.Error($"Failed to read limitdcreport. {e}");
                }
            });

        //the server emits welcome synchronously from its connection handler, so it can be
        //processed before ConnectAsync returns - the emit guard has to already be open
        Connected = true;

        try
        {
            await Socket.ConnectAsync()
                        .ConfigureAwait(false);
        } catch
        {
            Connected = false;

            throw;
        }
    }

    public async Task DisconnectAsync(bool intentional = true)
    {
        try
        {
            //ahead of the connected check, so a client that never connected still ends its pump rather than leaving
            //it parked on a queue nothing will ever write to. The pump drains what is already queued and then ends;
            //frames that drain after the subscriptions below are disposed find an empty list and do nothing, which
            //is the same outcome they had before the queue existed
            Frames.Writer.TryComplete();

            if (!Connected)
                return;

            if (intentional)
                Logger.Info("(Intentionally) Disconnecting...");

            Connected = false;

            foreach ((_, var subList) in Subscriptions)
                foreach (var sub in subList)
                    sub.Dispose();

            Subscriptions.Clear();

            await Socket.DisconnectAsync()
                        .ConfigureAwait(false);

            try
            {
                Socket.Dispose();
                Disposed = true;
            } catch
            {
                //ignored
            }
        } catch
        {
            //ignored
        }
    }

    public async ValueTask DisposeAsync()
        => await DisconnectAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Socket is null or closed.
    /// </exception>
    public async Task EmitAsync<T>(ALSocketEmitType emitType, T data)
    {
        Logger.Trace($"{emitType}, {data}");

        if ((Socket == null) || !Connected)
            throw new InvalidOperationException("Socket is null or closed.");

        await Socket.EmitAsync(
                        EnumHelper.ToString(emitType)
                                  .ToLowerInvariant(),
                        data)
                    .ConfigureAwait(false);

        //after the await, so a throw on the way to the wire is not billed to anyone - the server never saw it
        OnEmit?.Invoke(this, emitType);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Socket is null or closed.
    /// </exception>
    public async Task EmitAsync(ALSocketEmitType emitType)
    {
        Logger.Trace($"{emitType}");

        if ((Socket == null) || !Connected)
            throw new InvalidOperationException("Socket is null or closed.");

        await Socket.EmitAsync(
                        EnumHelper.ToString(emitType)
                                  .ToLowerInvariant())
                    .ConfigureAwait(false);

        OnEmit?.Invoke(this, emitType);
    }

    //private async void EventHandler(object? sender, SocketIO e) => await HandleEventAsync(e.Value);

    /// <inheritdoc />
    public async ValueTask HandleEventAsync(string rawJson)
    {
        ALSocketMessage message;

        try
        {
            message = JsonSerializer.Deserialize<ALSocketMessage>(rawJson, SocketJson.Options)!;
        } catch (Exception ex)
        {
            //this runs per hitchhiker inside the player chain, so throwing here would skip the
            //remaining hitchhikers and every later player subscriber
            Logger.Error(
                $@"Failed to deserialize top level message.
RAW JSON:
{rawJson}
{ex}");

            return;
        }

        try
        {
            if (Subscriptions.TryGetValue(message.MessageType, out var subscriptionList))
                await InvokeAsync(
                        message.MessageType,
                        subscriptionList,
                        rawJson,
                        message.Data)
                    .ConfigureAwait(false);
        } catch (Exception ex)
        {
            Logger.Error(
                $@"Uncaught exception in handler.
RAW JSON:
{rawJson}
{ex}");
        }
    }

    public IDisposable On<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
    {
        //check-then-set would let racing first-registrations orphan a list, and its subscribers never fire
        var invocationList = Subscriptions.GetOrAdd(socketMessageType, _ => new ALSocketSubscriptionList(typeof(T)));

        return new AlSocketSubscription<T>(invocationList, callback);
    }

    /// <summary>
    ///     An event fired when the socket disconnects unintentionally.
    /// </summary>
    public event EventHandler<string>? OnDisconnected;

    /// <summary>
    ///     Raised after each emit reaches the wire, carrying what was sent. Every one of those is billed against the
    ///     server's <see cref="CallCost.LIMIT" />, so this is the hook for metering who is spending the budget.
    /// </summary>
    public event EventHandler<ALSocketEmitType>? OnEmit;

    /// <summary>
    ///     Raised when the server sends a rate-limit kick report immediately before disconnecting.
    /// </summary>
    public event EventHandler<LimitDcReportData>? OnLimitDcReport;

    public void Unsub<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
    {
        if (Subscriptions.TryGetValue(socketMessageType, out var invocationList))
            foreach (var subscription in invocationList)
                if (subscription.Callback == (Delegate)callback)
                    invocationList.Remove(subscription);
    }

    private void DisconnectedEvent(object? sender, string e)
    {
        try
        {
            if (Connected)
            {
                Logger.Error("(Unintentionally) Disconnecting...");
                OnDisconnected -= DisconnectedEvent;
                OnDisconnected?.Invoke(sender, e);
            }

            DisconnectAsync(false)
                .GetAwaiter()
                .GetResult();
        } catch
        {
            //ignored
        }
    }

    private ValueTask InvokeAsync(
        ALSocketMessageType messageType,
        ALSocketSubscriptionList invocationList,
        string raw,
        JsonNode data)
    {
        Logger.Trace(raw);

        var dataObject = data.Deserialize(invocationList.Type, SocketJson.Options);

        if (dataObject == null)
        {
            Logger.Error($"Failed to deserialize message. {Environment.NewLine}{raw}");

            return default;
        }

        return InvokeAsync(messageType, invocationList, dataObject);
    }

    private async ValueTask InvokeAsync(ALSocketMessageType messageType, ALSocketSubscriptionList invocationList, object dataObject)
    {
        foreach (var subscription in invocationList)
        {
            bool handled;

            try
            {
                handled = await subscription.InvokeAsync(dataObject)
                                            .ConfigureAwait(false);
            } catch (Exception e)
            {
                //one frame can carry several in-flight awaits; a thrower must not starve the rest.
                //a type mismatch here means a later On<T> disagreed with the list's type
                Logger.Error(
                    $"Subscriber for \"{messageType}\" declared as {subscription.SubscriptionType} threw, list type is {invocationList.Type}. {e}");

                continue;
            }

            if (handled)
                return;
        }
    }

    private void OnAny(string eventName, SocketIOResponse response)
    {
        try
        {
            if (!EnumHelper.TryParse(eventName, out ALSocketMessageType messageType))
                return;

            if (!Subscriptions.TryGetValue(messageType, out var subscriptionList))
                return;

            //bound against the transport's own parse and the shared options, rather than through
            //response.GetValue<T>() or response.ToString(). GetValue routes through a JsonSerializerOptions the
            //transport builds fresh per frame to carry that packet's binary attachments, and a new instance starts
            //with an empty type cache, so every frame re-reflects every type it touches. ToString() is no cheaper:
            //it serializes the array the transport already parsed back into a string, which then has to be parsed
            //into a second dom before anything can bind. Reading the message directly measured as 0.21ms and 99KB
            //per character frame against 0.49ms and 119KB. Binary attachments are the thing given up, and no frame
            //this game sends carries one
            if (MessageOf(response) is not JsonMessage { JsonArray: { Count: > 0 } payloads } message)
            {
                Logger.Error($"Dropped \"{eventName}\" frame: the payload is not a populated array. {response}");

                return;
            }

            Logger.Trace(message.ReceivedText);

            var dataObject = payloads[0]
                ?.Deserialize(subscriptionList.Type, SocketJson.Options);

            if (dataObject == null)
            {
                Logger.Error($"Dropped \"{eventName}\" frame: the payload deserialized to null. {message.ReceivedText}");

                return;
            }

            if (!TryEnqueue(messageType, dataObject, eventName))
                Logger.Error($"Dropped \"{eventName}\" frame: the queue is closed. {message.ReceivedText}");
        } catch (Exception e)
        {
            //a frame dropped here is otherwise indistinguishable from a frame never sent, so
            //carry the event name and the raw payload - this is how the next drift gets found
            Logger.Error($"Dropped \"{eventName}\" frame: {response}. {e}");
        }
    }

    /// <summary>Reads the parsed message a <see cref="SocketIOResponse" /> was built around.</summary>
    /// <remarks>
    ///     The response exposes no public route to it that does not re-serialize. This pins a private field name,
    ///     so <c>ResponseCarriesAReadableMessage</c> asserts the accessor still resolves - a library bump that
    ///     renames the field fails that test rather than every frame at runtime.
    /// </remarks>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_message")]
    internal static extern ref IMessage MessageOf(SocketIOResponse response);

    /// <summary>
    ///     Queues a decoded frame for the pump to hand to its subscribers.
    /// </summary>
    /// <remarks>
    ///     Frames are queued rather than handled on the socket callback so that handling happens in arrival order
    ///     rather than in whatever order the thread pool gets to it. Two frames from one server burst race
    ///     otherwise, and the loser can be the older one: a character frame carrying no town channel, applied after
    ///     the one that opened it, reads as a recall somebody cancelled. The same shape sits under every "did this
    ///     field go away" test in the client, and under ShallowMerge writing a stale snapshot over a fresh one.
    ///     <br />
    ///     An ordered chain was tried once before and reverted. What was measured then was pathfinding that was
    ///     broken for unrelated reasons, so the reading did not say what it appeared to; the cost that is real is
    ///     the wait the pump warns about.
    /// </remarks>
    /// <returns>
    ///     <c>false</c> when nothing subscribes to <paramref name="messageType" />, or the queue has closed
    /// </returns>
    internal bool TryEnqueue(ALSocketMessageType messageType, object data, string eventName)
    {
        if (!Subscriptions.TryGetValue(messageType, out var subscriptionList))
            return false;

        return Frames.Writer.TryWrite(
            new QueuedFrame(
                messageType,
                subscriptionList,
                data,
                eventName,
                Stopwatch.GetTimestamp()));
    }
}