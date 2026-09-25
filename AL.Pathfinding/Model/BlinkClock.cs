#region
using AL.Data;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     A route's state on arrival at a node: how long until blink may be cast, the penalty still pending, and the bar.
/// </summary>
internal readonly record struct TravelState(float BlinkReadyInMs, float PenaltyMs, float Mp);

/// <summary>
///     Advances a <see cref="TravelState" /> across one move under the server's rules, for one search's options.
/// </summary>
internal readonly struct BlinkClock
{
    private readonly float BlinkCooldownMs;
    private readonly float BlinkMp;
    private readonly float MaxMp;
    private readonly float MpPerSecond;
    private readonly float Reserve;
    private readonly bool Tracked;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlinkClock" /> struct from blink's game data and the search's bar
    ///     settings; the bar is tracked only when <see cref="PathOptions.BlinkMpPerSecond" /> is set.
    /// </summary>
    public BlinkClock(PathOptions options)
    {
        BlinkCooldownMs = GameData.Skills.Blink.CooldownMS;
        BlinkMp = GameData.Skills.Blink.MP;
        MaxMp = options.MaxMp;
        MpPerSecond = options.BlinkMpPerSecond ?? 0f;
        Reserve = options.BlinkMpReserve;
        Tracked = options.BlinkMpPerSecond is not null;
    }

    /// <summary>
    ///     The state at the route's start, read from <paramref name="options" />; an untracked bar is held at zero.
    /// </summary>
    public TravelState Start(PathOptions options)
        => new(Math.Max(0f, options.BlinkReadyInMs), Math.Max(0f, options.PenaltyMs), Tracked ? options.Mp : 0f);

    /// <summary>
    ///     The state after <paramref name="ms" /> pass: both timers run down to no less than zero, and a tracked bar refills
    ///     to no more than its maximum.
    /// </summary>
    public TravelState Pass(TravelState state, float ms)
        => new(
            Math.Max(0f, state.BlinkReadyInMs - ms),
            Math.Max(0f, state.PenaltyMs - ms),
            Tracked ? Math.Min(MaxMp, state.Mp + MpPerSecond * ms / 1000f) : 0f);

    /// <summary>
    ///     The state after a map change adds <paramref name="ms" /> of <c>penalty_cd</c>, capped where the server caps it.
    /// </summary>
    public static TravelState AddPenalty(TravelState state, float ms)
        => state with
        {
            PenaltyMs = Math.Min(CONSTANTS.PENALTY_CAP_MS, state.PenaltyMs + ms)
        };

    /// <summary>
    ///     One cast from <paramref name="state" />: false when a tracked bar can never pay for it.
    /// </summary>
    public bool TryBlink(TravelState state, out float spentMs, out TravelState after)
    {
        spentMs = 0f;
        after = state;
        var waitMs = WaitMs(state);

        if (float.IsPositiveInfinity(waitMs))
            return false;

        var cast = Pass(state, waitMs);

        cast = new TravelState(
            Math.Min(cast.PenaltyMs, CONSTANTS.PENALTY_CHARGE_CAP_MS) + BlinkCooldownMs,
            cast.PenaltyMs,
            Tracked ? cast.Mp - BlinkMp : 0f);

        after = AddPenalty(Pass(cast, CONSTANTS.BLINK_LANDING_MS), CONSTANTS.EFFECT_PENALTY_MS);
        spentMs = waitMs + CONSTANTS.BLINK_LANDING_MS;

        return true;
    }

    /// <summary>
    ///     How long from <paramref name="state" /> until a cast can go out: the cooldown, and the refill a tracked bar needs;
    ///     infinity when the bar can never hold one.
    /// </summary>
    public float WaitMs(TravelState state)
    {
        if (!Tracked)
            return state.BlinkReadyInMs;

        var need = BlinkMp + Reserve;

        if (need > MaxMp)
            return float.PositiveInfinity;

        if (state.Mp >= need)
            return state.BlinkReadyInMs;

        if (MpPerSecond <= 0f)
            return float.PositiveInfinity;

        return Math.Max(state.BlinkReadyInMs, (need - state.Mp) / MpPerSecond * 1000f);
    }

    /// <summary>
    ///     Whether <paramref name="a" /> is at least as ready as <paramref name="b" /> now, and at the moment each could next
    ///     cast: no later, with no more penalty still pending and no less in the bar.
    /// </summary>
    /// <remarks>
    ///     Readiness now is not enough once a cast has a least price: a wait the price absorbs costs nothing, and the penalty
    ///     that runs down during it leaves the cast after it ready sooner. The moment of the next cast holds both. Walking
    ///     leaves the penalty and the bar at that moment unchanged, so the comparison holds until one of them casts.
    /// </remarks>
    public bool AtLeastAsWellPlaced(TravelState a, TravelState b)
    {
        if (!AtLeastAsReady(a, b))
            return false;

        var waitB = WaitMs(b);

        //b can never cast, so nothing after this point favours it
        if (float.IsPositiveInfinity(waitB))
            return true;

        var waitA = WaitMs(a);

        if (Math.Max(0f, a.PenaltyMs - waitA) > Math.Max(0f, b.PenaltyMs - waitB))
            return false;

        return !Tracked || (Math.Min(MaxMp, a.Mp + MpPerSecond * waitA / 1000f) >= Math.Min(MaxMp, b.Mp + MpPerSecond * waitB / 1000f));
    }

    /// <summary>
    ///     Whether <paramref name="a" /> is at least as ready as <paramref name="b" /> on every count.
    /// </summary>
    public static bool AtLeastAsReady(TravelState a, TravelState b)
        => (a.BlinkReadyInMs <= b.BlinkReadyInMs) && (a.PenaltyMs <= b.PenaltyMs) && (a.Mp >= b.Mp);
}