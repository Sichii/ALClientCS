#region
using AL.Data;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents a route's state on arrival at a node: how long until blink may be cast, the penalty still pending, and the bar.
/// </summary>
internal readonly record struct TravelState(float BlinkReadyInMs, float PenaltyMs, float Mp);

/// <summary>
///     Provides the server's rules for advancing a <see cref="TravelState" /> across one move, for one search's options.
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
    ///     settings.
    /// </summary>
    /// <param name="options">
    ///     The search's options; the bar is tracked only when <see cref="PathOptions.BlinkMpPerSecond" /> is set.
    /// </param>
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
    ///     Creates the state at the route's start; an untracked bar is held at zero.
    /// </summary>
    /// <param name="options">
    ///     The search's options.
    /// </param>
    /// <returns>
    ///     The starting state.
    /// </returns>
    public TravelState CreateInitialState(PathOptions options)
        => new(Math.Max(0f, options.BlinkReadyInMs), Math.Max(0f, options.PenaltyMs), Tracked ? options.Mp : 0f);

    /// <summary>
    ///     Advances a state by a span of time: both timers run down to zero, and a tracked bar refills to its maximum.
    /// </summary>
    /// <param name="state">
    ///     The state before.
    /// </param>
    /// <param name="ms">
    ///     The time that passes, in milliseconds.
    /// </param>
    /// <returns>
    ///     The state after.
    /// </returns>
    public TravelState AdvanceTime(TravelState state, float ms)
        => new(
            Math.Max(0f, state.BlinkReadyInMs - ms),
            Math.Max(0f, state.PenaltyMs - ms),
            Tracked ? Math.Min(MaxMp, state.Mp + MpPerSecond * ms / 1000f) : 0f);

    /// <summary>
    ///     Adds <c>penalty_cd</c> to a state, capped at <see cref="CONSTANTS.PENALTY_CAP_MS" />.
    /// </summary>
    /// <param name="state">
    ///     The state before.
    /// </param>
    /// <param name="ms">
    ///     The penalty to add, in milliseconds.
    /// </param>
    /// <returns>
    ///     The state after.
    /// </returns>
    public static TravelState AddPenalty(TravelState state, float ms)
        => state with
        {
            PenaltyMs = Math.Min(CONSTANTS.PENALTY_CAP_MS, state.PenaltyMs + ms)
        };

    /// <summary>
    ///     Casts one blink from a state, waiting first for the cooldown and the bar.
    /// </summary>
    /// <param name="state">
    ///     The state before the cast.
    /// </param>
    /// <param name="spentMs">
    ///     The wait plus the landing, in milliseconds.
    /// </param>
    /// <param name="after">
    ///     The state on landing.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the cast can go out; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     when a tracked bar can never pay for it.
    /// </returns>
    public bool TryBlink(TravelState state, out float spentMs, out TravelState after)
    {
        spentMs = 0f;
        after = state;
        var waitMs = CalculateWaitMs(state);

        if (float.IsPositiveInfinity(waitMs))
            return false;

        var cast = AdvanceTime(state, waitMs);

        cast = new TravelState(
            Math.Min(cast.PenaltyMs, CONSTANTS.PENALTY_CHARGE_CAP_MS) + BlinkCooldownMs,
            cast.PenaltyMs,
            Tracked ? cast.Mp - BlinkMp : 0f);

        after = AddPenalty(AdvanceTime(cast, CONSTANTS.BLINK_LANDING_MS), CONSTANTS.EFFECT_PENALTY_MS);
        spentMs = waitMs + CONSTANTS.BLINK_LANDING_MS;

        return true;
    }

    /// <summary>
    ///     Calculates how long until a cast can go out: the cooldown, and the refill a tracked bar needs.
    /// </summary>
    /// <param name="state">
    ///     The state to wait from.
    /// </param>
    /// <returns>
    ///     The wait in milliseconds, or infinity when the bar can never hold a cast.
    /// </returns>
    public float CalculateWaitMs(TravelState state)
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
    ///     Determines whether one state is at least as ready as another now, and at the moment each could next cast.
    /// </summary>
    /// <param name="a">
    ///     The state that must be as ready.
    /// </param>
    /// <param name="b">
    ///     The state it is compared to.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if <paramref name="a" /> is no later, has no more penalty pending and no less in the bar at both moments;
    ///     otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    /// <remarks>
    ///     A wait absorbed by a cast's least price costs nothing, and the penalty that runs down during it readies the next cast.
    /// </remarks>
    public bool IsAtLeastAsWellPlaced(TravelState a, TravelState b)
    {
        if (!IsAtLeastAsReady(a, b))
            return false;

        var waitB = CalculateWaitMs(b);

        //b can never cast, so nothing after this point favours it
        if (float.IsPositiveInfinity(waitB))
            return true;

        var waitA = CalculateWaitMs(a);

        if (Math.Max(0f, a.PenaltyMs - waitA) > Math.Max(0f, b.PenaltyMs - waitB))
            return false;

        return !Tracked || (Math.Min(MaxMp, a.Mp + MpPerSecond * waitA / 1000f) >= Math.Min(MaxMp, b.Mp + MpPerSecond * waitB / 1000f));
    }

    /// <summary>
    ///     Determines whether one state is at least as ready as another on every count.
    /// </summary>
    /// <param name="a">
    ///     The state that must be as ready.
    /// </param>
    /// <param name="b">
    ///     The state it is compared to.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if <paramref name="a" /> is no later on blink, has no more penalty and no less mana; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public static bool IsAtLeastAsReady(TravelState a, TravelState b)
        => (a.BlinkReadyInMs <= b.BlinkReadyInMs) && (a.PenaltyMs <= b.PenaltyMs) && (a.Mp >= b.Mp);
}