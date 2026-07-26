#region
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Json.SystemTextJson;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
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
    private readonly ConcurrentDictionary<Type, Func<SocketIOResponse, int, object>> CompiledExpressions;
    private readonly IFormattedLogger Logger;
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
        CompiledExpressions = new ConcurrentDictionary<Type, Func<SocketIOResponse, int, object>>();
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
                        $"Rate-limited: {report.TotalCalls} total calls, exceeded a call-cost limit of {report.CallLimit} in 4s. {report.Calls}");
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
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Socket is null or closed.
    /// </exception>
    public Task EmitAsync(ALSocketEmitType emitType)
    {
        Logger.Trace($"{emitType}");

        if ((Socket == null) || !Connected)
            throw new InvalidOperationException("Socket is null or closed.");

        return Socket.EmitAsync(
            EnumHelper.ToString(emitType)
                      .ToLowerInvariant());
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

    public void Unsub<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
    {
        if (Subscriptions.TryGetValue(socketMessageType, out var invocationList))
            foreach (var subscription in invocationList)
                if (subscription.Callback == (Delegate)callback)
                    invocationList.Remove(subscription);
    }

    internal static Func<SocketIOResponse, int, object> CreateLambda(Type type)
    {
        //compile an expression for a given type, that called response.GetValue<T> where T is the type object
        var responseParam = Expression.Parameter(typeof(SocketIOResponse), "response");
        var callParam = Expression.Parameter(typeof(int));

        var method = typeof(SocketIOResponse).GetMethods()
                                             .Where(mInfo => mInfo.Name.EqualsI(nameof(SocketIOResponse.GetValue)))
                                             .FirstOrDefault(mInfo => mInfo.IsGenericMethod)!.MakeGenericMethod(type);

        var call = Expression.Call(responseParam, method, callParam);

        var lambda = Expression.Lambda<Func<SocketIOResponse, int, object>>(call, responseParam, callParam);

        return lambda.Compile();
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

            var type = subscriptionList.Type;
            var getValue = CompiledExpressions.GetOrAdd(type, CreateLambda);
            var dataObject = getValue(response, 0);
            Logger.Trace($"{messageType}, {response}");

            Task.Run(async () =>
            {
                try
                {
                    await InvokeAsync(messageType, subscriptionList, dataObject)
                        .ConfigureAwait(false);
                } catch (Exception e)
                {
                    Logger.Error($"Handler for \"{eventName}\" threw. {e}");
                }
            });
        } catch (Exception e)
        {
            //a frame dropped here is otherwise indistinguishable from a frame never sent, so
            //carry the event name and the raw payload - this is how the next drift gets found
            Logger.Error($"Dropped \"{eventName}\" frame: {response}. {e}");
        }
    }
}