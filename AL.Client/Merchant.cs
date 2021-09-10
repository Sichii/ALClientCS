using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
using Chaos.Core.Extensions;
using Common.Logging;

namespace AL.Client
{
    /// <summary>
    ///     <inheritdoc cref="ALClient" /> <br />
    ///     Contains merchant specific functionality.
    /// </summary>
    public class Merchant : ALClient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Merchant" /> class.
        /// </summary>
        /// <param name="characterName">The name of the merchant.</param>
        /// <param name="apiClient">An API client implementation.</param>
        /// <param name="socketClient">A socket client implementation.</param>
        /// <exception cref="ArgumentNullException">characterName</exception>
        /// <exception cref="ArgumentNullException">apiClient</exception>
        /// <exception cref="ArgumentNullException">socketClient</exception>
        public Merchant(string characterName, IALAPIClient apiClient, IALSocketClient socketClient)
            : base(characterName, apiClient, socketClient) { }

        /// <summary>
        ///     Asynchronously closes the merchant stand.
        /// </summary>
        public async Task CloseStandAsync()
        {
            if (Character.Stand == Stand.None)
                return;

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
                {
                    if (data.Stand == Stand.None)
                        source.TrySetResult(Expectation.Success);

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Merchant, new { close = 1 }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses Fishing. <br />
        ///     <b>USEABLE BUT INCOMPLETE, I don't own a fishing rod lmaokai</b>
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'fishing'. ({reason})</exception>
        //TODO: complete fishing callbacks
        public async Task FishingAsync()
        {
            const string SKILL_NAME = "fishing";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (wrong weapon type)"),
                        _                               => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            /*
            await using var uiDataCallback = Socket.On<UIData>(ALSocketMessageType.UI, data =>
            {
                var result = data.UIDataType switch
                {
                    UIDataType.FishingFail => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (failed)"),
                    UIDataType.FishingNone => source.TrySetResult(default),
                    UIDataType.FishingStart => source.TrySetResult(default)
                }
            })
            */

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses MassProduction.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'massproduction'. ({reason})</exception>
        public async Task MassProductionAsync()
        {
            const string SKILL_NAME = "massproduction";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillSuccess when data.Name.EqualsI(SKILL_NAME) => source.TrySetResult(Expectation.Success),
                        _ => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses MassProductionPP.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'massproductionpp'. ({reason})</exception>
        public async Task MassProductionPPAsync()
        {
            const string SKILL_NAME = "massproductionpp";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillSuccess when data.Name.EqualsI(SKILL_NAME) => source.TrySetResult(Expectation.Success),
                        _ => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses Mining. <br />
        ///     <b>USEABLE BUT INCOMPLETE, I don't own a pickaxe lmaokai</b>
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to use 'mining'. ({reason})</exception>
        //TODO: complete mining callbacks
        public async Task MiningAsync()
        {
            const string SKILL_NAME = "mining";

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (disabled)"),
                        GameResponseType.Cooldown when data.Place.EqualsI(SKILL_NAME) => source.TrySetResult(
                            $"Failed to use '{SKILL_NAME}'. (on cooldown)"),
                        GameResponseType.NoLevel        => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (level too low)"),
                        GameResponseType.NoMP           => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (no mp)"),
                        GameResponseType.SkillCantWType => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (wrong weapon type)"),
                        _                               => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            /*
            await using var uiDataCallback = Socket.On<UIData>(ALSocketMessageType.UI, data =>
            {
                var result = data.UIDataType switch
                {
                    UIDataType.MiningFail => source.TrySetResult($"Failed to use '{SKILL_NAME}'. (failed)"),
                    UIDataType.MiningNone => source.TrySetResult(default),
                    UIDataType.MiningStart => source.TrySetResult(default)
                }
            })
            */

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously uses MLuck on a target.
        /// </summary>
        /// <param name="targetId">The id of the target.</param>
        /// <exception cref="ArgumentNullException">targetId</exception>
        /// <exception cref="InvalidOperationException">Failed to use 'mluck' on {targetId}. ({reason})</exception>
        public async Task MLuckAsync(string targetId)
        {
            const string SKILL_NAME = "mluck";

            if (string.IsNullOrEmpty(targetId))
                throw new ArgumentNullException(nameof(targetId));

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.Disabled => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (disabled)"),
                        GameResponseType.Cooldown when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (on cooldown)"),
                        GameResponseType.NoLevel => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (level too low)"),
                        GameResponseType.TooFar when data.TargetID.EqualsI(targetId) && data.Place.EqualsI(SKILL_NAME) => source
                            .TrySetResult(
                                $"Failed to use '{SKILL_NAME}' on {targetId}. (too far)"),
                        GameResponseType.NoMP => source.TrySetResult($"Failed to use '{SKILL_NAME}' on {targetId}. (no mp)"),
                        _                     => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var evalCallback = Socket.On<EvalData>(ALSocketMessageType.Eval, data =>
                {
                    Match match;

                    if (!string.IsNullOrEmpty(data.Code)
                        && (match = RegexCache.SKILL_TIMEOUT.Match(data.Code)).Success
                        && match.Groups[1].Value.EqualsI(SKILL_NAME))
                        return Task.FromResult(source.TrySetResult(Expectation.Success));

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Skill, new { name = SKILL_NAME, id = targetId }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously opens the merchant stand, favoring the computer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to open stand. ({reason})</exception>
        public async Task OpenStandAsync()
        {
            if (Character.Stand != Stand.None)
                return;

            var stand = Character.Inventory.FindItem("computer")
                        ?? Character.Inventory.FindItem(item => item.GetData()?.Type == ItemType.Stand);

            if (stand == null)
                throw new InvalidOperationException("Failed to open stand. (no stand)");

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
                {
                    if (data.Stand != Stand.None)
                        source.TrySetResult(Expectation.Success);

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Merchant, new { num = stand.Index }).ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously posts a buy order for an item.
        /// </summary>
        /// <param name="itemName">The name of the item to post a buy order for.</param>
        /// <param name="itemLevel">The level of the item to buy.</param>
        /// <param name="tradeSlot">The slot to post the buy order.</param>
        /// <param name="price">The price per item the buy order is for.</param>
        /// <param name="quantity">The number of items to buy.</param>
        /// <exception cref="ArgumentNullException">itemName</exception>
        /// <exception cref="InvalidOperationException">Failed to post item {itemName} to buy. ({reason})</exception>
        public async Task PostBuyOrderAsync(
            string itemName,
            int? itemLevel,
            TradeSlot tradeSlot,
            long price,
            int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemName))
                throw new ArgumentNullException(nameof(itemName));

            var itemData = GameData.Items[itemName];

            if (itemData == null)
                throw new InvalidOperationException($"Failed to post item {itemName} to buy. (not a valid name)");

            if (price <= 0)
                throw new InvalidOperationException($"Failed to post item {itemName} to buy. (invalid price)");

            if (quantity <= 0)
                throw new InvalidOperationException($"Failed to post item {itemName} to buy. (invalid quantity)");

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.SlotOccupied => source.TrySetResult($"Failed to post item {itemName} to buy. (slot occupied)"),
                        _                             => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
                {
                    var slotItem = data.Slots[tradeSlot.ToSlot()];

                    if (slotItem is { Buying: true } && slotItem.Name.EqualsI(itemName))
                        source.TrySetResult(Expectation.Success);

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.TradeWishlist, new
                {
                    q = quantity.ToString(),
                    slot = tradeSlot,
                    price = price.ToString(),
                    level = itemLevel?.ToString() ?? "undefined",
                    name = itemName
                })
                .ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously lists an item for sale.
        /// </summary>
        /// <param name="inventorySlot">The slot in the inventory of the item to list.</param>
        /// <param name="tradeSlot">The trade slot to list the item to.</param>
        /// <param name="price">The list price of the item.</param>
        /// <param name="quantity">The quantity of the item to sell.</param>
        /// <exception cref="InvalidOperationException">Failed to list item {itemNameOrSlot} for sale. ({reason})</exception>
        public async Task PostSaleItemAsync(int inventorySlot, TradeSlot tradeSlot, long price, int quantity = 1)
        {
            var item = Character.Inventory[inventorySlot];

            if (item == null)
                throw new InvalidOperationException($"Failed to post item {inventorySlot} for sale. (slot empty)");

            if (price <= 0)
                throw new InvalidOperationException($"Failed to post item {item.Name} for sale. (invalid price)");

            var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var gameLogCallback = Socket.On<string>(ALSocketMessageType.GameLog, data =>
                {
                    var result = false;

                    if (data.EqualsI("not enough"))
                        result = source.TrySetResult($"Failed to post item {item.Name} for sale. (not enough)");

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var gameResponseCallback = Socket.On<GameResponseData>(ALSocketMessageType.GameResponse, data =>
                {
                    var result = data.ResponseType switch
                    {
                        GameResponseType.SlotOccupied =>
                            source.TrySetResult($"Failed to list item {item.Name} for sale. (trade slot occupied)"),
                        _ => false
                    };

                    return Task.FromResult(result);
                })
                .ConfigureAwait(false);

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
                {
                    var inventoryItem = data.Inventory[inventorySlot];

                    if ((inventoryItem == null) || (inventoryItem.Quantity == item.Quantity - quantity))
                    {
                        var slotItem = data.Slots[tradeSlot.ToSlot()];

                        if ((slotItem != null) && slotItem.Name.EqualsI(item.Name))
                            source.TrySetResult(Expectation.Success);
                    }

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Equip, new { num = inventorySlot, q = quantity, slot = tradeSlot, price })
                .ConfigureAwait(false);

            var expectation = await source.Task.WithNetworkTimeout().ConfigureAwait(false);
            expectation.ThrowIfUnsuccessful();
        }

        /// <summary>
        ///     Asynchronously creates a Merchant client and connects. <br />
        /// </summary>
        /// <param name="characterName">The name of the character to log in as.</param>
        /// <param name="region">The region to log into.</param>
        /// <param name="identifier">The identifier suffic for the region.</param>
        /// <param name="apiClient">An <see cref="IALAPIClient" /> with your authorization credentials.</param>
        /// <returns>
        ///     <see cref="Merchant" />
        /// </returns>
        /// <exception cref="ArgumentNullException">characterName</exception>
        /// <exception cref="ArgumentNullException">apiClient</exception>
        public static async Task<Merchant> StartAsync(
            string characterName,
            ServerRegion region,
            ServerId identifier,
            IALAPIClient apiClient)
        {
            if (string.IsNullOrEmpty(characterName))
                throw new ArgumentNullException(nameof(characterName));

            if (apiClient == null)
                throw new ArgumentNullException(nameof(apiClient));

            var logger = new FormattedLogger(characterName, LogManager.GetLogger<ALSocketClient>());
            var socketClient = new ALSocketClient(logger);

            var client = new Merchant(characterName, apiClient, socketClient);
            await client.ConnectAsync(region, identifier).ConfigureAwait(false);

            return client;
        }

        /// <summary>
        ///     Asynchronously unposts a trade item.
        /// </summary>
        /// <param name="tradeSlot">The trade slot of the item to unpost.</param>
        /// <returns>
        ///     <see cref="IndexedInventoryItem" /> <br />
        ///     If the unposted item was an item for sale, this will return information about that item within the inventory.
        ///     <br />
        ///     Otherwise this will return null
        /// </returns>
        /// <exception cref="InvalidOperationException">Failed to unpost trade item {itemNameOrSlot}. ({reason})</exception>
        public async Task<IndexedInventoryItem?> UnpostItemAsync(TradeSlot tradeSlot)
        {
            var slot = tradeSlot.ToSlot();
            var tradeItem = Character.Slots[slot];

            if (tradeItem == null)
                throw new InvalidOperationException($"Failed to unpost trade item {tradeSlot}. (slot empty)");

            if (!tradeItem.Buying && (Character.EmptySlots == 0))
                throw new InvalidOperationException($"Failed to unpost trade item {tradeItem.Name}. (no space)");

            var source = new TaskCompletionSource<Expectation<IndexedInventoryItem?>>(TaskCreationOptions.RunContinuationsAsynchronously);
            var previousInventory = tradeItem.Buying ? null : Character.Inventory.AsIndexed();

            await using var characterCallback = Socket.On<CharacterData>(ALSocketMessageType.Character, data =>
                {
                    var slotItem = data.Slots[slot];

                    if (slotItem == null)
                    {
                        if (tradeItem.Buying)
                        {
                            source.TrySetResult(default(IndexedInventoryItem?));

                            return TaskCache.FALSE;
                        }

                        var inventoryItem = data.Inventory.AsIndexed()
                            .Except(previousInventory!)
                            .FirstOrDefault(indexed =>
                                indexed.Item.Name.EqualsI(tradeItem.Name) && (indexed.Item.Level == tradeItem.Level));

                        if (inventoryItem != null)
                            source.TrySetResult(inventoryItem);
                    }

                    return TaskCache.FALSE;
                })
                .ConfigureAwait(false);

            await Socket.EmitAsync(ALSocketEmitType.Unequip, new { slot }).ConfigureAwait(false);

            return await source.Task.WithNetworkTimeout().ConfigureAwait(false);
        }

        //TODO: Throw Stuff... but i dont think anyone will ever use it so it's extremely low priority
    }
}