using System;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;

namespace AL.Core.Extensions
{
    public static class PointExtensions
    {
        public static Point AngularOffset(this IPoint point, float angle, float distance = 1f)
        {
            var theta = angle * Math.PI / 180;
            var x = (float) Math.Cos(theta) * distance;
            var y = (float) Math.Sin(theta) * distance;

            return new Point(point.X + x, point.Y + y);
        }

        public static float AngularRelationTo(this IPoint p1, IPoint p2)
        {
            if (p2 == null)
                throw new Exception("Other point is null when getting relation.");

            var deltaX = p1.X - p2.X;
            var deltaY = p1.Y - p2.Y;

            return (float) (Math.Atan2(deltaY, deltaX) * (180 / Math.PI));
        }

        public static Point DirectionalOffset(this IPoint point, Direction direction, float distance = 1f) =>
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            direction switch
            {
                Direction.Up    => new Point(point.X, point.Y - distance),
                Direction.Right => new Point(point.X + distance, point.Y),
                Direction.Down  => new Point(point.X, point.Y + distance),
                Direction.Left  => new Point(point.X - distance, point.Y),
                _               => throw new Exception($"Can not offset by {direction} direction.")
            };

        public static Direction DirectionalRelationTo(this IPoint p1, IPoint p2)
        {
            if (p2 == null)
                throw new Exception("Other point is null when getting relation.");

            var direction = Direction.Invalid;
            var degree = 0.0F;
            if (p1.Y.SignificantlyLessThan(p2.Y, CONSTANTS.EPSILON))
            {
                degree = p2.Y - p1.Y;
                direction = Direction.Up;
            }

            if (p1.X.SignificantlyGreaterThan(p2.X, CONSTANTS.EPSILON)
                && degree.SignificantlyLessThan(p1.X - p2.X, CONSTANTS.EPSILON))
            {
                degree = p1.X - p2.X;
                direction = Direction.Right;
            }

            if (p1.Y.SignificantlyGreaterThan(p2.Y, CONSTANTS.EPSILON)
                && degree.SignificantlyLessThan(p1.Y - p2.Y, CONSTANTS.EPSILON))
            {
                degree = p1.Y - p2.Y;
                direction = Direction.Down;
            }

            if (p1.X.SignificantlyLessThan(p2.X, CONSTANTS.EPSILON)
                && degree.SignificantlyLessThan(p2.X - p1.X, CONSTANTS.EPSILON))
                direction = Direction.Left;

            return direction;
        }

        public static float Distance(this IPoint p1, IPoint p2)
        {
            var result = MathEx.Hypot(p2.X - p1.X, p2.Y - p1.Y);
            return result.NearlyEquals(0, CONSTANTS.EPSILON) ? 0 : result;
        }

        public static float FastDistance(this IPoint p1, IPoint p2) =>
            (float) (Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

        public static Point Lerp(this IPoint p1, IPoint p2, float maxMove, float minDiff) =>
            new(MathEx.Lerp(p1.X, p2.X, maxMove, minDiff), MathEx.Lerp(p1.Y, p2.Y, maxMove, minDiff));

        public static Point MidPoint(this IPoint p1, IPoint p2) => new((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        public static Point Point(this IPoint point) => new(point.X, point.Y);
    }
}