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

        public async Task<PingAckData> PingAsync(long pingCount)
        {
            var source = new TaskCompletionSource<Expectation<PingAckData>>();

            Task<bool> OnPingAck(PingAckData data) => Task.FromResult(source.TrySetResult(data));
            await using var pingAckCallback = Socket.On<PingAckData>(ALSocketMessageType.PingAck, OnPingAck);

            await Socket.Emit(ALSocketEmitType.Ping, new { id = pingCount.ToString() });
            return await source.Task.WithTimeout();
        }

        public async Task<EntitiesData> RequestEntitiesAsync()
        {
            var source = new TaskCompletionSource<Expectation<EntitiesData>>();

            Task<bool> OnEntities(EntitiesData data) => Task.FromResult(source.TrySetResult(data));
            await using var entitiesDataCallback = Socket.On<EntitiesData>(ALSocketMessageType.Entities, OnEntities);

            await Socket.Emit(ALSocketEmitType.SendUpdates, new object());
            return await source.Task.WithTimeout();
        }

        public async Task<CharacterData> RequestCharacterAsync()
        {
            var source = new TaskCompletionSource<Expectation<CharacterData>>();

            Task<bool> OnCharacter(CharacterData data) => Task.FromResult(source.TrySetResult(data));
            await using var characterDataCallback =
                Socket.On<CharacterData>(ALSocketMessageType.Character, OnCharacter);

            await Socket.Emit(ALSocketEmitType.Property, new { typing = true });
            return await source.Task.WithTimeout();
        }

        public async Task<NewMapData> AcceptMagiportAsync(string from)
        {
            var source = new TaskCompletionSource<Expectation<NewMapData>>();

            Task<bool> OnNewMap(NewMapData data) => Task.FromResult(source.TrySetResult(data));
            await using var newMapDataCallback = Socket.On<NewMapData>(ALSocketMessageType.NewMap, OnNewMap);

            await Socket.Emit(ALSocketEmitType.Magiport, new { name = from });
            return await source.Task.WithTimeout();
        }

        public async Task<PartyUpdateData> AcceptPartyInviteAsync(string from)
        {
            var source = new TaskCompletionSource<Expectation<PartyUpdateData>>();

            Task<bool> OnPartyUpdate(PartyUpdateData data)
            {
                var result = false;

                if (data.MemberNames.ContainsI(from))
                    result = source.TrySetResult(data);

                return Task.FromResult(result);
            }

            Task<bool> OnGameLog(string data)
            {
                var result = false;

                if (data.EqualsI("invitation expired") || data.EqualsI($"{from} is not found"))
                    result = source.TrySetResult(data);
                else if (data.EqualsI("already partying"))
                    result = Party.MemberNames.ContainsI(@from)
                        ? source.TrySetResult(Party)
                        : source.TrySetResult(data);

                return Task.FromResult(result);
            }

            await using var partyUpdateCallback =
                Socket.On<PartyUpdateData>(ALSocketMessageType.PartyUpdate, OnPartyUpdate);

            await using var gameLogCallback = Socket.On<string>(ALSocketMessageType.GameLog, OnGameLog);

            await Socket.Emit(ALSocketEmitType.Party, new { @event = "accept", name = from });
            return await source.Task.WithTimeout();
        }
    }
}