using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Pathfinding.Objects;
using Poly2Tri;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Point = AL.Core.Geometry.Point;

namespace AL.Visualizer.Extensions
{
    public static class ImageExtensions
    {
        public static Image<Rgba32> DrawConnections(
            this Image<Rgba32> image,
            IEnumerable<GraphNode<Point>> nodes,
            Color color = default)
        {
            if (color == default)
                color = Color.Red;

            foreach (var node in nodes)
                foreach (var neighbor in node.Neighbors)
                    image.DrawLine(node.Edge, node.Edge.MidPoint(neighbor.Edge), color);

            return image;
        }

        public static Image<Rgba32> DrawLine(
            this Image<Rgba32> image,
            Point start,
            Point end,
            Color color = default,
            Color ptColor = default)
        {
            if (color == default)
                color = Color.Gold;

            if (ptColor == default)
                ptColor = Color.Magenta;

            image.Mutate(context =>
            {
                (var sx, var sy) = start;
                (var ex, var ey) = end;
                context.DrawLines(color, 1, new PointF(sx, sy), new PointF(ex, ey));
            });

            image[(int) start.X, (int) start.Y] = ptColor;
            image[(int) end.X, (int) end.Y] = ptColor;

            return image;
        }

        public static Image<Rgba32> DrawPath(this Image<Rgba32> image, IEnumerable<Point> points, Color color = default)
        {
            if (color == default)
                color = Color.Gold;

            _ = points.Aggregate((prev, cur) =>
            {
                image.DrawLine(prev, cur, color);
                return cur;
            });

            return image;
        }

        public static Image<Rgba32> DrawTriangles(
            this Image<Rgba32> image,
            IEnumerable<DelaunayTriangle> triangles,
            Color color = default)
        {
            if (color == default)
                color = Color.Red;

            image.Mutate(context =>
            {
                foreach (var triangle in triangles)
                    context.DrawPolygon(color, 1,
                        triangle.Points.Select(point => new PointF((float) point.X, (float) point.Y)).ToArray());
            });

            return image;
        }
    }
}