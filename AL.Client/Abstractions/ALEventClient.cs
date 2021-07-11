using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Model;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Core.Model;
using AL.Data;
using AL.SocketClient;
using AL.SocketClient.Abstractions;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces.Responses;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Common.Logging;
using Character = AL.SocketClient.Model.Character;
using Condition = AL.Core.Definitions.Condition;

namespace AL.Client.Abstractions
{
    public class ALEventClient : NamedLoggerBase, IAsyncDisposable
    {
        public BankInfo Bank { get; protected set; }
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> BaseGold { get; protected set; }
        public Character Character { get; protected set; }
        public EventAndBossInfo EventsAndBosses { get; protected set; }
        public string Identifier { get; protected set; }
        public sealed override ILog Logger { get; init; }
        public sealed override string Name { get; init; }
        public PartyUpdateData Party { get; protected set; }
        public Server Server { get; protected set; }
        public ALSocketClient Socket { get; protected set; }
        public AwaitableDictionary<string, AchievementProgressData> AchievementProgress { get; }
        public ALAPIClient API { get; }
        public AwaitableDictionary<string, DropData> Chests { get; }
        public AwaitableDictionary<string, CooldownInfo> Cooldowns { get; }
        public AwaitableDictionary<string, Monster> Monsters { get; }
        public AwaitableDictionary<string, Player> Players { get; }
        public AwaitableDictionary<string, ActionData> Projectiles { get; }

        internal ALEventClient(string name, ALAPIClient apiClient)
        {
            Name = name;
            Logger = LogManager.GetLogger<ALClient>();
            AchievementProgress = new AwaitableDictionary<string, AchievementProgressData>();
            API = apiClient;
            Monsters = new AwaitableDictionary<string, Monster>();
            Players = new AwaitableDictionary<string, Player>();
            Projectiles = new AwaitableDictionary<string, ActionData>();
            BaseGold = new Dictionary<string, IReadOnlyDictionary<string, int>>();
            Cooldowns = new AwaitableDictionary<string, CooldownInfo>(StringComparer.OrdinalIgnoreCase);
            EventsAndBosses = new EventAndBossInfo();
            Chests = new AwaitableDictionary<string, DropData>();
            Character = new Character();
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return Socket?.DisposeAsync() ?? default;
        }

        protected async Task<bool> OnAchievementProgressAsync(AchievementProgressData data)
        {
            await AchievementProgress.AddOrUpdateAsync(data.Name, data);
            return false;
        }

        protected async Task<bool> OnActionAsync(ActionData data)
        {
            await Projectiles.AddOrUpdateAsync(data.ProjectileId, data);
            return false;
        }

        protected async Task<bool> OnCharacterAsync(CharacterData data)
        {
            Character = data;

            //keep a copy of the bank data
            if (data.Bank != null)
                Bank = data.Bank;

            if (data.ExtraEvents?.Count > 0)
                foreach (var jArr in data.ExtraEvents)
                {
                    var raw = jArr.ToString();
                    await Socket.HandleEventAsync(raw);
                }

            return false;
        }

        protected async Task<bool> OnDeathAsync(DeathData data)
        {
            await DestroyEntity(data.Id);
            return false;
        }

        protected async Task<bool> OnDisappearAsync(DisappearData data)
        {
            await DestroyEntity(data.Id);
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

        protected async Task<bool> OnHitAsync(HitData data)
        {
            if (!string.IsNullOrEmpty(data.ProjectileId)
                && await Projectiles.TryGetValueAsync(data.ProjectileId, out var projectileTask))
            {
                var projectile = await projectileTask;
                await Projectiles.RemoveAsync(data.ProjectileId);

                if (data.Reflect != 0)
                {
                    var newProjectile = projectile with
                    {
                        Damage = data.Reflect, Target = data.HID, X = Character.X, Y = Character.Y
                    };

                    await Projectiles.AddOrUpdateAsync(newProjectile.ProjectileId, newProjectile);
                }
            }

            if (data.Kill)
                await DestroyEntity(data.Id);
            else if (data.Damage != 0)
            {
                var entity = await GetEntity(data.Id);

                if (entity != null)
                    entity.Mutate(new Mutation(ALAttribute.Hp, -data.Damage));
            }

            if (data.Reflect != 0)
            {
                var sourceEntity = await GetEntity(data.Source);

                if (sourceEntity != null)
                    sourceEntity.Mutate(new Mutation(ALAttribute.Hp, -data.Reflect));
            }

            return false;
        }

        protected async Task<bool> OnNewMapAsync(NewMapData data)
        {
            await Projectiles.ClearAsync();
            Character?.Mutate(data);

            await OnEntitiesAsync(data.Entities);

            return false;
        }

        protected Task<bool> OnPartyUpdateAsync(PartyUpdateData data)
        {
            Party = data;
            return Task.FromResult(false);
        }

        protected Task<bool> OnQueuedActionAsync(QueuedActionData data)
        {
            Character.Mutate(data.QueuedActionInfo);

            return Task.FromResult(false);
        }

        protected Task<bool> OnServerInfo(EventAndBossData data)
        {
            var bossInfoDic = (Dictionary<string, BossInfo>) data.BossInfo;
            EventsAndBosses = data;

            foreach ((var name, var bossInfo) in bossInfoDic)
                if (bossInfo.HP == 0)
                    bossInfo.Mutate(new Mutation(ALAttribute.Hp, GameData.Monsters[name].HP));

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
                    Character.Mutate(Character.QueuedActions with { Compound = null });
                    break;
                case UpgradeType.Upgrade:
                    Character.Mutate(Character.QueuedActions with { Upgrade = null });
                    break;
                case UpgradeType.Exchange:
                    Character.Mutate(Character.QueuedActions with { Exchange = null });
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown upgrade type {(int) data.UpgradeType}.");
            }

            return Task.FromResult(false);
        }

        protected async Task<bool> OnWelcomeAsync(WelcomeData data)
        {
            if ((Server.Identifier != data.Identifier) || (Server.Region != data.Region))
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

        #region Helpers

        protected async ValueTask<bool> DestroyEntity(string id)
        {
            var result = await Monsters.RemoveAsync(id) || await Players.RemoveAsync(id);

            var bossInfoDic = (Dictionary<string, BossInfo>) EventsAndBosses.BossInfo;
            bossInfoDic.Remove(id);

            return result;
        }

        protected async ValueTask<EntityBase> GetEntity(string id) =>
            await Players.TryGetValueAsync(id, out var playerTask)   ? await playerTask :
            await Monsters.TryGetValueAsync(id, out var monsterTask) ? await monsterTask : null;

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

        #endregion
    }
}