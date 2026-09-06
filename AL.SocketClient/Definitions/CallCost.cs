namespace AL.SocketClient.Definitions;

/// <summary>
///     The server's rate limiter, as it actually meters. Every socket handler is wrapped (node/server.js:4337), and the
///     accrued cost of the last <see cref="WINDOW" /> passing <see cref="LIMIT" /> is a
///     <c>
///         limitdcreport
///     </c>
///     and an immediate kick.
/// </summary>
/// <remarks>
///     A call is billed in two places, and <see cref="Of" /> is the sum of both: the method's row in the server's
///     <c>
///         CC
///     </c>
///     table (node/server.js:159), which most methods have none of, and every
///     <c>
///         resend
///     </c>
///     the handler runs before it returns (node/server.js:4017). A resend bills one unit when its events carry
///     <c>
///         u
///     </c>
///     and one more when they lack
///     <c>
///         nc
///     </c>
///     , each at the method's
///     <c>
///         call_modifier
///     </c>
///     : 1 for everything but
///     <c>
///         skill
///     </c>
///     (0.05),
///     <c>
///         target
///     </c>
///     (0.5) and
///     <c>
///         open_chest
///     </c>
///     (0.1). So an attack,
///     <c>
///         u+cid
///     </c>
///     at 1, is two; the same resend from a skill is a fifth of one.
///     <br />
///     There is no per-call base charge. The wrapper's
///     <c>
///         add_call_cost(-1)
///     </c>
///     runs before
///     <c>
///         current_socket
///     </c>
///     is set, and the previous handler left that pointing at the module's
///     <c>
///         false_socket
///     </c>
///     (node/server.js:4421), so the unit never lands on a player. Billing one anyway reads a skill-heavy character at
///     about double the server; leaving the resends out reads an attack-heavy one at about half.
/// </remarks>
public static class CallCost
{
    /// <summary>
    ///     <c>
    ///         limits.calls
    ///     </c>
    ///     (node/server.js:174). Quartered for a socket with no player behind it yet, so the pre-login handshake is metered
    ///     four times as harshly as this reads.
    /// </summary>
    public const double LIMIT = 200d;

    /// <summary>
    ///     The sliding window the limit is measured over - entries older than this are shifted off the front before every
    ///     read.
    /// </summary>
    public static readonly TimeSpan WINDOW = TimeSpan.FromSeconds(4);

    /// <summary>
    ///     What a
    ///     <c>
    ///         u+cid
    ///     </c>
    ///     resend bills at modifier 1: a unit for
    ///     <c>
    ///         u
    ///     </c>
    ///     and a unit for the stats pass.
    /// </summary>
    private const double RESEND = 2d;

    /// <summary>
    ///     <c>
    ///         CC.equip
    ///     </c>
    ///     , kept apart from the equip row because <see cref="OfEquipBatch" /> scales it alone.
    /// </summary>
    private const double EQUIP_ROW = 3d;

    /// <summary>
    ///     The
    ///     <c>
    ///         open_chest
    ///     </c>
    ///     modifier: what one unit of a resend costs from inside that handler.
    /// </summary>
    private const double OPEN_CHEST = 0.1d;

    /// <summary>
    ///     What a resend that reopens somebody else's socket bills on top, in units (node/server.js:4051).
    /// </summary>
    private const double REOPEN_OTHER = 4d;

    /// <summary>
    ///     What
    ///     <c>
    ///         transport_player_to
    ///     </c>
    ///     bills whoever it moves (node/server.js:4180), from a loop as readily as from a handler. Public for the one cast
    ///     that ends in it - a landed blink is a skill at 0.1 and then this when its condition runs out
    ///     (node/server.js:13386), which no row keyed on the emit type can carry.
    /// </summary>
    public const double TRANSPORT = 8d;

