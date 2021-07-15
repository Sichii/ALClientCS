using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Pathfinding.Definitions;
using Poly2Tri;

namespace AL.Pathfinding.Model
{
    internal record NavMeshBuilderContext
    {
        internal List<GraphNode<Point>> Nodes { get; init; } = null!;
        internal PointType[,] PointMap { get; init; } = null!;
        internal GraphNode<Point>? TownNode { get; init; }
        internal Dictionary<Point, DelaunayTriangle> Triangles { get; init; } = null!;
        internal int XOffset { get; init; }
        internal int YOffset { get; init; }
    }
}