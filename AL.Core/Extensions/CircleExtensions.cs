#region
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Extensions;

public static class CircleExtensions
{
    extension<T>(T circle) where T: ICircle, allows ref struct
    {
        /// <summary>
        ///     The distance from the edge of this circle to a point, zero when the point is inside.
        /// </summary>
        public float EdgeToCenterDistance<T2>(T2 other) where T2: IPoint, allows ref struct
            => MathF.Max(0f, circle.Distance(other) - circle.Radius);

        /// <summary>
        ///     The distance between the edges of two circles, zero when they overlap.
        /// </summary>
        public float EdgeToEdgeDistance<T2>(T2 other) where T2: ICircle, allows ref struct
            => MathF.Max(0f, circle.Distance(other) - circle.Radius - other.Radius);

        /// <summary>
        ///     Lazily generates points spaced evenly around the circumference.
        /// </summary>
        public IEnumerable<Point> GenerateCircumferencePoints(float numberOfPoints, float startingAngle = 0f)
            => Circumference(
                circle.X,
                circle.Y,
                circle.Radius,
                numberOfPoints,
                startingAngle);

        /// <summary>
        ///     Whether two circles touch or overlap.
        /// </summary>
        public bool Intersects<T2>(T2 other) where T2: ICircle, allows ref struct
            => circle.Distance(other) <= (circle.Radius + other.Radius);

        /// <summary>
        ///     Lazily generates points inside the circle on a grid of <paramref name="numberOfSteps" /> per diameter.
        /// </summary>
        public IEnumerable<Point> Points(float numberOfSteps)
            => InnerPoints(
                circle.X,
                circle.Y,
                circle.Radius,
                numberOfSteps);
    }

    //an extension member with a ref struct receiver cannot be an iterator, so the lazy ones hand their numbers to these
    private static IEnumerable<Point> Circumference(
        float x,
        float y,
        float radius,
        float numberOfPoints,
        float startingAngle)
    {
        var center = new Point(x, y);
        var anglePerPoint = 360 / numberOfPoints;

        for (var traversedAngle = 0f; traversedAngle.IsLess(360, CONSTANTS.EPSILON); traversedAngle += anglePerPoint)
            yield return center.AngularOffset(startingAngle + traversedAngle, radius);
    }

    //kept on the interface: Contains has the same shape on rectangles, polygons and triangles

    /// <summary>
    ///     Determines whether this circle fully encompasses another circle.
    /// </summary>
    /// <param name="circle">
    ///     This circle.
    /// </param>
    /// <param name="other">
    ///     Another circle.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if this circle fully encompasses the other (or edges touch); otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     circle
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     other
    /// </exception>
    public static bool Contains(this ICircle circle, ICircle other)
    {
        ArgumentNullException.ThrowIfNull(circle);

        ArgumentNullException.ThrowIfNull(other);

        return circle.Radius >= (circle.EdgeToEdgeDistance(other) + other.Radius);
    }

    /// <summary>
    ///     Determines whether this circle contains the given point.
    /// </summary>
    /// <param name="circle">
    ///     This circle.
    /// </param>
    /// <param name="point">
    ///     A point.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if this circle contains the point, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     circle
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     point
    /// </exception>
    public static bool Contains(this ICircle circle, IPoint point)
    {
        ArgumentNullException.ThrowIfNull(circle);

        ArgumentNullException.ThrowIfNull(point);

        return point.Distance(circle) < circle.Radius;
    }

    private static IEnumerable<Point> InnerPoints(
        float cx,
        float cy,
        float radius,
        float numberOfSteps)
    {
        var stepSize = radius / numberOfSteps * 2;
        var radiusSquared = radius * radius;

        for (var x = cx - radius; x <= cx; x += stepSize)
            for (var y = cy - radius; y <= cy; y += stepSize)
            {
                var xdc = x - cx;
                var ydc = y - cy;

                if ((xdc * xdc + ydc * ydc) <= radiusSquared)
                {
                    var xS = cx - xdc;
                    var yS = cy - ydc;

                    yield return new Point(x, y);
                    yield return new Point(x, yS);
                    yield return new Point(xS, y);
                    yield return new Point(xS, yS);
                }
            }
    }
}