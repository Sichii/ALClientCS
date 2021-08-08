using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="IRectangle" />s.
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        ///     Determines whether a rectangle fully encompasses another rectangle.
        /// </summary>
        /// <param name="rect">A rectangle.</param>
        /// <param name="other">Another rectangle.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if this rectangle fully encompasses the other (or edges touch); otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">rect</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static bool Contains(this IRectangle rect, IRectangle other)
        {
            if (rect == null)
                throw new ArgumentNullException(nameof(rect));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return (rect.Bottom >= other.Bottom) && (rect.Left >= other.Left) && (rect.Right <= other.Right) && (rect.Top <= other.Top);
        }

        /// <summary>
        ///     Determines whether a rectangle contains a given point.
        /// </summary>
        /// <param name="rect">A rectangle.</param>
        /// <param name="point">A point.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the point lies within or on the edge of the rectangle; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">rect</exception>
        /// <exception cref="System.ArgumentNullException">point</exception>
        public static bool Contains(this IRectangle rect, IPoint point)
        {
            if (rect == null)
                throw new ArgumentNullException(nameof(rect));

            if (point == null)
                throw new ArgumentNullException(nameof(point));

            return (rect.Left <= point.X) && (rect.Right > point.X) && (rect.Top <= point.Y) && (rect.Bottom > point.Y);
        }

        /// <summary>
        ///     Calculates the edge-to-edge euclidean distance between two rectangles.
        /// </summary>
        /// <param name="rect">A rectangle.</param>
        /// <param name="other">Another rectangle.</param>
        /// <returns>
        ///     <see cref="float" />
        ///     <br />
        ///     The euclidean distance between the two closest points of <paramref name="rect" /> and the <paramref name="other" />
        ///     .
        /// </returns>
        /// <exception cref="System.ArgumentNullException">rect</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static float Distance(this IRectangle rect, IRectangle other)
        {
            if (rect == null)
                throw new ArgumentNullException(nameof(rect));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return rect.Intersects(other) ? 0f : rect.Vertices.SelectMany(point => other.Vertices.Select(point.Distance)).Min();
        }

        /// <summary>
        ///     Determines whether two rectangles overlap eachother.
        /// </summary>
        /// <param name="rect">A rectangle.</param>
        /// <param name="other">Another rectangle.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the rectangles touch or overlap, <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">rect</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static bool Intersects(this IRectangle rect, IRectangle other)
        {
            if (rect == null)
                throw new ArgumentNullException(nameof(rect));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return !((rect.Bottom > other.Top) || (rect.Left > other.Right) || (rect.Right < other.Left) || (rect.Top < other.Bottom));
        }

        /// <summary>
        ///     Lazily generates all points within a rectangle.
        /// </summary>
        /// <param name="rect">A rectangle.</param>
        /// <param name="widthStepNum">
        ///     The number of points to generate horizontally.<br />
        ///     -1 is equivalent to specifying the width. <br />
        ///     <b>Even numbers work best.</b>
        /// </param>
        /// <param name="heightStepNum">
        ///     The number of points to generate vertically.<br />
        ///     -1 is equivalent to the height. <br />
        ///     <b>Even numbers work best.</b>
        /// </param>
        /// <returns>
        ///     <see cref="IEnumerable{T}" /> of <see cref="Point" /> <br />
        ///     An enumeration of points generates from top left to bottom right.
        /// </returns>
        /// <exception cref="ArgumentNullException">rect</exception>
        public static IEnumerable<Point> Points(this IRectangle rect, float widthStepNum = -1f, float heightStepNum = -1f)
        {
            if (rect == null)
                throw new ArgumentNullException(nameof(rect));

            var horizontalStep = widthStepNum.NearlyEquals(-1f, CONSTANTS.EPSILON) ? 1 : rect.Width / widthStepNum;
            var verticalStep = heightStepNum.NearlyEquals(-1f, CONSTANTS.EPSILON) ? 1 : rect.Height / heightStepNum;

            for (var x = rect.Left; x <= rect.Right; x += horizontalStep)
                for (var y = rect.Top; y <= rect.Bottom; y += verticalStep)
                    yield return new Point(x, y);
        }
    }
}