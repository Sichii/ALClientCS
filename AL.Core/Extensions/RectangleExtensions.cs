#region
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="IRectangle" />s.
/// </summary>
public static class RectangleExtensions
{
    extension<T>(T rect) where T: IRectangle, allows ref struct
    {
        /// <summary>
        ///     Calculates the distance between this rectangle and a point, taken per axis and clamped at zero, the way the
        ///     server measures it.
        /// </summary>
        /// <param name="other">
        ///     A point.
        /// </param>
        /// <returns>
        ///     The distance from the rectangle's edge to the point, zero when the point is inside.
        /// </returns>
        public float EdgeToCenterDistance<T2>(T2 other) where T2: IPoint, allows ref struct
        {
            var dx = MathF.Max(MathF.Max(other.X - rect.Right, rect.Left - other.X), 0f);
            var dy = MathF.Max(MathF.Max(other.Y - rect.Bottom, rect.Top - other.Y), 0f);

            return MathEx.Hypot(dx, dy);
        }

        /// <summary>
        ///     Calculates the gap between two rectangles, taken per axis and clamped at zero. The server resolves every attack,
        ///     skill and aggro range check with this measure.
        /// </summary>
        /// <param name="other">
        ///     Another rectangle.
        /// </param>
        /// <returns>
        ///     The gap between the rectangles, zero when they overlap.
        /// </returns>
        public float EdgeToEdgeDistance<T2>(T2 other) where T2: IRectangle, allows ref struct
        {
            var dx = MathF.Max(MathF.Max(other.Left - rect.Right, rect.Left - other.Right), 0f);
            var dy = MathF.Max(MathF.Max(other.Top - rect.Bottom, rect.Top - other.Bottom), 0f);

            return MathEx.Hypot(dx, dy);
        }

        /// <summary>
        ///     Determines whether two rectangles touch or overlap.
        /// </summary>
        /// <param name="other">Another rectangle.</param>
        /// <returns>
        ///     <c>true</c> if the rectangles touch or overlap; otherwise, <c>false</c>.
        /// </returns>
        public bool Intersects<T2>(T2 other) where T2: IRectangle, allows ref struct
            => (rect.Left <= other.Right) && (rect.Right >= other.Left) && (rect.Top <= other.Bottom) && (rect.Bottom >= other.Top);

        /// <summary>
        ///     Lazily generates the points inside the rectangle, one per unit by default.
        /// </summary>
        /// <param name="widthStepNum">
        ///     The number of steps across the width, or -1 for one per unit.
        /// </param>
        /// <param name="heightStepNum">
        ///     The number of steps down the height, or -1 for one per unit.
        /// </param>
        /// <returns>
        ///     The points inside the rectangle.
        /// </returns>
        public IEnumerable<Point> Points(float widthStepNum = -1f, float heightStepNum = -1f)
            => GenerateInnerPoints(
                rect.Left,
                rect.Top,
                rect.Right,
                rect.Bottom,
                widthStepNum.IsNear(-1f, CONSTANTS.EPSILON) ? 1 : rect.Width / widthStepNum,
                heightStepNum.IsNear(-1f, CONSTANTS.EPSILON) ? 1 : rect.Height / heightStepNum);
    }

    //kept on the interface: Contains has the same shape on circles, polygons and triangles

    /// <summary>
    ///     Determines whether a rectangle fully encompasses another rectangle.
    /// </summary>
    /// <param name="rect">
    ///     A rectangle.
    /// </param>
    /// <param name="other">
    ///     Another rectangle.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if this rectangle fully encompasses the other (or edges touch); otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     rect
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     other
    /// </exception>
    public static bool Contains(this IRectangle rect, IRectangle other)
    {
        ArgumentNullException.ThrowIfNull(rect);

        ArgumentNullException.ThrowIfNull(other);

        return (rect.Bottom >= other.Bottom) && (rect.Left <= other.Left) && (rect.Right >= other.Right) && (rect.Top <= other.Top);
    }

    /// <summary>
    ///     Determines whether a rectangle contains a given point.
    /// </summary>
    /// <param name="rect">
    ///     A rectangle.
    /// </param>
    /// <param name="point">
    ///     A point.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if the point lies within or on the edge of the rectangle; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     rect
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     point
    /// </exception>
    public static bool Contains(this IRectangle rect, IPoint point)
    {
        ArgumentNullException.ThrowIfNull(rect);

        ArgumentNullException.ThrowIfNull(point);

        return (rect.Left <= point.X) && (rect.Right > point.X) && (rect.Top <= point.Y) && (rect.Bottom > point.Y);
    }

    /// <summary>
    ///     Generates the points for <c>Points</c>, which cannot be an iterator itself because an extension member with a
    ///     ref struct receiver cannot be one.
    /// </summary>
    /// <param name="left">
    ///     The left edge.
    /// </param>
    /// <param name="top">
    ///     The top edge.
    /// </param>
    /// <param name="right">
    ///     The right edge.
    /// </param>
    /// <param name="bottom">
    ///     The bottom edge.
    /// </param>
    /// <param name="horizontalStep">
    ///     The distance between points along x.
    /// </param>
    /// <param name="verticalStep">
    ///     The distance between points along y.
    /// </param>
    /// <returns>
    ///     The points inside the rectangle.
    /// </returns>
    private static IEnumerable<Point> GenerateInnerPoints(
        float left,
        float top,
        float right,
        float bottom,
        float horizontalStep,
        float verticalStep)
    {
        for (var x = left; x <= right; x += horizontalStep)
            for (var y = top; y <= bottom; y += verticalStep)
                yield return new Point(x, y);
    }
}