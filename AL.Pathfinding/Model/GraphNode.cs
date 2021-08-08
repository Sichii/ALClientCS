using System;
using System.Collections.Generic;
using AL.Pathfinding.Interfaces;
using Priority_Queue;

namespace AL.Pathfinding.Model
{
    /// <inheritdoc cref="IGraphNode{TEdge}" />
    public class GraphNode<T> : FastPriorityQueueNode, IGraphNode<T> where T: IEquatable<T>
    {
        public bool Closed { get; set; }
        public List<IGraphNode<T>> Neighbors { get; internal set; }
        public int? Parent { get; set; }
        public T Edge { get; }
        public int Index { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GraphNode{T}" /> class.
        /// </summary>
        /// <param name="edge">The edge of this node.</param>
        /// <param name="index">The index of this node.</param>
        /// <exception cref="ArgumentNullException">edge</exception>
        public GraphNode(T edge, int index)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            Index = index;
            Edge = edge;
            Neighbors = new List<IGraphNode<T>>();
        }

        public int CompareTo(IGraphNode<T>? other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            return ReferenceEquals(null, other) ? 1 : Priority.CompareTo(other.Priority);
        }

        public bool Equals(IGraphNode<T>? other) => IGraphNode<T>.Comparer.Equals(this, other);

        public override int GetHashCode() => IGraphNode<T>.Comparer.GetHashCode(this);

        public void Reset()
        {
            Closed = false;
            Parent = null;
        }
    }
}