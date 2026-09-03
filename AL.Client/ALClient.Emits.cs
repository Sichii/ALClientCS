#region
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client;

/// <summary>
///     Phase 9 - emit surface coverage (tier 1). Thin senders for server handlers a parity client needs but could not
///     previously reach. These emit the action; any confirmation arrives as a separate inbound event (Phase 10).
///     <see cref="Slot" />/<see cref="TradeSlot" /> serialize lowercase via their own converters, so they can be placed
///     straight into the payload.
/// </summary>
public abstract partial class ALClient
{
    #region Misc
    /// <summary>
    ///     Places a tavern bet (roulette/dice) (node/server.js:11456).
    /// </summary>
    public Task BetAsync(string type, long gold, string? odds = null)
        => Socket.EmitAsync(
            ALSocketEmitType.Bet,
            new
            {
                type,
                gold,
                odds
            });

    /// <summary>
    ///     Places a dice bet on <paramref name="number" />, with <paramref name="up" /> picking which side of it wins
    ///     (node/server.js:11514). Only works on the tavern map. The server clamps the number to 0.01-99.99, raises
    ///     the gold to at least 10,000, takes it at placement, and refuses a second bet while one is unresolved
    ///     (<c>tavern_dice_exist</c>) or outside the betting window (<c>tavern_not_yet</c>/<c>tavern_too_late</c>).
    ///     Leaving the tavern with a bet unresolved refunds it in full (node/server.js:4131-4135).
    /// </summary>
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
    ///     Asks the tavern for its current house rules (node/server.js:11589) and returns them.
    /// </summary>
    /// <remarks>
    ///     Both numbers come from one quantity, the house's free bankroll <c>S.gold - house_debt()</c>, where
    ///     <c>house_debt</c> sums what every unresolved dice bet on the server stands to pay out. So
    ///     <see cref="TavernData.Max" /> falls while other players hold large bets open and recovers as those
    ///     resolve, and it is the only way to know in advance whether a stake will be taken rather than refused with
    ///     <c>tavern_gold_not_enough</c>. <b>Ask again per bet rather than caching it.</b>
    ///     <br />
    ///     The reply arrives on the same <c>tavern</c> event that carries the round's bet, win and loss broadcasts,
    ///     so this resolves on <see cref="TavernData.Event" /> being <c>info</c> and leaves the rest for whoever
    ///     else is listening.
    ///     <br />
    ///     The handler has no map, state or gold check, so it answers wherever the character is standing. It throws
    ///     <see cref="TimeoutException" /> only when no reply comes at all, which on a live server means a tavern
    ///     instance that never started: <c>house_debt</c> walks <c>tavern.dice.players</c>
    ///     (node/server_functions.js:1229) and throws before the reply is built.
    /// </remarks>
    public async Task<TavernData> RequestTavernInfoAsync()
    {
        var source = new TaskCompletionSource<Expectation<TavernData>>(TaskCreationOptions.RunContinuationsAsynchronously);

        //false on anything else: returning true consumes the frame, starving a subscriber waiting on bet/won/lost
        using var tavernCallback = Socket.On<TavernData>(
            ALSocketMessageType.Tavern,
            data => Task.FromResult((data.Event is not null) && "info".EqualsI(data.Event) && source.TrySetResult(data)));

        await Socket.EmitAsync(ALSocketEmitType.Tavern, new { @event = "info" });

        return (await source.Task.WithNetworkTimeout()).Result;
    }

    /// <summary>
    ///     Sets an upper cap on this character's movement speed (node/server.js:5122).
    /// </summary>
    /// <remarks>
    ///     A cap and not a setting: the server applies it as <c>min(speed, cruise || 200000)</c> after every other
    ///     modifier, so it can only ever slow the character. It persists on the player until changed, so a caller
    ///     that stops wanting it has to clear it rather than falling silent - and <b>zero is the clear</b>, being
    ///     falsy in that expression. The payload is a bare number, matching the handler's own
    ///     <c>function (speed)</c> signature; the server parses it with <c>parseInt</c> and floors the resulting
    ///     speed at 5.
    ///     <br />
    ///     It costs 10 call units of the 200 a character has every four seconds - the move emit costs 1.5.
    /// </remarks>
    public Task CruiseAsync(int speed) => Socket.EmitAsync(ALSocketEmitType.Cruise, speed);

