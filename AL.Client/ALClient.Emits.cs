#region
using System.Runtime.CompilerServices;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Data;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client;

/// <summary>
///     Provides thin senders for server handlers with no richer wrapper of their own. Most emit the action only; any
///     confirmation arrives as a separate inbound event. <see cref="Slot" /> and <see cref="TradeSlot" /> serialize
///     lowercase through their own converters, so they go straight into a payload.
/// </summary>
public abstract partial class ALClient
{
    #region Misc
    /// <summary>Asynchronously places a tavern bet.</summary>
    /// <param name="type">
    ///     The game to bet on, such as <c>roulette</c> or <c>dice</c>.
    /// </param>
    /// <param name="gold">The stake.</param>
    /// <param name="odds">
    ///     The odds to bet at, for the games that take them.
    /// </param>
    /// <exception cref="ArgumentNullException">type</exception>
    public Task BetAsync(string type, long gold, string? odds = null)
    {
        ArgumentNullException.ThrowIfNull(type);

        return Socket.EmitAsync(
            ALSocketEmitType.Bet,
            new
            {
                type,
                gold,
                odds
            });
    }

    /// <summary>
    ///     Asynchronously places a dice bet on <paramref name="number" />. Only works on the tavern map.
    /// </summary>
    /// <param name="gold">
    ///     The stake. The server raises it to at least 10,000 and takes it at placement.
    /// </param>
    /// <param name="number">
    ///     The number to bet against, clamped by the server to 0.01-99.99.
    /// </param>
    /// <param name="up">Specifies whether a roll above the number wins.</param>
    /// <remarks>
    ///     A second bet while one is unresolved is refused with <c>tavern_dice_exist</c>, and one outside the betting window
    ///     with <c>tavern_not_yet</c> or <c>tavern_too_late</c>. Leaving the tavern with a bet unresolved refunds it in full.
    /// </remarks>
    public Task BetDiceAsync(long gold, float number, bool up)
        => Socket.EmitAsync(
            ALSocketEmitType.Bet,
            new
            {
                type = "dice",
                gold,
                num = number,
                dir = up ? "up" : "down"
            });

    /// <summary>
    ///     Asynchronously stakes <paramref name="gold" /> on one side of the tavern's wheel and waits for it to settle.
    /// </summary>
    /// <param name="gold">
    ///     The stake, at least <c>GameData.Games.Wheel.Min</c>.
    /// </param>
    /// <param name="side">
    ///     One of <c>GameData.Games.Wheel.Sides</c>, <c>sun</c> or <c>moon</c>.
    /// </param>
    /// <returns>The server's settlement.</returns>
    /// <remarks>
    ///     The payload field carrying the side is inferred from the game data's own <c>sides</c> key; no published handler
    ///     confirms it. If the wheel answers <c>invalid</c> or settles against the wrong side, that field is the one to fix.
    /// </remarks>
    /// <exception cref="ArgumentNullException">side</exception>
    public Task<GameResponseData> BetWheelAsync(long gold, string side)
    {
        ArgumentNullException.ThrowIfNull(side);

        return PlaceWagerAsync(
            "wheel",
            new Dictionary<string, object?>
            {
                ["gold"] = gold,
                ["side"] = side
            });
    }

    /// <summary>
    ///     Asynchronously pulls the tavern's slots machine and waits for the reels to settle. The price is fixed at
    ///     <c>GameData.Games.Slots.Gold</c> and taken whatever the outcome.
    /// </summary>
    /// <returns>
    ///     The server's settlement, answered <c>slots_success</c> or <c>slots_fail</c>.
    /// </returns>
    /// <remarks>
    ///     The settlement rides <see cref="GameResponseData.Won" />, <see cref="GameResponseData.Cost" />,
    ///     <see cref="GameResponseData.Payout" /> and <see cref="GameResponseData.Net" />. A second pull while one is still
    ///     spinning is refused with <c>in_progress</c> rather than queued.
    /// </remarks>
    public Task<GameResponseData> PlaySlotsAsync() => PlaceWagerAsync("slots");

    /// <summary>
    ///     The time to wait for a wager to settle. Both machines settle on a timer after their spin, and the game's own script
    ///     functions allow this long.
    /// </summary>
    private const int WAGER_TIMEOUT_MS = 10_000;

