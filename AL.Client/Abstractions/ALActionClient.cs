using System;
using System.Threading.Tasks;
using AL.APIClient;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.Receive;
using Chaos.Core.Extensions;

namespace AL.Client.Abstractions
{
    public class ALActionClient : ALEventClient
    {
        internal ALActionClient(string name, ALAPIClient apiClient)
            : base(name, apiClient) { }

        public async Task<NewMapData> AcceptMagiportAsync(string from)
        {
            var source = new TaskCompletionSource<Expectation<NewMapData>>();
            
            await using var newMapCallback = Socket.On<NewMapData>(ALSocketMessageType.NewMap,
                data => Task.FromResult(source.TrySetResult(data)));

            await Socket.Emit(ALSocketEmitType.Magiport, new { name = from });
            return await source.Task.WithNetworkTimeout();
        }

        public async Task<PartyUpdateData> AcceptPartyInviteAsync(string from)
        {
            var source = new TaskCompletionSource<Expectation<PartyUpdateData>>();
            
            await using var partyUpdateCallback = Socket.On<PartyUpdateData>(ALSocketMessageType.PartyUpdate, data =>
            {
                var result = false;

                if (data.MemberNames.ContainsI(from))
                    result = source.TrySetResult(data);

                return Task.FromResult(result);
            });

            await using var gameLogCallback = Socket.On<string>(ALSocketMessageType.GameLog, data =>
            {
                var result = false;

                if (data.EqualsI("invitation expired") || data.EqualsI($"{from} is not found"))
                    result = source.TrySetResult(data);
                else if (data.EqualsI("already partying"))
                    result = Party.MemberNames.ContainsI(from) ? source.TrySetResult(Party) : source.TrySetResult(data);

                return Task.FromResult(result);
            });

            await Socket.Emit(ALSocketEmitType.Party, new { @event = "accept", name = from });
            return await source.Task.WithNetworkTimeout();
        }

        public async Task<PartyUpdateData> AcceptPartyRequestAsync(string from)
        {
            var source = new TaskCompletionSource<Expectation<PartyUpdateData>>();

            await using var partyUpdateCallback = Socket.On<PartyUpdateData>(ALSocketMessageType.PartyUpdate, data =>
            {
                var result = false;

                if (data.MemberNames.ContainsI(from))
                    result = source.TrySetResult(data);

                return Task.FromResult(result);
            });

            await using var gameLogCallback = Socket.On<string>(ALSocketMessageType.GameLog, data =>
            {
                var result = false;

                if (data.EqualsI("request expired") || data.EqualsI($"{from} is not found"))
                    result = source.TrySetResult(data);
                else if (data.EqualsI("already partying"))
                    result = Party.MemberNames.ContainsI(from) ? source.TrySetResult(Party) : source.TrySetResult(data);

                return Task.FromResult(result);
            });

            await Socket.Emit(ALSocketEmitType.Party, new { @event = "raccept", name = from });
            return await source.Task.WithNetworkTimeout();
        }

        public async Task<ActionData> AttackAsync(string targetId)
        {
            var source = new TaskCompletionSource<Expectation<ActionData>>();

            await using var deathCallback = Socket.On<DeathData>(ALSocketMessageType.Death,
                data => Task.FromResult(data.Id.EqualsI(targetId)
                                        && source.TrySetResult($"Attack on {targetId} failed. (target died)")));

            await using var responseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.Disabled when data.TargetID.EqualsI(targetId) => source.TrySetResult(
                        $"Attack on {targetId} failed. (disabled)"),
                    GameResponseType.AttackFailed when data.TargetID.EqualsI(targetId) => source.TrySetResult(
                        $"Attack on {targetId} failed."),
                    GameResponseType.TooFar when data.TargetID.EqualsI(targetId) => source.TrySetResult(
                        $"Attack on {targetId} failed. (too far: {data.Distance})"),
                    GameResponseType.Cooldown when data.TargetID.EqualsI(targetId) => source.TrySetResult(
                        $"Attack on {targetId} failed. (on cooldown: {data.CooldownMS}"),
                    GameResponseType.NoMP when data.Place == "attack" => source.TrySetResult(
                        $"Attack on {targetId} failed. (not enough mp)"),
                    _ => false
                };

                return Task.FromResult(result);
            });

            await using var actionCallback = Socket.On<ActionData>(ALSocketMessageType.Action, data =>
            {
                var result = false;

                if (data.Attacker.EqualsI(Character.Id) && data.Target.EqualsI(targetId) && data.Type == "attack")
                    result = source.TrySetResult(data);

                return Task.FromResult(result);
            });

            if (!await Monsters.ContainsKeyAsync(targetId) && !await Players.ContainsKeyAsync(targetId))
                throw new InvalidOperationException($"Attack on {targetId} failed. (not found)");

            await Socket.Emit(ALSocketEmitType.Attack, new { id = targetId });
            return await source.Task.WithNetworkTimeout();
        }

        public async Task<PingAckData> PingAsync(long pingCount)
        {
            var source = new TaskCompletionSource<Expectation<PingAckData>>();
            
            await using var pingAckCallback = Socket.On<PingAckData>(ALSocketMessageType.PingAck,
                data => Task.FromResult(source.TrySetResult(data)));

            await Socket.Emit(ALSocketEmitType.Ping, new { id = pingCount.ToString() });
            return await source.Task.WithNetworkTimeout();
        }

        public async Task<CharacterData> RequestCharacterAsync()
        {
            var source = new TaskCompletionSource<Expectation<CharacterData>>();

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character,
                data => Task.FromResult(source.TrySetResult(data)));

            await Socket.Emit(ALSocketEmitType.Property, new { typing = true });
            return await source.Task.WithNetworkTimeout();
        }

        public async Task<EntitiesData> RequestEntitiesAsync()
        {
            var source = new TaskCompletionSource<Expectation<EntitiesData>>();
            
            await using var entitiesCallback = Socket.On<EntitiesData>(ALSocketMessageType.Entities,
                data => Task.FromResult(source.TrySetResult(data)));

            await Socket.Emit(ALSocketEmitType.SendUpdates, new object());
            return await source.Task.WithNetworkTimeout();
        }
    }
}