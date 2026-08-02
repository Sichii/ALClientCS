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

    /// <summary>
    ///     Calculated the edge-to-center euclidean distance between this rectange, and some center-point.
    /// </summary>
    /// <param name="rect">
    ///     A rectangle.
    /// </param>
    /// <param name="other">
    ///     A center-point.
    /// </param>
    /// <returns>
    ///     <see cref="float" />
    ///     <br />
    ///     The lowest euclidean distance among the distances between this rectangle's vertices, and the other center-point.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// </exception>
    public static float EdgeToCenterDistance(this IRectangle rect, IPoint other)
    {
        ArgumentNullException.ThrowIfNull(rect);

        ArgumentNullException.ThrowIfNull(other);

        //per-axis separation, not the nearest vertex: an axis the point already lies within contributes nothing, which
        //is what makes this agree with the server. Taking the closest corner instead measures around the rectangle and
        //answers too large for anything alongside it
        var dx = MathF.Max(MathF.Max(other.X - rect.Right, rect.Left - other.X), 0f);
        var dy = MathF.Max(MathF.Max(other.Y - rect.Bottom, rect.Top - other.Y), 0f);

        return MathEx.Hypot(dx, dy);
    }

    /// <summary>
    ///     Calculates the edge-to-edge euclidean distance between two rectangles.
    /// </summary>
    /// <param name="rect">
    ///     A rectangle.
    /// </param>
    /// <param name="other">
    ///     Another rectangle.
    /// </param>
    /// <returns>
    ///     <see cref="float" />
    ///     <br />
    ///     The euclidean distance between the two closest points of <paramref name="rect" /> and the <paramref name="other" />
    ///     .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     rect
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     other
    /// </exception>
    public static float EdgeToEdgeDistance(this IRectangle rect, IRectangle other)
    {
        ArgumentNullException.ThrowIfNull(rect);

        ArgumentNullException.ThrowIfNull(other);

        //the gap on each axis independently, each clamped at zero, then combined. An axis the two already overlap on
        //contributes nothing, so two boxes side by side at the same height are exactly their horizontal gap apart -
        //which is the measure the server resolves every attack, skill and aggro check with. The nearest-vertex reading
        //this replaced answered too large whenever the boxes overlapped on one axis without their corners lining up,
        //and every range check in the client was built on it
        var dx = MathF.Max(MathF.Max(other.Left - rect.Right, rect.Left - other.Right), 0f);
        var dy = MathF.Max(MathF.Max(other.Top - rect.Bottom, rect.Top - other.Bottom), 0f);

        return MathEx.Hypot(dx, dy);
    }

    /// <summary>
    ///     Determines whether two rectangles overlap eachother.
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
    ///     if the rectangles touch or overlap,
    ///     <c>
    ///         false
    ///     </c>
    ///     otherwise.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     rect
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     other
    /// </exception>
    public static bool Intersects(this IRectangle rect, IRectangle other)
    {
        ArgumentNullException.ThrowIfNull(rect);

        ArgumentNullException.ThrowIfNull(other);

        return !((rect.Bottom > other.Top) || (rect.Left > other.Right) || (rect.Right < other.Left) || (rect.Top < other.Bottom));
    }

    /// <summary>
    ///     Lazily generates all points within a rectangle.
    /// </summary>
    /// <param name="rect">
    ///     A rectangle.
    /// </param>
    /// <param name="widthStepNum">
    ///     The number of points to generate horizontally.
    ///     <br />
    ///     -1 is equivalent to specifying the width.
    ///     <br />
    ///     <b>
    ///         Even numbers work best.
    ///     </b>
    /// </param>
    /// <param name="heightStepNum">
    ///     The number of points to generate vertically.
    ///     <br />
    ///     -1 is equivalent to the height.
    ///     <br />
    ///     <b>
    ///         Even numbers work best.
    ///     </b>
    /// </param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" /> of <see cref="Point" />
    ///     <br />
    ///     An enumeration of points generates from top left to bottom right.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     rect
    /// </exception>
    public static IEnumerable<Point> Points(this IRectangle rect, float widthStepNum = -1f, float heightStepNum = -1f)
    {
        ArgumentNullException.ThrowIfNull(rect);

        var horizontalStep = widthStepNum.IsNear(-1f, CONSTANTS.EPSILON) ? 1 : rect.Width / widthStepNum;
        var verticalStep = heightStepNum.IsNear(-1f, CONSTANTS.EPSILON) ? 1 : rect.Height / heightStepNum;

        for (var x = rect.Left; x <= rect.Right; x += horizontalStep)
            for (var y = rect.Top; y <= rect.Bottom; y += verticalStep)
                yield return new Point(x, y);
    }
}