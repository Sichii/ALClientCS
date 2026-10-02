namespace AL.Pathfinding.Model;

/// <summary>
///     Represents how a route is priced: whether a recall counts as a move, how fast the character walks, and when and how
///     often it may blink.
/// </summary>
public sealed record PathOptions
{
    /// <summary>Town on, nominal speed, no blink.</summary>
    public static readonly PathOptions Default = new();

    /// <summary>A route without a single recall leg.</summary>
    public static readonly PathOptions NoTown = new()
    {
        UseTown = false
    };

    /// <summary>
    ///     If populated, the shortest walk a blink may replace, and the least a cast is priced at.
    /// </summary>
    /// <remarks>
    ///     A cast costs the time it takes (cooldown, penalty and bar waits plus the landing) when that comes to more.
    /// </remarks>
    public float? BlinkCost { get; init; }

    /// <summary>
    ///     If populated, the mana regained per second between casts; null leaves the bar untracked and unlimited.
    /// </summary>
    /// <remarks>
    ///     Tracking reads <see cref="Mp" /> and <see cref="MaxMp" />, so set both with it: a zero maximum never holds a cast.
    /// </remarks>
    public float? BlinkMpPerSecond { get; init; }

    /// <summary>
    ///     The mana left in the bar after a cast. A blink leg waits until the bar holds the skill's cost plus this.
    /// </summary>
    public float BlinkMpReserve { get; init; }

    /// <summary>
    ///     How long until blink may be cast at the start, in milliseconds.
    /// </summary>
    public float BlinkReadyInMs { get; init; }

    /// <summary>
    ///     The largest the bar can hold; read only when <see cref="BlinkMpPerSecond" /> is set.
    /// </summary>
    public float MaxMp { get; init; }

    /// <summary>
    ///     The mana in the bar at the start; read only when <see cref="BlinkMpPerSecond" /> is set.
    /// </summary>
    public float Mp { get; init; }

    /// <summary>
    ///     The <c>penalty_cd</c> still pending at the start, in milliseconds.
    /// </summary>
    public float PenaltyMs { get; init; }

    /// <summary>
    ///     Whether a recall counts as a move, from the start map and every map the route lands on alike.
    /// </summary>
    public bool UseTown { get; init; } = true;

    /// <summary>
    ///     If populated, the character's speed, which prices a recall and a blink; nominal when null.
    /// </summary>
    public float? WalkSpeed { get; init; }
}