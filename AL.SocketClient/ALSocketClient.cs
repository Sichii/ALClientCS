using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Chaos.Core.Extensions;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;

namespace AL.SocketClient
{
    /// <summary>
    ///     Provides a basic implementation for interacting with the Adventure Land socket server.
    /// </summary>
    /// <seealso cref="FormattedLogger" />
    /// <seealso cref="IAsyncDisposable" />
    public class ALSocketClient : IALSocketClient
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
        public event EventHandler<string>? Disconnected;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ALSocketClient" /> class.
        /// </summary>
        /// <param name="logger">The prefixed logged to log messages to.</param>
        public ALSocketClient(IFormattedLogger logger)
        {
            Logger = logger;
            Subscriptions = new ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList>();
            CompiledExpressions = new ConcurrentDictionary<Type, Func<SocketIOResponse, int, object>>();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Socket is already open.</exception>
        public async Task ConnectAsync(Server server)
        {
            if (Connected)
                throw new InvalidOperationException("Socket is already open.");

            if (Disposed)
                throw new ObjectDisposedException(nameof(ALSocketClient));

            var host = $"ws://{server.IPAddress}:{server.Port}";

            Logger.Info($"Connecting to {host}");
            Socket = new SocketIO(host);
            Socket.JsonSerializer = new NewtonsoftJsonSerializer(Socket.Options.EIO);
            Socket.OnDisconnected += OnDisconnected;
            Socket.OnAny(OnAny);

            await Socket.ConnectAsync().ConfigureAwait(false);
            Connected = true;
        }

        private static Func<SocketIOResponse, int, object> CreateLambda(Type type)
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

        public async Task DisconnectAsync()
        {
            if (!Connected)
                return;

            Logger.Warn("Disconnecting");
            Connected = false;

            foreach ((_, var subList) in Subscriptions)
                foreach (var sub in await subList.ToArrayAsync().ConfigureAwait(false))
                    await sub.DisposeAsync().ConfigureAwait(false);

            Subscriptions.Clear();
            await Socket.DisconnectAsync().ConfigureAwait(false);

            try
            {
                Socket.Dispose();
                Disposed = true;
            } catch
            {
                //ignored
            }
        }

        public async ValueTask DisposeAsync() => await DisconnectAsync().ConfigureAwait(false);

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Socket is null or closed.</exception>
        public async Task EmitAsync<T>(ALSocketEmitType emitType, T data)
        {
            Logger.Trace($"{emitType}, {data}");

            if ((Socket == null) || !Connected)
                throw new InvalidOperationException("Socket is null or closed.");

            await Socket.EmitAsync(EnumHelper.ToString(emitType).ToLowerInvariant(), data).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Socket is null or closed.</exception>
        public Task EmitAsync(ALSocketEmitType emitType)
        {
            Logger.Trace($"{emitType}");

            if ((Socket == null) || !Connected)
                throw new InvalidOperationException("Socket is null or closed.");

            return Socket.EmitAsync(EnumHelper.ToString(emitType).ToLowerInvariant());
        }

        //private async void EventHandler(object? sender, SocketIO e) => await HandleEventAsync(e.Value).ConfigureAwait(false);

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        ///     Failed to deserialize top level message. See inner exception. <br />
        ///     RAW JSON: <br />
        ///     {rawJson}
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Uncaught exception in handler. See inner exception. <br />
        ///     RAW JSON: <br />
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
                var wrapper = new InvalidOperationException($@"Failed to deserialize top level message. See inner exception.
RAW JSON:
{rawJson}", ex);

                Logger.Fatal(wrapper);

                throw wrapper;
            }

            try
            {
                if (Subscriptions.TryGetValue(message.MessageType, out var subscriptionList))
                    await InvokeAsync(subscriptionList, rawJson, message.Data.CreateReader()).ConfigureAwait(false);
            } catch (Exception ex)
            {
                var wrapper = new Exception($@"Uncaught exception in handler. See inner exception.
RAW JSON:
{rawJson}", ex);

                Logger.Fatal(wrapper);

                throw wrapper;
            }
        }

        private ValueTask InvokeAsync(ALSocketSubscriptionList invocationList, string raw, JsonReader reader)
        {
            async ValueTask InnerInvokeAsync(List<ALSocketSubscription> subscriptions)
            {
                Logger.Trace(raw);

                var dataObject = JsonSerializer.Deserialize(reader, invocationList.Type);

                if (dataObject == null)
                {
                    Logger.Error($"Failed to deserialize message. {Environment.NewLine}{raw}");

                    return;
                }

                foreach (var subscription in subscriptions)
                {
                    var handled = await subscription.InvokeAsync(dataObject).ConfigureAwait(false);

                    if (handled)
                        return;
                }
            }

            return invocationList.AssertAsync(InnerInvokeAsync);
        }

        public IAsyncDisposable On<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
        {
            if (!Subscriptions.TryGetValue(socketMessageType, out var invocationList))
            {
                invocationList = new ALSocketSubscriptionList(typeof(T));
                Subscriptions[socketMessageType] = invocationList;
            }

            return AlSocketSubscription<T>.Create(invocationList, callback);
        }

        private void OnAny(string eventName, SocketIOResponse response)
        {
            if (!EnumHelper.TryParse(eventName, out ALSocketMessageType messageType))
                return;

            if (!Subscriptions.TryGetValue(messageType, out var subscriptionList))
                return;

            var type = subscriptionList.Type;
            var getValue = CompiledExpressions.GetOrAdd(type, CreateLambda);
            var dataObject = getValue(response, 0);

            _ = subscriptionList.InvokeAsync(dataObject);
        }

        private void OnDisconnected(object? sender, string e)
        {
            try
            {
                if (Connected)
                    Disconnected?.Invoke(sender, e);
            } catch
            {
                //ignored
            }

            DisconnectAsync().GetAwaiter().GetResult();
        }

        public async ValueTask Unsub<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
        {
            if (Subscriptions.TryGetValue(socketMessageType, out var invocationList))
                await invocationList.RemoveAllAsync(subscription => subscription.Callback == (Delegate)callback).ConfigureAwait(false);
        }

        #region Do Not ReOrder
        /// <summary>
        ///     A default <see cref="JsonSerializerSettings" /> instance, used for serializing Emits and deserializing
        ///     messages.
        ///     <br />
        ///     Caching an instance of this helps with performance. <br />
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
}