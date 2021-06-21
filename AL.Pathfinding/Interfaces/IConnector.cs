using AL.Pathfinding.Definitions;

namespace AL.Pathfinding.Interfaces
{
    public interface IConnector<out TEdge>
    {
        ConnectorType Type { get; internal set; }
        float Distance { get; }
        TEdge End { get; }
        TEdge Start { get; }
    }
}