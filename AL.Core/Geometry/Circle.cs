using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Geometry
{
    /// <summary>
    ///     Represents a circle.
    /// </summary>
    /// <seealso cref="AL.Core.Interfaces.IPoint" />
    public record Circle(float X, float Y, float Radius) : IPoint
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Circle" /> class.
        /// </summary>
        /// <param name="center">The center point of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <exception cref="System.ArgumentNullException">center</exception>
        public Circle(IPoint center, float radius)
            : this(center.X, center.Y, radius)
        {
            if (center == null)
                throw new ArgumentNullException(nameof(center));
        }

        /// <summary>
        ///     Determines whether this circle fully encompasses another circle.
        /// </summary>
        /// <param name="other">Another circle.</param>
        /// <returns><c>true</c> if this circle fully encompasses the other (or edges touch); otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public bool Contains(Circle other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Radius >= Distance(other) + other.Radius;
        }

        /// <summary>
        /// Determines whether this circle contains the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(IPoint point) => point.Distance(this) > Radius;

        /// <summary>
        ///     Calculates the edge-to-edge euclidean distance to another circle.
        /// </summary>
        /// <param name="other">Another circle.</param>
        /// <returns>
        ///     <see cref="float" />
        ///     <br />
        ///     The euclidean distance between the two closest points of this circle and the <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public float Distance(Circle other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return Math.Max(0, this.Distance((IPoint) other) - (Radius + other.Radius));
        }

        /// <summary>
        ///     Lazily generates points along the circumference of this circle.
        /// </summary>
        /// <param name="numberOfPoints">The number of points to generate.</param>
        /// <param name="startingAngle">The starting angle.</param>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="Point" /></returns>
        public IEnumerable<Point> GenerateCircumferencePoints(float numberOfPoints, float startingAngle = 0f)
        {
            var anglePerPoint = 360 / numberOfPoints;

            for (var traversedAngle = 0f; traversedAngle.SignificantlyLessThan(360, CONSTANTS.EPSILON);
                traversedAngle += anglePerPoint)
                yield return this.AngularOffset(startingAngle + traversedAngle, Radius);
        }

        public override int GetHashCode() => HashCode.Combine(X, Y, Radius);

        /// <summary>
        ///     Determines whether this circle intersects with another circle.
        /// </summary>
        /// <param name="other">Another circle.</param>
        /// <returns><c>true</c> if this circle intersects the <paramref name="other" />, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public bool Intersects(Circle other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return this.Distance((IPoint) other) <= Radius + other.Radius;
        }

        /// <summary>
        ///     Determines where a line first intersects this circle.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>
        ///     <see cref="Point" /> <br />
        ///     The point at which the
        ///     <paramref name="line"/>
        ///         intersects this circle. <br />
        ///         <c>null</c> if they don't intersect.
        /// </returns>
        /// <exception cref="ArgumentNullException">nameof(line)</exception>
        public Point? Intersects(ILine line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            var start = line.Point1;
            var end = line.Point2;
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var sqr = dx * dx + dy * dy;
            var lne = 2 * (dx * (start.X - X) + dy * (start.Y - Y));
            var quad = (start.X - X) * (start.X - X) + (start.Y - Y) * (start.Y - Y) - Radius * Radius;

            var descriminant = lne * lne - 4 * sqr * quad;

            if ((sqr <= 0.0000001) || (descriminant < 0))
                return default;

            descriminant = (float) Math.Sqrt(descriminant);

            if (descriminant > 0)
            {
                var t = (-lne - descriminant) / (2 * sqr);

                if (t is >= 0 and <= 1)
                    return new Point(start.X + t * dx, start.Y + t * dy);
            }

            return default;
        }

        /// <summary>
        ///     Lazily generates all points within this circle.
        /// </summary>
        /// <param name="numberOfSteps">
        ///     Determines step size (Diameter / numberOfSteps). <br />
        ///     <b>Even numbers work best.</b>
        /// </param>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="Point" /></returns>
        public IEnumerable<Point> Points(float numberOfSteps)
        {
            var stepSize = Radius / numberOfSteps * 2;

            for (var x = X - Radius; x <= X; x += stepSize)
                for (var y = Y - Radius; y <= Y; y += stepSize)
                {
                    var xdc = x - X;
                    var ydc = y - Y;

                    if (xdc * xdc + ydc * ydc <= Radius * Radius)
                    {
                        var xS = X - xdc;
                        var yS = Y - ydc;

                        yield return new Point(x, y);
                        yield return new Point(x, yS);
                        yield return new Point(xS, y);
                        yield return new Point(xS, yS);
                    }
                }
        }
    }
}