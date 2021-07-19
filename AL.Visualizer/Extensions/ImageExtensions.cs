using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Core.Interfaces;
using AL.Pathfinding.Interfaces;
using AL.Pathfinding.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
        public static Image<Rgba32> DrawConnections(this Image<Rgba32> image, NavMesh navMesh, Color color = default)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (navMesh == null)
                throw new ArgumentNullException(nameof(navMesh));

            if (color == default)
                color = Color.Red;

            foreach (var connection in navMesh.Connectors)
                if (connection != null)
                    image.DrawLine(connection.Start, connection.End.MidPoint(connection.Start), color);

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
        public static Image<Rgba32> DrawLine<TLine>(
            this Image<Rgba32> image,
            TLine line,
            Color color = default,
            Color ptColor = default) where TLine: ILine
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

            image.Mutate(context =>
                context.DrawLines(color, 1, new PointF(start.X, start.Y), new PointF(end.X, end.Y)));

            image[(int) start.X, (int) start.Y] = ptColor;
            image[(int) end.X, (int) end.Y] = ptColor;

            return image;
        }

        /// <summary>
        ///     Draws a path along a number of path connectors on an image.
        /// </summary>
        /// <param name="image">The image to draw on.</param>
        /// <param name="pathConnectors">The connectors to draw.</param>
        /// <param name="color">The color to draw the path.</param>
        /// <returns>
        ///     <see cref="Image{TPixel}" /> <br />
        ///     The image with the path drawn on it.
        /// </returns>
        /// <exception cref="ArgumentNullException">image</exception>
        /// <exception cref="ArgumentNullException">pathConnectors</exception>
        public static Image<Rgba32> DrawPath<TConnector, TPoint>(
            this Image<Rgba32> image,
            IEnumerable<TConnector?> pathConnectors,
            Color color = default) where TPoint: IPoint where TConnector: IConnector<TPoint>
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (pathConnectors == null)
                throw new ArgumentNullException(nameof(pathConnectors));

            static IEnumerable<TPoint> SelectPoints(IEnumerable<TConnector?> connectors)
            {
                foreach (var connector in connectors)
                    if (connector != null)
                    {
                        yield return connector.Start;
                        yield return connector.End;
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
        public static Image<Rgba32> DrawPath<TPoint>(
            this Image<Rgba32> image,
            IEnumerable<TPoint> points,
            Color color = default) where TPoint: IPoint
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