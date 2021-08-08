using System;
using System.Collections.Generic;
using AL.Pathfinding.Comparers;

namespace AL.Pathfinding.Interfaces
{
    /// <summary>
    ///     Represents a node on a graph intended to be searched via dijkstra algorithm.
    /// </summary>
    /// <typeparam name="TEdge">The type of the underlying edge.</typeparam>
    /// <seealso cref="IEquatable{T}" />
    /// <seealso cref="IComparable{T}" />
    public interface IGraphNode<TEdge> : IEquatable<IGraphNode<TEdge>>, IComparable<IGraphNode<TEdge>> where TEdge: IEquatable<TEdge>
    {
        /// <summary>
        ///     If true, the node and it's neighbors have been searched.
        /// </summary>
        bool Closed { get; set; }

        /// <summary>
        ///     The index of the parent of this node.
        /// </summary>
        int? Parent { get; set; }
        static IEqualityComparer<IGraphNode<TEdge>> Comparer { get; } = new GraphNodeComparer<TEdge>();

        /// <summary>
        ///     The underlying edge data.
        /// </summary>
        TEdge Edge { get; }

        /// <summary>
        ///     The index of this node.
        /// </summary>
        int Index { get; }

        /// <summary>
        ///     The neighboring nodes of this node.
        /// </summary>
        List<IGraphNode<TEdge>> Neighbors { get; }

        /// <summary>
        ///     The priority of this node, as used in a Priority Queue.
        /// </summary>
        float Priority { get; }

        /// <summary>
        ///     Resets this nodes <see cref="Closed" /> and <see cref="Parent" /> to their default values. <br />
        ///     The containing connection will need to reset the <see cref="Priority" />.
        /// </summary>
        void Reset();
    }
}