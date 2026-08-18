#region
using AL.Core.Definitions;
using AL.SocketClient.Definitions;
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
    ///     Places a tavern bet (roulette/dice) (node/server.js:11241).
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
    ///     (node/server.js:11295). Only works on the tavern map. The server clamps the number to 0.01-99.99, raises
    ///     the gold to at least 10,000, takes it at placement, and refuses a second bet while one is unresolved
    ///     (<c>tavern_dice_exist</c>) or outside the betting window (<c>tavern_not_yet</c>/<c>tavern_too_late</c>).
    ///     Leaving the tavern with a bet unresolved refunds it in full (node/server.js:4110).
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
    public Task EquipBatchAsync(IEnumerable<(int InventorySlot, Slot? Slot)> equips)
        => Socket.EmitAsync(
            ALSocketEmitType.EquipBatch,
            equips.Select(equip => new
                  {
                      num = equip.InventorySlot,
                      slot = equip.Slot
                  })
                  .ToArray());

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
}