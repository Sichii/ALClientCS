using System.Collections.Generic;
using System.Linq;
using AL.Core.Geometry;
using Poly2Tri;
using Polygon = Poly2Tri.Polygon;

namespace AL.Pathfinding.Extensions
{
    internal static class TriangleNetExtensions
    {
        internal static Point Centroid(this DelaunayTriangle triangle) =>
            new((float)(triangle.Points._0.X + triangle.Points._1.X + triangle.Points._2.X) / 3,
                (float)(triangle.Points._0.Y + triangle.Points._1.Y + triangle.Points._2.Y) / 3);

        internal static bool ContainsPoint(this Polygon polygon, float x, float y) =>
            PolyLineEncircles(polygon.Points, x, y);

        internal static bool ContainsPoint(this DelaunayTriangle triangle, float x, float y) =>
            PolyLineEncircles(triangle.Points.ToArray(), x, y);

        private static bool PolyLineEncircles(IList<TriangulationPoint> poly, float x, float y)
        {
            var inside = false;
            var count = poly.Count;

            for (int i = 0, j = count - 1; i < count; j = i++)
                if ((((poly[i].Y < y) && (poly[j].Y >= y)) || ((poly[j].Y < y) && (poly[i].Y >= y)))
                    && ((poly[i].X <= x) || (poly[j].X <= x)))
                    inside ^= poly[i].X + (y - poly[i].Y) / (poly[j].Y - poly[i].Y) * (poly[j].X - poly[i].X) < x;

            return inside;
        }
    }
}