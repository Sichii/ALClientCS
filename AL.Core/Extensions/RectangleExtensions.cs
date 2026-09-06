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
        ///     The separation between this rectangle and a point, taken per axis and clamped at zero. An axis the point already
        ///     lies within contributes nothing, which is what makes this agree with the server.
        /// </summary>
        public float EdgeToCenterDistance<T2>(T2 other) where T2: IPoint, allows ref struct
        {
            var dx = MathF.Max(MathF.Max(other.X - rect.Right, rect.Left - other.X), 0f);
            var dy = MathF.Max(MathF.Max(other.Y - rect.Bottom, rect.Top - other.Y), 0f);

            return MathEx.Hypot(dx, dy);
        }

        /// <summary>
        ///     The gap between two rectangles, taken per axis and clamped at zero, which is the measure the server resolves every
        ///     attack, skill and aggro check with.
        /// </summary>
        public float EdgeToEdgeDistance<T2>(T2 other) where T2: IRectangle, allows ref struct
        {
            var dx = MathF.Max(MathF.Max(other.Left - rect.Right, rect.Left - other.Right), 0f);
            var dy = MathF.Max(MathF.Max(other.Top - rect.Bottom, rect.Top - other.Bottom), 0f);

            return MathEx.Hypot(dx, dy);
        }

        //top is the smaller y and bottom the larger, y growing downward, so the vertical terms pair top against bottom
        /// <summary>
        ///     Whether two rectangles touch or overlap.
        /// </summary>
        public bool Intersects<T2>(T2 other) where T2: IRectangle, allows ref struct
            => (rect.Left <= other.Right) && (rect.Right >= other.Left) && (rect.Top <= other.Bottom) && (rect.Bottom >= other.Top);

        /// <summary>
        ///     Lazily generates the points inside the rectangle, one per unit by default.
        /// </summary>
        public IEnumerable<Point> Points(float widthStepNum = -1f, float heightStepNum = -1f)
            => InnerPoints(
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
    ///     <c>
    ///         true
    ///     </c>
    ///     if this rectangle fully encompasses the other (or edges touch); otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
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

        return (rect.Bottom >= other.Bottom) && (rect.Left >= other.Left) && (rect.Right <= other.Right) && (rect.Top <= other.Top);
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
    ///     <c>
    ///         true
    ///     </c>
    ///     if the point lies within or on the edge of the rectangle; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
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

    //an extension member with a ref struct receiver cannot be an iterator, so the lazy one hands its numbers to this
    private static IEnumerable<Point> InnerPoints(
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