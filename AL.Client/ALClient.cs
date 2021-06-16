using System.Collections.Generic;
using System.Threading.Tasks;
using AL.APIClient;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.Receive;
using Chaos.Core.Collections.Synchronized.Awaitable;

namespace ALClientCS
{
    public class ALClient
    {
        public ALAPIClient API { get; }
        public ALSocketClient Socket { get; }
        public Character Character { get; private set; }
        public AwaitableDictionary<string, Monster> Monsters { get; }
        public AwaitableDictionary<string, Player> Players { get; }
        public AwaitableDictionary<string, Item> Inventory { get; }
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> BaseGold { get; private set; }
        public ServerInfoData ServerInfo { get; private set; }

        internal ALClient(ALAPIClient apiClient, ALSocketClient socketClient)
        {
            API = apiClient;
            Socket = socketClient;
            Monsters = new AwaitableDictionary<string, Monster>();
            Players = new AwaitableDictionary<string, Player>();
            Inventory = new AwaitableDictionary<string, Item>();

            Socket.On<StartData>(ALSocketMessageType.Start, OnStart);
        }

        private async Task<bool> OnStart(StartData data)
        {
            Character = data;
            BaseGold = data.BaseGold;
            ServerInfo = data.ServerInfo;
            return await OnEntities(data.Entities);
        }

        internal async Task<bool> OnEntities(EntitiesData data)
        {
            if (data.UpdateType == EntitiesUpdateType.Positions)
            {
                
            }

            return true;
        }
    }
}