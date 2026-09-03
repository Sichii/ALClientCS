namespace AL.SocketClient.Definitions;

/// <summary>
///     The server's rate limiter, as it actually meters. Every socket handler is wrapped (node/server.js:4337) and
///     charges the method's entry in the server's own <c>CC</c> table (node/server.js:159); the accrued cost of the
///     last <see cref="WINDOW" /> passing <see cref="LIMIT" /> is a <c>limitdcreport</c> and an immediate kick.
/// </summary>
/// <remarks>
///     There is no per-call base charge. The wrapper's <c>add_call_cost(-1)</c> runs one line before
///     <c>current_socket</c> is set, so its 1 lands on the module's <c>false_socket</c> and is never metered - and a
///     method absent from <c>CC</c> costs the character nothing at all.
/// </remarks>
public static class CallCost
{
    /// <summary>
    ///     <c>limits.calls</c> (node/server.js:174). Quartered for a socket with no player behind it yet, so the
    ///     pre-login handshake is metered four times as harshly as this reads.
    /// </summary>
    public const double LIMIT = 200d;

    /// <summary>
    ///     What every call is billed before its method's own charge, if it has one. The wrapper opens with
    ///     <c>add_call_cost(-1)</c> (node/server.js:4347), and that sentinel pushes a fresh entry worth one rather
    ///     than adding to the last - so an attack, a skill or a use is one apiece, not free.
    /// </summary>
    /// <remarks>
    ///     The charge lands on whichever socket ran the previous handler, because <c>current_socket</c> is assigned
    ///     on the line after. That misattributes each individual unit by one call but conserves the count: every
    ///     call a socket makes is the predecessor of exactly one later call, so one per emit is what it collects
    ///     back. Leaving this out is what had the meter reading far under the server's own figure for a farming
    ///     character, whose emits are almost all rows the table below does not have.
    /// </remarks>
    public const double BASE = 1d;

    /// <summary>
    ///     The sliding window the limit is measured over - entries older than this are shifted off the front before
    ///     every read.
    /// </summary>
    public static readonly TimeSpan WINDOW = TimeSpan.FromSeconds(4);

    //node/server.js:159. random_look and ccreport are in the server's table too and are not on this client's emit
    //surface. equip_batch is deliberately absent: its charge is a function of the batch rather than a constant, so
    //Of cannot price it from the type alone and answers the bare base for it - see OfEquipBatch
    private static readonly IReadOnlyDictionary<ALSocketEmitType, double> COSTS
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
    ///     What one emit of this type costs against <see cref="LIMIT" />: <see cref="BASE" /> plus its row in
    ///     <c>CC</c>, which most methods have none of.
    /// </summary>
    /// <remarks>
    ///     <see cref="ALSocketEmitType.EquipBatch" /> is the one type this cannot answer for - it bills the batch's
    ///     items rather than the call, so ask <see cref="OfEquipBatch" /> instead. The bare base is what it hands
    ///     back here.
    /// </remarks>
    public static double Of(ALSocketEmitType emitType) => BASE + COSTS.GetValueOrDefault(emitType, 0d);

    /// <summary>
    ///     What one <c>equip_batch</c> carrying <paramref name="count" /> items costs against <see cref="LIMIT" />.
    /// </summary>
    /// <remarks>
    ///     <c>CC.equip * (0.5 + count/2)</c> over the base (node/server.js:4357), derived from the single-equip
    ///     charge rather than restated. From two items up it beats sending the same equips one at a time and the gap
    ///     widens with each: two cost 5.5 against 8, five cost 10 against 20. It buys nothing on the penalty
    ///     cooldown.
    /// </remarks>
    public static double OfEquipBatch(int count)
        => BASE + (COSTS[ALSocketEmitType.Equip] * (0.5d + (Math.Max(0, count) / 2d)));
}
