using System;
using System.Collections.Generic;
using AL.Core.Interfaces;

namespace AL.Pathfinding.Interfaces
{
    /// <summary>
    ///     Represents a point on a graph, with zero or more edges.
    /// </summary>
    /// <typeparam name="TEdge"></typeparam>
    public interface IGraphNode<TEdge> : IEquatable<IGraphNode<TEdge>>
    {
        /// <summary>
        ///     Whether or not this node has been fully explored. (do not set)
        /// </summary>
        bool Closed { get; set; }

        /// <summary>
        ///     The uni-directional connections between this node and other nodes.
        /// </summary>
        ICollection<TEdge> Edges { get; init; }

        /// <summary>
        ///     A node with an edge to this node. (do not set)
        /// </summary>
        TEdge? Parent { get; set; }

        /// <summary>
        ///     The vertex this node represents.
        /// </summary>
        ILocation Vertex { get; init; }

        /// <summary>
        ///     Resets this node's <see cref="Closed" /> and <see cref="Parent" /> properties.
        /// </summary>
        void Reset();
    }
}