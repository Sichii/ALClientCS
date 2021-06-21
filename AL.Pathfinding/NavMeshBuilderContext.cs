using System.Collections.Generic;
using AL.Core.Geometry;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Objects;
using Poly2Tri;

namespace AL.Pathfinding
{
    internal record NavMeshBuilderContext
    {
        internal string Key { get; init; }
        internal List<GraphNode<Point>> Nodes { get; init; }
        internal PointType[,] PointMap { get; init; }
        internal GraphNode<Point> TownNode { get; init; }
        internal Dictionary<Point, DelaunayTriangle> Triangles { get; init; }
        internal int XOffset { get; init; }
        internal int YOffset { get; init; }
    }
}