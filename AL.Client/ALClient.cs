using System;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Definitions;
using AL.Client.Abstractions;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Client.Managers;
using AL.SocketClient.Definitions;
using AL.SocketClient.Receive;
using Chaos.Core.Extensions;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace AL.Client
{
    public class ALClient : ALActionClient
    {
        private readonly PositionManager PositionManager;
        private readonly PingManager PingManager;

        public ALClient(string name, ALAPIClient apiClient)
            : base(name, apiClient)
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
            
            PositionManager = new PositionManager(this);
            PingManager = new PingManager(this);
        }

        public async Task ConnectAsync(ServerRegion region, ServerId identifier)
        {
            if (API.Servers == null || API.Servers.Length == 0 || API.Characters == null || API.Characters.Length == 0)
                await API.UpdateServersAndCharactersAsync();

            var charInfo = API.Characters.FirstOrDefault(character => character.Name.EqualsI(Name));
            if (charInfo == null)
                throw new InvalidOperationException($@"Character ""{Name}"" not found.");

            Identifier = charInfo.Id;

            var serverInfo =
                API.Servers.FirstOrDefault(server => server.Region == region && server.Identifier == identifier);

            if (serverInfo == null)
                throw new InvalidOperationException($@"Server {region} {identifier} not found.");

            var source = new TaskCompletionSource<Expectation<WelcomeData>>();

            Task<bool> OnGameError(GameErrorData data) => Task.FromResult(source.TrySetResult(data.Message));

            async Task<bool> OnWelcome(WelcomeData data)
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
                    width = 1920,
                });

                return source.TrySetResult(data);
            }

            await using var gameErrorCallback = Socket.On<GameErrorData>(ALSocketMessageType.GameError, OnGameError);
            await using var welcomeCallback = Socket.On<WelcomeData>(ALSocketMessageType.Welcome, OnWelcome);

            await Socket.ConnectAsync(serverInfo);

            var result = await source.Task.WithTimeout(5000);
            result.ThrowIfUnsuccessful();

            PositionManager.Start(30);
            PingManager.Start(1 / 4f);
        }

        public static async Task<ALClient> CreateAsync(string characterName, ServerRegion region, ServerId identifier, ALAPIClient apiClient)
        {
            var client = new ALClient(characterName, apiClient);
            await client.ConnectAsync(region, identifier);

            return client;
        }
    }
}