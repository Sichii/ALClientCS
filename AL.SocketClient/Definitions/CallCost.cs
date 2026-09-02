namespace AL.SocketClient.Definitions;

/// <summary>
///     The server's rate limiter, as it actually meters. Every socket handler is wrapped (node/server.js:4337): it
///     charges <see cref="BASE" /> for the call itself and then a per-method surcharge from the server's own
///     <c>
///         CC
///     </c>
///     table (node/server.js:159). The accrued cost of the last <see cref="WINDOW" /> is compared against
///     <see cref="LIMIT" />, and exceeding it is an immediate <c>limitdcreport</c> plus a disconnect.
/// </summary>
/// <remarks>
///     <see cref="Of" /> is a <b>floor</b>, not the whole bill. A handler's own work bills further: <c>resend</c> and
///     <c>calculate_player_stats</c> each add a call modifier, which is 1 for most methods but 0.05 for <c>skill</c>,
///     0.5 for <c>target</c> and 0.1 for <c>open_chest</c> - so a skill is cheaper than this says and a move is
///     dearer. The authoritative running total is the server's own, which it sends on every <c>player</c> frame as
///     <c>cc</c>; use this to apportion blame between callers and that to know how close to the ceiling you are.
/// </remarks>
public static class CallCost
{
    /// <summary>
    ///     Charged for every emit regardless of method - <c>add_call_cost(-1)</c> at the top of the wrapper, which
    ///     pushes an entry costing exactly 1 rather than decrementing anything.
    /// </summary>
    public const double BASE = 1d;

    /// <summary>
    ///     <c>limits.calls</c> (node/server.js:174). Quartered for a socket with no player behind it yet, so the
    ///     pre-login handshake is metered four times as harshly as this reads.
    /// </summary>
    public const double LIMIT = 200d;

    /// <summary>
    ///     The sliding window the limit is measured over - entries older than this are shifted off the front before
    ///     every read.
    /// </summary>
    public static readonly TimeSpan WINDOW = TimeSpan.FromSeconds(4);

    //node/server.js:159. random_look and ccreport are in the server's table too and are not on this client's emit
    //surface. equip_batch is deliberately absent: its surcharge is a function of the batch rather than a constant,
    //so Of cannot price it from the type alone and bills it as a bare BASE - see OfEquipBatch
    private static readonly IReadOnlyDictionary<ALSocketEmitType, double> SURCHARGES
        = new Dictionary<ALSocketEmitType, double>
        {
            [ALSocketEmitType.Auth] = 2d,
            [ALSocketEmitType.Move] = 1.5d,
            [ALSocketEmitType.Players] = 12d,
            [ALSocketEmitType.SecondHands] = 16d,
            [ALSocketEmitType.Friend] = 24d,
            [ALSocketEmitType.SendUpdates] = 12d,
            [ALSocketEmitType.Cruise] = 10d,
            [ALSocketEmitType.Equip] = 3d,
            [ALSocketEmitType.Unequip] = 6d,
            [ALSocketEmitType.Tracker] = 50d
        };

    /// <summary>
    ///     What one emit of this type costs against <see cref="LIMIT" />, at minimum.
    /// </summary>
    /// <remarks>
    ///     <see cref="ALSocketEmitType.EquipBatch" /> is the one type this cannot answer for - it bills the batch's
    ///     items rather than the call, so ask <see cref="OfEquipBatch" /> instead. What this hands back for it is the
    ///     bare <see cref="BASE" />, which keeps a meter summing over emit types a floor rather than a fiction.
    /// </remarks>
    public static double Of(ALSocketEmitType emitType)
        => BASE + (SURCHARGES.TryGetValue(emitType, out var surcharge) ? surcharge : 0d);

    /// <summary>
    ///     What one <c>equip_batch</c> carrying <paramref name="count" /> items costs against <see cref="LIMIT" />.
    /// </summary>
    /// <remarks>
    ///     The server charges <c>CC.equip * (0.5 + count/2)</c> on top of the call itself (node/server.js:4357), so
    ///     the surcharge is derived from the single-equip one rather than restated. From two items up this beats
    ///     sending the same equips one at a time and the gap widens with each item: two cost 5.5 against 8, five cost
    ///     10 against 20. It buys nothing on the penalty cooldown, which the handler charges per item either way.
    ///     <br />
    ///     The server clamps a batch to 15 items and drops the rest silently, so a caller splitting a longer run has
    ///     to split it itself.
    /// </remarks>
    public static double OfEquipBatch(int count)
        => BASE + ((Of(ALSocketEmitType.Equip) - BASE) * (0.5d + (Math.Max(0, count) / 2d)));
}