    /// <summary>
    ///     Kills this character outright (node/server.js:11680-11691), running the same <c>defeat_player</c> and
    ///     <c>rip</c> a monster kill would (:11685, :11687) - the normal death penalty, not a free trip to town.
    ///     Refused while already dead, and takes no arguments.
    /// </summary>
    /// <remarks>
    ///     Wire coverage. Nothing in this client or the bot above it calls this; it exists so the emit surface is
    ///     complete.
    /// </remarks>
    public Task HarakiriAsync() => Socket.EmitAsync(ALSocketEmitType.Harakiri);

    /// <summary>
    ///     Signs this character up at Bean (node/server.js:11326) and answers <c>signed_up</c>. Distinct from the
    ///     giveaway join, which is its own emit.
    /// </summary>
    /// <remarks>
    ///     Wire coverage. Nothing in this client or the bot above it calls this.
    /// </remarks>
    public Task SignUpAsync() => Socket.EmitAsync(ALSocketEmitType.Signup);
    #endregion

    #region Chat
    /// <summary>
    ///     Sends a public chat message. <paramref name="code" /> flags it as code-manager output, which the server rate-limits
    ///     to once per 15 seconds (node/server.js:4514).
    /// </summary>
    public Task SayAsync(string message, int? code = null)
        => Socket.EmitAsync(
            ALSocketEmitType.Say,
            new
            {
                message,
                code
            });

    /// <summary>
    ///     Sends a message to the character's party chat (node/server.js:4524).
    /// </summary>
    public Task SayToPartyAsync(string message)
        => Socket.EmitAsync(
            ALSocketEmitType.Say,
            new
            {
                message,
                party = true
            });

    /// <summary>
    ///     Sends a private message to a named character (node/server.js:4529).
    /// </summary>
    public Task WhisperAsync(string name, string message)
        => Socket.EmitAsync(
            ALSocketEmitType.Say,
            new
            {
                message,
                name
            });

    /// <summary>
    ///     Sends a code-manager message to one or more characters - the channel AL bots use to coordinate a multi-character
    ///     party (node/server.js:4338, :4499).
    /// </summary>
    public Task SendCmAsync(IEnumerable<string> to, object message)
        => Socket.EmitAsync(
            ALSocketEmitType.Command,
            new
            {
                to = to.ToArray(),
                message
            });

    /// <summary>
    ///     Plays an unlocked emotion by name (node/server.js:8838).
    /// </summary>
    public Task UseEmotionAsync(string name)
        => Socket.EmitAsync(
            ALSocketEmitType.Emotion,
            new
            {
                name
            });
    #endregion

    #region Movement / instances
    /// <summary>
    ///     Enters instanced content (crypt, tomb, winter/spider instance, duelland, ...). This is the only path that creates
    ///     an instance; <see cref="TransportAsync" /> covers static doors only (node/server.js:5526).
    /// </summary>
    public Task EnterAsync(string place, string? instanceName = null)
        => Socket.EmitAsync(
            ALSocketEmitType.Enter,
            new
            {
                place,
                name = instanceName
            });

    /// <summary>
    ///     Joins ongoing event content by name (goobrawl, crabxx, arenas, ...) (node/server.js:11122).
    /// </summary>
    public Task JoinEventAsync(string eventName)
        => Socket.EmitAsync(
            ALSocketEmitType.Join,
            new
            {
                name = eventName
            });

    /// <summary>
    ///     Sets the character's home point to the current server (node/server.js:5070). Rate-limited to once per 36 hours
    ///     server-side.
    /// </summary>
    public Task SetHomeAsync() => Socket.EmitAsync(ALSocketEmitType.SetHome);

    /// <summary>
    ///     Triggers a seasonal map-object interaction by type (newyear_tree, redorb, ...) (node/server.js:9892).
    /// </summary>
    public Task InteractionAsync(string type)
        => Socket.EmitAsync(
            ALSocketEmitType.Interaction,
            new
            {
                type
            });
    #endregion

