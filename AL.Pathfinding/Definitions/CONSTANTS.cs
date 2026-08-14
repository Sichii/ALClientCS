#region
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Definitions;

public static class CONSTANTS
{
    /// <summary>How long the town channel runs. The server caps it at 3000ms whatever it was asked for.</summary>
    public const float TOWN_CHANNEL_SECONDS = 3f;

    /// <summary>
    ///     What the channel is priced at over its raw duration.
    /// </summary>
    /// <remarks>
    ///     Small, because the speed it is applied to is now the character's own. The flat cost this replaced was 2.4x
    ///     the raw duration at a base speed, and that factor was carrying the spread between a slowed character and a
    ///     boosted one rather than any risk - which is what made it wrong for both ends of it.
    ///     <br />
    ///     What is left to charge for is the part of the channel a walk of the same length does not cost: an
    ///     interruption leaves the character where it started, and landing adds 3200ms of penalty cooldown to the next
    ///     skill where a walk adds none. 1.2 counts about 600ms of that against the three second channel. This is the
    ///     one number to move to make the bot town more or less readily.
    /// </remarks>
    public const float TOWN_RISK_PREMIUM = 1.2f;

    /// <summary>What the channel is priced at when the character's own speed is not to hand.</summary>
    /// <remarks>
    ///     A character's base speed before any boost. Used for a frame that has not filled in yet as well, where a
    ///     literal reading of a zero speed would make the channel free and win every search.
    /// </remarks>
    public const float NOMINAL_WALK_SPEED = 50f;

    /// <summary>
    ///     The cost of a town teleport in the walk-distance units every other edge is measured in: the ground the
    ///     character would have covered on foot while the channel ran, times <see cref="TOWN_RISK_PREMIUM" />.
    /// </summary>
    /// <remarks>
    ///     Speed-derived rather than flat because the thing being compared is a walk, and how far a walk gets in three
    ///     seconds is the whole of what makes the trade. A boosted character should reach for the channel less readily
    ///     than a slow one, and a flat cost had it the same for both.
    /// </remarks>
    public static float TownCost(float walkSpeed)
        => TOWN_CHANNEL_SECONDS * (walkSpeed > 0f ? walkSpeed : NOMINAL_WALK_SPEED) * TOWN_RISK_PREMIUM;

    /// <summary>
    ///     <see cref="TownCost" /> at <see cref="NOMINAL_WALK_SPEED" />, for a search given no speed to price against.
    /// </summary>
    public static readonly float NOMINAL_TOWN_COST = TownCost(NOMINAL_WALK_SPEED);

    /// <summary>
    ///     The heuristic value of a transport, door, or leave connection.
    /// </summary>
    public const float TRANSPORT_HEURISTIC = 50f;

    /// <summary>
    ///     How far a search will look for standable ground around a point the flood fill never reached, before giving
    ///     up on it. Sized to clear the widest padded band - a vertical line is padded by the bounding base's half
    ///     width on each side - with room for a corner where two bands stack. Past that the point is not a character
    ///     grazing a wall, it is one somewhere no walk should be starting from.
    /// </summary>
    public const int MAX_UNSTICK_DISTANCE = 24;

    /// <summary>
    ///     How many of the nearest mesh nodes a point off the mesh will try to reach before settling for the nearest
    ///     one whether it can be reached or not. Only the fallback path pays for this - a point inside a triangle is
    ///     answered from the triangle's own three vertices - and a walkable point almost always reaches the first
    ///     candidate, so the bound is there for the cases that do not. Sixteen left one map with a start whose whole
    ///     near neighbourhood was across a line; this closed it, and cost nothing measurable to do so.
    /// </summary>
    public const int REACHABLE_NODE_CANDIDATES = 64;

    /// <summary>
    ///     The default values of a player bounding base.
    /// </summary>
    public static readonly BoundingBase DEFAULT_BOUNDING_BASE = new(8, 7, 2);
}