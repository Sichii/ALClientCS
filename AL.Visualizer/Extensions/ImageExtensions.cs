using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Interfaces;
using Priority_Queue;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AL.Visualizer.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="Image{TPixel}" />.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        ///     Draws all connections between all navMesh and their neighbors on an image.
        /// </summary>
        /// <param name="image">The image to draw on.</param>
        /// <param name="navMesh">The navMesh to draw connections for.</param>
        /// <param name="color">The color to draw the connections as.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     The image with the connections drawn on it.
        /// </returns>
        /// <exception cref="ArgumentNullException">image</exception>
        /// <exception cref="ArgumentNullException">navMesh</exception>
        public static Image<Rgba32> DrawEdges<TNode, TEdge>(this Image<Rgba32> image, MeshBase<TNode, TEdge> navMesh, Color color = default)
            where TEdge: IGraphEdge<TNode>, new() where TNode: FastPriorityQueueNode, IGraphNode<TEdge>

        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (navMesh == null)
                throw new ArgumentNullException(nameof(navMesh));

            if (color == default)
                color = Color.Red;

            foreach (var edge in navMesh.TraverseEdges())
            {
                var start = navMesh.ApplyOffset(edge.Start.Vertex);
                var midEnd = navMesh.ApplyOffset(edge.End.Vertex.MidPoint(edge.Start.Vertex));

                image.DrawLine(start, midEnd, color);
            }

            return image;
        }

        /// <summary>
        ///     Draws a line on an image.
        /// </summary>
        /// <param name="image">The image to draw on.</param>
        /// <param name="line">The line to draw on the image.</param>
        /// <param name="color">The color to draw the line.</param>
        /// <param name="ptColor">The color to draw the pixel the start/end points.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     The image with the line drawn on it.
        /// </returns>
        /// <exception cref="ArgumentNullException">image</exception>
        /// <exception cref="ArgumentNullException">line</exception>
        public static Image<Rgba32> DrawLine<TLine>(this Image<Rgba32> image, TLine line, Color color = default, Color ptColor = default)
            where TLine: ILine
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (line == null)
                throw new ArgumentNullException(nameof(line));

            return image.DrawLine(line.Point1, line.Point2, color, ptColor);
        }

        /// <summary>
        ///     Draws a line on an image.
        /// </summary>
        /// <param name="image">The image to draw on.</param>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <param name="color">The color to draw the line.</param>
        /// <param name="ptColor">The color to draw the pixel the start/end points.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     The image with the line drawn on it.
        /// </returns>
        /// <exception cref="ArgumentNullException">image</exception>
        /// <exception cref="ArgumentNullException">start</exception>
        /// <exception cref="ArgumentNullException">end</exception>
        public static Image<Rgba32> DrawLine<TPoint>(
            this Image<Rgba32> image,
            TPoint start,
            TPoint end,
            Color color = default,
            Color ptColor = default) where TPoint: IPoint
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (end == null)
                throw new ArgumentNullException(nameof(end));

            if (color == default)
                color = Color.Gold;

            if (ptColor == default)
                ptColor = Color.Magenta;

            foreach ((var x, var y) in new Line(start, end).Points())
                image[Math.Clamp(Convert.ToInt32(x), 0, image.Width - 1), Math.Clamp(Convert.ToInt32(y), 0, image.Height - 1)] = color;

            //image.Mutate(context =>
            //    context.DrawLines(color, 1, new PointF(start.X, start.Y), new PointF(end.X, end.Y)));

            image[Math.Clamp(Convert.ToInt32(start.X), 0, image.Width - 1), Math.Clamp(Convert.ToInt32(start.Y), 0, image.Height - 1)] =
                ptColor;

            image[Math.Clamp(Convert.ToInt32(end.X), 0, image.Width - 1), Math.Clamp(Convert.ToInt32(end.Y), 0, image.Height - 1)] =
                ptColor;

            return image;
        }

        /// <summary>
        ///     Draws a path along a number of path connectors on an image.
        /// </summary>
        /// <param name="image">The image to draw on.</param>
        /// <param name="navMesh">The navmesh this path is for.</param>
        /// <param name="pathConnectors">The connectors to draw.</param>
        /// <param name="color">The color to draw the path.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     The image with the path drawn on it.
        /// </returns>
        /// <exception cref="ArgumentNullException">image</exception>
        /// <exception cref="ArgumentNullException">pathConnectors</exception>
        public static Image<Rgba32> DrawPath<TNode, TEdge>(
            this Image<Rgba32> image,
            MeshBase<TNode, TEdge> navMesh,
            IEnumerable<TEdge?> pathConnectors,
            Color color = default) where TEdge: IGraphEdge<TNode>, new() where TNode: FastPriorityQueueNode, IGraphNode<TEdge>

        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (pathConnectors == null)
                throw new ArgumentNullException(nameof(pathConnectors));

            IEnumerable<IPoint> SelectPoints(IEnumerable<TEdge?> connectors)
            {
                foreach (var edge in connectors)
                    if (edge != null)
                    {
                        yield return navMesh.ApplyOffset(edge.Start.Vertex);
                        yield return navMesh.ApplyOffset(edge.End.Vertex);
                    }
            }

            return image.DrawPath(SelectPoints(pathConnectors), color);
        }

        /// <summary>
        ///     Draws a path along a number of points on an image.
        /// </summary>
        /// <param name="image">The image to draw on.</param>
        /// <param name="points">The points to draw the path along.</param>
        /// <param name="color">The color to draw the path.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     The image with the path drawn on it.
        /// </returns>
        /// <exception cref="ArgumentNullException">image</exception>
        /// <exception cref="ArgumentNullException">points</exception>
        public static Image<Rgba32> DrawPath<TPoint>(this Image<Rgba32> image, IEnumerable<TPoint> points, Color color = default)
            where TPoint: IPoint
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (points == null)
                throw new ArgumentNullException(nameof(points));

            if (color == default)
                color = Color.Gold;

            _ = points.Aggregate((prev, cur) =>
            {
                image.DrawLine(prev, cur, color);

                return cur;
            });

            return image;
        }
    }
}