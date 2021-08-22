using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Extensions
{
    public static class CircleExtensions
    {
        /// <summary>
        ///     Determines where a line first intersects this circle.
        /// </summary>
        /// <param name="circle">This circle.</param>
        /// <param name="line">The line.</param>
        /// <returns>
        ///     <see cref="Point" /> <br />
        ///     The point at which the
        ///     <paramref name="line" />
        ///     intersects this circle. <br />
        ///     <c>null</c> if they don't intersect.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">circle</exception>
        /// <exception cref="ArgumentNullException">null</exception>
        public static Point? CalculateIntersectionEntryPoint(this ICircle circle, ILine line)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            if (line == null)
                throw new ArgumentNullException(nameof(line));

            var start = line.Point1;
            var end = line.Point2;
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var sqr = dx * dx + dy * dy;
            var lne = 2 * (dx * (start.X - circle.X) + dy * (start.Y - circle.Y));

            var quad = (start.X - circle.X) * (start.X - circle.X)
                       + (start.Y - circle.Y) * (start.Y - circle.Y)
                       - circle.Radius * circle.Radius;

            var descriminant = lne * lne - 4 * sqr * quad;

            if ((sqr <= 0.0000001) || (descriminant < 0))
                return default;

            descriminant = (float)Math.Sqrt(descriminant);

            if (descriminant > 0)
            {
                var t = (-lne - descriminant) / (2 * sqr);

                if (t is >= 0 and <= 1)
                    return new Point(start.X + t * dx, start.Y + t * dy);
            }

            return default;
        }

        /// <summary>
        ///     Determines whether this circle fully encompasses another circle.
        /// </summary>
        /// <param name="circle">This circle.</param>
        /// <param name="other">Another circle.</param>
        /// <returns><c>true</c> if this circle fully encompasses the other (or edges touch); otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">circle</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static bool Contains(this ICircle circle, ICircle other)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return circle.Radius >= circle.Distance(other) + other.Radius;
        }

        /// <summary>
        ///     Determines whether this circle contains the given point.
        /// </summary>
        /// <param name="circle">This circle.</param>
        /// <param name="point">A point.</param>
        /// <returns><c>true</c> if this circle contains the point, otherwise <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">circle</exception>
        /// <exception cref="System.ArgumentNullException">point</exception>
        public static bool Contains(this ICircle circle, IPoint point)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            if (point == null)
                throw new ArgumentNullException(nameof(point));

            return point.Distance(circle) > circle.Radius;
        }

        /// <summary>
        ///     Calculates the edge-to-edge euclidean distance to another circle.
        /// </summary>
        /// <param name="circle">This circle.</param>
        /// <param name="other">Another circle.</param>
        /// <returns>
        ///     <see cref="float" />
        ///     <br />
        ///     The euclidean distance between the two closest points of this circle and the <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">circle</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static float Distance(this ICircle circle, ICircle other)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Math.Max(0, circle.Distance((IPoint)other) - circle.Radius - other.Radius);
        }

        /// <summary>
        ///     Lazily generates points along the circumference of this circle.
        /// </summary>
        /// <param name="circle">This circle.</param>
        /// <param name="numberOfPoints">The number of points to generate.</param>
        /// <param name="startingAngle">The starting angle.</param>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="Point" /></returns>
        /// <exception cref="System.ArgumentNullException">circle</exception>
        public static IEnumerable<Point> GenerateCircumferencePoints(this ICircle circle, float numberOfPoints, float startingAngle = 0f)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            var anglePerPoint = 360 / numberOfPoints;

            for (var traversedAngle = 0f; traversedAngle.SignificantlyLessThan(360, CONSTANTS.EPSILON); traversedAngle += anglePerPoint)
                yield return circle.AngularOffset(startingAngle + traversedAngle, circle.Radius);
        }

        /// <summary>
        ///     Determines whether this circle intersects with another circle.
        /// </summary>
        /// <param name="circle">This circle.</param>
        /// <param name="other">Another circle.</param>
        /// <returns><c>true</c> if this circle intersects the <paramref name="other" />, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">circle</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static bool Intersects(this ICircle circle, ICircle other)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return circle.Distance((IPoint)other) <= circle.Radius + other.Radius;
        }

        /// <summary>
        ///     Lazily generates all points within this circle.
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="numberOfSteps">
        ///     Determines step size (Diameter / numberOfSteps). <br />
        ///     <b>Even numbers work best.</b>
        /// </param>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="Point" /></returns>
        /// <exception cref="System.ArgumentNullException">circle</exception>
        public static IEnumerable<Point> Points(this ICircle circle, float numberOfSteps)
        {
            if (circle == null)
                throw new ArgumentNullException(nameof(circle));

            var stepSize = circle.Radius / numberOfSteps * 2;

            for (var x = circle.X - circle.Radius; x <= circle.X; x += stepSize)
                for (var y = circle.Y - circle.Radius; y <= circle.Y; y += stepSize)
                {
                    var xdc = x - circle.X;
                    var ydc = y - circle.Y;

                    if (xdc * xdc + ydc * ydc <= Math.Pow(circle.Radius, 2))
                    {
                        var xS = circle.X - xdc;
                        var yS = circle.Y - ydc;

                        yield return new Point(x, y);
                        yield return new Point(x, yS);
                        yield return new Point(xS, y);
                        yield return new Point(xS, yS);
                    }
                }
        }
    }
}