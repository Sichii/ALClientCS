using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Objects
{
    public record EdgeConnector<TEdge> : IConnector<TEdge>
    {
        public TEdge Start { get; init; }
        public TEdge End { get; init; }
        public float Distance { get; init; }

        public ConnectorType Type
        {
            get => ((IConnector<TEdge>) this).Type;
            init => ((IConnector<TEdge>) this).Type = value;
        }

        ConnectorType IConnector<TEdge>.Type { get; set; }
        //public ConnectorType Type { get; internal set; }
    }
}