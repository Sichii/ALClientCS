using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.Core.Helpers;
using AL.Core.Json.Converters;
using AL.SocketClient.Abstractions;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using Common.Logging;
using H.Socket.IO;
using H.Socket.IO.EventsArgs;
using Newtonsoft.Json;

namespace AL.SocketClient
{
    public class ALSocketClient : NamedLoggerBase, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList> Subscriptions;
        private Server Server;
        private SocketIoClient Socket;
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new();
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static JsonSerializer JsonSerializer { get; set; } =
            JsonSerializer.CreateDefault(JsonSerializerSettings);

        public sealed override ILog Logger { get; init; }
        public sealed override string Name { get; init; }

        public ALSocketClient(string name)
        {
            Name = name;
            Logger = LogManager.GetLogger<ALSocketClient>();
            Subscriptions = new ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList>();
        }

        public Task ConnectAsync(Server server)
        {
            Server = server;
            var host = $"ws://{Server.IPAddress}:{Server.Port}";

            Socket = new SocketIoClient();
            Socket.EventReceived += EventHandler;

            Info($"Connecting to {host}");
            return Socket.ConnectAsync(new Uri(host));
        }

        public async Task DisconnectAsync()
        {
            Warn("Disconnecting");
            await Socket.DisconnectAsync();
            await Socket.DisposeAsync();
        }

        public Task Emit<T>(ALSocketEmitType socketEmitType, T data)
        {
            Trace($"{socketEmitType}, {data}");
            return Socket.Emit(EnumHelper.ToString(socketEmitType).ToLowerInvariant(), data);
        }

        private async void EventHandler(object sender, SocketIoEventArgs e) => await HandleEventAsync(e.Value);

        public async ValueTask HandleEventAsync(string raw)
        {
            await Task.Yield();
            ALSocketMessage message;

            try
            {
                message = JsonConvert.DeserializeObject<ALSocketMessage>(raw,
                    ArrayToObjectConverter<ALSocketMessage>.Singleton)!;
            } catch (Exception ex)
            {
                var wrapper = new Exception($@"Failed to deserialize top level message. See inner exception.
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

        public IAsyncDisposable On<T>(ALSocketMessageType socketMessageType, Func<T, Task<bool>> callback)
        {
            if (!Subscriptions.TryGetValue(socketMessageType, out var invocationList))
            {
                invocationList = new ALSocketSubscriptionList(typeof(T));
                Subscriptions[socketMessageType] = invocationList;
            }

            return AlSocketSubscription<T>.Create(invocationList, callback);
        }

        public async ValueTask Unsubscribe<T>(
            ALSocketMessageType socketMessageType,
            Func<string, T, Task<bool>> callback)
        {
            if (Subscriptions.TryGetValue(socketMessageType, out var invocationList))
                await invocationList.RemoveAllAsync(subscription => subscription.Callback == (Delegate) callback);
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            foreach ((_, var subList) in Subscriptions)
                foreach (var sub in await subList.ToArrayAsync())
                    await sub.DisposeAsync();

            Subscriptions.Clear();
            await Socket.DisposeAsync();
        }
    }
}