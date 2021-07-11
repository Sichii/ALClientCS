using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Model
{
    public record EdgeConnector<TEdge> : IConnector<TEdge>
    {
        public float Distance { get; init; }
        public TEdge End { get; init; }
        public TEdge Start { get; init; }

        public ConnectorType Type
        {
            get => ((IConnector<TEdge>) this).Type;
            init => ((IConnector<TEdge>) this).Type = value;
        }

        ConnectorType IConnector<TEdge>.Type { get; set; }
        //public ConnectorType Type { get; internal set; }
    }
}