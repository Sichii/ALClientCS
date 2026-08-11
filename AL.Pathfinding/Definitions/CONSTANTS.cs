#region
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Definitions;

public static class CONSTANTS
{
    /// <summary>
    ///     The cost of a town teleport, in the same walk-distance units every other edge is measured in. The channel
    ///     itself is 3 seconds - about 150 units at a speed of 50 - so this carries a premium over the raw time, since
    ///     an interrupted town costs the whole search again. Doubles as the bound on the straight-line shortcut in
    ///     <c>DirectedGraph.FindPathAsync</c>, which stays sound only while no town path can cost less than it.
    /// </summary>
    public const float TOWN_HEURISTIC = 360f;

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