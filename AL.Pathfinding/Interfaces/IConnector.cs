using AL.Pathfinding.Definitions;

namespace AL.Pathfinding.Interfaces
{
    public interface IConnector<out TEdge>
    {
        TEdge Start { get; }
        TEdge End { get; }
        float Distance { get; }
        ConnectorType Type { get; internal set; }
    }
}