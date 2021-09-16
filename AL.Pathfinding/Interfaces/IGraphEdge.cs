using AL.Pathfinding.Definitions;

namespace AL.Pathfinding.Interfaces
{
    /// <summary>
    ///     A connection.
    /// </summary>
    public interface IGraphEdge<TNode>
    {
        /// <summary>
        ///     The ending edge of this connection.
        /// </summary>
        TNode End { get; init; }

        /// <summary>
        ///     The heuristic value assigned to this combination of edges. (lower is better)
        /// </summary>
        float Heuristic { get; init; }

        /// <summary>
        ///     The starting edge of this connection.
        /// </summary>
        TNode Start { get; init; }
        /// <summary>
        ///     The type of this connector.
        /// </summary>
        ConnectorType Type { get; init; }
    }
}