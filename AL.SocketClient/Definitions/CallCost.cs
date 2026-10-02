namespace AL.SocketClient.Definitions;

/// <summary>
///     Represents the server's rate limiter. Every socket handler is metered, and an accrued cost over the last
///     <see cref="WINDOW" /> above <see cref="LIMIT" /> is a <c>limitdcreport</c> and an immediate kick.
/// </summary>
/// <remarks>
///     A call bills its method's row in the server's <c>CC</c> table plus every <c>resend</c> the handler runs. A resend
///     bills one unit when its events carry <c>u</c> and one more when they lack <c>nc</c>, each at the method's
///     <c>call_modifier</c>: 1 for everything but <c>skill</c> (0.05), <c>target</c> (0.5) and <c>open_chest</c> (0.1).
///     <br />
///     There is no per-call base charge; the wrapper's <c>add_call_cost(-1)</c> lands on the previous handler's socket,
///     never on a player.
/// </remarks>
public static class CallCost
{
    /// <summary>
    ///     The server's <c>limits.calls</c>. Quartered for a socket with no player behind it yet, so the pre-login handshake
    ///     is metered four times as harshly.
    /// </summary>
    public const double LIMIT = 200d;

    /// <summary>
    ///     The sliding window the limit is measured over.
    /// </summary>
    public static readonly TimeSpan WINDOW = TimeSpan.FromSeconds(4);

    /// <summary>
    ///     The cost of a <c>u+cid</c> resend at modifier 1: a unit for <c>u</c> and a unit for the stats pass.
    /// </summary>
    private const double RESEND = 2d;

    /// <summary>
    ///     The server's <c>CC.equip</c> row.
    /// </summary>
    private const double EQUIP_ROW = 3d;

    /// <summary>
    ///     The <c>open_chest</c> modifier: the cost of one resend unit from inside that handler.
    /// </summary>
    private const double OPEN_CHEST = 0.1d;

    /// <summary>
    ///     The extra units billed by a resend that reopens somebody else's socket.
    /// </summary>
    private const double REOPEN_OTHER = 4d;

    /// <summary>
    ///     The cost <c>transport_player_to</c> bills whoever it moves, from a loop as readily as from a handler.
    /// </summary>
    /// <remarks>
    ///     A landed blink is a skill at 0.1 and then this when its condition runs out, which no row keyed on the emit type
    ///     can carry.
    /// </remarks>
    public const double TRANSPORT = 8d;

    /// <summary>
    ///     The cost of each emit type. <c>random_look</c> and <c>ccreport</c> are in the server's table too and are not on
    ///     this client's emit surface.
    /// </summary>
    /// <remarks>
    ///     A method absent here bills nothing: no <c>CC</c> row, and either no resend or a <c>reopen+nc+inv</c> one, which
    ///     is what every bench, shop and inventory handler sends.
    /// </remarks>
    private static readonly IReadOnlyDictionary<ALSocketEmitType, double> COSTS = new Dictionary<ALSocketEmitType, double>
    {
        //commence_attack sets u+cid on every swing that lands. attack and heal keep their own method names through
        //the skill handler, so they pay the full modifier and a skill does not
        [ALSocketEmitType.Attack] = RESEND,
        [ALSocketEmitType.Heal] = RESEND,
        [ALSocketEmitType.Skill] = 0.05d * RESEND,

        //CC rows, plus the resend where the handler makes one
        [ALSocketEmitType.Move] = 1.5d,
        [ALSocketEmitType.Cruise] = 10d + RESEND,
        [ALSocketEmitType.Equip] = EQUIP_ROW + RESEND,
        [ALSocketEmitType.Unequip] = 6d + RESEND,
        [ALSocketEmitType.Auth] = 2d,
        [ALSocketEmitType.Players] = 12d,
        [ALSocketEmitType.SendUpdates] = 12d,
        [ALSocketEmitType.SecondHands] = 16d,
        [ALSocketEmitType.Friend] = 24d,
        [ALSocketEmitType.Tracker] = 50d,

        //every handler that ends in transport_player_to, the town channel and a magiport included. A death is not
        //one: the body stays until the respawn moves it. A bank crossing bills CalculateBankCrossingCost beside the door
        [ALSocketEmitType.Transport] = TRANSPORT,
        [ALSocketEmitType.LeaveMap] = TRANSPORT,
        [ALSocketEmitType.Enter] = RESEND + TRANSPORT,
        [ALSocketEmitType.Respawn] = RESEND + TRANSPORT,
        [ALSocketEmitType.Magiport] = RESEND + TRANSPORT,
        [ALSocketEmitType.ReturnToTown] = RESEND / 2d + TRANSPORT,
        [ALSocketEmitType.Join] = RESEND / 2d + TRANSPORT,

        //a one-item batch; the emit hook does not see the count
        [ALSocketEmitType.EquipBatch] = CalculateEquipBatchCost(1),

        //resend-only handlers at modifier 1: u+cid, with or without a reopen of the caller's own socket. stop bills
        //only when it has a channel to cancel
        [ALSocketEmitType.Stop] = RESEND,
        [ALSocketEmitType.MonsterHunt] = RESEND,
        [ALSocketEmitType.Merchant] = RESEND,
        [ALSocketEmitType.Blend] = RESEND,
        [ALSocketEmitType.Interaction] = RESEND,
        [ALSocketEmitType.Activate] = RESEND,
        [ALSocketEmitType.Harakiri] = RESEND,
        [ALSocketEmitType.Cx] = RESEND,

        //one unit: u with nc, or a bare reopen. property is refunded one when the previous charged call was also
        //property, so a burst reads high by all but the first
        [ALSocketEmitType.Property] = RESEND / 2d,
        [ALSocketEmitType.Party] = RESEND / 2d,
        [ALSocketEmitType.Say] = RESEND / 2d,
        [ALSocketEmitType.Bank] = RESEND / 2d,
        [ALSocketEmitType.Booster] = RESEND / 2d,
        [ALSocketEmitType.Convert] = RESEND / 2d,
        [ALSocketEmitType.Split] = RESEND / 2d,
        [ALSocketEmitType.Destroy] = RESEND / 2d,
        [ALSocketEmitType.Donate] = RESEND / 2d,
        [ALSocketEmitType.Mail] = RESEND / 2d,
        [ALSocketEmitType.TakeMailItem] = RESEND / 2d,
        [ALSocketEmitType.TradeWishlist] = RESEND / 2d,

        //a bare reopen for the caller, then u+cid+reopen for the counterparty on their own socket
        [ALSocketEmitType.TradeBuy] = RESEND / 2d + RESEND + REOPEN_OTHER,
        [ALSocketEmitType.TradeSell] = RESEND / 2d + RESEND + REOPEN_OTHER,

        //one short message to one recipient; longer or wider costs more
        [ALSocketEmitType.Command] = 1d,

        //alone and with nothing in it. In a party it is CalculateChestOpenCost
        [ALSocketEmitType.OpenChest] = OPEN_CHEST
    };

