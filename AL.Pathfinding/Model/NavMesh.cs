#region
using System;
using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     <inheritdoc cref="MeshBase{TNode,TEdge}" />
/// </summary>
public sealed class NavMesh : MeshBase<GraphNode, GraphEdge>
{
    internal NavMesh(
        string map,
        IEnumerable<IGenericTriangle<ILocation>> triangles,
        PointType[,] pointMap,
        int xOffset,
        int yOffset)
        : base(
            map,
            triangles,
            pointMap,
            xOffset,
            yOffset) { }

    protected internal override GraphEdge ConstructEdge(GraphNode start, GraphNode end, EdgeType? typeOverride = null)
    {
        var type = typeOverride ?? ConnectorTypeSelector(start.Vertex, end.Vertex);

        var heuristic = type switch
        {
            EdgeType.Leave     => CONSTANTS.TRANSPORT_HEURISTIC,
            EdgeType.Transport => CONSTANTS.TRANSPORT_HEURISTIC,
            EdgeType.Door      => CONSTANTS.TRANSPORT_HEURISTIC,
            EdgeType.Town      => CONSTANTS.TOWN_HEURISTIC,
            EdgeType.Walk      => CalculateHeuristic(start.Vertex, end.Vertex),
            _                  => throw new ArgumentOutOfRangeException(nameof(type))
        };

        return new GraphEdge
        {
            Start = start,
            End = end,
            Heuristic = heuristic,
            Type = type
        };
    }

    protected internal override GraphNode ConstructNode(ILocation vertex)
    {
        var radius = 0f;

        if (vertex is ICircle circle)
            radius = circle.Radius;

        return new GraphNode
        {
            Vertex = vertex,
            Radius = radius,
            Edges = new HashSet<GraphEdge>()
        };
    }

    protected internal override GraphTriangle ConstructTriangle(GraphNode node1, GraphNode node, GraphNode node3)
    {
        var centroid = new Location(
            node1.Vertex.Map,
            (node1.Vertex.X + node.Vertex.X + node3.Vertex.X) / 3f,
            (node1.Vertex.Y + node.Vertex.Y + node3.Vertex.Y) / 3f);

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