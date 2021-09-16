using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Model
{
    public class GraphEdge : IGraphEdge<GraphNode2>
    {
        public GraphNode2 End { get; init; } = null!;
        public float Heuristic { get; init; }
        public GraphNode2 Start { get; init; } = null!;
        public ConnectorType Type { get; init; }

        public override string ToString() => $"{{ {Start} }} => {{ End: {End} }} (H: {Heuristic}, Type: {Type})";
    }
}