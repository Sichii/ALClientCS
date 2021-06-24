using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Model;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Receive;
using AL.SocketClient.SocketModel;
using ALClientCS.Managers;
using ALClientCS.Model;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Common.Logging;
using Character = AL.SocketClient.SocketModel.Character;
using Condition = AL.Core.Definitions.Condition;

namespace ALClientCS
{
    public class ALClient
    {
        private readonly ILog Logger;
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> BaseGold { get; private set; }
        public Character Character { get; private set; }
        public EventAndBossInfo EventsAndBosses { get; private set; }
        public PartyUpdateData Party { get; private set; }
        public AwaitableDictionary<string, AchievementProgressData> AchievementProgress { get; }
        public ALAPIClient API { get; }
        public AwaitableDictionary<string, DropData> Chests { get; }
        public AwaitableDictionary<string, CooldownInfo> Cooldowns { get; }
        public AwaitableDictionary<string, Monster> Monsters { get; }
        public string Name { get; }
        public AwaitableDictionary<string, Player> Players { get; }
        public Server Server => Socket.Server;
        public ALSocketClient Socket { get; }
        private readonly PositionManager PositionManager;
        
        internal ALClient(string name, ALAPIClient apiClient, ALSocketClient socketClient)
        {
            Name = name;
            Logger = LogManager.GetLogger<ALClient>();
            AchievementProgress = new AwaitableDictionary<string, AchievementProgressData>();
            API = apiClient;
            Socket = socketClient;
            Monsters = new AwaitableDictionary<string, Monster>();
            Players = new AwaitableDictionary<string, Player>();
            BaseGold = new Dictionary<string, IReadOnlyDictionary<string, int>>();
            Cooldowns = new AwaitableDictionary<string, CooldownInfo>(StringComparer.OrdinalIgnoreCase);
            EventsAndBosses = new EventAndBossInfo();
            Chests = new AwaitableDictionary<string, DropData>();
            Character = new Character();

            Socket.On<StartData>(ALSocketMessageType.Start, OnStart);
            Socket.On<CharacterData>(ALSocketMessageType.Character, OnCharacter);
            Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, OnGameResponse);
            Socket.On<EntitiesData>(ALSocketMessageType.Entities, OnEntities);
            Socket.On<AchievementProgressData>(ALSocketMessageType.AchievementProgress, OnAchievementProgress);
            Socket.On<DropData>(ALSocketMessageType.Drop, OnDrop);
            Socket.On<EvalData>(ALSocketMessageType.Eval, OnEval);
            Socket.On<GameErrorData>(ALSocketMessageType.GameError, OnGameError);
            Socket.On<PartyUpdateData>(ALSocketMessageType.PartyUpdate, OnPartyUpdate);
            Socket.On<QueuedActionData>(ALSocketMessageType.QueuedActionData, OnQueuedAction);
            Socket.On<UpgradeData>(ALSocketMessageType.Upgrade, OnUpgrade);
            Socket.On<WelcomeData>(ALSocketMessageType.Welcome, OnWelcome);
            
            PositionManager = new PositionManager(this);
            PositionManager.Start(30);
        }
        
        private async Task<bool> OnAchievementProgress(AchievementProgressData data)
        {
            await AchievementProgress.AddOrUpdateAsync(data.Name, data);
            return false;
        }

        private async Task<bool> OnCharacter(CharacterData data)
        {
            Character = data;

            if (data.ExtraEvents is { Length: > 0 })
                foreach (var jArr in data.ExtraEvents)
                {
                    var raw = jArr.ToString();
                    await Socket.HandleMessageAsync(raw);
                }

            return false;
        }

        private async Task<bool> OnDrop(DropData data)
        {
            await Chests.AddOrUpdateAsync(data.Id, data);
            return false;
        }

        private async Task<bool> OnEntities(EntitiesData data)
        {
            await UpdateMonsters(data.Monsters, data.UpdateType == EntitiesUpdateType.All);
            await UpdatePlayers(data.Players, data.UpdateType == EntitiesUpdateType.All);

            return false;
        }

        private async Task<bool> OnEval(EvalData data) =>
            //TODO: figure out how to handle eval
            false;

        private Task<bool> OnGameError(GameErrorData data)
        {
            Warn($"GAME ERROR: {data.Message}");
            return Task.FromResult(false);
        }

        private async Task<bool> OnGameResponse(GameResponseData data)
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

        private Task<bool> OnPartyUpdate(PartyUpdateData data)
        {
            Party = data;
            return Task.FromResult(false);
        }

        private Task<bool> OnQueuedAction(QueuedActionData data)
        {
            Character.QueuedActions.Mutate(data.QueuedActionInfo);

            return Task.FromResult(false);
        }

        private async Task<bool> OnStart(StartData data)
        {
            Character = data;
            BaseGold = data.BaseGold;
            EventsAndBosses = data.EventAndBossInfo;

            var success = true;
            success &= await OnEntities(data.Entities);
            success &= await OnCharacter(data);

            return success;
        }

        private Task<bool> OnUpgrade(UpgradeData data)
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

        private async Task<bool> OnWelcome(WelcomeData data)
        {
            if (Server.Name != data.Name || Server.Region != data.Region)
                throw new Exception(
                    $"Logged into wrong server. Expected: {Server.Region} {Server.Name}  Current: {data.Region} {data.Name}");

            await Socket.Emit(ALSocketEmitType.Loaded, new
            {
                height = 1080,
                width = 1920,
                scale = 2,
                success = 1
            });

            return false;
        }

        private ValueTask UpdateMonsters(IEnumerable<Monster> monsters, bool full = false)
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

        private ValueTask UpdatePlayers(IEnumerable<Player> players, bool full = false)
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

        #region Logging

        public void Warn(object message) => Logger.Warn($"[{Name}] {message}");
        public void Trace(object message) => Logger.Trace($"[{Name}] {message}");
        public void Debug(object message) => Logger.Debug($"[{Name}] {message}");
        public void Error(object message) => Logger.Error($"[{Name}] {message}");
        public void Fatal(object message) => Logger.Fatal($"[{Name}] {message}");
        public void Info(object message) => Logger.Info($"[{Name}] {message}");

        #endregion
    }
}