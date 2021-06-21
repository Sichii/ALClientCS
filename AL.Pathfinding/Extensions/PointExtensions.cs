using System;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Poly2Tri;

namespace AL.Pathfinding.Extensions
{
    public static class PointExtensions
    {
        public static float FastDistance(this IPoint p1, TriangulationPoint p2) =>
            (float) (Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

        public static Point ToPoint(this TriangulationPoint tPoint, int xOffset = 0, int yOffset = 0) =>
            new((float) tPoint.X + xOffset, (float) tPoint.Y + yOffset);

        public static PolygonPoint ToPolyPoint(this Point point, int index) => new(point.X, point.Y, index);
    }
}