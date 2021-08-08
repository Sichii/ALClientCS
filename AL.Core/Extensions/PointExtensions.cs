using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="IPoint" />s.
    /// </summary>
    public static class PointExtensions
    {
        /// <summary>
        ///     Calculates a new point, offsetting a given point by a given distance at a given angle.
        /// </summary>
        /// <param name="point">The point to offset.</param>
        /// <param name="angle">The angle to offset at in degrees.</param>
        /// <param name="distance">The distance to offset by.</param>
        /// <returns>
        ///     <see cref="AL.Core.Geometry.Point" />
        ///     <br />
        ///     A new, offset point.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">point</exception>
        public static Point AngularOffset(this IPoint point, float angle, float distance = 1f)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            var theta = angle * Math.PI / 180;
            var x = (float)Math.Cos(theta) * distance;
            var y = (float)Math.Sin(theta) * distance;

            return new Point(point.X + x, point.Y + y);
        }

        /// <summary>
        ///     Calculates this point's relation to another point in degrees.
        /// </summary>
        /// <param name="point">The point who's relation to another point you want to know.</param>
        /// <param name="other">The other point</param>
        /// <returns>
        ///     <see cref="float" />
        ///     <br />
        ///     The angle of <paramref name="point" /> from the <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">point</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static float AngularRelationTo(this IPoint point, IPoint other)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var deltaX = point.X - other.X;
            var deltaY = point.Y - other.Y;

            return (float)(Math.Atan2(deltaY, deltaX) * (180 / Math.PI));
        }

        /// <summary>
        ///     Calculates a new point, offsetting this point by a given distance, in a given direction.
        /// </summary>
        /// <param name="point">The point to offset from.</param>
        /// <param name="direction">The direction to offset in.</param>
        /// <param name="distance">The distance to offset by.</param>
        /// <returns>
        ///     <see cref="Geometry.Point" />
        ///     <br />
        ///     A new, offset point.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">point</exception>
        public static Point DirectionalOffset(this IPoint point, Direction direction, float distance = 1f)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (direction == Direction.Invalid)
                throw new ArgumentOutOfRangeException(nameof(direction));

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return direction switch
            {
                Direction.Up    => new Point(point.X, point.Y - distance),
                Direction.Right => new Point(point.X + distance, point.Y),
                Direction.Down  => new Point(point.X, point.Y + distance),
                Direction.Left  => new Point(point.X - distance, point.Y),
                _               => throw new Exception($"Can not offset by {direction} direction.")
            };
        }

        /// <summary>
        ///     Calculates this point's relation to another point by local direction.
        /// </summary>
        /// <param name="point">The point who's relation to another point you want to know.</param>
        /// <param name="other">The other point</param>
        /// <returns>
        ///     <see cref="Direction" />
        ///     <br />
        ///     The local direction of <paramref name="point" /> from the <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">point</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static Direction DirectionalRelationTo(this IPoint point, IPoint other)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var direction = Direction.Invalid;
            var degree = 0.0f;

            if (point.Y.SignificantlyLessThan(other.Y, CONSTANTS.EPSILON))
            {
                degree = other.Y - point.Y;
                direction = Direction.Up;
            } else if (point.Y.SignificantlyGreaterThan(other.Y, CONSTANTS.EPSILON))
            {
                degree = point.Y - other.Y;
                direction = Direction.Down;
            }

            if (point.X.SignificantlyGreaterThan(other.X, CONSTANTS.EPSILON))
            {
                if (degree.SignificantlyLessThan(point.X - other.X, CONSTANTS.EPSILON))
                    direction = Direction.Right;
            } else if (point.X.SignificantlyLessThan(other.X, CONSTANTS.EPSILON))
                if (degree.SignificantlyLessThan(other.X - point.X, CONSTANTS.EPSILON))
                    direction = Direction.Left;

            return direction;
        }

        /// <summary>
        ///     Calculates the euclidean distance between two points.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <param name="other">Another point.</param>
        /// <returns>
        ///     <see cref="float" />
        ///     <br />
        ///     The euclidean distance between <paramref name="point" /> and the <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">point</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static float Distance(this IPoint point, IPoint other)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var result = MathEx.Hypot(other.X - point.X, other.Y - point.Y);

            return result.NearlyEquals(0, CONSTANTS.EPSILON) ? 0 : result;
        }

        /// <summary>
        ///     Calculates the euclidean distance between two points, skipping the sqrt op. <br />
        ///     This is useful when doing operations based on distance while not needing the actual value.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <param name="other">Another point.</param>
        /// <returns>
        ///     <see cref="float" />
        ///     <br />
        ///     The euclidean distance between <paramref name="point" /> and the <paramref name="other" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">point</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static float FastDistance(this IPoint point, IPoint other)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return (float)(Math.Pow(other.X - point.X, 2) + Math.Pow(other.Y - point.Y, 2));
        }

        /// <summary>
        ///     Creates a new <see cref="Geometry.Point" /> from a <see cref="IPoint" />.
        /// </summary>
        /// <param name="point">An implementation of <see cref="IPoint" />.</param>
        /// <returns>
        ///     <see cref="Geometry.Point" />
        ///     <br />
        ///     A new point.
        /// </returns>
        /// <exception cref="ArgumentNullException">point</exception>
        public static Point GetPoint(this IPoint point) =>
            point switch
            {
                null     => throw new ArgumentNullException(nameof(point)),
                Point pt => pt,
                _        => new Point(point.X, point.Y)
            };

        /// <summary>
        ///     Calculates the midpoint between two points.
        /// </summary>
        /// <param name="point">A point.</param>
        /// <param name="other">Another point.</param>
        /// <returns>
        ///     <see cref="Geometry.Point" /> <br />
        ///     The midpoint between <paramref name="point" /> and <paramref name="other" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">point</exception>
        /// <exception cref="ArgumentNullException">other</exception>
        public static Point MidPoint(this IPoint point, IPoint other)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return new Point((point.X + other.X) / 2, (point.Y + other.Y) / 2);
        }

        /// <summary>
        ///     Lazily generates points between two points. <br />
        ///     https://playtechs.blogspot.com/2007/03/raytracing-on-grid.html
        /// </summary>
        /// <param name="point">A point.</param>
        /// <param name="other">Another point.</param>
        /// <returns>
        ///     <see cref="IEnumerable{T}" /> of <see cref="Point" /><br />
        ///     An enumeration of points that a line drawn from <paramref name="point" /> to the <paramref name="other" /> would
        ///     cross over.
        /// </returns>
        /// <exception cref="ArgumentNullException">point</exception>
        /// <exception cref="ArgumentNullException">other</exception>
        public static IEnumerable<Point> RayTraceTo(this IPoint point, IPoint other)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            var x0 = point.X;
            var y0 = point.Y;
            var x1 = other.X;
            var y1 = other.Y;
            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var x = (int)Math.Floor(x0);
            var y = (int)Math.Floor(y0);
            var n = 1;
            int xOffset;
            int yOffset;
            float error;

            if (dx == 0)
            {
                xOffset = 0;
                error = float.PositiveInfinity;
            } else if (x1 > x0)
            {
                xOffset = 1;
                n += (int)Math.Floor(x1) - x;
                error = (float)(Math.Floor(x0) + 1 - x0) * dy;
            } else
            {
                xOffset = -1;
                n += x - (int)Math.Floor(x1);
                error = (float)(x0 - Math.Floor(x0)) * dy;
            }

            if (dy == 0)
            {
                yOffset = 0;
                error -= float.PositiveInfinity;
            } else if (y1 > y0)
            {
                yOffset = 1;
                n += (int)Math.Floor(y1) - y;
                error -= (float)(Math.Floor(y0) + 1 - y0) * dx;
            } else
            {
                yOffset = -1;
                n += y - (int)Math.Floor(y1);
                error -= (float)(y0 - Math.Floor(y0)) * dx;
            }

            for (; n > 0; --n)
            {
                yield return new Point(x, y);

                if (error > 0)
                {
                    y += yOffset;
                    error -= dx;
                } else
                {
                    x += xOffset;
                    error += dy;
                }
            }
        }

        /// <summary>
        ///     Moves an point towards another at a given speed.
        /// </summary>
        /// <param name="p1">The starting point.</param>
        /// <param name="p2">The end point.</param>
        /// <param name="maxDistance">The max distance to translate by.</param>
        /// <returns>
        ///     <see cref="Geometry.Point" /> <br />
        ///     A new point.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">point</exception>
        /// <exception cref="System.ArgumentNullException">other</exception>
        public static Point Translate(this IPoint p1, IPoint p2, float maxDistance)
        {
            if (p1 == null)
                throw new ArgumentNullException(nameof(p1));

            if (p2 == null)
                throw new ArgumentNullException(nameof(p2));

            var distance = p1.Distance(p2);

            if (distance.SignificantlyGreaterThan(maxDistance, CONSTANTS.EPSILON))
                distance = maxDistance;
            else if (distance.NearlyEqualOrLessThan(maxDistance, CONSTANTS.EPSILON))
                return p2.GetPoint();

            var angle = p2.AngularRelationTo(p1);

            return p1.AngularOffset(angle, distance);
        }
    }
}