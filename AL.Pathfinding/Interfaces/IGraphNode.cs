#region
using AL.Core.Interfaces;
#endregion

namespace AL.Pathfinding.Interfaces;

/// <summary>
///     Represents a point on a graph, with zero or more edges.
/// </summary>
/// <typeparam name="TEdge"></typeparam>
public interface IGraphNode<TEdge> : IEquatable<IGraphNode<TEdge>>
{
    /// <summary>
    ///     The uni-directional connections between this node and other nodes.
    /// </summary>
    ICollection<TEdge> Edges { get; init; }

    /// <summary>Where this node stands.</summary>
    ILocation Location { get; init; }
}