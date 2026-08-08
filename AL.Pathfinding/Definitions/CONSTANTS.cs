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
    ///     The default values of a player bounding base.
    /// </summary>
    public static readonly BoundingBase DEFAULT_BOUNDING_BASE = new(8, 7, 2);
}