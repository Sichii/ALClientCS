using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using H.Socket.IO;
using H.Socket.IO.EventsArgs;
using Newtonsoft.Json;

namespace AL.SocketClient
{
    /// <summary>
    ///     Provides a basic implementation for interacting with the Adventure Land socket server.
    /// </summary>
    /// <seealso cref="FormattedLogger" />
    /// <seealso cref="IAsyncDisposable" />
    public class ALSocketClient : IALSocketClient
    {
        private readonly IFormattedLogger Logger;
        private readonly ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList> Subscriptions;
        private bool Disposed;
        private SocketIoClient Socket;

        public bool Opened => Socket.EngineIoClient.IsOpened;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ALSocketClient" /> class.
        /// </summary>
        /// <param name="logger">The prefixed logged to log messages to.</param>
        public ALSocketClient(IFormattedLogger logger)
        {
            Logger = logger;
            Subscriptions = new ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList>();
            Socket = new SocketIoClient();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Socket is already open.</exception>
        public Task ConnectAsync(Server server)
        {
            if (Opened)
                throw new InvalidOperationException("Socket is already open.");

            if (Disposed)
            {
                Socket = new SocketIoClient();
                Disposed = false;
            }

            var host = $"ws://{server.IPAddress}:{server.Port}";

            Socket.EventReceived += EventHandler;

            Logger.Info($"Connecting to {host}");
            return Socket.ConnectAsync(new Uri(host));
        }


        public async Task DisconnectAsync()
        {
            Logger.Warn("Disconnecting");

            foreach ((_, var subList) in Subscriptions)
                foreach (var sub in await subList.ToArrayAsync())
                    await sub.DisposeAsync();

            Subscriptions.Clear();

            await Socket.DisconnectAsync();

            try
            {
                await Socket.DisposeAsync();
                Disposed = true;
            } catch
            {
                //ignored
            }
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await DisconnectAsync();
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Socket is null or closed.</exception>
        public Task Emit<T>(ALSocketEmitType socketEmitType, T data)
        {
            #if DEBUG
            Logger.Trace($"{socketEmitType}, {data}");
            #endif

            if ((Socket == null) || !Opened)
                throw new InvalidOperationException("Socket is null or closed.");

            return Socket.Emit(EnumHelper.ToString(socketEmitType).ToLowerInvariant(), data);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Socket is null or closed.</exception>
        public Task Emit(ALSocketEmitType socketEmitType)
        {
            #if DEBUG
            Logger.Trace($"{socketEmitType}");
            #endif

            if ((Socket == null) || !Opened)
                throw new InvalidOperationException("Socket is null or closed.");

            return Socket.Emit(EnumHelper.ToString(socketEmitType).ToLowerInvariant());
        }

        private async void EventHandler(object? sender, SocketIoEventArgs e) => await HandleEventAsync(e.Value);

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
                    await InvokeAsync(subscriptionList, rawJson, message.Data.CreateReader());
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
                //TODO: Remove this when not capturing stuff
                Logger.Trace(raw);

                var dataObject = JsonSerializer.Deserialize(reader, invocationList.Type);

                if (dataObject == null)
                {
                    Logger.Error($"Failed to deserialize message. {Environment.NewLine}{raw}");
                    return;
                }

                foreach (var subscription in subscriptions)
                {
                    var handled = await subscription.InvokeAsync(dataObject);

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


        public async ValueTask Unsub<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
        {
            if (Subscriptions.TryGetValue(socketMessageType, out var invocationList))
                await invocationList.RemoveAllAsync(subscription => subscription.Callback == (Delegate)callback);
        }

        #region Do Not ReOrder

        /// <summary>
        ///     A default <see cref="JsonSerializerSettings" /> instance, used for serializing emits and deserializing messages.
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