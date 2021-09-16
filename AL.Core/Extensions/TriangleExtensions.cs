using AL.Core.Interfaces;

namespace AL.Core.Extensions
{
    public static class TriangleExtensions
    {
        public static bool Contains(this ITriangle triangle, IPoint point)
        {
            var p1 = triangle.Vertices[0];
            var p2 = triangle.Vertices[1];
            var p3 = triangle.Vertices[2];

            var alpha = ((p2.Y - p3.Y) * (point.X - p3.X) + (p3.X - p2.X) * (point.Y - p3.Y))
                        / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            var beta = ((p3.Y - p1.Y) * (point.X - p3.X) + (p1.X - p3.X) * (point.Y - p3.Y))
                       / ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            var gamma = 1.0f - alpha - beta;

            return (alpha > 0) && (beta > 0) && (gamma > 0);
        }
    }
}