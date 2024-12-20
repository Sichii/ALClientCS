﻿#region
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using Chaos.Extensions.Common;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
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
    private SocketIO Socket = null!;

    /// <summary>
    ///     Whether or not this socket is currently connected.
    /// </summary>
    public bool Connected { get; private set; }

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

        var host = $"ws://{server.IPAddress}:{server.Port}";

        Logger.Info($"Connecting to {host}");
        Socket = new SocketIO(host);
        Socket.JsonSerializer = new NewtonsoftJsonSerializer();
        Socket.OnDisconnected += DisconnectedEvent;
        Socket.OnAny(OnAny);

        await Socket.ConnectAsync()
                    .ConfigureAwait(false);
        Connected = true;
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
    /// <exception cref="InvalidOperationException">
    ///     Failed to deserialize top level message. See inner exception.
    ///     <br />
    ///     RAW JSON:
    ///     <br />
    ///     {rawJson}
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Uncaught exception in handler. See inner exception.
    ///     <br />
    ///     RAW JSON:
    ///     <br />
    ///     {rawJson}
    /// </exception>
    public async ValueTask HandleEventAsync(string rawJson)
    {
        ALSocketMessage message;

        try
        {
            message = JsonConvert.DeserializeObject<ALSocketMessage>(rawJson, ArrayToObjectConverter<ALSocketMessage>.Singleton)!;
        } catch (Exception ex)
        {
            var wrapper = new InvalidOperationException(
                $@"Failed to deserialize top level message. See inner exception.
RAW JSON:
{rawJson}",
                ex);

            throw wrapper;
        }

        try
        {
            if (Subscriptions.TryGetValue(message.MessageType, out var subscriptionList))
                await InvokeAsync(subscriptionList, rawJson, message.Data.CreateReader())
                    .ConfigureAwait(false);
        } catch (Exception ex)
        {
            var wrapper = new Exception(
                $@"Uncaught exception in handler. See inner exception.
RAW JSON:
{rawJson}",
                ex);

            throw wrapper;
        }
    }

    public IDisposable On<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
    {
        if (!Subscriptions.TryGetValue(socketMessageType, out var invocationList))
        {
            invocationList = new ALSocketSubscriptionList(typeof(T));
            Subscriptions[socketMessageType] = invocationList;
        }

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

    private async ValueTask InvokeAsync(ALSocketSubscriptionList invocationList, string raw, JsonReader reader)
    {
        Logger.Trace(raw);

        var dataObject = JsonSerializer.Deserialize(reader, invocationList.Type);

        if (dataObject == null)
        {
            Logger.Error($"Failed to deserialize message. {Environment.NewLine}{raw}");

            return;
        }

        foreach (var subscription in invocationList)
        {
            var handled = await subscription.InvokeAsync(dataObject)
                                            .ConfigureAwait(false);

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

            Task.Run(
                async () =>
                {
                    try
                    {
                        await subscriptionList.InvokeAsync(dataObject)
                                              .ConfigureAwait(false);
                    } catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                });
        } catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    #region Do Not ReOrder
    /// <summary>
    ///     A default <see cref="JsonSerializerSettings" /> instance, used for serializing Emits and deserializing messages.
    ///     <br />
    ///     Caching an instance of this helps with performance.
    ///     <br />
    ///     If you replace this instance, you must also replace the <see cref="JsonSerializer" /> instance.
    /// </summary>

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public static JsonSerializerSettings JsonSerializerSettings { get; } = new();

    /// <summary>
    ///     A default <see cref="JsonSerializer" /> instance using the default <see cref="JsonSerializerSettings" /> instance.
    ///     <br />
    ///     Caching an instance of this helps with performance.
    /// </summary>
    public static JsonSerializer JsonSerializer { get; set; } = JsonSerializer.CreateDefault(JsonSerializerSettings);
    #endregion
}