using AL.Pathfinding.Definitions;

namespace AL.Pathfinding.Interfaces
{
    /// <summary>
    ///     Represent a uni-directional connection between nodes.
    /// </summary>
    public interface IGraphEdge<TNode>
    {
        /// <summary>
        ///     The node at the end of this edge.
        /// </summary>
        TNode End { get; init; }

        /// <summary>
        ///     The heuristic value assigned to this uni-directional traversel between edges. (lower is better)
        /// </summary>
        float Heuristic { get; init; }

        /// <summary>
        ///     The starting node of this edge.
        /// </summary>
        TNode Start { get; init; }
        /// <summary>
        ///     The type of this edge.
        /// </summary>
        EdgeType Type { get; init; }
    }
}