    #region Inventory
    /// <summary>
    ///     Equips up to 15 items in a single call (node/server.js:6989). A null slot lets the server pick the item's default
    ///     slot. Directly supports the weapon-swap pattern.
    /// </summary>
    /// <summary>
    ///     Equips up to 15 items in a single call (node/server.js:7057). A null slot lets the server pick the item's
    ///     default slot. Directly supports the weapon-swap pattern.
    /// </summary>
    /// <remarks>
    ///     The server applies the entries in order and stops at the first one it refuses, keeping everything it
    ///     applied before that - so the batch is not atomic, and the last entry landing is what proves the whole of
    ///     it did. That is the confirmation awaited here.
    ///     <br />
    ///     The failure arm is not a <c>fail_response</c>, which is the trap: a refused entry is answered with a
    ///     <c>success_response</c> carrying the reason as a string inside its <c>slots</c> array, so the universal
    ///     <c>failed</c> discriminator is never set for it. What separates the two is ordering. The handler resends
    ///     the character frame before it answers (node/server.js:7115), so reaching that answer without the frame
    ///     having satisfied the slot check means the server stopped partway.
    ///     <br />
    ///     A batch whose last entry names no slot cannot be confirmed, since the server picks that slot from the
    ///     item's own type; it is emitted and not awaited.
    /// </remarks>
    public async Task EquipBatchAsync(IEnumerable<(int InventorySlot, Slot? Slot)> equips)
    {
        var batch = equips.ToArray();

        if (batch.Length == 0)
            return;

        var payload = batch.Select(equip => new
                           {
                               num = equip.InventorySlot,
                               slot = equip.Slot
                           })
                           .ToArray();

        var last = batch[^1];
        var item = last.InventorySlot >= 0 ? Character.Inventory[last.InventorySlot] : null;

        if ((item is null) || (last.Slot is not { } slot))
        {
            await Socket.EmitAsync(ALSocketEmitType.EquipBatch, payload);

            return;
        }

        var source = new TaskCompletionSource<Expectation>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var characterCallback = Socket.On<CharacterData>(
            ALSocketMessageType.Character,
            data =>
            {
                var slotItem = data.Slots[slot];

                if ((slotItem != null) && slotItem.Name.EqualsI(item.Name) && (slotItem.Level == item.Level))
                    source.TrySetResult(Expectation.Success);

                return TaskCache.FALSE;
            });

        using var gameResponseCallback = Socket.On<GameResponseData>(
            ALSocketMessageType.GameResponse,
            data =>
            {
                //the literal is the receiver: Place can be null, and EqualsI throws on a null receiver while
                //tolerating a null argument
                if ("equip_batch".EqualsI(data.Place!))
                    source.TrySetResult($"Failed to equip {batch.Length} items. (the server refused one of them)");

                return TaskCache.FALSE;
            });

        await Socket.EmitAsync(ALSocketEmitType.EquipBatch, payload);

        var expectation = await source.Task.WithNetworkTimeout();
        expectation.ThrowIfUnsuccessful();
    }

    /// <summary>
    ///     Splits a stackable item, moving <paramref name="quantity" /> into a new inventory slot (node/server.js:7350).
    /// </summary>
    public Task SplitAsync(int inventorySlot, int quantity)
        => Socket.EmitAsync(
            ALSocketEmitType.Split,
            new
            {
                num = inventorySlot,
                quantity
            });

    /// <summary>
    ///     Permanently destroys an item (or <paramref name="quantity" /> of a stack) (node/server.js:7932).
    /// </summary>
    public Task DestroyAsync(int inventorySlot, int? quantity = null)
        => Socket.EmitAsync(
            ALSocketEmitType.Destroy,
            new
            {
                num = inventorySlot,
                q = quantity
            });

    /// <summary>
    ///     Buys-and-exchanges a token/quest item atomically (node/server.js:6108). <paramref name="quantity" /> is a safety
    ///     check against the current stack size.
    /// </summary>
    public Task ExchangeBuyAsync(int inventorySlot, string name, int quantity)
        => Socket.EmitAsync(
            ALSocketEmitType.ExchangeBuy,
            new
            {
                num = inventorySlot,
                name,
                q = quantity
            });

    /// <summary>
    ///     Activates a booster item, starting its expiry timer (node/server.js:8788).
    /// </summary>
    public Task ActivateBoosterAsync(int inventorySlot)
        => Socket.EmitAsync(
            ALSocketEmitType.Booster,
            new
            {
                num = inventorySlot,
                action = "activate"
            });

