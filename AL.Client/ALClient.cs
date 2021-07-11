using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Definitions;
using AL.Client.Abstractions;
using AL.Client.Helpers;
using AL.Client.Managers;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Extensions;
using AL.SocketClient.SocketModel;
using Chaos.Core.Extensions;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace AL.Client
{
    public class ALClient : ALActionClient
    {
        private static readonly SemaphoreSlim ConnectSync = new(1, 1);
        private readonly PingManager PingManager;
        private readonly PositionManager PositionManager;
        private readonly SemaphoreSlim Sync = new(1, 1);

        public ALClient(string name, ALAPIClient apiClient)
            : base(name, apiClient)
        {
            PositionManager = new PositionManager(this);
            PingManager = new PingManager(this);
        }

        private void AttachListeners()
        {
            Socket.On<StartData>(ALSocketMessageType.Start, OnStartAsync);
            Socket.On<CharacterData>(ALSocketMessageType.Character, OnCharacterAsync);
            Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, OnGameResponseAsync);
            Socket.On<EntitiesData>(ALSocketMessageType.Entities, OnEntitiesAsync);
            Socket.On<AchievementProgressData>(ALSocketMessageType.AchievementProgress, OnAchievementProgressAsync);
            Socket.On<DropData>(ALSocketMessageType.Drop, OnDropAsync);
            Socket.On<EvalData>(ALSocketMessageType.Eval, OnEvalAsync);
            Socket.On<GameErrorData>(ALSocketMessageType.GameError, OnGameErrorAsync);
            Socket.On<PartyUpdateData>(ALSocketMessageType.PartyUpdate, OnPartyUpdateAsync);
            Socket.On<QueuedActionData>(ALSocketMessageType.QueuedActionData, OnQueuedActionAsync);
            Socket.On<UpgradeData>(ALSocketMessageType.Upgrade, OnUpgradeAsync);
            Socket.On<WelcomeData>(ALSocketMessageType.Welcome, OnWelcomeAsync);
            Socket.On<ActionData>(ALSocketMessageType.Action, OnActionAsync);
            Socket.On<DeathData>(ALSocketMessageType.Death, OnDeathAsync);
            Socket.On<DisappearData>(ALSocketMessageType.Disappear, OnDisappearAsync);
            Socket.On<HitData>(ALSocketMessageType.Hit, OnHitAsync);
            Socket.On<NewMapData>(ALSocketMessageType.NewMap, OnNewMapAsync);
            Socket.On<EventAndBossData>(ALSocketMessageType.ServerInfo, OnServerInfo);
        }

        public async Task ConnectAsync(ServerRegion region, ServerId identifier)
        {
            if (Socket != null)
                throw new InvalidOperationException("Attempting to connect while DisconnectAsync() not called.");

            await FetchCharacterAndServerAsync(region, identifier);

            await Sync.WaitAsync();

            try
            {
                Socket = new ALSocketClient(Name);
                AttachListeners();

                var source = new TaskCompletionSource<Expectation<WelcomeData>>();

                await using var gameErrorCallback = Socket.On<GameErrorData>(ALSocketMessageType.GameError,
                    data => Task.FromResult(source.TrySetResult(data.Message)));

                await using var welcomeCallback = Socket.On<WelcomeData>(ALSocketMessageType.Welcome, async data =>
                {
                    await Socket.Emit(ALSocketEmitType.Auth, new
                    {
                        auth = API.AuthUser.AuthKey,
                        character = Identifier,
                        height = 1080,
                        no_graphics = "True",
                        no_html = "1",
                        passphrase = string.Empty,
                        scale = 2,
                        user = API.AuthUser.UserID.ToString(),
                        width = 1920
                    });

                    return source.TrySetResult(data);
                });

                await Socket.ConnectAsync(Server);

                var result = await source.Task.WithTimeout(5000);
                result.ThrowIfUnsuccessful();

                PositionManager.Start(30);
                PingManager.Start(1 / 4f);
            } finally
            {
                Sync.Release();
            }
        }

        public static async Task<ALClient> CreateAsync(
            string characterName,
            ServerRegion region,
            ServerId identifier,
            ALAPIClient apiClient)
        {
            var client = new ALClient(characterName, apiClient);
            await client.ConnectAsync(region, identifier);

            return client;
        }

        public async Task DisconnectAsync()
        {
            await Sync.WaitAsync();

            try
            {
                Warn("Disconnecting");
                await Socket.DisconnectAsync();
                await Socket.DisposeAsync();
                Socket = null;
            } finally
            {
                Sync.Release();
            }
        }

        private async Task FetchCharacterAndServerAsync(ServerRegion region, ServerId identifier)
        {
            await ConnectSync.WaitAsync();

            try
            {
                //if servers/characters not populated, populate them
                if ((API.Servers == null)
                    || (API.Servers.Count == 0)
                    || (API.Characters == null)
                    || (API.Characters.Count == 0))
                    await API.UpdateServersAndCharactersAsync();
            } finally
            {
                ConnectSync.Release();
            }

            //find server and character
            var charInfo = API.Characters.FirstOrDefault(character => character.Name.EqualsI(Name));
            var serverInfo =
                API.Servers.FirstOrDefault(server => (server.Region == region) && (server.Identifier == identifier));

            if (charInfo == null)
                throw new InvalidOperationException($@"Character ""{Name}"" not found.");

            if (serverInfo == null)
                throw new InvalidOperationException($@"Server {region} {identifier} not found.");

            Server = serverInfo;
            Identifier = charInfo.Id;
        }
    }
}