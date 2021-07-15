using AL.Pathfinding.Definitions;

namespace AL.Pathfinding.Interfaces
{
    /// <summary>
    ///     A connection.
    /// </summary>
    public interface IConnector
    {
        /// <summary>
        ///     The type of this connector.
        /// </summary>
        ConnectorType Type { get; internal set; }

        /// <summary>
        ///     The heuristic value assigned to this combination of edges. (lower is better)
        /// </summary>
        float Heuristic { get; }
    }

    /// <summary>
    ///     A connection between two edges of a graph.
    /// </summary>
    /// <typeparam name="TEdge">The type of the underlying edge.</typeparam>
    /// <seealso cref="IConnector" />
    public interface IConnector<out TEdge> : IConnector
    {
        /// <summary>
        ///     The ending edge of this connection.
        /// </summary>
        TEdge End { get; }

        /// <summary>
        ///     The starting edge of this connection.
        /// </summary>
        TEdge Start { get; }
    }
}