    /// <summary>
    ///     Throws one throwable item at a point on the ground (node/server.js:8039), consuming it.
    /// </summary>
    /// <remarks>
    ///     Not <c>Merchant.ThrowAsync</c>, which sends the merchant's "Throw Stuff" skill at an entity. This is the
    ///     handler behind the game's own THROW! button, and only items whose def carries <c>throw</c> reach it -
    ///     firecrackers, whiteegg, confetti and smoke.
    ///     <br />
    ///     The reach is <c>str * 3</c> from the character. The too-far branch answers <c>too_far</c> and then falls
    ///     through to throw anyway, and the item is consumed either way, so nothing here waits on an answer.
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
    ///     Sells an item into another player's standing buy-order (node/server.js:8072) - the merchant-loop counterpart to
    ///     buying from a stand.
    /// </summary>
    public Task TradeSellAsync(
        string buyerId,
        TradeSlot slot,
        int quantity,
        string? rid = null)
        => Socket.EmitAsync(
            ALSocketEmitType.TradeSell,
            new
            {
                id = buyerId,
                slot,
                q = quantity,
                rid
            });

    /// <summary>
    ///     Joins a giveaway posted in a player's stand slot (node/server.js:7987).
    /// </summary>
    public Task JoinGiveawayAsync(string sellerId, TradeSlot slot, string? rid = null)
        => Socket.EmitAsync(
            ALSocketEmitType.JoinGiveaway,
            new
            {
                id = sellerId,
                slot,
                rid
            });

    /// <summary>
    ///     Donates gold at a shrine; a donation of 1,000,000+ unlocks lost-and-found access (node/server.js:7897).
    /// </summary>
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
    ///     Sends mail to a character, optionally attaching the item in inventory slot 0 (node/server.js:5184). Costs gold
    ///     server-side.
    /// </summary>
    public Task MailAsync(
        string to,
        string? subject = null,
        string? message = null,
        bool sendItem = false)
        => Socket.EmitAsync(
            ALSocketEmitType.Mail,
            new
            {
                to,
                subject,
                message,
                item = sendItem
            });

    /// <summary>
    ///     Takes the item attached to a received mail into inventory (node/server.js:5146).
    /// </summary>
    public Task TakeMailItemAsync(string mailId)
        => Socket.EmitAsync(
            ALSocketEmitType.TakeMailItem,
            new
            {
                id = mailId
            });
    #endregion

    #region Social
    /// <summary>
    ///     Sends a friend request to a nearby online character (node/server.js:10637).
    /// </summary>
    public Task SendFriendRequestAsync(string name)
        => Socket.EmitAsync(
            ALSocketEmitType.Friend,
            new
            {
                @event = "request",
                name
            });

    /// <summary>
    ///     Accepts a pending friend request from the named character (node/server.js:10655).
    /// </summary>
    public Task AcceptFriendRequestAsync(string name)
        => Socket.EmitAsync(
            ALSocketEmitType.Friend,
            new
            {
                @event = "accept",
                name
            });
    #endregion

    #region Pets
    /// <summary>
    ///     Spawns the character's active pet (node/server.js:11376).
    /// </summary>
    public Task SpawnPetAsync() => Socket.EmitAsync(ALSocketEmitType.Pet);

    /// <summary>
    ///     Recalls (whistles) the character's pet to their location (node/server.js:11392).
    /// </summary>
    public Task WhistlePetAsync() => Socket.EmitAsync(ALSocketEmitType.Whistle);

    /// <summary>
    ///     Requests the character's owned pets; the result arrives as a
    ///     <c>
    ///         players
    ///     </c>
    ///     event (node/server.js:11450).
    /// </summary>
    public Task RequestPetsAsync() => Socket.EmitAsync(ALSocketEmitType.Pets);
    #endregion

