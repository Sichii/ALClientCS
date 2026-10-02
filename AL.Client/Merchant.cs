#region
using System.Net;
using AL.APIClient.Definitions;
using AL.APIClient.Interfaces;
using AL.APIClient.Model;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient.Definitions;
using AL.SocketClient.Interfaces;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client;

/// <summary>
///     <inheritdoc cref="ALClient" />
///     <br />
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
    public Merchant(string characterName, IAlApiClient apiClient, IALSocketClient socketClient)
        : base(characterName, apiClient, socketClient) { }

    /// <summary>Asynchronously closes the merchant stand.</summary>
    public async Task CloseStandAsync()
    {
        if (Character.Stand == Stand.None)
            return;

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                if (data.Stand == Stand.None)
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Merchant,
            new
            {
                close = 1
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>Asynchronously starts Fishing.</summary>
    /// <remarks>
    ///     Returns once the server accepts the cast, which starts the channel rather than landing a fish. Fishing runs for 5
    ///     to 15 seconds and catches something one time in ten; the cooldown is taken then, not now.
    ///     <br />
    ///     The server does not restore its cooldown at login, so until the first state-changing action
    ///     <see cref="ALClient.Cooldowns" /> has no entry for it and it reads as ready.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Failed to use 'fishing'. ({reason})</exception>
    public Task FishingAsync()
        => UseSkillCoreAsync(
            "fishing",
            completion: SkillCompletion.ResponseData,
            getExtraFailureFunc: static data => data.ResponseType switch
            {
                GameResponseType.SkillCantWType => "wrong weapon type",

                //accepting and rejecting the cast use the same frame shape; only in_progress tells them apart
                GameResponseType.Data when "fishing".EqualsI(data.Place!) && !data.InProgress => "not in a fishing zone",
                _                                                                             => null
            });

    /// <summary>
    ///     Asynchronously uses MCourage, raising your defenses.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failed to use 'mcourage'. ({reason})</exception>
    public Task MCourageAsync() => UseSkillCoreAsync("mcourage");

    /// <summary>
    ///     Asynchronously uses MFrenzy, raising your attack speed sharply for a short time.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failed to use 'mfrenzy'. ({reason})</exception>
    public Task MFrenzyAsync() => UseSkillCoreAsync("mfrenzy");

    /// <summary>
    ///     Asynchronously uses MLuck on a target, luck-buffing them for a long duration.
    /// </summary>
    /// <param name="targetId">The id of the target.</param>
    /// <exception cref="ArgumentNullException">targetId</exception>
    /// <exception cref="InvalidOperationException">Failed to use 'mluck' on {targetId}. ({reason})</exception>
    public Task MLuckAsync(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync("mluck", targetId);
    }

    /// <summary>
    ///     Asynchronously uses MassExchange, buffing your next exchange to take half the time.
    /// </summary>
    /// <remarks>
    ///     A one-shot buff, spent by the next exchange and expiring 10 seconds after the cast either way.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Failed to use 'massexchange'. ({reason})</exception>
    public Task MassExchangeAsync() => UseSkillCoreAsync("massexchange");

    /// <summary>
    ///     Asynchronously uses MassExchangePP, buffing your next exchange to take a tenth of the time.
    /// </summary>
    /// <remarks>
    ///     A one-shot buff, spent by the next exchange and expiring 10 seconds after the cast either way. Applied after
    ///     <see cref="MassExchangeAsync" /> rather than instead of it, so with both up an exchange runs in a twentieth of its
    ///     time.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Failed to use 'massexchangepp'. ({reason})</exception>
    public Task MassExchangePPAsync() => UseSkillCoreAsync("massexchangepp");

    /// <summary>
    ///     Asynchronously uses MassProduction, buffing your next upgrade or compound to take half the time.
    /// </summary>
    /// <remarks>
    ///     A one-shot buff, spent by the next upgrade or compound and expiring 10 seconds after the cast either way.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Failed to use 'massproduction'. ({reason})</exception>
    public Task MassProductionAsync() => UseSkillCoreAsync("massproduction");

    /// <summary>
    ///     Asynchronously uses MassProductionPP, buffing your next upgrade or compound to take a tenth of the time.
    /// </summary>
    /// <remarks>
    ///     A one-shot buff, spent by the next upgrade or compound and expiring 10 seconds after the cast either way.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Failed to use 'massproductionpp'. ({reason})</exception>
    public Task MassProductionPPAsync() => UseSkillCoreAsync("massproductionpp");

    /// <summary>Asynchronously starts Mining.</summary>
    /// <remarks>
    ///     Returns once the server accepts the cast, which starts the channel rather than landing a strike. Mining runs for 5
    ///     to 15 seconds and yields something one time in five; the cooldown is taken then, not now.
    ///     <br />
    ///     The server does not restore its cooldown at login, so until the first state-changing action
    ///     <see cref="ALClient.Cooldowns" /> has no entry for it and it reads as ready.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Failed to use 'mining'. ({reason})</exception>
    public Task MiningAsync()
        => UseSkillCoreAsync(
            "mining",
            completion: SkillCompletion.ResponseData,
            getExtraFailureFunc: static data => data.ResponseType switch
            {
                GameResponseType.SkillCantWType => "wrong weapon type",

                //accepting and rejecting the cast use the same frame shape; only in_progress tells them apart
                GameResponseType.Data when "mining".EqualsI(data.Place!) && !data.InProgress => "not in a mining zone",
                _                                                                            => null
            });

    /// <summary>
    ///     Normalizes <paramref name="want" /> the way the server keeps it.
    /// </summary>
    /// <param name="want">What the offer asks for.</param>
    /// <returns>
    ///     The normalized want, or null for one the server refuses outright.
    /// </returns>
    /// <remarks>
    ///     The name must be a real item other than the upgrade placeholder, and a title must be a real title. A level is kept
    ///     only on an item that upgrades or compounds, capped at 12; a quantity only on one that stacks, capped at its stack
    ///     size.
    /// </remarks>
    /// <exception cref="ArgumentNullException">want</exception>
    public static TradeWant? NormalizeTradeWant(TradeWant want)
    {
        ArgumentNullException.ThrowIfNull(want);

        if (string.IsNullOrEmpty(want.Name) || (want.Name == "placeholder") || GameData.Items[want.Name] is not { } item)
            return null;

        if (!string.IsNullOrEmpty(want.Title) && GameData.Titles[want.Title] is null)
            return null;

        var levels = item.UpgradeModifiers is not null || item.CompoundModifiers is not null;

        return new TradeWant
        {
            Name = want.Name,
            Level = levels && want.Level is { } level and > 0 ? Math.Min(12, level) : null,
            Title = string.IsNullOrEmpty(want.Title) ? null : want.Title,
            Quantity = item.StackSize > 1 ? Math.Min(item.StackSize, Math.Max(1, want.Quantity ?? 1)) : null
        };
    }

    /// <summary>
    ///     Asynchronously opens the merchant stand, favoring a computer.
    /// </summary>
    /// <remarks>
    ///     A computer opens a <c>cstand</c>, which grants 24 trade slots below level 70 where a plain stand grants 16. Items
    ///     match by type, so a <c>supercomputer</c>, typed <c>computer</c>, counts too.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Failed to open stand. ({reason})</exception>
    public async Task OpenStandAsync()
    {
        if (Character.Stand != Stand.None)
            return;

        var stand = Character.Inventory.FindItem(item => item.GetData()
                                                             ?.Type
                                                         == ItemType.Computer)
                    ?? Character.Inventory.FindItem(item => item.GetData()
                                                                ?.Type
                                                            == ItemType.Stand);

        if (stand == null)
            throw new InvalidOperationException("Failed to open stand. (no stand)");

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                if (data.Stand != Stand.None)
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Merchant,
            new
            {
                num = stand.Index
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>Asynchronously posts a buy order for an item.</summary>
    /// <param name="itemName">The name of the item to post a buy order for.</param>
    /// <param name="itemLevel">The level of the item to buy.</param>
    /// <param name="tradeSlot">The slot to post the buy order.</param>
    /// <param name="price">The price per item the buy order is for.</param>
    /// <param name="quantity">The number of items to buy.</param>
    /// <exception cref="ArgumentNullException">itemName</exception>
    /// <exception cref="InvalidOperationException">
    ///     Failed to post item {itemName} to buy. ({reason})
    /// </exception>
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

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.SlotOccupied => source.TrySetResult($"Failed to post item {itemName} to buy. (slot occupied)"),

                    //fail_response defaults "place" to the name of the socket method that failed
                    _ when data.Failed && "trade_wishlist".EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to post item {itemName} to buy. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _ => false
                };

                return Task.FromResult(result);
            });

        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                var slotItem = data.Slots[tradeSlot.ToSlot()];

                if (slotItem is { Buying: true } && slotItem.Name.EqualsI(itemName))
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.TradeWishlist,
            new
            {
                q = quantity.ToString(),
                slot = tradeSlot,
                price = price.ToString(),
                level = itemLevel?.ToString() ?? "undefined",
                name = itemName
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>Asynchronously lists an item for sale.</summary>
    /// <param name="inventorySlot">The slot in the inventory of the item to list.</param>
    /// <param name="tradeSlot">The trade slot to list the item to.</param>
    /// <param name="price">The list price of the item.</param>
    /// <param name="quantity">The quantity of the item to sell.</param>
    /// <exception cref="InvalidOperationException">
    ///     Failed to list item {itemNameOrSlot} for sale. ({reason})
    /// </exception>
    public async Task PostSaleItemAsync(
        int inventorySlot,
        TradeSlot tradeSlot,
        long price,
        int quantity = 1)
    {
        var item = Character.Inventory[inventorySlot];

        if (item == null)
            throw new InvalidOperationException($"Failed to post item {inventorySlot} for sale. (slot empty)");

        if (price <= 0)
            throw new InvalidOperationException($"Failed to post item {item.Name} for sale. (invalid price)");

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameLogCallback = Socket.On<GameMessageData>(
            ALSocketMessageType.GameLog,
            data =>
            {
                var result = false;
                var message = data.Message;

                if (message.EqualsI("not enough"))
                    result = source.TrySetResult($"Failed to post item {item.Name} for sale. (not enough)");

                return Task.FromResult(result);
            });

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.SlotOccupied => source.TrySetResult(
                        $"Failed to list item {item.Name} for sale. (trade slot occupied)"),

                    //fail_response defaults "place" to the name of the socket method that failed
                    _ when data.Failed && "equip".EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to list item {item.Name} for sale. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _ => false
                };

                return Task.FromResult(result);
            });

        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                var inventoryItem = data.Inventory[inventorySlot];

                if ((inventoryItem == null) || (inventoryItem.Quantity == (item.Quantity - quantity)))
                {
                    var slotItem = data.Slots[tradeSlot.ToSlot()];

                    if ((slotItem != null) && slotItem.Name.EqualsI(item.Name))
                        source.TrySetResult(Expectation.Success);
                }

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Equip,
            new
            {
                num = inventorySlot,
                q = quantity,
                slot = tradeSlot,
                price
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously offers an item on the stand in exchange for another item rather than for gold.
    /// </summary>
    /// <param name="inventorySlot">The slot in the inventory of the item to offer.</param>
    /// <param name="tradeSlot">The trade slot to offer it in.</param>
    /// <param name="want">
    ///     What the offer asks for. Normalized first by <see cref="NormalizeTradeWant" />, the way the server reads it.
    /// </param>
    /// <param name="quantity">How many of a stack to offer.</param>
    /// <exception cref="InvalidOperationException">
    ///     Failed to offer item {itemNameOrSlot} for trade. ({reason})
    /// </exception>
    public async Task PostTradeOfferAsync(
        int inventorySlot,
        TradeSlot tradeSlot,
        TradeWant want,
        int quantity = 1)
    {
        var item = Character.Inventory[inventorySlot];

        if (item == null)
            throw new InvalidOperationException($"Failed to offer item {inventorySlot} for trade. (slot empty)");

        if (NormalizeTradeWant(want) is not { } normalized)
            throw new InvalidOperationException($"Failed to offer item {item.Name} for trade. (cannot ask for {want.Name})");

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.SlotOccupied => source.TrySetResult(
                        $"Failed to offer item {item.Name} for trade. (trade slot occupied)"),

                    //fail_response defaults "place" to the name of the socket method that failed
                    _ when data.Failed && "equip".EqualsI(data.Place!) => source.TrySetResult(
                        $"Failed to offer item {item.Name} for trade. ({data.Reason ?? data.ResponseType.ToString()})"),
                    _ => false
                };

                return Task.FromResult(result);
            });

        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                var slotItem = data.Slots[tradeSlot.ToSlot()];

                if (slotItem is { Want: not null } && slotItem.Name.EqualsI(item.Name))
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Equip,
            new
            {
                num = inventorySlot,
                q = quantity,
                slot = tradeSlot,
                want = normalized
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously creates a Merchant client and connects.
    ///     <br />
    /// </summary>
    /// <param name="characterName">The name of the character to log in as.</param>
    /// <param name="region">The region to log into.</param>
    /// <param name="identifier">The identifier suffic for the region.</param>
    /// <param name="apiClient">
    ///     An <see cref="IAlApiClient" /> with your authorization credentials.
    /// </param>
    /// <param name="proxy">
    ///     The proxy to reach the game through, or null for the machine's own connection.
    /// </param>
    /// <returns>
    ///     <see cref="Merchant" />
    /// </returns>
    /// <exception cref="ArgumentNullException">characterName</exception>
    /// <exception cref="ArgumentNullException">apiClient</exception>
    public static Task<Merchant> StartAsync(
        string characterName,
        ServerRegion region,
        ServerId identifier,
        IAlApiClient apiClient,
        IWebProxy? proxy = null)
        => StartClientAsync(
            characterName,
            region,
            identifier,
            apiClient,
            static (name, api, socket) => new Merchant(name, api, socket),
            proxy);

    /// <summary>
    ///     Asynchronously throws an item from your inventory at a target.
    /// </summary>
    /// <param name="targetId">The id of the target.</param>
    /// <param name="inventorySlot">The inventory slot holding the item to throw.</param>
    /// <remarks>
    ///     The item is consumed. Throwing an item the server considers harmful at another player fails outside of pvp.
    /// </remarks>
    /// <exception cref="ArgumentNullException">targetId</exception>
    /// <exception cref="InvalidOperationException">Failed to use 'throw' on {targetId}. ({reason})</exception>
    public Task ThrowAsync(string targetId, int inventorySlot)
    {
        if (string.IsNullOrEmpty(targetId))
            throw new ArgumentNullException(nameof(targetId));

        return UseSkillCoreAsync(
            "throw",
            targetId,
            payload: new
            {
                name = "throw",
                id = targetId,
                num = inventorySlot
            });
    }

    /// <summary>Asynchronously unposts a trade item.</summary>
    /// <param name="tradeSlot">The trade slot of the item to unpost.</param>
    /// <returns>
    ///     <see cref="InventoryIndexer" />
    ///     <br />
    ///     If the unposted item was an item for sale, this will return information about that item within the inventory.
    ///     <br />
    ///     Otherwise this will return null
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Failed to unpost trade item {itemNameOrSlot}. ({reason})
    /// </exception>
    public async Task<InventoryIndexer?> UnpostItemAsync(TradeSlot tradeSlot)
    {
        var slot = tradeSlot.ToSlot();
        var tradeItem = Character.Slots[slot];

        if (tradeItem == null)
            throw new InvalidOperationException($"Failed to unpost trade item {tradeSlot}. (slot empty)");

        if (!tradeItem.Buying && (Character.EmptySlots <= 0))
            throw new InvalidOperationException($"Failed to unpost trade item {tradeItem.Name}. (no space)");

        var source = new TaskCompletionSource<Expectation<InventoryIndexer?>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var previousInventory = tradeItem.Buying ? null : Character.Inventory.AsIndexed();

        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                var slotItem = data.Slots[slot];

                if (slotItem == null)
                {
                    if (tradeItem.Buying)
                    {
                        source.TrySetResult(default(InventoryIndexer?));

                        return TaskCache.FALSE;
                    }

                    var inventoryItem = data.Inventory
                                            .AsIndexed()
                                            .Except(previousInventory!)
                                            .FirstOrDefault(indexed
                                                => indexed.Item.Name.EqualsI(tradeItem.Name) && (indexed.Item.Level == tradeItem.Level));

                    if (inventoryItem != null)
                        source.TrySetResult(inventoryItem);
                }

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Unequip,
            new
            {
                slot
            });

        return await source.Task.WithNetworkTimeout();
    }

    //TODO: Throw Stuff... but i dont think anyone will ever use it so it's extremely low priority
}