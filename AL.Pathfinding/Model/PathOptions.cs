namespace AL.Pathfinding.Model;

/// <summary>
///     How a route is priced: whether a recall counts as a move, how fast the character walks, and what a blink is worth.
/// </summary>
/// <remarks>
///     A sealed record rather than a record struct, so a missing argument and
///     <c>
///         new PathOptions()
///     </c>
///     both mean "town on", and
///     <c>
///         default
///     </c>
///     cannot silently mean "town off". <see cref="BlinkMpReserve" /> is the walker's alone: the search takes the bar as
///     unlimited and prices every cast at <see cref="BlinkCost" />.
/// </remarks>
public sealed record PathOptions
{
    /// <summary>
    ///     Town on, nominal speed, no blink.
    /// </summary>
    public static readonly PathOptions Default = new();

    /// <summary>
    ///     A route without a single recall leg.
    /// </summary>
    public static readonly PathOptions NoTown = new()
    {
        UseTown = false
    };

    /// <summary>
    ///     Whether a recall counts as a move. True prices one from anywhere on the route, the start map and every map the
    ///     route lands on alike; false leaves the route without a single recall leg.
    /// </summary>
    public bool UseTown { get; init; } = true;

    /// <summary>
    ///     The character's speed, which prices a recall; nominal when null. The walker overwrites it with the character's
    ///     own.
    /// </summary>
    public float? WalkSpeed { get; init; }

    /// <summary>
    ///     What a blink is worth in walk-distance units, or null for a route with no blink in it. A walk on one map dearer
    ///     than this comes back as one <see cref="AL.Pathfinding.Definitions.EdgeType.Blink" /> leg to the walk's own
    ///     target, charged this much; so does a pair on one map that no walk joins. Town and blink are then priced against
    ///     each other by the same search. Meant to sit at 400 or above: it stands in for the mana the cast spends, and a
    ///     cast priced at its real time would be taken everywhere.
    /// </summary>
    public float? BlinkCost { get; init; }

    /// <summary>
    ///     Mana the walker leaves in the bar after a cast. It stands still at a blink leg until the bar holds the skill's
    ///     cost plus this. Read by the walker only.
    /// </summary>
    public float BlinkMpReserve { get; init; }
}
