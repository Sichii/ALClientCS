#region
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     <inheritdoc cref="MeshBase{TNode,TEdge}" />
/// </summary>
public sealed class NodeMesh : MeshBase<GraphNode, GraphEdge>
{
    internal NodeMesh(NavMesh navMesh)
        : base(navMesh) { }

    protected internal override GraphEdge ConstructEdge(
        GraphNode start,
        GraphNode end,
        EdgeType type,
        float? cost = null)
    {
        var heuristic = cost
                        ?? type switch
                        {
                            EdgeType.Walk => CalculateHeuristic(start.Location, end.Location),
                            _             => throw new ArgumentOutOfRangeException(nameof(type))
                        };

        return new GraphEdge
        {
            Start = start,
            End = end,
            Cost = heuristic,
            Type = type
        };
    }

    protected internal override GraphNode ConstructNode(ILocation vertex)
        => new()
        {
            Location = vertex,
            Reach = CreateReach(vertex),
            Edges = Array.Empty<GraphEdge>()
        };

    protected internal override GraphNode ConstructVertexNode(ILocation vertex)
        => new()
        {
            Location = vertex,
            Reach = CreateReach(vertex),
            Edges = new HashSet<GraphEdge>()
        };

    /// <summary>
    ///     Creates the reach a node counts as arrived in: an exit's band inflated by its range, as the server measures it;
    ///     anything else a circle.
    /// </summary>
    /// <param name="vertex">The node's point.</param>
    /// <returns>The reach.</returns>
    private static Reach CreateReach(ILocation vertex)
        => vertex is Exit exit
            ? new Reach(exit.ReachBand, exit.ReachRange)
            : Reach.CreateCircle(vertex.X, vertex.Y, (vertex as ICircle)?.Radius ?? 0f);

    protected internal override GraphTriangle ConstructTriangle(GraphNode node1, GraphNode node, GraphNode node3)
    {
        var centroid = new Location(
            node1.Location.Map,
            (node1.Location.X + node.Location.X + node3.Location.X) / 3f,
            (node1.Location.Y + node.Location.Y + node3.Location.Y) / 3f);

        return new GraphTriangle(
            centroid,
            [
                node1,
                node,
                node3
            ]);
    }

    protected internal override ILocation ConstructVertex(ILocation location) => location;
}