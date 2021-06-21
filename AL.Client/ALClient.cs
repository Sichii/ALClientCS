using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AL.APIClient;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Model;
using AL.SocketClient.Receive;
using ALClientCS.Model;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Common.Logging;
using Condition = AL.Core.Definitions.Condition;

namespace ALClientCS
{
    public class ALClient
    {
        private readonly ILog Logger;
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> BaseGold { get; private set; }
        public Character Character { get; private set; }
        public ServerInfoData ServerInfo { get; private set; }
        public ALAPIClient API { get; }
        public AwaitableDictionary<string, CooldownInfo> CooldownInfo { get; }
        public AwaitableDictionary<string, Monster> Monsters { get; }
        public string Name { get; }
        public AwaitableDictionary<string, Player> Players { get; }
        public ALSocketClient Socket { get; }

        internal ALClient(string name, ALAPIClient apiClient, ALSocketClient socketClient)
        {
            Name = name;
            Logger = LogManager.GetLogger<ALClient>();
            API = apiClient;
            Socket = socketClient;
            Monsters = new AwaitableDictionary<string, Monster>();
            Players = new AwaitableDictionary<string, Player>();
            BaseGold = new Dictionary<string, IReadOnlyDictionary<string, int>>();
            CooldownInfo = new AwaitableDictionary<string, CooldownInfo>(StringComparer.OrdinalIgnoreCase);
            ServerInfo = new ServerInfoData();

            Socket.On<StartData>(ALSocketMessageType.Start, OnStart);
            Socket.On<CharacterData>(ALSocketMessageType.Player, OnCharacter);
            Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, OnGameResponse);
        }

        public void Debug(string message) => Logger.Debug($"[{Name}] {message}");
        public void Error(string message) => Logger.Error($"[{Name}] {message}");
        public void Fatal(string message) => Logger.Fatal($"[{Name}] {message}");
        public void Info(string message) => Logger.Info($"[{Name}] {message}");

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

        private async Task<bool> OnEntities(EntitiesData data)
        {
            await UpdateMonsters(data.Monsters, data.UpdateType == EntitiesUpdateType.All);
            await UpdatePlayers(data.Players, data.UpdateType == EntitiesUpdateType.All);

            return false;
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

                    if (await CooldownInfo.TryGetValueAsync(response.SkillName, out var infoTask))
                    {
                        var info = await infoTask;
                        await CooldownInfo.AddOrUpdateAsync(info.SkillName,
                            info with { ServerCooldownMS = response.CooldownMS });
                    } else
                        await CooldownInfo.AddOrUpdateAsync(response.SkillName, new CooldownInfo
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

                    if (await CooldownInfo.TryGetValueAsync(response.SkillName, out var infoTask))
                    {
                        var info = await infoTask;
                        await CooldownInfo.AddOrUpdateAsync(info.SkillName,
                            info with { ServerCooldownMS = cooldownMS });
                    } else
                        await CooldownInfo.AddOrUpdateAsync(response.SkillName, new CooldownInfo
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

        private async Task<bool> OnStart(StartData data)
        {
            Character = data;
            BaseGold = data.BaseGold;
            ServerInfo = data.ServerInfo;

            var success = true;
            success &= await OnEntities(data.Entities);
            success &= await OnCharacter(data);

            return success;
        }

        public void Trace(string message) => Logger.Trace($"[{Name}] {message}");

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
                        monsterX.Update(monster);
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
                        Character.Update(player);

                    if (dic.TryGetValue(player.Id, out var playerX))
                        playerX.Update(player);
                }
            });
        }

        public void Warn(string message) => Logger.Warn($"[{Name}] {message}");
    }
}