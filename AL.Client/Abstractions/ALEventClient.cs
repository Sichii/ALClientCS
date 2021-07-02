using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Model;
using AL.Client.Model;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient;
using AL.SocketClient.Abstractions;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Receive;
using AL.SocketClient.SocketModel;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Common.Logging;
using Character = AL.SocketClient.SocketModel.Character;
using Condition = AL.Core.Definitions.Condition;

namespace AL.Client.Abstractions
{
    public class ALEventClient : NamedLoggerBase
    {
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> BaseGold { get; protected set; }
        public Character Character { get; protected set; }
        public string Identifier { get; protected set; }
        public EventAndBossInfo EventsAndBosses { get; protected set; }
        public PartyUpdateData Party { get; protected set; }
        public AwaitableDictionary<string, AchievementProgressData> AchievementProgress { get; }
        public ALAPIClient API { get; }
        public AwaitableDictionary<string, DropData> Chests { get; }
        public AwaitableDictionary<string, CooldownInfo> Cooldowns { get; }
        public AwaitableDictionary<string, Monster> Monsters { get; }
        public sealed override ILog Logger { get; init; }
        public sealed override string Name { get; init; }
        public AwaitableDictionary<string, Player> Players { get; }
        public Server Server => Socket.Server;
        public ALSocketClient Socket { get; }

        internal ALEventClient(string name, ALAPIClient apiClient)
        {
            Name = name;
            Logger = LogManager.GetLogger<ALClient>();
            AchievementProgress = new AwaitableDictionary<string, AchievementProgressData>();
            API = apiClient;
            Socket = new ALSocketClient(name);
            Monsters = new AwaitableDictionary<string, Monster>();
            Players = new AwaitableDictionary<string, Player>();
            BaseGold = new Dictionary<string, IReadOnlyDictionary<string, int>>();
            Cooldowns = new AwaitableDictionary<string, CooldownInfo>(StringComparer.OrdinalIgnoreCase);
            EventsAndBosses = new EventAndBossInfo();
            Chests = new AwaitableDictionary<string, DropData>();
            Character = new Character();
        }
        
        protected async Task<bool> OnAchievementProgressAsync(AchievementProgressData data)
        {
            await AchievementProgress.AddOrUpdateAsync(data.Name, data);
            return false;
        }

        protected async Task<bool> OnCharacterAsync(CharacterData data)
        {
            Character = data;

            if (data.ExtraEvents is { Length: > 0 })
                foreach (var jArr in data.ExtraEvents)
                {
                    var raw = jArr.ToString();
                    await Socket.HandleEventAsync(raw);
                }

            return false;
        }

        protected async Task<bool> OnDropAsync(DropData data)
        {
            await Chests.AddOrUpdateAsync(data.Id, data);
            return false;
        }

        protected async Task<bool> OnEntitiesAsync(EntitiesData data)
        {
            await UpdateMonsters(data.Monsters, data.UpdateType == EntitiesUpdateType.All);
            await UpdatePlayers(data.Players, data.UpdateType == EntitiesUpdateType.All);

            return false;
        }

        //TODO: figure out how to handle eval
        protected Task<bool> OnEvalAsync(EvalData data) => Task.FromResult(false);

        protected Task<bool> OnGameErrorAsync(GameErrorData data)
        {
            Warn($"GAME ERROR: {data.Message}");
            return Task.FromResult(false);
        }