    //random_look and ccreport are in the server's table too and are not on this client's emit surface. A method
    //absent here bills nothing: no CC row, and either no resend or one carrying nc without u (reopen+nc+inv, which
    //is what every bench, shop and inventory handler sends)
    private static readonly IReadOnlyDictionary<ALSocketEmitType, double> COSTS = new Dictionary<ALSocketEmitType, double>
    {
        //commence_attack sets u+cid on every swing that lands (node/server.js:3180). attack and heal keep
        //their own method names through the skill handler, so they pay the full modifier and a skill does not
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

        //every handler that ends in transport_player_to carries TRANSPORT, the town channel and a magiport
        //included though theirs lands seconds later. A death is not one: the body lies where it fell until the
        //respawn moves it. A bank crossing bills OfBankCrossing beside the door
        [ALSocketEmitType.Transport] = TRANSPORT,
        [ALSocketEmitType.LeaveMap] = TRANSPORT,
        [ALSocketEmitType.Enter] = RESEND + TRANSPORT,
        [ALSocketEmitType.Respawn] = RESEND + TRANSPORT,
        [ALSocketEmitType.Magiport] = RESEND + TRANSPORT,
        [ALSocketEmitType.ReturnToTown] = RESEND / 2d + TRANSPORT,
        [ALSocketEmitType.Join] = RESEND / 2d + TRANSPORT,

        //a one-item batch. The real bill is OfEquipBatch, but the emit hook does not see the count
        [ALSocketEmitType.EquipBatch] = OfEquipBatch(1),

        //resend-only handlers at modifier 1: u+cid, with or without a reopen of the caller's own socket. stop
        //bills only when it has a channel to cancel, which is the only reason this client sends it
        [ALSocketEmitType.Stop] = RESEND,
        [ALSocketEmitType.MonsterHunt] = RESEND,
        [ALSocketEmitType.Merchant] = RESEND,
        [ALSocketEmitType.Blend] = RESEND,
        [ALSocketEmitType.Interaction] = RESEND,
        [ALSocketEmitType.Activate] = RESEND,
        [ALSocketEmitType.Harakiri] = RESEND,
        [ALSocketEmitType.Cx] = RESEND,

        //one unit: u with nc, or a bare reopen, which lacks nc and so still pays the stats pass. property is
        //refunded one when the previous charged call was also property (node/server.js:5127), so a burst of
        //them reads high here by all but the first
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

        //one short message to one recipient (node/server.js:4366); longer or wider costs more
        [ALSocketEmitType.Command] = 1d,

        //alone and with nothing in it: an empty resend lacks nc. In a party it is OfChestOpen
        [ALSocketEmitType.OpenChest] = OPEN_CHEST
    };

    /// <summary>
    ///     What one emit of this type costs against <see cref="LIMIT" />: its
    ///     <c>
    ///         CC
    ///     </c>
    ///     row plus what its handler's resend bills, and nothing for a method that has neither.
    /// </summary>
    /// <remarks>
    ///     <see cref="ALSocketEmitType.EquipBatch" /> is priced here as a batch of one, because the count is not a function of
    ///     the type - ask <see cref="OfEquipBatch" /> when it is known.
    /// </remarks>
    public static double Of(ALSocketEmitType emitType) => COSTS.GetValueOrDefault(emitType, 0d);

    /// <summary>
    ///     What a transport bills on top of <see cref="Of" /> for crossing the bank's threshold: 32 to mount the account's
    ///     bank on the way in, 16 to unmount it on the way out (node/server.js:5569, :5580), under the name
    ///     <c>
    ///         bank
    ///     </c>
    ///     beside the door's own charge. A door between two bank floors, or two ordinary maps, adds nothing.
    /// </summary>
    /// <param name="fromBank">
    ///     Whether the map being left has the bank mounted (
    ///     <c>
    ///         GMap.Mount
    ///     </c>
    ///     ).
    /// </param>
    /// <param name="toBank">
    ///     Whether the destination does.
    /// </param>
    public static double OfBankCrossing(bool fromBank, bool toBank)
        => (fromBank, toBank) switch
        {
            (false, true) => 32d,
            (true, false) => 16d,
            _             => 0d
        };

    /// <summary>
    ///     What one
    ///     <c>
    ///         open_chest
    ///     </c>
    ///     costs in a party of <paramref name="partySize" />, one meaning alone. The handler resends every member, the opener
    ///     included, and the whole bill lands on the opener (node/server.js:10460). A member who got nothing is an empty
    ///     resend, a tenth; one who got an item is reopened instead, which is free for the opener and
    ///     <see cref="REOPEN_OTHER" /> tenths for anybody else.
    /// </summary>
    /// <param name="partySize">
    ///     Everybody in the party, wherever they are - the server does not check the map.
    /// </param>
    /// <param name="othersWithItems">
    ///     Members other than the opener who received an item from the chest.
    /// </param>
    /// <param name="openerGotItem">
    ///     Whether the opener received one.
    /// </param>
    public static double OfChestOpen(int partySize, int othersWithItems, bool openerGotItem)
    {
        var emptyResends = Math.Max(0, partySize - othersWithItems - (openerGotItem ? 1 : 0));

        return OPEN_CHEST * (emptyResends + REOPEN_OTHER * othersWithItems);
    }

    /// <summary>
    ///     What one
    ///     <c>
    ///         equip_batch
    ///     </c>
    ///     carrying <paramref name="count" /> items costs against <see cref="LIMIT" />.
    /// </summary>
    /// <remarks>
    ///     <c>
    ///         CC.equip * (0.5 + count/2)
    ///     </c>
    ///     (node/server.js:4357) plus the one
    ///     <c>
    ///         reopen+u+cid
    ///     </c>
    ///     resend the handler ends on. From two items up it beats sending the same equips one at a time and the gap widens
    ///     with each: two cost 6.5 against 10, five cost 11 against 25. It buys nothing on the penalty cooldown.
    /// </remarks>
    public static double OfEquipBatch(int count) => EQUIP_ROW * (0.5d + Math.Max(0, count) / 2d) + RESEND;
}