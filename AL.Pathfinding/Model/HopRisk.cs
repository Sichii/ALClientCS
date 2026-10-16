namespace AL.Pathfinding.Model;

/// <summary>
///     Provides the replay of the server's in-motion penalty along a hop: every tick the score decays by
///     <see cref="SCORE_DECAY" /> and gains the lattice value under the character less one, and a score past 60 costs half
///     the character's hp and sends it back to the map's spawn.
/// </summary>
internal static class HopRisk
{
    /// <summary>
    ///     The distance between two samples of a hop's depth profile.
    /// </summary>
    public const float SAMPLE_STEP = 5f;

    /// <summary>
    ///     The highest peak a hop may reach: 5 under the server's limit of 60, which covers an estimate running a lattice level
    ///     low near the shore; the depth estimate's own overstatement covers the rest.
    /// </summary>
    public const float SAFE_SCORE = 55f;

    /// <summary>
    ///     The score past which the server halves the character's hp and sends it back to the map's spawn.
    /// </summary>
    public const float SERVER_LIMIT = 60f;

    private const float SCORE_DECAY = 0.99f;

    /// <summary>
    ///     The score a walk rounds down to 0; under it no hop's peak moves by more than a point, which
    ///     <see cref="SAFE_SCORE" />'s margin covers.
    /// </summary>
    private const float MIN_SCORE = 1f;

    /// <summary>
    ///     The shortest gap between two of the server's updates of a moving player; a shorter gap means more ticks and a
    ///     higher score, so the replay assumes this one.
    /// </summary>
    private const float SERVER_TICK_SECONDS = 0.075f;

    /// <summary>
    ///     The gap assumed between two of the server's updates of a moving player when decaying a score: a little over the
    ///     75 ms the server reschedules its loop at unless the loop itself runs long.
    /// </summary>
    private const float DECAY_TICK_SECONDS = 0.083f;

    private const float LATTICE_STEP = 10f;
    private const float MAX_PENALTY = 7f;

    /// <summary>
    ///     Turns a straight-line distance into an upper bound on the lattice's four-neighbour steps.
    /// </summary>
    private static readonly float STEPS_PER_DISTANCE = MathF.Sqrt(2f) / LATTICE_STEP;

    /// <summary>
    ///     Calculates the score one tick adds while the character stands at the given depth from solid ground.
    /// </summary>
    /// <param name="depth">
    ///     The distance from the character to the nearest solid ground.
    /// </param>
    /// <returns>The penalty, from 0 to 7.</returns>
    /// <remarks>
    ///     Near the shore this can run one lattice level low, since the server truncates positions to its 10-unit lattice;
    ///     <see cref="SAFE_SCORE" />'s margin and the server's walkable area reaching past our mesh edges absorb that.
    /// </remarks>
    public static float CalculatePenalty(float depth)
    {
        if (depth <= 0f)
            return 0f;

        var level = MathF.Ceiling(depth * STEPS_PER_DISTANCE);

        return Math.Clamp(level - 1f, 0f, MAX_PENALTY);
    }

    /// <summary>
    ///     Calculates the highest score reached while replaying a straight move along a depth profile.
    /// </summary>
    /// <param name="depths">
    ///     The depth at every <see cref="SAMPLE_STEP" /> along the move.
    /// </param>
    /// <param name="length">The length of the move.</param>
    /// <param name="speed">The character's speed in units per second.</param>
    /// <returns>
    ///     The peak score; infinite when the speed is not positive or too slow to cross.
    /// </returns>
    public static float CalculatePeakScore(ReadOnlySpan<float> depths, float length, float speed)
        => Replay(
                depths,
                length,
                speed,
                0f)
            .Peak;

    /// <summary>
    ///     Replays a straight move along a depth profile from a score already carried into it.
    /// </summary>
    /// <param name="depths">
    ///     The depth at every <see cref="SAMPLE_STEP" /> along the move.
    /// </param>
    /// <param name="length">The length of the move.</param>
    /// <param name="speed">The character's speed in units per second.</param>
    /// <param name="startScore">The score at the start of the move.</param>
    /// <returns>
    ///     The highest score after any tick, and the score after the last; both infinite when the speed is not positive or the
    ///     crossing would take over 100,000 server updates.
    /// </returns>
    public static (float Peak, float End) Replay(
        ReadOnlySpan<float> depths,
        float length,
        float speed,
        float startScore)
    {
        //over two hours of crossing; past it the count would also overflow the loop's int
        const float MAX_TICKS = 100_000f;

        if (speed is not > 0f)
            return (float.PositiveInfinity, float.PositiveInfinity);

        var step = speed * SERVER_TICK_SECONDS;
        var tickCount = MathF.Max(1f, MathF.Ceiling(length / step));

        //written to fail on NaN too, which a step underflowing to 0 over no length gives
        if (tickCount is not <= MAX_TICKS)
            return (float.PositiveInfinity, float.PositiveInfinity);

        var ticks = (int)tickCount;
        var last = depths.Length - 1;
        var score = startScore;
        var peak = 0f;

        for (var tick = 1; tick <= ticks; tick++)
        {
            var at = MathF.Min(tick * step, length);
            var sample = Math.Min(last, (int)(at / SAMPLE_STEP));

            //the samples bracket the character; the deeper one keeps the estimate from lagging into the water
            var depth = MathF.Max(depths[sample], depths[Math.Min(last, sample + 1)]);

            score = score * SCORE_DECAY + CalculatePenalty(depth);
            peak = MathF.Max(peak, score);
        }

        return (peak, score);
    }

    /// <summary>
    ///     Calculates the score left after walking on solid ground, where every tick only decays it.
    /// </summary>
    /// <param name="score">The score at the start of the walk.</param>
    /// <param name="seconds">How long the walk takes.</param>
    /// <returns>
    ///     The decayed score, or 0 once it falls under <see cref="MIN_SCORE" />.
    /// </returns>
    /// <remarks>
    ///     Without the floor a decayed score never reaches 0, so a search keeps every longer walk as a new lower-score arrival
    ///     and never ends when no route exists.
    /// </remarks>
    public static float DecayWhileWalking(float score, float seconds)
    {
        var decayed = score * MathF.Pow(SCORE_DECAY, seconds / DECAY_TICK_SECONDS);

        return decayed < MIN_SCORE ? 0f : decayed;
    }

    /// <summary>
    ///     Determines whether a move started from a score of 0 stays under <see cref="SAFE_SCORE" /> at the given speed.
    /// </summary>
    /// <param name="depths">
    ///     The depth at every <see cref="SAMPLE_STEP" /> along the move.
    /// </param>
    /// <param name="length">The length of the move.</param>
    /// <param name="speed">The character's speed in units per second.</param>
    /// <returns>
    ///     <c>true</c> if the speed is positive and the peak stays under the limit; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsSafe(ReadOnlySpan<float> depths, float length, float speed)
        => CalculatePeakScore(depths, length, speed) < SAFE_SCORE;
}