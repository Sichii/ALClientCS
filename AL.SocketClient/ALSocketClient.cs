#region
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
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
using SocketIO.Core;
using SocketIO.Serializer.Core;
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
    /// <summary>
    ///     The query string every socket connects with. The server reads it once, off the handshake, and a character whose
    ///     socket opened without it stays refused for the life of that connection.
    /// </summary>
    /// <remarks>
    ///     <c>map_protocol</c> is the dungeon's client-version gate and <c>no_graphics</c> asks for a headless subset of a
    ///     generated floor. Both are separate from the same-named fields on the <c>auth</c> emit, which the server reads from
    ///     the frame rather than the handshake.
    /// </remarks>
    private static readonly KeyValuePair<string, string>[] HEADLESS_HANDSHAKE_QUERY =
    [
        new("map_protocol", "1"),
        new("no_graphics", "1")
    ];

    /// <summary>
    ///     <see cref="HEADLESS_HANDSHAKE_QUERY" /> without <c>no_graphics</c>, for a socket that receives a generated floor's
    ///     art.
    /// </summary>
    private static readonly KeyValuePair<string, string>[] HANDSHAKE_QUERY = [new("map_protocol", "1")];

    /// <summary>
    ///     The time a frame may wait, or a handler may run, before it is logged. Nothing is dropped past it.
    /// </summary>
    private static readonly TimeSpan QUEUE_LAG_WARN = TimeSpan.FromMilliseconds(50);

    /// <summary>
    ///     Frames waiting to be handled, in the order the transport read them off the wire.
    /// </summary>
    private readonly Channel<QueuedFrame> Frames;

    private readonly IFormattedLogger Logger;

    /// <summary>
    ///     The single consumer of <see cref="Frames" />, held only to keep it rooted for the client's lifetime.
    /// </summary>

    // ReSharper disable once NotAccessedField.Local
    private readonly Task Pump;

    private readonly ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList> Subscriptions;
    private bool Disposed;
    private SocketIOClient.SocketIO Socket = null!;

    /// <summary>
    ///     Whether or not this socket is currently connected.
    /// </summary>
    public bool Connected { get; private set; }

    /// <inheritdoc />
    public string? LastDisconnectReason { get; private set; }

    /// <summary>
    ///     Whether a generated floor arrives with its tiles and sprite placements. True by default. Set to false to receive
    ///     only its collision lines, which is all the pathfinder reads.
    /// </summary>
    /// <remarks>
    ///     Read when a socket connects, so a change reaches the next connection and not an open one.
    /// </remarks>
    public static bool ReceiveGeneratedMapArt { get; set; } = true;

    /// <summary>
    ///     Whether to connect over TLS. True for the public game host, which sets its auth cookie with the "secure" flag. Set
    ///     to false to reach a locally hosted server.
    /// </summary>
    public static bool UseSecureTransport { get; set; } = true;

    /// <summary>
    ///     The proxy this socket dials the game through, or null for the machine's own connection.
    /// </summary>
    /// <remarks>
    ///     A dead proxy refuses the connection and the login fails; it never falls back to the machine's own connection.
    /// </remarks>
    public IWebProxy? Proxy { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ALSocketClient" /> class.
    /// </summary>
    /// <param name="logger">The prefixed logger to log messages to.</param>
    /// <param name="proxy">
    ///     The proxy to reach the game through, or null for the machine's own connection.
    /// </param>
    public ALSocketClient(IFormattedLogger logger, IWebProxy? proxy = null)
    {
        Logger = logger;
        Proxy = proxy;
        Subscriptions = new ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList>();

        //a client is single use, so the queue and its consumer live as long as the instance
        Frames = Channel.CreateUnbounded<QueuedFrame>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,

                //keeps handlers off the transport's receive callback, which is the writer
                AllowSynchronousContinuations = false
            });

        Pump = PumpAsync();
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Socket is already open.</exception>
    /// <exception cref="ObjectDisposedException">The client has already disconnected.</exception>
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
            Reconnection = false,

            //null is the machine's own connection, which is what all but a routed character uses
            Proxy = Proxy,

            //read once at the handshake. map_protocol=1 admits this client to a generated dungeon floor, and
            //no_graphics=1 trims a floor's delivery to its collision lines
            Query = ReceiveGeneratedMapArt ? HANDSHAKE_QUERY : HEADLESS_HANDSHAKE_QUERY
        };

        //the server publishes its engine.io mount path with a trailing slash, and the library appends "/?EIO=..." to it
        if (!string.IsNullOrEmpty(server.Path))
            options.Path = server.Path.TrimEnd('/');

        Logger.Info($"Connecting to {host}{options.Path}{(Proxy is null ? string.Empty : " through a proxy")}");
        Socket = new SocketIOClient.SocketIO(host, options);
        Socket.Serializer = new SynchronousSerializer(this, SocketJson.Options);
        Socket.OnDisconnected += DisconnectedEvent;

        //both arrive just before the server drops the connection, so read them on the library's own dispatch rather
        //than through the frame queue, which the disconnect would race
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

        //welcome can be handled before ConnectAsync returns, so the emit guard has to be open already
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
            //ahead of the connected check, so a client that never connected still ends its pump
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

            //a socket left open after a failed close handshake still counts against the account's login limit
            try
            {
                await Socket.DisconnectAsync()
                            .ConfigureAwait(false);
            } catch (Exception e)
            {
                Logger.Warn($"Graceful disconnect failed; closing the socket. {e.Message}");
            }

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
    /// <exception cref="InvalidOperationException">Socket is null or closed.</exception>
    public async Task EmitAsync<T>(ALSocketEmitType emitType, T data)
    {
        Logger.Trace($"{emitType}, {data}");

        //captured once, since a reconnect replaces the field
        var socket = Socket;

        if ((socket == null) || !Connected)
            throw new InvalidOperationException("Socket is null or closed.");

        try
        {
            await socket.EmitAsync(
                            EnumHelper.ToString(emitType)
                                      .ToLowerInvariant(),
                            data)
                        .ConfigureAwait(false);
        } catch (Exception e) when (e is NullReferenceException or ObjectDisposedException)
        {
            //a reconnect can tear the transport down between the guard and the call
            throw new InvalidOperationException("Socket closed while emitting.", e);
        }

        //after the await, so a throw on the way to the wire is not billed
        OnEmit?.Invoke(this, emitType);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Socket is null or closed.</exception>
    public async Task EmitAsync(ALSocketEmitType emitType)
    {
        Logger.Trace($"{emitType}");

        //captured once, since a reconnect replaces the field
        var socket = Socket;

        if ((socket == null) || !Connected)
            throw new InvalidOperationException("Socket is null or closed.");

        try
        {
            await socket.EmitAsync(
                            EnumHelper.ToString(emitType)
                                      .ToLowerInvariant())
                        .ConfigureAwait(false);
        } catch (Exception e) when (e is NullReferenceException or ObjectDisposedException)
        {
            throw new InvalidOperationException("Socket closed while emitting.", e);
        }

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
    ///     Occurs after an emit reaches the wire, where it is billed against the server's <see cref="CallCost.LIMIT" />.
    /// </summary>
    public event EventHandler<ALSocketEmitType>? OnEmit;

    /// <summary>
    ///     Occurs when the server sends a rate-limit kick report, just before it disconnects.
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
                //one thrower must not starve the rest. A type mismatch here means a later On<T> disagreed with the list
                Logger.Error(
                    $"Subscriber for \"{messageType}\" declared as {subscription.SubscriptionType} threw, list type is {invocationList.Type}. {e}");

                continue;
            }

            if (handled)
                return;
        }
    }

    private void OnMessage(JsonMessage message)
    {
        try
        {
            var eventName = message.Event;

            if (!EnumHelper.TryParse(eventName, out ALSocketMessageType messageType))
                return;

            if (!Subscriptions.TryGetValue(messageType, out var subscriptionList))
                return;

            //bound from the transport's own parse with the shared options; GetValue<T>() builds fresh options per frame
            if (message.JsonArray is not { Count: > 0 } payloads)
            {
                Logger.Error($"Dropped \"{eventName}\" frame: the payload is not a populated array. {message.ReceivedText}");

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
            //carry the raw payload, or a dropped frame is indistinguishable from one never sent
            Logger.Error($"Dropped frame: {message.ReceivedText}. {e}");
        }
    }

    /// <summary>
    ///     Hands each frame to its subscribers, one at a time, in the order the transport read them.
    /// </summary>
    /// <remarks>
    ///     <b>No subscriber may await a server response.</b> The answer queues behind the subscriber waiting for it, and the
    ///     socket stops. Hitchhiked events dispatch inline through <see cref="HandleEventAsync" /> and are not affected.
    /// </remarks>
    private async Task PumpAsync()
    {
        var previousEvent = "nothing";

        await foreach (var frame in Frames.Reader
                                          .ReadAllAsync()
                                          .ConfigureAwait(false))
        {
            var waited = Stopwatch.GetElapsedTime(frame.EnqueuedAt);

            //names the frame ahead of this one, whose handler held the line
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

    /// <summary>
    ///     Queues a decoded frame for the pump to hand to its subscribers.
    /// </summary>
    /// <remarks>
    ///     Frames are handled in arrival order; handled on the socket callback, an older frame could apply after a newer one.
    /// </remarks>
    /// <param name="messageType">The frame's message type.</param>
    /// <param name="data">The decoded payload.</param>
    /// <param name="eventName">The frame's event name, for logging.</param>
    /// <returns>
    ///     <c>true</c> if the frame was queued; otherwise, <c>false</c> when nothing subscribes to
    ///     <paramref name="messageType" /> or the queue has closed.
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

    /// <summary>
    ///     Represents one frame, decoded on the transport's receive loop and waiting for its handler call.
    /// </summary>
    private readonly record struct QueuedFrame(
        ALSocketMessageType MessageType,
        ALSocketSubscriptionList Subscriptions,
        object Data,
        string EventName,
        long EnqueuedAt);

    /// <summary>
    ///     The transport's serializer, with every event frame queued from inside its parse.
    /// </summary>
    /// <remarks>
    ///     The library dispatches each frame on a thread-pool task of its own, out of order; the parse is the one call it
    ///     makes inline on its receive loop. Binary frames are not hooked; the server sends none.
    /// </remarks>
    internal sealed class SynchronousSerializer(ALSocketClient owner, JsonSerializerOptions options)
        : SystemTextJsonSerializer(options), ISerializer
    {
        IMessage ISerializer.Deserialize(EngineIO eio, string text)
        {
            var message = base.Deserialize(eio, text);

            if (message is JsonMessage { Type: MessageType.Event } frame)
                owner.OnMessage(frame);

            return message;
        }
    }
}