    #region Desertland
    /// <summary>
    ///     Locks, seals or unlocks the item in <paramref name="inventorySlot" /> at the locksmith
    ///     (node/server.js:6279). 250,000 gold for every operation except the final clear of an expired seal.
    /// </summary>
    /// <remarks>
    ///     Refused in the bank, and distance-gated to Smith in desertland at <c>B.sell_dist</c> unless a
    ///     <c>computer</c> is in the bags. <c>uscroll</c>, <c>cscroll</c>, <c>pscroll</c>, <c>offering</c> and
    ///     <c>tome</c> are refused outright with <c>locksmith_cant</c>.
    ///     <br />
    ///     <b>The payload field is <c>num</c>.</b> The handler reads <c>data.item_num</c> first and then immediately
    ///     redeclares both locals from <c>data.num</c> (node/server.js:6281-6293), so the first read is dead and a
    ///     payload carrying only <c>item_num</c> answers <c>no_item</c>.
    ///     <br />
    ///     <b><see cref="LocksmithOperation.Seal" /> validates nothing but the purse.</b> Unlike
    ///     <see cref="LocksmithOperation.Lock" />, which refuses an already-flagged item, sealing one that is
    ///     mid-unseal takes the gold and discards however much of the 48 hours had elapsed.
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
                    GameResponseType.LocksmithLocked          => source.TrySetResult(data),
                    GameResponseType.LocksmithSealed          => source.TrySetResult(data),
                    GameResponseType.LocksmithUnlocked        => source.TrySetResult(data),
                    GameResponseType.LocksmithUnsealed        => source.TrySetResult(data),
                    GameResponseType.LocksmithUnsealComplete  => source.TrySetResult(data),
                    GameResponseType.LocksmithUnsealing       => source.TrySetResult(data),
                    GameResponseType.LocksmithCant            => source.TrySetResult(data),
                    GameResponseType.LocksmithAlreadyLocked   => source.TrySetResult(data),
                    GameResponseType.LocksmithAlreadyUnlocked => source.TrySetResult(data),
                    GameResponseType.GoldNotEnough when "locksmith".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.CantInBank    when "locksmith".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.NoItem        when "locksmith".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.Distance      when "locksmith".EqualsI(data.Place!) => source.TrySetResult(data),
                    _                                         => false
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
    ///     Strips the stat scroll off the item in <paramref name="inventorySlot" /> at the scrollsmith
    ///     (node/server.js:6238) and refunds the scrolls.
    /// </summary>
    /// <remarks>
    ///     Costs ten times the refund's market value: the item's grade at level zero picks a scroll count from
    ///     <c>[1, 10, 100, 1000, 9999, 9999, 9999]</c> and the charge is that count times the scroll's <c>g</c>
    ///     (<c>G.items</c>, every stat scroll: 8,000) times ten. So a base-grade item is 80,000 gold for one scroll
    ///     back and a grade-2 item is 8,000,000 for a hundred.
    ///     <br />
    ///     Refused in the bank, distance-gated to Sir Bob in desertland at <c>B.sell_dist</c> with the same
    ///     <c>computer</c> waiver as <see cref="LocksmithAsync" />, and refused with <c>inv_size</c> when there is no
    ///     room for the refund. The scrolls stack, so that is one free slot rather than a hundred.
    ///     <br />
    ///     <c>scrollsmith_success</c> carries the gold actually spent in <see cref="GameResponseData.Gold" />.
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
                    GameResponseType.ScrollsmithSuccess => source.TrySetResult(data),
                    GameResponseType.ScrollsmithCant    => source.TrySetResult(data),
                    GameResponseType.GoldNotEnough when "destat".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.CantInBank    when "destat".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.NoItem        when "destat".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.InvSize       when "destat".EqualsI(data.Place!) => source.TrySetResult(data),
                    GameResponseType.Distance      when "destat".EqualsI(data.Place!) => source.TrySetResult(data),
                    _                                   => false
                };

                return Task.FromResult(result);
            });

        await Socket.EmitAsync(ALSocketEmitType.Destat, new { num = inventorySlot });

        return (await source.Task.WithNetworkTimeout()).Result;
    }
    #endregion

    #region Activate
    /// <summary>
    ///     Activates the cosmetic behaviour of an equipped item (node/server.js:8801-8859). Cosmetic only: every
    ///     reachable branch of this arm writes <c>player.tskin</c> and nothing else - the one exception
    ///     (<c>etherealamulet</c>, node/server.js:8806-8813, which grants a timed invisibility) is dead code, since
    ///     that item name appears nowhere in game data and <c>item.name</c> can never equal it.
    /// </summary>
    /// <remarks>
    ///     <c>angelwings</c> toggles the <c>snow_angel</c> skin, and needs a mage or priest with the cape at +8 or
    ///     better - anything else answers <c>nothing</c>. <c>tristone</c> and <c>darktristone</c> roll a transform
    ///     skin from a pool picked by level and gender, and clear instead of setting whenever a skin is already on
    ///     or the activation count is exactly one hundred (node/server.js:8824, :8843) - an equality check, not a
    ///     cap, so the hundred-and-first activation sets a skin again. That count is kept in
    ///     <c>player.tactivations</c> (node/server.js:8840, :8855) and is never sent, so no caller can read where it
    ///     stands.
    ///     <br />
    ///     Trade slots are rejected outright (node/server.js:8803).
    /// </remarks>
    public Task ActivateEquippedAsync(Slot slot) => Socket.EmitAsync(ALSocketEmitType.Activate, new { slot });

    /// <summary>
    ///     Activates an inventory item (node/server.js:8860-8915). This is the arm with the permanent effects: the
    ///     three bank keys.
    /// </summary>
    /// <remarks>
    ///     <c>bkey</c> and <c>ukey</c> open the second and third bank floors, and <c>dkey</c> opens the first of the
    ///     forty-eight bank packs the account does not already have. All three are consumed, and all three answer
    ///     <c>only_in_bank</c> unless the character is standing in the vault. <c>bkey</c> and <c>ukey</c> answer
    ///     <c>already_unlocked</c> for a floor that is already open.
    ///     <br />
    ///     <b>Those three names are the only ones this arm answers at all</b> (node/server.js:8860-8915). Every
    ///     other item name, an empty or out-of-range slot, and <c>dkey</c> with all forty-eight packs already
    ///     taken fall through to a bare <c>resend</c> carrying no <c>game_response</c>, so the call throws
    ///     <see cref="TimeoutException" /> rather than returning. <c>frozenstone</c> is the one that costs
    ///     something - consumed for no effect (node/server.js:8863-8865).
    /// </remarks>
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

        await Socket.EmitAsync(ALSocketEmitType.Activate, new { num = inventorySlot });

        return (await source.Task.WithNetworkTimeout()).Result;
    }
    #endregion

    #region Cosmetics
    /// <summary>
    ///     Equips an owned cosmetic into <paramref name="slot" /> (node/server.js:4843-4853).
    /// </summary>
    /// <remarks>
    ///     Answers <c>cx_not_found</c> for anything the account does not own. Ownership is not the same as the
    ///     <c>acx</c> dictionary: the server expands it through <c>G.cosmetics.bundle</c> and <c>G.cosmetics.map</c>
    ///     and adds the account's and the class's exclusives before deciding, so a name absent from <c>acx</c> can
    ///     still be equippable.
    ///     <br />
    ///     A body, armor or character sprite sent to the <c>skin</c> slot writes <c>player.skin</c> rather than the
    ///     cosmetic map. Every call is followed server-side by a prune that silently drops any slot whose sprite
    ///     type no longer matches it.
    /// </remarks>
    public Task SetCosmeticAsync(string slot, string name)
        => Socket.EmitAsync(
            ALSocketEmitType.Cx,
            new
            {
                slot,
                name
            });

    /// <summary>
    ///     Clears <paramref name="slot" /> (node/server.js:4835-4842). Sends no <c>name</c>, which is what the
    ///     server reads as "clear" rather than "equip".
    /// </summary>
    /// <remarks>
    ///     <b>Two slots clear two things.</b> Clearing <c>back</c> also deletes <c>tail</c>, and clearing
    ///     <c>face</c> also deletes <c>makeup</c>. One call, two slots empty.
    /// </remarks>
    public Task ClearCosmeticAsync(string slot) => Socket.EmitAsync(ALSocketEmitType.Cx, new { slot });

    /// <summary>
    ///     Copies the nearest monster's skin onto this character (node/server.js:11702).
    /// </summary>
    /// <remarks>
    ///     No cost, no cooldown and no range check - it walks every monster in the instance and takes the closest,
    ///     needing only that the character is alive.
    ///     <br />
    ///     <b>It cannot be undone by itself.</b> The result lands on <c>player.tskin</c>, which outranks
    ///     <c>player.skin</c> on the wire (<c>data.skin = player.tskin || player.skin</c>, node/server.js:805). The
    ///     cosmetic map goes out unchanged (<c>data.cx = player.tcx || player.cx</c>, :806) - a blended skin hiding
    ///     it is the client's own render rule, not something the server withholds. The only clears anywhere are
    ///     activating a <c>tristone</c> or <c>darktristone</c>, or activating <c>angelwings</c> while the current
    ///     skin is <c>snow_angel</c>.
    /// </remarks>
    public Task BlendAsync() => Socket.EmitAsync(ALSocketEmitType.Blend);
    #endregion
}