    /// <summary>
    ///     Calculates what one emit of this type costs against <see cref="LIMIT" />.
    /// </summary>
    /// <param name="emitType">
    ///     The emit type.
    /// </param>
    /// <returns>
    ///     The emit's <c>CC</c> row plus its handler's resend, or 0 for a method with neither.
    ///     <see cref="ALSocketEmitType.EquipBatch" /> is priced as a batch of one; use
    ///     <see cref="CalculateEquipBatchCost" /> when the count is known.
    /// </returns>
    public static double CalculateCost(ALSocketEmitType emitType) => COSTS.GetValueOrDefault(emitType, 0d);

    /// <summary>
    ///     Calculates what a transport bills on top of <see cref="CalculateCost" /> for crossing the bank's threshold, under the
    ///     name <c>bank</c>: 32 to mount the account's bank on the way in, 16 to unmount it on the way out.
    /// </summary>
    /// <param name="fromBank">
    ///     Whether the map being left has the bank mounted.
    /// </param>
    /// <param name="toBank">
    ///     Whether the destination has the bank mounted.
    /// </param>
    /// <returns>
    ///     The extra cost, 0 for a door between two bank floors or two ordinary maps.
    /// </returns>
    public static double CalculateBankCrossingCost(bool fromBank, bool toBank)
        => (fromBank, toBank) switch
        {
            (false, true) => 32d,
            (true, false) => 16d,
            _             => 0d
        };

    /// <summary>
    ///     Calculates what one <c>open_chest</c> costs the opener. The handler resends every party member, the opener
    ///     included, and bills the opener for all of them.
    /// </summary>
    /// <param name="partySize">
    ///     Everybody in the party, wherever they are, or 1 when alone.
    /// </param>
    /// <param name="othersWithItems">
    ///     Members other than the opener who received an item from the chest.
    /// </param>
    /// <param name="openerGotItem">
    ///     Whether the opener received an item.
    /// </param>
    /// <returns>
    ///     A tenth for each member who got nothing, and <see cref="REOPEN_OTHER" /> tenths for each other member reopened
    ///     with an item.
    /// </returns>
    public static double CalculateChestOpenCost(int partySize, int othersWithItems, bool openerGotItem)
    {
        var emptyResends = Math.Max(0, partySize - othersWithItems - (openerGotItem ? 1 : 0));

        return OPEN_CHEST * (emptyResends + REOPEN_OTHER * othersWithItems);
    }

    /// <summary>
    ///     Calculates what one <c>equip_batch</c> costs against <see cref="LIMIT" />: <c>CC.equip * (0.5 + count/2)</c> plus
    ///     the one <c>reopen+u+cid</c> resend the handler ends on.
    /// </summary>
    /// <param name="count">
    ///     The number of items in the batch.
    /// </param>
    /// <returns>
    ///     The batch's cost; from two items up it is less than the same equips sent one at a time.
    /// </returns>
    public static double CalculateEquipBatchCost(int count) => EQUIP_ROW * (0.5d + Math.Max(0, count) / 2d) + RESEND;
}