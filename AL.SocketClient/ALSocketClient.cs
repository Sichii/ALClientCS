using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using AL.APIClient.Model;
using AL.Core.Helpers;
using AL.Core.Json.Converters;
using AL.SocketClient.Abstractions;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using Common.Logging;
using Newtonsoft.Json;
using H.Socket.IO;
using H.Socket.IO.EventsArgs;

namespace AL.SocketClient
{
    public class ALSocketClient : NamedLoggerBase
    {
        private readonly ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList> Subscriptions;
        public Server Server;
        private SocketIoClient Socket;

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

        public Task Emit<T>(ALSocketEmitType socketEmitType, T data) =>
            Socket.Emit(EnumHelper.ToString(socketEmitType).ToLowerInvariant(), data);

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
                var wrapper = new Exception("Failed to deserialize top level message. See inner exception.", ex);
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
                var dataObject = JsonSerializer.CreateDefault().Deserialize(reader, invocationList.Type);

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

        private async void EventHandler(object sender, SocketIoEventArgs e) => await HandleEventAsync(e.Value);

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

        public sealed override ILog Logger { get; init; }
        public sealed override string Name { get; init; }
    }
}