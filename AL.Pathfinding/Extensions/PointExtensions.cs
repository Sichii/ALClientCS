using AL.Core.Geometry;
using Poly2Tri;

namespace AL.Pathfinding.Extensions
{
    internal static class PointExtensions
    {
        internal static Point ToPoint(this TriangulationPoint tPoint, int xOffset = 0, int yOffset = 0) =>
            new((float) tPoint.X + xOffset, (float) tPoint.Y + yOffset);

        internal static PolygonPoint ToPolyPoint(this Point point, int index) => new(point.X, point.Y, index);
    }
}