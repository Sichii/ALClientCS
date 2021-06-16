using System.Collections.Generic;
using System.Linq;
using AL.Core.Geometry;
using AL.Core.Interfaces;

namespace AL.Core.Extensions
{
    public static class RectangleExtensions
    {
        public static bool Contains(this IRectangle rect, IRectangle other) =>
            rect.Bottom > other.Bottom && rect.Left > other.Left && rect.Right < other.Right && rect.Top < other.Top;

        public static bool Contains(this IRectangle rect, IPoint point) =>
            rect.Left <= point.X && rect.Right > point.X && rect.Top <= point.Y && rect.Bottom > point.Y;

        public static float Distance(this IRectangle rect, IRectangle other) =>
            rect.Intersects(other) ? 0f : rect.Vertices.SelectMany(point => other.Vertices.Select(point.Distance)).Min();

        public static bool Intersects(this IRectangle rect, IRectangle other) =>
            !(rect.Bottom > other.Top || rect.Left > other.Right || rect.Right < other.Left || rect.Top < other.Bottom);

        public static IEnumerable<IPoint> Points(this IRectangle rect, float numberOfSteps = -1f)
        {
            var horizontalStep = numberOfSteps.NearlyEquals(-1f) ? 1 : rect.Width / numberOfSteps;
            var verticalStep = numberOfSteps.NearlyEquals(-1f) ? 1 : rect.Height / numberOfSteps;

            for (var x = rect.Left; x <= rect.Right; x += horizontalStep)
                for (var y = rect.Top; y <= rect.Bottom; y += verticalStep)
                    yield return new Point(x, y);
        }
    }
}