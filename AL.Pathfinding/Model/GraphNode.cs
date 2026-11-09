#region
using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents a point on a graph, with zero or more edges.
/// </summary>
public sealed class GraphNode : IGraphNode<GraphEdge>
{
    public ICollection<GraphEdge> Edges { get; init; } = null!;
    public ILocation Location { get; init; } = null!;

    /// <summary>
    ///     Where a walk to this node counts as arrived: an exit's band and range, or a circle about any other node.
    /// </summary>
    public Reach Reach { get; init; }

    public bool Equals(IGraphNode<GraphEdge>? other) => other is not null && ILocation.Comparer.Equals(Location, other.Location);

    public override string ToString() => ILocation.ToString(Location);
}