    /// <summary>
    ///     Asynchronously places a wager, correlated on the request id alone, since a wheel reply's <c>place</c> cannot be
    ///     predicted.
    /// </summary>
    /// <param name="game">The game to wager on.</param>
    /// <param name="fields">The game's own payload fields, if it takes any.</param>
    /// <returns>The server's settlement.</returns>
    private async Task<GameResponseData> PlaceWagerAsync(string game, Dictionary<string, object?>? fields = null)
    {
        var requestId = RequestId.Create();
        var source = new TaskCompletionSource<Expectation<GameResponseData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                if (!requestId.EqualsI(data.RequestId!))
                    return TaskCache.FALSE;

                var result = data.Failed
                    ? source.TrySetResult($"The {game} wager was refused. ({data.Reason ?? data.ResponseType.ToString()})")
                    : source.TrySetResult(data);

                return Task.FromResult(result);
            });

        var payload = new Dictionary<string, object?>(fields ?? [])
        {
            ["type"] = game,
            ["request_id"] = requestId
        };

        await Socket.EmitAsync(ALSocketEmitType.Bet, payload);

        return await source.Task.WithTimeout(WAGER_TIMEOUT_MS);
    }

    /// <summary>
    ///     Asynchronously asks the tavern for its current house rules.
    /// </summary>
    /// <returns>
    ///     The house rules, with <see cref="TavernData.Event" /> set to <c>info</c> and only <see cref="TavernData.Edge" />
    ///     and <see cref="TavernData.Max" /> filled.
    /// </returns>
    /// <remarks>
    ///     <see cref="TavernData.Max" /> is the house's free bankroll, which falls while other players hold large bets open,
    ///     so ask again per bet rather than caching it.
    ///     <br />
    ///     The handler answers anywhere, but sends nothing at all when the tavern instance never started.
    /// </remarks>
    /// <exception cref="TimeoutException">The server never answered.</exception>
    public async Task<TavernData> RequestTavernInfoAsync()
    {
        var requestId = RequestId.Create();
        var source = new TaskCompletionSource<Expectation<TavernData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        //gated on the success code as well as the token, so a refusal never resolves as an all-zero reading
        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data => Task.FromResult(
                (data.ResponseType == GameResponseType.Data)
                && (data.RequestId != null)
                && requestId.EqualsI(data.RequestId)
                && source.TrySetResult(
                    new TavernData
                    {
                        Event = "info",
                        Edge = data.Edge,
                        Max = data.MaxPayout
                    })));

        await Socket.EmitAsync(
            ALSocketEmitType.Tavern,
            new
            {
                @event = "info",
                request_id = requestId
            });

        return (await source.Task.WithNetworkTimeout()).Result;
    }

    /// <summary>
    ///     Asynchronously sets an upper cap on this character's movement speed.
    /// </summary>
    /// <param name="speed">The cap, or zero to clear it.</param>
    /// <remarks>
    ///     The server applies it as <c>min(speed, cruise || 200000)</c> after every other modifier and keeps it until changed,
    ///     so it can only slow the character and has to be cleared with zero. It costs 10 call units; a move costs 1.5.
    /// </remarks>
    public Task CruiseAsync(int speed) => Socket.EmitAsync(ALSocketEmitType.Cruise, speed);

    /// <summary>
    ///     Asynchronously kills this character outright, with the normal death penalty. Refused while already dead.
    /// </summary>
    public Task HarakiriAsync() => Socket.EmitAsync(ALSocketEmitType.Harakiri);

    /// <summary>
    ///     Asynchronously signs this character up at Bean, answered <c>signed_up</c>. Distinct from joining a giveaway.
    /// </summary>
    public Task SignUpAsync() => Socket.EmitAsync(ALSocketEmitType.Signup);
    #endregion

    #region Chat
    /// <summary>Asynchronously sends a public chat message.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="code">
    ///     Flags the message as code-manager output, which the server rate-limits to once per 15 seconds.
    /// </param>
    /// <exception cref="ArgumentNullException">message</exception>
    public Task SayAsync(string message, int? code = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        return Socket.EmitAsync(
            ALSocketEmitType.Say,
            new
            {
                message,
                code
            });
    }

    /// <summary>
    ///     Asynchronously sends a message to the character's party chat.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <exception cref="ArgumentNullException">message</exception>
    public Task SayToPartyAsync(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return Socket.EmitAsync(
            ALSocketEmitType.Say,
            new
            {
                message,
                party = true
            });
    }

    /// <summary>
    ///     Asynchronously sends a private message to a named character.
    /// </summary>
    /// <param name="name">The name of the character.</param>
    /// <param name="message">The message to send.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    /// <exception cref="ArgumentNullException">message</exception>
    public Task WhisperAsync(string name, string message)
    {
        ArgumentNullException.ThrowIfNull(name);

        ArgumentNullException.ThrowIfNull(message);

        return Socket.EmitAsync(
            ALSocketEmitType.Say,
            new
            {
                message,
                name
            });
    }

    /// <summary>
    ///     Asynchronously sends a code-manager message to one or more characters, the channel bots use to coordinate a party.
    /// </summary>
    /// <param name="to">The names of the characters.</param>
    /// <param name="message">The message to send.</param>
    /// <exception cref="ArgumentNullException">to</exception>
    /// <exception cref="ArgumentNullException">message</exception>
    public Task SendCmAsync(IEnumerable<string> to, object message)
    {
        ArgumentNullException.ThrowIfNull(to);

        ArgumentNullException.ThrowIfNull(message);

        return Socket.EmitAsync(
            ALSocketEmitType.Command,
            new
            {
                to = to.ToArray(),
                message
            });
    }

    /// <summary>Asynchronously plays an unlocked emotion.</summary>
    /// <param name="name">The name of the emotion.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public Task UseEmotionAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Socket.EmitAsync(
            ALSocketEmitType.Emotion,
            new
            {
                name
            });
    }
    #endregion

    #region Movement / instances
    /// <summary>
    ///     Asynchronously enters instanced content such as a crypt, a tomb or duelland. This is the only path that creates an
    ///     instance; <see cref="TransportAsync" /> covers static doors only.
    /// </summary>
    /// <param name="place">The instance to enter.</param>
    /// <param name="instanceName">The name of an existing copy to join, if any.</param>
    /// <exception cref="ArgumentNullException">place</exception>
    public Task EnterAsync(string place, string? instanceName = null)
    {
        ArgumentNullException.ThrowIfNull(place);

        return Socket.EmitAsync(
            ALSocketEmitType.Enter,
            new
            {
                place,
                name = instanceName
            });
    }

    /// <summary>
    ///     Asynchronously joins ongoing event content such as goobrawl, crabxx or an arena.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <exception cref="ArgumentNullException">eventName</exception>
    public Task JoinEventAsync(string eventName)
    {
        ArgumentNullException.ThrowIfNull(eventName);

        return Socket.EmitAsync(
            ALSocketEmitType.Join,
            new
            {
                name = eventName
            });
    }

    /// <summary>
    ///     Asynchronously sets this character's home to the current server. The game refuses another change for 36 hours.
    /// </summary>
    /// <returns>
    ///     <see langword="null" /> when <c>home_set</c> landed and <see cref="Home" /> is updated; otherwise the hours still
    ///     to wait, from <c>sh_time</c>.
    /// </returns>
    public async Task<float?> SetHomeAsync()
    {
        var source = new TaskCompletionSource<Expectation<float?>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                switch (data.ResponseType)
                {
                    case GameResponseType.HomeSet:
                        if (data.Home is { } home)
                            Home = home;

                        source.TrySetResult((float?)null);

                        break;
                    case GameResponseType.SetHomeCooldown:
                        source.TrySetResult(data.Hours);

                        break;
                }

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(ALSocketEmitType.SetHome);

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();

        return expectation.Result;
    }

    /// <summary>
    ///     Asynchronously triggers a seasonal map-object interaction, such as <c>newyear_tree</c> or <c>redorb</c>.
    /// </summary>
    /// <param name="type">The type of the interaction.</param>
    /// <exception cref="ArgumentNullException">type</exception>
    public Task InteractionAsync(string type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return Socket.EmitAsync(
            ALSocketEmitType.Interaction,
            new
            {
                type
            });
    }
    #endregion

    #region Inventory
    /// <summary>
    ///     Asynchronously equips up to 15 items in a single call.
    /// </summary>
    /// <param name="equips">
    ///     Each inventory slot to equip from, and the slot to equip into, or null for the item's default slot.
    /// </param>
    /// <remarks>
    ///     The server applies the entries in order and stops at the first one it refuses, keeping what it applied before that.
    ///     Its answer is a <c>success_response</c> either way; only <see cref="GameResponseData.EquipBatchEntries" /> tells a
    ///     refusal apart.
    /// </remarks>
    /// <exception cref="ArgumentNullException">equips</exception>
    /// <exception cref="InvalidOperationException">Failed to equip {count} items. ({reason})</exception>
    public async Task EquipBatchAsync(IEnumerable<(int InventorySlot, Slot? Slot)> equips)
    {
        ArgumentNullException.ThrowIfNull(equips);

        var batch = equips.ToArray();

        if (batch.Length == 0)
            return;

        var payload = batch.Select(equip => new
                           {
                               num = equip.InventorySlot,
                               slot = equip.Slot
                           })
                           .ToArray();

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                //the literal is the receiver because EqualsI throws on a null receiver
                if (!"equip_batch".EqualsI(data.Place!) || !IsEquipBatchAnswer(data.EquipBatchEntries, batch))
                    return TaskCache.FALSE;

                var answered = data.EquipBatchEntries!;
                var last = answered[^1];

                if (last.ContainsData)
                    source.TrySetResult(Expectation.Success);
                else
                    source.TrySetResult(
                        $"Failed to equip {batch.Length} items. (the server refused entry {answered.Length} - {last.Refusal})");

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(ALSocketEmitType.EquipBatch, payload);
        ChargeBatch(batch.Length);

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Determines whether <paramref name="answered" /> answers <paramref name="batch" /> rather than another batch in
    ///     flight, judged on the inventory slots the server echoed back.
    /// </summary>
    /// <param name="answered">The entries the server answered.</param>
    /// <param name="batch">The batch that was sent.</param>
    /// <returns>
    ///     true if the answer belongs to the batch; otherwise, false.
    /// </returns>
    /// <remarks>
    ///     A batch refused on its first entry echoes no slot, so two in flight can both claim that one answer.
    /// </remarks>
    internal static bool IsEquipBatchAnswer(EquipBatchEntry[]? answered, (int InventorySlot, Slot? Slot)[] batch)
    {
        if (answered is not { Length: > 0 } || (answered.Length > batch.Length))
            return false;

        for (var i = 0; i < answered.Length; i++)
            if (answered[i].ContainsData && (answered[i].InventorySlot != batch[i].InventorySlot))
                return false;

        //the server stops where it refuses, so an answer short of the whole batch has to end in a refusal
        return (answered.Length == batch.Length) || !answered[^1].ContainsData;
    }

    /// <summary>
    ///     Tops up an equip batch's charge to its real length, since the emit hook charged a batch of one. The server prices
    ///     by length before the handler runs, so a refused batch is billed the same.
    /// </summary>
    /// <param name="count">The number of entries in the batch.</param>
    private void ChargeBatch(int count)
        => CallMeter.Charge(
            ALSocketEmitType.EquipBatch,
            CallCost.CalculateEquipBatchCost(count) - CallCost.CalculateCost(ALSocketEmitType.EquipBatch));

    /// <summary>
    ///     Asynchronously splits a stackable item, moving <paramref name="quantity" /> into a new inventory slot.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the stack.</param>
    /// <param name="quantity">The number of items to move.</param>
    public Task SplitAsync(int inventorySlot, int quantity)
        => Socket.EmitAsync(
            ALSocketEmitType.Split,
            new
            {
                num = inventorySlot,
                quantity
            });

    /// <summary>
    ///     Asynchronously destroys an item, or part of a stack, for good.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the item.</param>
    /// <param name="quantity">
    ///     The number of items to destroy, or null for the whole stack.
    /// </param>
    public Task DestroyAsync(int inventorySlot, int? quantity = null)
        => Socket.EmitAsync(
            ALSocketEmitType.Destroy,
            new
            {
                num = inventorySlot,
                q = quantity
            });

    /// <summary>
    ///     Asynchronously buys and exchanges a token or quest item in one step.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the item.</param>
    /// <param name="name">The name of the item to buy.</param>
    /// <param name="quantity">
    ///     The current stack size, checked by the server as a safeguard.
    /// </param>
    /// <exception cref="ArgumentNullException">name</exception>
    public Task ExchangeBuyAsync(int inventorySlot, string name, int quantity)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Socket.EmitAsync(
            ALSocketEmitType.ExchangeBuy,
            new
            {
                num = inventorySlot,
                name,
                q = quantity
            });
    }

    /// <summary>
    ///     Asynchronously activates a booster item, starting its expiry timer of 30 days plus two per level.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the booster.</param>
    /// <returns>
    ///     The server's answer: <c>data</c> with <c>place</c> set to <c>booster</c>, or <c>invalid</c> for an empty slot or
    ///     anything that is not a booster.
    /// </returns>
    /// <remarks>
    ///     Activating a booster that is already running succeeds without changing its expiry.
    /// </remarks>
    public Task<GameResponseData> ActivateBoosterAsync(int inventorySlot)
        => UseBoosterAsync(
            new
            {
                num = inventorySlot,
                action = "activate"
            });

    /// <summary>
    ///     Asynchronously turns a booster into one of the other two kinds in place, keeping its level and expiry.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the booster.</param>
    /// <param name="to">
    ///     <c>xpbooster</c>, <c>luckbooster</c> or <c>goldbooster</c>.
    /// </param>
    /// <returns>
    ///     The server's answer, as for <see cref="ActivateBoosterAsync" />, with <c>invalid</c> also covering a
    ///     <paramref name="to" /> that is not a booster name.
    /// </returns>
    /// <remarks>
    ///     The swap resets <c>xpm</c>, <c>goldm</c> and <c>luckm</c> to 1 and adds 240ms to <c>penalty_cd</c>, capped at 120s.
    /// </remarks>
    /// <exception cref="ArgumentNullException">to</exception>
    public Task<GameResponseData> ShiftBoosterAsync(int inventorySlot, string to)
    {
        ArgumentNullException.ThrowIfNull(to);

        return UseBoosterAsync(
            new
            {
                num = inventorySlot,
                action = "shift",
                to
            });
    }

    private async Task<GameResponseData> UseBoosterAsync(object payload, [CallerMemberName] string? caller = null)
    {
        var source = new TaskCompletionSource<Expectation<GameResponseData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        //success_response and fail_response both stamp place with the emit's own name, so that is the whole filter
        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data => Task.FromResult("booster".EqualsI(data.Place!) && source.TrySetResult(data)));

        await Socket.EmitAsync(ALSocketEmitType.Booster, payload);

        return (await source.Task.WithNetworkTimeout(caller)).Result;
    }

    /// <summary>
    ///     Asynchronously converts a discontinued <c>stoneofxp</c>, <c>stoneofgold</c> or <c>stoneofluck</c> into shells: 3600
    ///     if never activated, otherwise prorated down from 600 by the hours since.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the stone.</param>
    /// <remarks>
    ///     The server sends no <c>game_response</c>, only the inventory, and ignores any other item.
    /// </remarks>
    public Task ConvertStoneAsync(int inventorySlot)
        => Socket.EmitAsync(
            ALSocketEmitType.Convert,
            new
            {
                num = inventorySlot
            });

    /// <summary>
    ///     Asynchronously throws one throwable item at a point on the ground, consuming it.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the item.</param>
    /// <param name="x">The x coordinate to throw at.</param>
    /// <param name="y">The y coordinate to throw at.</param>
    /// <remarks>
    ///     The game's THROW! button, not <c>Merchant.ThrowAsync</c>; only items whose def carries <c>throw</c> reach it. The
    ///     reach is <c>str * 3</c>, but a throw past it answers <c>too_far</c> and lands anyway.
    /// </remarks>
    public Task ThrowToGroundAsync(int inventorySlot, float x, float y)
        => Socket.EmitAsync(
            ALSocketEmitType.Throw,
            new
            {
                num = inventorySlot,
                x,
                y
            });
    #endregion

    #region Merchant / trade
    /// <summary>
    ///     Asynchronously sells an item into another player's standing buy order.
    /// </summary>
    /// <param name="buyerId">The id of the buyer.</param>
    /// <param name="slot">The trade slot holding the buy order.</param>
    /// <param name="quantity">The number of items to sell.</param>
    /// <param name="rid">
    ///     The listing's <c>rid</c>, so a replaced listing is refused.
    /// </param>
    /// <exception cref="ArgumentNullException">buyerId</exception>
    public Task TradeSellAsync(
        string buyerId,
        TradeSlot slot,
        int quantity,
        string? rid = null)
    {
        ArgumentNullException.ThrowIfNull(buyerId);

        return Socket.EmitAsync(
            ALSocketEmitType.TradeSell,
            new
            {
                id = buyerId,
                slot,
                q = quantity,
                rid
            });
    }

    /// <summary>
    ///     Asynchronously joins a giveaway posted in a player's stand slot.
    /// </summary>
    /// <param name="sellerId">The id of the player giving the item away.</param>
    /// <param name="slot">The trade slot holding the giveaway.</param>
    /// <param name="rid">
    ///     The listing's <c>rid</c>, so a replaced listing is refused.
    /// </param>
    /// <remarks>
    ///     A joiner never sees its own name arrive in the participant list, so only the <c>game_response</c> confirms it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">sellerId</exception>
    /// <exception cref="InvalidOperationException">
    ///     The server refused: out of range, seller gone, listing gone or replaced, or not a giveaway.
    /// </exception>
    public async Task JoinGiveawayAsync(string sellerId, TradeSlot slot, string? rid = null)
    {
        ArgumentNullException.ThrowIfNull(sellerId);

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                //the literal is the receiver because EqualsI throws on a null receiver
                if (!"join_giveaway".EqualsI(data.Place!))
                    return TaskCache.FALSE;

                if (data.Failed)
                    source.TrySetResult($"Failed to join {sellerId}'s giveaway. ({data.Reason ?? data.ResponseType.ToString()})");
                else if (data.Success)
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(
            ALSocketEmitType.JoinGiveaway,
            new
            {
                id = sellerId,
                slot,
                rid
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously takes a trade offer on another player's stand, giving the item in <paramref name="inventorySlot" />
    ///     for the item the slot holds.
    /// </summary>
    /// <param name="merchantId">
    ///     The id of the player whose stand holds the offer.
    /// </param>
    /// <param name="slot">The trade slot holding the offer.</param>
    /// <param name="rid">
    ///     The listing's <c>rid</c>, so a replaced offer is refused.
    /// </param>
    /// <param name="inventorySlot">The slot holding the item to give.</param>
    /// <remarks>
    ///     The emit names the item as this client saw it, so a bag reordered in between is refused rather than giving a
    ///     different item.
    /// </remarks>
    /// <exception cref="ArgumentNullException">merchantId</exception>
    /// <exception cref="ArgumentNullException">rid</exception>
    /// <exception cref="InvalidOperationException">
    ///     The slot is empty, or the server refused: out of range, the offer gone or replaced, the item not what the offer
    ///     wants, or the merchant out of room.
    /// </exception>
    public async Task SwapWithPlayerAsync(
        string merchantId,
        TradeSlot slot,
        string rid,
        int inventorySlot)
    {
        ArgumentNullException.ThrowIfNull(merchantId);

        ArgumentNullException.ThrowIfNull(rid);

        var item = Character.Inventory[inventorySlot];

        if (item == null)
            throw new InvalidOperationException($"Failed to trade with {merchantId}. (slot {inventorySlot} empty)");

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                //the literal is the receiver because EqualsI throws on a null receiver
                if (!"trade_swap".EqualsI(data.Place!))
                    return TaskCache.FALSE;

                source.TrySetResult(
                    data.Failed
                        ? $"Failed to trade {item.Name} with {merchantId}. ({data.Reason ?? data.ResponseType.ToString()})"
                        : Expectation.Success);

                return TaskCache.FALSE;
            });

        //rebuilt the way the server writes a bag item: a level on anything that levels, even at 0, and a stack count on
        //anything that stacks, even at 1. Keys the item does not carry are left out rather than sent as null
        var data = GameData.Items[item.Name];

        var given = new Dictionary<string, object>
        {
            ["name"] = item.Name
        };

        if (data is { UpgradeModifiers: not null } or { CompoundModifiers: not null } || (item.Level > 0))
            given["level"] = item.Level;

        if (data is { StackSize: > 1 } || (item.Quantity > 1))
            given["q"] = item.Quantity;

        if (item.Prediction?.Title is { Length: > 0 } title)
            given["p"] = title;

        if (item.StatType != ALAttribute.None)
            given["stat_type"] = EnumHelper.ToString(item.StatType);

        await Socket.EmitAsync(
            ALSocketEmitType.TradeSwap,
            new
            {
                slot,
                id = merchantId,
                rid,
                num = inventorySlot,
                item = given
            });

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Asynchronously donates gold at a shrine. A donation of 1,000,000 or more unlocks lost-and-found access.
    /// </summary>
    /// <param name="gold">The gold to donate.</param>
    public Task DonateAsync(long gold)
        => Socket.EmitAsync(
            ALSocketEmitType.Donate,
            new
            {
                gold
            });
    #endregion

    #region Mail
    /// <summary>
    ///     Asynchronously sends mail to a character, which costs gold.
    /// </summary>
    /// <param name="to">The name of the character.</param>
    /// <param name="subject">The subject line.</param>
    /// <param name="message">The message body.</param>
    /// <param name="sendItem">
    ///     Specifies whether the item in inventory slot 0 is attached.
    /// </param>
    /// <exception cref="ArgumentNullException">to</exception>
    public Task MailAsync(
        string to,
        string? subject = null,
        string? message = null,
        bool sendItem = false)
    {
        ArgumentNullException.ThrowIfNull(to);

        return Socket.EmitAsync(
            ALSocketEmitType.Mail,
            new
            {
                to,
                subject,
                message,
                item = sendItem
            });
    }

    /// <summary>
    ///     Asynchronously takes the item attached to a received mail into the inventory.
    /// </summary>
    /// <param name="mailId">The id of the mail.</param>
    /// <exception cref="ArgumentNullException">mailId</exception>
    public Task TakeMailItemAsync(string mailId)
    {
        ArgumentNullException.ThrowIfNull(mailId);

        return Socket.EmitAsync(
            ALSocketEmitType.TakeMailItem,
            new
            {
                id = mailId
            });
    }
    #endregion

    #region Social
    /// <summary>
    ///     Asynchronously sends a friend request to a nearby online character.
    /// </summary>
    /// <param name="name">The name of the character.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public Task SendFriendRequestAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Socket.EmitAsync(
            ALSocketEmitType.Friend,
            new
            {
                @event = "request",
                name
            });
    }

    /// <summary>Asynchronously accepts a pending friend request.</summary>
    /// <param name="name">The name of the character who sent it.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public Task AcceptFriendRequestAsync(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Socket.EmitAsync(
            ALSocketEmitType.Friend,
            new
            {
                @event = "accept",
                name
            });
    }
    #endregion

    #region Pets
    /// <summary>
    ///     Asynchronously spawns the character's active pet.
    /// </summary>
    public Task SpawnPetAsync() => Socket.EmitAsync(ALSocketEmitType.Pet);

    /// <summary>
    ///     Asynchronously whistles the character's pet back to them.
    /// </summary>
    public Task WhistlePetAsync() => Socket.EmitAsync(ALSocketEmitType.Whistle);

    /// <summary>
    ///     Asynchronously requests the character's owned pets; the result arrives as a <c>players</c> event.
    /// </summary>
    public Task RequestPetsAsync() => Socket.EmitAsync(ALSocketEmitType.Pets);
    #endregion

    #region Desertland
    /// <summary>
    ///     Asynchronously locks, seals or unlocks the item in <paramref name="inventorySlot" /> at the locksmith, for 250,000
    ///     gold except the final clear of an expired seal.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the item.</param>
    /// <param name="operation">The operation to perform.</param>
    /// <returns>The server's answer.</returns>
    /// <remarks>
    ///     Refused in the bank, and distance-gated to Smith in desertland unless a <c>computer</c> is in the bags. Scrolls,
    ///     offerings and tomes are refused with <c>locksmith_cant</c>.
    ///     <br />
    ///     <see cref="LocksmithOperation.Seal" /> checks only the purse: sealing an item mid-unseal takes the gold and
    ///     discards however much of the 48 hours had elapsed.
    /// </remarks>
    public async Task<GameResponseData> LocksmithAsync(int inventorySlot, LocksmithOperation operation)
    {
        var source = new TaskCompletionSource<Expectation<GameResponseData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.LocksmithLocked                                     => source.TrySetResult(data),
                    GameResponseType.LocksmithSealed                                     => source.TrySetResult(data),
                    GameResponseType.LocksmithUnlocked                                   => source.TrySetResult(data),
                    GameResponseType.LocksmithUnsealed                                   => source.TrySetResult(data),
                    GameResponseType.LocksmithUnsealComplete                             => source.TrySetResult(data),
                    GameResponseType.LocksmithUnsealing                                  => source.TrySetResult(data),
                    GameResponseType.LocksmithCant                                       => source.TrySetResult(data),
                    GameResponseType.LocksmithAlreadyLocked                              => source.TrySetResult(data),
                    GameResponseType.LocksmithAlreadyUnlocked                            => source.TrySetResult(data),
                    GameResponseType.GoldNotEnough when "locksmith".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.CantInBank when "locksmith".EqualsI(data.Place!)    => source.TrySetResult(data),
                    GameResponseType.NoItem when "locksmith".EqualsI(data.Place!)        => source.TrySetResult(data),
                    GameResponseType.Distance when "locksmith".EqualsI(data.Place!)      => source.TrySetResult(data),
                    _                                                                    => false
                };

                return Task.FromResult(result);
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Locksmith,
            new
            {
                num = inventorySlot,
                operation
            });

        return (await source.Task.WithNetworkTimeout()).Result;
    }

    /// <summary>
    ///     Asynchronously strips the stat scroll off the item in <paramref name="inventorySlot" /> at the scrollsmith and
    ///     refunds the scrolls.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the item.</param>
    /// <returns>
    ///     The server's answer; <c>scrollsmith_success</c> carries the gold spent in <see cref="GameResponseData.Gold" />.
    /// </returns>
    /// <remarks>
    ///     Costs ten times the refund's value: 80,000 gold for a base-grade item's one scroll, 8,000,000 for a grade-2 item's
    ///     hundred. Refused in the bank, distance-gated like <see cref="LocksmithAsync" />, and refused with <c>inv_size</c>
    ///     when there is no free slot for the refund.
    /// </remarks>
    public async Task<GameResponseData> DestatAsync(int inventorySlot)
    {
        var source = new TaskCompletionSource<Expectation<GameResponseData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.ScrollsmithSuccess                               => source.TrySetResult(data),
                    GameResponseType.ScrollsmithCant                                  => source.TrySetResult(data),
                    GameResponseType.GoldNotEnough when "destat".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.CantInBank when "destat".EqualsI(data.Place!)    => source.TrySetResult(data),
                    GameResponseType.NoItem when "destat".EqualsI(data.Place!)        => source.TrySetResult(data),
                    GameResponseType.InvSize when "destat".EqualsI(data.Place!)       => source.TrySetResult(data),
                    GameResponseType.Distance when "destat".EqualsI(data.Place!)      => source.TrySetResult(data),
                    _                                                                 => false
                };

                return Task.FromResult(result);
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Destat,
            new
            {
                num = inventorySlot
            });

        return (await source.Task.WithNetworkTimeout()).Result;
    }
    #endregion

    #region Activate
    /// <summary>
    ///     Asynchronously activates the cosmetic behaviour of an equipped item, which only ever changes the character's
    ///     temporary skin.
    /// </summary>
    /// <param name="slot">
    ///     The equipment slot holding the item. Trade slots are refused.
    /// </param>
    /// <remarks>
    ///     <c>angelwings</c> toggles the <c>snow_angel</c> skin for a mage or priest wearing it at +8 or better.
    ///     <c>tristone</c> and <c>darktristone</c> roll a transform skin, and clear it instead whenever a skin is already on
    ///     or the unsent activation count is exactly one hundred.
    /// </remarks>
    public Task ActivateEquippedAsync(Slot slot)
        => Socket.EmitAsync(
            ALSocketEmitType.Activate,
            new
            {
                slot
            });

    /// <summary>
    ///     Asynchronously activates an inventory item. Only the three bank keys answer.
    /// </summary>
    /// <param name="inventorySlot">The slot holding the item.</param>
    /// <returns>The server's answer.</returns>
    /// <remarks>
    ///     <c>bkey</c> and <c>ukey</c> open the second and third bank floors and <c>dkey</c> the next bank pack, all consumed,
    ///     all answering <c>only_in_bank</c> outside the vault. <c>frozenstone</c> is consumed for nothing.
    /// </remarks>
    /// <exception cref="TimeoutException">
    ///     The item is not a bank key, or every bank pack is already open, so no <c>game_response</c> came.
    /// </exception>
    public async Task<GameResponseData> ActivateItemAsync(int inventorySlot)
    {
        var source = new TaskCompletionSource<Expectation<GameResponseData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = data.ResponseType switch
                {
                    GameResponseType.DoorUnlocked     => source.TrySetResult(data),
                    GameResponseType.BankPackUnlocked => source.TrySetResult(data),
                    GameResponseType.OnlyInBank       => source.TrySetResult(data),
                    GameResponseType.AlreadyUnlocked  => source.TrySetResult(data),
                    _                                 => false
                };

                return Task.FromResult(result);
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Activate,
            new
            {
                num = inventorySlot
            });

        return (await source.Task.WithNetworkTimeout()).Result;
    }
    #endregion

    #region Cosmetics
    /// <summary>
    ///     Asynchronously equips an owned cosmetic into <paramref name="slot" />.
    /// </summary>
    /// <param name="slot">The cosmetic slot.</param>
    /// <param name="name">The name of the cosmetic.</param>
    /// <remarks>
    ///     Answers <c>cx_not_found</c> for anything the account does not own. Ownership includes bundles and exclusives, so a
    ///     name absent from <c>acx</c> can still be equippable. The server then drops any slot whose sprite type no longer
    ///     matches it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">slot</exception>
    /// <exception cref="ArgumentNullException">name</exception>
    public Task SetCosmeticAsync(string slot, string name)
    {
        ArgumentNullException.ThrowIfNull(slot);

        ArgumentNullException.ThrowIfNull(name);

        return Socket.EmitAsync(
            ALSocketEmitType.Cx,
            new
            {
                slot,
                name
            });
    }

    /// <summary>
    ///     Asynchronously clears <paramref name="slot" />, by sending no <c>name</c>.
    /// </summary>
    /// <param name="slot">The cosmetic slot.</param>
    /// <remarks>
    ///     Clearing <c>back</c> also clears <c>tail</c>, and clearing <c>face</c> also clears <c>makeup</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">slot</exception>
    public Task ClearCosmeticAsync(string slot)
    {
        ArgumentNullException.ThrowIfNull(slot);

        return Socket.EmitAsync(
            ALSocketEmitType.Cx,
            new
            {
                slot
            });
    }

    /// <summary>
    ///     Asynchronously moves one owned copy of a cosmetic to another character on the same account.
    /// </summary>
    /// <param name="toPlayerId">
    ///     The receiving character, which must be on the same map and within <c>B.dist</c> of this one.
    /// </param>
    /// <param name="name">
    ///     A sprite name, not an <c>acx</c> key. Naming one member of a bundle moves the whole bundle.
    /// </param>
    /// <returns>
    ///     The server's answer: <see cref="GameResponseType.CosmeticSent" /> on success, otherwise the failure, such as
    ///     <see cref="GameResponseType.SendNoCosmetic" /> for a cosmetic this character has no unworn copy of.
    /// </returns>
    /// <remarks>
    ///     Anything worn blocks the send, however many copies are owned.
    /// </remarks>
    /// <exception cref="ArgumentNullException">toPlayerId</exception>
    /// <exception cref="ArgumentNullException">name</exception>
    public async Task<GameResponseData> SendCosmeticAsync(string toPlayerId, string name)
    {
        ArgumentNullException.ThrowIfNull(toPlayerId);

        ArgumentNullException.ThrowIfNull(name);

        var source = new TaskCompletionSource<GameResponseData>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                var result = "send".EqualsI(data.Place ?? string.Empty)
                             && (data.Failed || (data.ResponseType == GameResponseType.CosmeticSent))
                             && source.TrySetResult(data);

                return Task.FromResult(result);
            });

        await Socket.EmitAsync(
            ALSocketEmitType.Send,
            new
            {
                name = toPlayerId,
                cx = name
            });

        return await source.Task.WithNetworkTimeout();
    }

    /// <summary>
    ///     Asynchronously copies the nearest monster's skin in the instance onto this character, with no cost, cooldown or
    ///     range check.
    /// </summary>
    /// <remarks>
    ///     The skin lands on <c>player.tskin</c>, which outranks <c>player.skin</c>. Only activating a <c>tristone</c>,
    ///     <c>darktristone</c>, or <c>angelwings</c> over the <c>snow_angel</c> skin clears it.
    /// </remarks>
    public Task BlendAsync() => Socket.EmitAsync(ALSocketEmitType.Blend);
    #endregion
}