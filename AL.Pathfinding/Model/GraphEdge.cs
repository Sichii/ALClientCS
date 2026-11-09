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
    public float Cost { get; init; }
    public float[]? HopPenalties { get; init; }
    public float HopScore { get; init; }
    public GraphNode Start { get; init; } = null!;
    public EdgeType Type { get; init; }

    float ILine.Length => Cost;

    IPoint ILine.Point1 => Start.Location;

    IPoint ILine.Point2 => End.Location;

    public override string ToString() => $"{{ {Start} }} => {{ {End} }} T: {Type}, H: {Cost}";

    /// <summary>
    ///     Converts the edge into the leg a route hands its caller.
    /// </summary>
    /// <returns>The leg.</returns>
    internal PathEdge ToPathEdge()
        => new(
            Type,
            Start.Location,
            End.Location,
            Cost)
        {
            HopScore = HopScore
        };
}