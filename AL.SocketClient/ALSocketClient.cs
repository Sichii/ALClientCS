using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Definitions;
using AL.APIClient.Model;
using AL.Core.Json.Converters;
using AL.SocketClient.ClientModel;
using AL.SocketClient.Definitions;
using Common.Logging;
using Newtonsoft.Json;
using WebSocketSharp;

namespace AL.SocketClient
{
    public class ALSocketClient
    {
        private readonly ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList> Subscriptions;
        public Server Server;
        private WebSocket Socket;
        internal ALAPIClient APIClient { get; }
        private ILog Logger { get; }

        public ALSocketClient(ALAPIClient apiClient)
        {
            APIClient = apiClient;
            Logger = LogManager.GetLogger<ALSocketClient>();
            Subscriptions = new ConcurrentDictionary<ALSocketMessageType, ALSocketSubscriptionList>();
        }

        public async Task ConnectAsync(ServerRegion region, ServerId identifier)
        {
            if (APIClient.Servers == null || APIClient.Servers.Length == 0)
                await APIClient.UpdateServersAndCharactersAsync();

            Server = APIClient.Servers.FirstOrDefault(server => server.Region == region && server.Name == identifier);

            if (Server == null)
                throw new Exception($"Failed to find server with region {region} and id {identifier}");

            var host = $"ws://{Server.IPAddress}:{Server.Port}";

            Socket = new WebSocket(host);
            Socket.OnMessage += MessageHandler;

            Logger.Info($"Connecting to {host}");
            Socket.ConnectAsync();
        }

        public async Task<bool> Emit<T>(ALSocketEmitType socketEmitType, T data, bool waitForCompletion = false)
        {
            var source = new TaskCompletionSource<bool>();
            void Callback(bool success) => source.SetResult(success);
            var obj = new object[] { socketEmitType, data };

            Socket.SendAsync(JsonConvert.SerializeObject(obj), Callback);
            return !waitForCompletion || await source.Task;
        }

        public async ValueTask HandleMessageAsync(string raw)
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
                Logger.Fatal(wrapper);

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

                Logger.Fatal(wrapper);

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

        private async void MessageHandler(object sender, MessageEventArgs e) => await HandleMessageAsync(e.Data);

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
    }
}