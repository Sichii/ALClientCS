namespace AL.Pathfinding.Model;

/// <summary>
///     How a route is priced: whether a recall counts as a move, how fast the character walks, and when and how often it
///     may blink.
/// </summary>
/// <remarks>
///     A sealed record rather than a record struct, so a missing argument and <c>new PathOptions()</c> both mean "town
///     on", and <c>default</c> cannot silently mean "town off".
/// </remarks>
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
    ///     The shortest walk worth a blink, or null for a route with no blink in it. A cast is priced at the time it takes
    ///     (the wait for the cooldown, the penalty and the bar, plus the landing) and nothing else. This is a rule, not a
    ///     price: a cast may replace only a walk at least this long, may not land nearer than this to where the route
    ///     entered the map, and may not be chained around it by leaving a map and coming back.
    /// </summary>
    public float? BlinkCost { get; init; }

    /// <summary>
    ///     The mana regained per second, which the search refills the bar at between casts; null leaves the bar untracked and
    ///     unlimited. Tracking reads <see cref="Mp" /> and <see cref="MaxMp" />, so set both with it: a zero maximum can
    ///     never hold a cast.
    /// </summary>
    public float? BlinkMpPerSecond { get; init; }

    /// <summary>
    ///     Mana left in the bar after a cast. The walker stands still at a blink leg until the bar holds the skill's cost plus
    ///     this, and the search, when the bar is tracked, charges the same wait.
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
    ///     Whether a recall counts as a move. True prices one from anywhere on the route, the start map and every map the
    ///     route lands on alike; false leaves the route without a single recall leg.
    /// </summary>
    public bool UseTown { get; init; } = true;

    /// <summary>
    ///     The character's speed, which prices a recall and a blink; nominal when null. The walker overwrites it with the
    ///     character's own.
    /// </summary>
    public float? WalkSpeed { get; init; }
}