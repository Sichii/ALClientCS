#region
using AL.Core.Interfaces;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     <inheritdoc cref="IGraphEdge{TNode}" />
/// </summary>
public sealed class GraphEdge : IGraphEdge<GraphNode>, ILine
{
    public GraphNode End { get; init; } = null!;
    public float Heuristic { get; init; }
    public GraphNode Start { get; init; } = null!;
    public EdgeType Type { get; init; }

    float ILine.Length => Heuristic;

    IPoint ILine.Point1 => Start.Vertex;

    IPoint ILine.Point2 => End.Vertex;

    public override string ToString() => $"{{ {Start} }} => {{ {End} }} T: {Type}, H: {Heuristic}";
}