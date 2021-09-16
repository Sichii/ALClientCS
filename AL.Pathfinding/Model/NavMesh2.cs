using System;
using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;

namespace AL.Pathfinding.Model
{
    public class NavMesh2 : MeshBase<GraphNode2, GraphEdge, ILocation, GraphTriangle>
    {
        protected internal NavMesh2(
            string map,
            IEnumerable<IGenericTriangle<ILocation>> triangles,
            PointType[,] pointMap,
            int xOffset,
            int yOffset)
            : base(map, triangles, pointMap, xOffset, yOffset) { }

        protected internal override GraphEdge ConstructEdge(GraphNode2 start, GraphNode2 end, ConnectorType? typeOverride = null)
        {
            var type = typeOverride ?? ConnectorTypeSelector(start.Vertex, end.Vertex);

            var heuristic = type switch
            {
                ConnectorType.Leave     => CONSTANTS.TRANSPORT_HEURISTIC,
                ConnectorType.Transport => CONSTANTS.TRANSPORT_HEURISTIC,
                ConnectorType.Door      => CONSTANTS.TRANSPORT_HEURISTIC,
                ConnectorType.Town      => CONSTANTS.TOWN_HEURISTIC,
                ConnectorType.Walk      => CalculateHeuristic(start.Vertex, end.Vertex),
                _                       => throw new ArgumentOutOfRangeException(nameof(type))
            };

            return new GraphEdge
            {
                Start = start,
                End = end,
                Heuristic = heuristic,
                Type = type
            };
        }

        protected internal override GraphNode2 ConstructNode(ILocation vertex)
        {
            var radius = 0f;

            if (vertex is ICircle circle)
                radius = circle.Radius;

            return new GraphNode2
            {
                Vertex = vertex,
                Radius = radius,
                Edges = new HashSet<GraphEdge>()
            };
        }

        protected internal override GraphTriangle ConstructTriangle(GraphNode2 node1, GraphNode2 node2, GraphNode2 node3)
        {
            var centroid = new Location(node1.Vertex.Map, (node1.Vertex.X + node2.Vertex.X + node3.Vertex.X) / 3f,
                (node1.Vertex.Y + node2.Vertex.Y + node3.Vertex.Y) / 3f);

            return new GraphTriangle(centroid, new[] { node1, node2, node3 });
        }

        protected internal override ILocation ConstructVertex(ILocation location) => location;
    }
}