        protected async Task<bool> OnGameResponseAsync(GameResponseData data)
        {
            if (!data.ContainsData)
                return false;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (data.ResponseType)
            {
                case GameResponseType.Cooldown:
                {
                    ICooldownResponse response = data;

                    if (await Cooldowns.TryGetValueAsync(response.SkillName, out var infoTask))
                    {
                        var info = await infoTask;
                        await Cooldowns.AddOrUpdateAsync(info.SkillName,
                            info with { ServerCooldownMS = response.CooldownMS });
                    } else
                        await Cooldowns.AddOrUpdateAsync(response.SkillName, new CooldownInfo
                        {
                            SkillName = response.SkillName,
                            LocalLastUse = DateTime.UtcNow,
                            ServerCooldownMS = response.CooldownMS
                        });

                    break;
                }
                case GameResponseType.ConditionExpired:
                {
                    ISkillNameResponse response = data;

                    if (EnumHelper.TryParse(response.SkillName, out Condition conditionName))
                        await Character.Conditions.RemoveAsync(conditionName);

                    break;
                }

                case GameResponseType.SkillSuccess:
                {
                    ISkillNameResponse response = data;
                    var cooldownMS = GameData.Skills[data.SkillName].CooldownMS;

                    if (await Cooldowns.TryGetValueAsync(response.SkillName, out var infoTask))
                    {
                        var info = await infoTask;
                        await Cooldowns.AddOrUpdateAsync(info.SkillName, info with { ServerCooldownMS = cooldownMS });
                    } else
                        await Cooldowns.AddOrUpdateAsync(response.SkillName, new CooldownInfo
                        {
                            SkillName = response.SkillName,
                            LocalLastUse = DateTime.UtcNow,
                            ServerCooldownMS = cooldownMS
                        });

                    break;
                }
            }

            return false;
        }

        protected Task<bool> OnPartyUpdateAsync(PartyUpdateData data)
        {
            Party = data;
            return Task.FromResult(false);
        }

        protected Task<bool> OnQueuedActionAsync(QueuedActionData data)
        {
            Character.QueuedActions.Mutate(data.QueuedActionInfo);

            return Task.FromResult(false);
        }

        protected async Task<bool> OnStartAsync(StartData data)
        {
            Character = data;
            BaseGold = data.BaseGold;
            EventsAndBosses = data.EventAndBossInfo;

            await OnEntitiesAsync(data.Entities);
            await OnCharacterAsync(data);

            return false;
        }

        protected Task<bool> OnUpgradeAsync(UpgradeData data)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (data.UpgradeType)
            {
                case UpgradeType.Compound:
                    Character.QueuedActions.Compound = null;
                    break;
                case UpgradeType.Upgrade:
                    Character.QueuedActions.Upgrade = null;
                    break;
                case UpgradeType.Exchange:
                    Character.QueuedActions.Exchange = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown upgrade type {(int) data.UpgradeType}.");
            }

            return Task.FromResult(false);
        }

        protected async Task<bool> OnWelcomeAsync(WelcomeData data)
        {
            if (Server.Identifier != data.Identifier || Server.Region != data.Region)
                throw new Exception(
                    $"Logged into wrong server. Expected: {Server.Region} {Server.Identifier}  Current: {data.Region} {data.Identifier}");

            await Socket.Emit(ALSocketEmitType.Loaded, new
            {
                height = 1080,
                width = 1920,
                scale = 2,
                success = 1
            });

            return false;
        }
        
        protected ValueTask UpdateMonsters(IEnumerable<Monster> monsters, bool full = false)
        {
            if (full)
                return Monsters.AssertAsync(dic =>
                {
                    dic.Clear();

                    foreach (var monster in monsters)
                        dic.Add(monster.Id, monster);
                });

            return Monsters.AssertAsync(dic =>
            {
                foreach (var monster in monsters)
                    if (dic.TryGetValue(monster.Id, out var monsterX))
                        monsterX.Mutate(monster);
            });
        }

        protected ValueTask UpdatePlayers(IEnumerable<Player> players, bool full = false)
        {
            if (full)
                return Players.AssertAsync(dic =>
                {
                    dic.Clear();

                    foreach (var player in players)
                        dic.Add(player.Id, player);
                });

            return Players.AssertAsync(dic =>
            {
                foreach (var player in players)
                {
                    if (Character == player)
                        Character.Mutate(player);

                    if (dic.TryGetValue(player.Id, out var playerX))
                        playerX.Mutate(player);
                }
            });
        }
    }
}