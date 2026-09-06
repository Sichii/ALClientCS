#region
using System.Runtime.CompilerServices;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="IPoint" />s. Generic over the receiver so a struct point is never
///     boxed to call them, and open to ref structs so <see cref="ValuePoint" /> can call them too.
/// </summary>
public static class PointExtensions
{
    extension<T>(T point) where T: IPoint, allows ref struct
    {
        /// <summary>
        ///     Calculates a new point, offsetting this point by a given distance at a given angle in degrees.
        /// </summary>
        public Point AngularOffset(float angle, float distance = 1f)
        {
            var theta = angle * Math.PI / 180;
            var x = (float)Math.Cos(theta) * distance;
            var y = (float)Math.Sin(theta) * distance;

            return new Point(point.X + x, point.Y + y);
        }

        /// <summary>
        ///     Calculates a new point, offsetting this point by a given distance in a given direction.
        /// </summary>
        public Point DirectionalOffset(Direction direction, float distance = 1f)
        {
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
        ///     Calculates the euclidean distance between two points.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance<T2>(T2 other) where T2: IPoint, allows ref struct
        {
            var dx = other.X - point.X;
            var dy = other.Y - point.Y;

            return MathF.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        ///     Calculates the squared euclidean distance between two points, for comparisons that do not need the root.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float FastDistance<T2>(T2 other) where T2: IPoint, allows ref struct
        {
            var dx = other.X - point.X;
            var dy = other.Y - point.Y;

            return dx * dx + dy * dy;
        }

        /// <summary>
        ///     Calculates the midpoint between two points.
        /// </summary>
        public Point MidPoint<T2>(T2 other) where T2: IPoint, allows ref struct => new((point.X + other.X) / 2, (point.Y + other.Y) / 2);

        /// <summary>
        ///     Lazily generates the grid cells a line drawn from this point to <paramref name="other" /> crosses.
        /// </summary>
        public RayTrace RayTraceTo<T2>(T2 other) where T2: IPoint, allows ref struct => new(point.X, point.Y, other.X, other.Y);

        /// <summary>
        ///     Creates a new <see cref="Point" /> from this point.
        /// </summary>
        public Point ToPoint() => new(point.X, point.Y);
    }

    //the three below keep their interface signatures. LocationExtensions and InstancedLocationExtensions carry
    //same-named overloads that add a map or instance check, and a generic receiver here would outrank those for
    //every class-typed caller and silently drop the check

    /// <summary>
    ///     Calculates this point's relation to another point in degrees.
    /// </summary>
    /// <param name="point">
    ///     The point who's relation to another point you want to know.
    /// </param>
    /// <param name="other">
    ///     The other point
    /// </param>
    /// <returns>
    ///     <see cref="float" />
    ///     <br />
    ///     The angle of <paramref name="point" /> from the <paramref name="other" />.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     point
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     other
    /// </exception>
    public static float AngularRelationTo(this IPoint point, IPoint other)
    {
        ArgumentNullException.ThrowIfNull(point);

        ArgumentNullException.ThrowIfNull(other);

        var deltaX = point.X - other.X;
        var deltaY = point.Y - other.Y;

        return (float)(Math.Atan2(deltaY, deltaX) * (180 / Math.PI));
    }

    /// <summary>
    ///     Calculates this point's relation to another point by local direction.
    /// </summary>
    /// <param name="point">
    ///     The point who's relation to another point you want to know.
    /// </param>
    /// <param name="other">
    ///     The other point
    /// </param>
    /// <returns>
    ///     <see cref="Direction" />
    ///     <br />
    ///     The local direction of <paramref name="point" /> from the <paramref name="other" />.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     point
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     other
    /// </exception>
    public static Direction DirectionalRelationTo(this IPoint point, IPoint other)
    {
        ArgumentNullException.ThrowIfNull(point);

        ArgumentNullException.ThrowIfNull(other);

        var direction = Direction.Invalid;
        var degree = 0.0f;

        if (point.Y.IsLess(other.Y, CONSTANTS.EPSILON))
        {
            degree = other.Y - point.Y;
            direction = Direction.Up;
        } else if (point.Y.IsGreater(other.Y, CONSTANTS.EPSILON))
        {
            degree = point.Y - other.Y;
            direction = Direction.Down;
        }

        if (point.X.IsGreater(other.X, CONSTANTS.EPSILON))
        {
            if (degree.IsLess(point.X - other.X, CONSTANTS.EPSILON))
                direction = Direction.Right;
        } else if (point.X.IsLess(other.X, CONSTANTS.EPSILON))
            if (degree.IsLess(other.X - point.X, CONSTANTS.EPSILON))
                direction = Direction.Left;

        return direction;
    }

    /// <summary>
    ///     Moves an point towards another at a given speed.
    /// </summary>
    /// <param name="p1">
    ///     The starting point.
    /// </param>
    /// <param name="p2">
    ///     The end point.
    /// </param>
    /// <param name="maxDistance">
    ///     The max distance to translate by.
    /// </param>
    /// <returns>
    ///     <see cref="Geometry.Point" />
    ///     <br />
    ///     A new point.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     point
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     other
    /// </exception>
    public static Point OffsetTowards(this IPoint p1, IPoint p2, float maxDistance)
    {
        ArgumentNullException.ThrowIfNull(p1);

        ArgumentNullException.ThrowIfNull(p2);

        var distance = p1.Distance(p2);

        if (distance.IsGreater(maxDistance, CONSTANTS.EPSILON))
            distance = maxDistance;
        else if (distance.IsLessOrEqual(maxDistance, CONSTANTS.EPSILON))
            return p2.ToPoint();

        var angle = p2.AngularRelationTo(p1);

        return p1.AngularOffset(angle, distance);
    }
}
