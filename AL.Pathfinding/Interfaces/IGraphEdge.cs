#region
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Interfaces;

/// <summary>
///     Represent a uni-directional connection between nodes.
/// </summary>
public interface IGraphEdge<TNode>
{
    /// <summary>The node at the end of this edge.</summary>
    TNode End { get; init; }

    /// <summary>The cost of travelling this edge.</summary>
    float Cost { get; init; }

    /// <summary>
    ///     If populated, the server's penalty under a wall hop, one sample per unit of its length.
    /// </summary>
    float[]? HopPenalties { get; init; }

    /// <summary>
    ///     The hop penalty score a route lands with at the end of a wall hop; 0 on every other edge.
    /// </summary>
    float HopScore { get; init; }

    /// <summary>The starting node of this edge.</summary>
    TNode Start { get; init; }

    /// <summary>The type of this edge.</summary>
    EdgeType Type { get; init; }
}