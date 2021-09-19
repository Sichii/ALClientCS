using System.Collections.Generic;
using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;
using Priority_Queue;

namespace AL.Pathfinding.Model
{
    /// <summary>
    ///     Represents a point on a graph , with zero or more edges. Inherits from <see cref="FastPriorityQueueNode" />
    ///     for use in <see cref="FastPriorityQueue{T}" />.
    /// </summary>
    public class GraphNode : FastPriorityQueueNode, IGraphNode<GraphEdge>
    {
        public bool Closed { get; set; }
        public ICollection<GraphEdge> Edges { get; init; } = null!;
        public GraphEdge? Parent { get; set; }
        public float Radius { get; init; }
        public ILocation Vertex { get; init; } = null!;
        public bool Equals(IGraphNode<GraphEdge>? other) => other is not null && ILocation.Comparer.Equals(Vertex, other.Vertex);

        public void Reset()
        {
            Parent = null;
            Closed = false;
        }

        public override string ToString() => ILocation.ToString(Vertex);
    }
}