using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Model
{
    /// <summary>
    ///     <inheritdoc cref="IConnector{TEdge}" />
    /// </summary>
    /// <typeparam name="TEdge">The type of the underlying edge.</typeparam>
    /// <seealso cref="IConnector{TEdge}" />
    public record EdgeConnector<TEdge> : IConnector<TEdge>
    {
        public TEdge End { get; init; } = default!;
        public float Heuristic { get; init; }
        public TEdge Start { get; init; } = default!;

        /// <summary>
        ///     <inheritdoc cref="IConnector{TEdge}.Type" />
        /// </summary>
        public ConnectorType Type
        {
            get => ((IConnector) this).Type;
            init => ((IConnector) this).Type = value;
        }

        ConnectorType IConnector.Type { get; set; }
    }
}