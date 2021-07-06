using System;
using System.Linq;
using System.Threading.Tasks;
using AL.APIClient;
using AL.APIClient.Model;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Data;
using AL.SocketClient.Definitions;
using AL.SocketClient.Receive;
using AL.SocketClient.SocketModel;
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

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
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

        // ReSharper disable once UnusedMethodReturnValue.Global
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

        public async Task<SimpleItem> BuyAsync(string itemName, int quantity = 1)
        {
            if (quantity < 1)
                throw new InvalidOperationException($"Invalid quantity of {quantity}.");

            var source = new TaskCompletionSource<Expectation<SimpleItem>>();

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.BuySuccess => source.TrySetResult(new SimpleItem
                    {
                        Name = data.ItemName,
                        Quantity = data.Quantity
                    }),
                    GameResponseType.BuyCantNPC => source.TrySetResult($"Failed to buy {itemName}. (wrong npc)"),
                    GameResponseType.BuyCantSpace => source.TrySetResult(
                        $"Failed to buy {itemName}. (not enough space)"),
                    GameResponseType.BuyCost => source.TrySetResult($"Failed to buy {itemName}. (not enough gold)"),
                    GameResponseType.BuyGetCloser => source.TrySetResult(
                        $"Failed to buy {itemName}. (get closer: {data.Distance})"),
                    _ => false
                };

                return Task.FromResult(result);
            });

            if (GameData.Items[itemName]?.StackSize > 1)
                await Socket.Emit(ALSocketEmitType.Buy, new { name = itemName, quantity });
            else
                await Socket.Emit(ALSocketEmitType.Buy, new { name = itemName });

            return await source.Task.WithNetworkTimeout();
        }

        public async Task<IndexedItem> BuyFromPlayerAsync(string playerName, TradeSlot slot, string itemId, int quantity = 1)
        {
            var source = new TaskCompletionSource<Expectation<IndexedItem>>();

            if (!await Players.TryGetValueAsync(playerName, out var playerTask))
                throw new InvalidOperationException($"{playerName} is not near.");

            var player = await playerTask;

            if (!player.Slots.TryGetValue(slot.ToSlot(), out var slotItem))
                throw new InvalidOperationException($"{playerName} does not have an item in {slot}.");

            if (slotItem.Id != itemId)
                throw new InvalidOperationException($"Item in {slot} has different id than specified.");

            var existingItems = Character.Inventory.AsIndexed();
            var existingTotal = Character.Inventory.CountOf(slotItem.Name);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
            {
                var result = false;

                if (data.ResponseType == GameResponseType.TradeGetCloser)
                    result = source.TrySetResult($"Failed to buy {slotItem.Name} from {player.Name}. (get closer)");

                return Task.FromResult(result);
            });

            await using var gameLogCallback = Socket.On<string>(ALSocketMessageType.GameLog, data =>
            {
                var result = false;

                if (data.EqualsI("not enough gold"))
                    result = source.TrySetResult($"Failed to buy {slotItem.Name} from {player.Name}. (not enough gold)");
                else if (data.EqualsI("you can't buy that many"))
                    result = source.TrySetResult($"Failed to buy {slotItem.Name} from {player.Name}. (wrong quantity)");

                return Task.FromResult(result);
            });

            await using var disappearingTextCallback = Socket.On<DisappearingTextData>(
                ALSocketMessageType.DisappearingText, data =>
                {
                    var result = false;

                    if (data.Message.EqualsI("no space"))
                        result = source.TrySetResult($"Failed to buy {slotItem.Name} from {player.Name}. (no space)");

                    return Task.FromResult(result);
                });

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
            {
                var result = false;

                var newTotal = data.Inventory.Where(item => item.Name.EqualsI(slotItem.Name))
                    .Sum(item => item.Quantity);

                if (newTotal - existingTotal == quantity)
                    result = source.TrySetResult(data.Inventory.AsIndexed().Except(existingItems).FirstOrDefault());

                return Task.FromResult(result);
            });

            await Socket.Emit(ALSocketEmitType.TradeBuy,
                new { slot, id = playerName, rid = itemId, q = quantity.ToString() });

            return await source.Task.WithNetworkTimeout();
        }

        public async Task<IndexedItem> BuyFromPontyAsync(TradeItem item)
        {
            var source = new TaskCompletionSource<Expectation<IndexedItem>>();
            var existingCount = Character.Inventory.CountOf(item.Name);
            var existingItems = Character.Inventory.AsIndexed();

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
            {
                var result = false;

                if (data.ResponseType == GameResponseType.BuyCost)
                    result = source.TrySetResult($"Failed to buy {item.Name} from Ponty. (not enough gold)");

                return Task.FromResult(result);
            });
            
            await using var gameLogCallback = Socket.On<string>(ALSocketMessageType.GameLog, data =>
            {
                var result = false;

                if (data.EqualsI("item gone"))
                    result = source.TrySetResult($"Failed to buy {item.Name} from Ponty. (item gone)");

                return Task.FromResult(result);
            });
            
            await using var disappearingTextCallback = Socket.On<DisappearingTextData>(
                ALSocketMessageType.DisappearingText, data =>
                {
                    var result = false;

                    if (data.Message.EqualsI("no space"))
                        result = source.TrySetResult($"Failed to buy {item.Name} from Ponty. (no space)");

                    return Task.FromResult(result);
                });

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
            {
                var result = false;
                var newCount = data.Inventory.CountOf(item.Name);

                if (newCount - existingCount == item.Quantity)
                    result = source.TrySetResult(data.Inventory.AsIndexed().Except(existingItems).FirstOrDefault());

                return Task.FromResult(result);
            });

            await Socket.Emit(ALSocketEmitType.SecondHandsBuy, new { rid = item.Id });

            return await source.Task.WithNetworkTimeout();
        }

        public bool CanBuy(string itemName, bool fromPonty = false)
        {
            if (Character.Inventory.Count >= Character.InventorySize)
                return false;

            var data = GameData.Items[itemName];
            var price = data.Gold;

            if (fromPonty)
                price *= Definitions.CONSTANTS.PONTY_MARKUP;

            if (price > Character.Gold)
                return false;

            var hasComputer = Character.Inventory.ContainsItem("computer");


            return false;
        }
    }
}