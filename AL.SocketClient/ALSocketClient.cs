using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.Core.Abstractions;
using AL.Core.Helpers;
using AL.Core.Json.Converters;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using H.Socket.IO;
using H.Socket.IO.EventsArgs;
using Newtonsoft.Json;

namespace AL.SocketClient
{
    /// <summary>
    ///     Provides a basic implementation for interacting with the Adventure Land socket server.
    /// </summary>
    /// <seealso cref="NamedLoggerBase" />
    /// <seealso cref="IAsyncDisposable" />
    public class ALSocketClient : NamedLoggerBase, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList> Subscriptions;
        private Server? Server;
        private SocketIoClient? Socket;
        
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
        public static JsonSerializer JsonSerializer { get; set; } =
            JsonSerializer.CreateDefault(JsonSerializerSettings);
        #endregion
        
        /// <summary>
        ///     The name of the character this client is for.
        /// </summary>
        public override string Name { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ALSocketClient" /> class.
        /// </summary>
        /// <param name="name">The name of the character this client is for.</param>
        public ALSocketClient(string name)
            : base(name)
        {
            Name = name;
            Subscriptions = new ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList>();
        }

        /// <summary>
        ///     Asynchronously connects to an Adventure Land server.
        /// </summary>
        /// <param name="server">
        ///     An object containing information about the server to connect to. Use the <see cref="APIClient" />
        ///     to get this information.
        /// </param>
        public Task ConnectAsync(Server server)
        {
            Server = server;
            var host = $"ws://{Server.IPAddress}:{Server.Port}";

            Socket = new SocketIoClient();
            Socket.EventReceived += EventHandler;

            Info($"Connecting to {host}");
            return Socket.ConnectAsync(new Uri(host));
        }

        /// <summary>
        ///     Asynchronously disconnects this client from the server. <br />
        ///     Also disposes of the internal socket.
        /// </summary>
        public async Task DisconnectAsync()
        {
            Warn("Disconnecting");

            if (Socket != null)
            {
                await Socket.DisconnectAsync();
                await Socket.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            foreach ((_, var subList) in Subscriptions)
                foreach (var sub in await subList.ToArrayAsync())
                    await sub.DisposeAsync();

            Subscriptions.Clear();

            if (Socket != null)
                await Socket.DisposeAsync();
        }

        /// <summary>
        ///     Serializes the data and emits a message to the server via socket.io protocol.
        /// </summary>
        /// <param name="socketEmitType">A value indicating the title of the message.</param>
        /// <param name="data">The data to serialize.</param>
        /// <typeparam name="T">The type of the data being serialized.</typeparam>
        /// <exception cref="InvalidOperationException">Socket is null or closed.</exception>
        public Task Emit<T>(ALSocketEmitType socketEmitType, T data)
        {
            #if DEBUG
            Trace($"{socketEmitType}, {data}");
            #endif

            if ((Socket == null) || !Socket.EngineIoClient.IsOpened)
                throw new InvalidOperationException("Socket is null or closed.");

            return Socket.Emit(EnumHelper.ToString(socketEmitType).ToLowerInvariant(), data);
        }

        private async void EventHandler(object? sender, SocketIoEventArgs e) => await HandleEventAsync(e.Value);

        /// <summary>
        ///     Handles a received socket event based on the title of the message, and how certain messages are set up to be
        ///     handled via <see cref="On{T}" />.
        /// </summary>
        /// <param name="raw">The raw json of the received message.</param>
        /// <exception cref="InvalidOperationException">
        ///     Failed to deserialize top level message. See inner exception. <br />
        ///     RAW JSON: <br />
        ///     {raw}
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Uncaught exception in handler. See inner exception. <br />
        ///     RAW JSON: <br />
        ///     {raw}
        /// </exception>
        public async ValueTask HandleEventAsync(string raw)
        {
            ALSocketMessage message;

            try
            {
                message = JsonConvert.DeserializeObject<ALSocketMessage>(raw,
                    ArrayToObjectConverter<ALSocketMessage>.Singleton)!;
            } catch (Exception ex)
            {
                var wrapper = new InvalidOperationException(
                    $@"Failed to deserialize top level message. See inner exception.
RAW JSON:
{raw}", ex);

                Fatal(wrapper);

                throw wrapper;
            }

            try
            {
                if (Subscriptions.TryGetValue(message!.MessageType, out var subscriptionList))
                    await InvokeAsync(subscriptionList, raw, message.Data.CreateReader());
            } catch (Exception ex)
            {
                var wrapper = new Exception($@"Uncaught exception in handler. See inner exception.
RAW JSON:
{raw}", ex);

                Fatal(wrapper);

                throw wrapper;
            }
        }

        private ValueTask InvokeAsync(ALSocketSubscriptionList invocationList, string raw, JsonReader reader)
        {
            async ValueTask InnerInvokeAsync(List<ALSocketSubscription> subscriptions)
            {
                //TODO: Remove this when not capturing stuff
                Trace(raw);

                var dataObject = JsonSerializer.Deserialize(reader, invocationList.Type);

                if (dataObject == null)
                {
                    Error($"Failed to deserialize message. {Environment.NewLine}{raw}");
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

        /// <summary>
        ///     Instructs the client on how to handle a certain message type. <br />
        ///     There can be any number of handlers stacked for a specific message. They will be executed in the order they were
        ///     configured. <br />
        ///     If any given handler returns <c>true</c>, execution will stop. (it signals that the event was handled)
        /// </summary>
        /// <param name="socketMessageType">The type of message.</param>
        /// <param name="callback">A function to be called when receiving the specified message type.</param>
        /// <typeparam name="T">The type of data to expect and deserialize when receiving the message.</typeparam>
        /// <returns>
        ///     A disposable object that when disposed will remove this handler from the handler list. <br />
        ///     This is the preferred way of handling this, but alternatively, you can use <see cref="Unsubscribe{T}" />.
        /// </returns>
        public IAsyncDisposable On<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
        {
            if (!Subscriptions.TryGetValue(socketMessageType, out var invocationList))
            {
                invocationList = new ALSocketSubscriptionList(typeof(T));
                Subscriptions[socketMessageType] = invocationList;
            }

            return AlSocketSubscription<T>.Create(invocationList, callback);
        }

        /// <summary>
        ///     If certain constraints restrict you from using the disposable pattern, this can be used to unsubscribe a callback.
        /// </summary>
        /// <param name="socketMessageType">The message type to unsubscribe from.</param>
        /// <param name="callback">The callback to remove from the subscription list.</param>
        /// <typeparam name="T">The type of data that was expected.</typeparam>
        public async ValueTask Unsubscribe<T>(
            ALSocketMessageType socketMessageType,
            Func<string, T, Task<bool>> callback)
        {
            if (Subscriptions.TryGetValue(socketMessageType, out var invocationList))
                await invocationList.RemoveAllAsync(subscription => subscription.Callback == (Delegate) callback);
        }
    }
}