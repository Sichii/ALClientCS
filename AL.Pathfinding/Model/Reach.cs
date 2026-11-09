#region
using AL.Core.Extensions;
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents where a walk may stop to count as having arrived: a rectangle band inflated by a range. A door opens
///     from exactly this shape, and a plain destination is a zero-size band with its radius as the range.
/// </summary>
public readonly struct Reach
{
    /// <summary>The rectangle the range is measured from.</summary>
    public Rectangle Band { get; }

    /// <summary>How far outside the band still counts.</summary>
    public float Range { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Reach" /> struct.
    /// </summary>
    /// <param name="band">The rectangle the range is measured from.</param>
    /// <param name="range">How far outside the band still counts.</param>
    public Reach(Rectangle band, float range)
    {
        Band = band;
        Range = range;
    }

    /// <summary>Creates a circular reach about a point.</summary>
    /// <param name="x">The centre's x.</param>
    /// <param name="y">The centre's y.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>
    ///     A zero-size band at the point with the radius as its range.
    /// </returns>
    public static Reach CreateCircle(float x, float y, float radius)
        => new(
            new Rectangle(
                x,
                y,
                0f,
                0f),
            radius);

    /// <summary>
    ///     Calculates the distance from a point to the band's edge.
    /// </summary>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <returns>The distance to the band, zero inside it.</returns>
    public float Distance(float x, float y) => Band.EdgeToCenterDistance(new ValuePoint(x, y));

    /// <summary>Determines whether a point is inside the reach.</summary>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <returns>
    ///     <c>true</c> if the point is within the range of the band; otherwise, <c>false</c> .
    /// </returns>
    public bool Contains(float x, float y) => Distance(x, y) <= Range;

    /// <summary>
    ///     Finds the point on the reach's boundary nearest to a point outside it.
    /// </summary>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <returns>
    ///     The nearest boundary point, or the point itself when it is already inside.
    /// </returns>
    public (float X, float Y) FindNearestEdgePoint(float x, float y)
    {
        var distance = Distance(x, y);

        if (distance <= Range)
            return (x, y);

        var clampX = Math.Clamp(x, Band.Left, Band.Right);
        var clampY = Math.Clamp(y, Band.Top, Band.Bottom);
        var scale = Range / distance;

        return (clampX + (x - clampX) * scale, clampY + (y - clampY) * scale);
    }

    /// <summary>
    ///     Finds the first point along a segment that lies inside the reach.
    /// </summary>
    /// <param name="x0">The segment start's x.</param>
    /// <param name="y0">The segment start's y.</param>
    /// <param name="x1">The segment end's x.</param>
    /// <param name="y1">The segment end's y.</param>
    /// <param name="entryX">
    ///     The entry point's x, or the start's when the segment never enters.
    /// </param>
    /// <param name="entryY">
    ///     The entry point's y, or the start's when the segment never enters.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the start or the end is inside the reach; otherwise, <c>false</c> .
    /// </returns>
    public bool TryFindEntry(
        float x0,
        float y0,
        float x1,
        float y1,
        out float entryX,
        out float entryY)
    {
        entryX = x0;
        entryY = y0;

        if (Contains(x0, y0))
            return true;

        if (!Contains(x1, y1))
            return false;

        //an end inside always has an entry; a segment grazing the edge can miss it by rounding, so it enters at the end
        if (!TryFindFirstEntry(
                x0,
                y0,
                x1,
                y1,
                out entryX,
                out entryY))
        {
            entryX = x1;
            entryY = y1;
        }

        return true;
    }

    /// <summary>
    ///     Finds the first point along a segment that lies inside the reach, wherever the segment ends.
    /// </summary>
    /// <param name="x0">The segment start's x.</param>
    /// <param name="y0">The segment start's y.</param>
    /// <param name="x1">The segment end's x.</param>
    /// <param name="y1">The segment end's y.</param>
    /// <param name="entryX">
    ///     The entry point's x, or the start's when the segment never enters.
    /// </param>
    /// <param name="entryY">
    ///     The entry point's y, or the start's when the segment never enters.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the segment has a point that tests inside the reach; otherwise, <c>false</c> .
    /// </returns>
    /// <remarks>
    ///     Exact rather than searched: the reach is the band widened by the range on x, the band widened by it on y, and a
    ///     circle of the range on each corner, and the entry is the earliest of the segment's entries into those six.
    /// </remarks>
    public bool TryFindFirstEntry(
        float x0,
        float y0,
        float x1,
        float y1,
        out float entryX,
        out float entryY)
    {
        entryX = x0;
        entryY = y0;

        if (Contains(x0, y0))
            return true;

        double dx = x1 - x0;
        double dy = y1 - y0;
        double range = Range;
        var band = Band;

        var t = CalculateRectangleEntry(
            band.Left - range,
            band.Top,
            band.Right + range,
            band.Bottom,
            x0,
            y0,
            dx,
            dy);

        t = Math.Min(
            t,
            CalculateRectangleEntry(
                band.Left,
                band.Top - range,
                band.Right,
                band.Bottom + range,
                x0,
                y0,
                dx,
                dy));

        t = Math.Min(
            t,
            CalculateCircleEntry(
                band.Left - x0,
                band.Top - y0,
                range,
                dx,
                dy));

        t = Math.Min(
            t,
            CalculateCircleEntry(
                band.Right - x0,
                band.Top - y0,
                range,
                dx,
                dy));

        t = Math.Min(
            t,
            CalculateCircleEntry(
                band.Left - x0,
                band.Bottom - y0,
                range,
                dx,
                dy));

        t = Math.Min(
            t,
            CalculateCircleEntry(
                band.Right - x0,
                band.Bottom - y0,
                range,
                dx,
                dy));

        if (t > 1)
            return false;

        entryX = (float)(x0 + dx * t);
        entryY = (float)(y0 + dy * t);

        //the exact entry can round to just outside; step in from a hundred-thousandth of a unit, doubling, until it
        //tests inside. A graze that never does is no entry
        var step = 1e-5 / Math.Sqrt(dx * dx + dy * dy);

        for (var i = 0; (i < 12) && !Contains(entryX, entryY); i++, step *= 2)
        {
            var nudged = Math.Min(1, t + step);
            var nx = nudged < 1 ? (float)(x0 + dx * nudged) : x1;
            var ny = nudged < 1 ? (float)(y0 + dy * nudged) : y1;

            if (!Contains(nx, ny))
                continue;

            entryX = nx;
            entryY = ny;
        }

        if (Contains(entryX, entryY))
            return true;

        entryX = x0;
        entryY = y0;

        return false;
    }

    /// <summary>
    ///     Calculates where a segment from outside first enters a circle, as a fraction of the segment.
    /// </summary>
    /// <param name="cx">
    ///     The circle centre's x, relative to the segment start.
    /// </param>
    /// <param name="cy">
    ///     The circle centre's y, relative to the segment start.
    /// </param>
    /// <param name="radius">The circle's radius.</param>
    /// <param name="dx">The segment's x extent.</param>
    /// <param name="dy">The segment's y extent.</param>
    /// <returns>
    ///     The entry fraction, or <see cref="double.MaxValue" /> when the segment misses.
    /// </returns>
    private static double CalculateCircleEntry(
        double cx,
        double cy,
        double radius,
        double dx,
        double dy)
    {
        var a = dx * dx + dy * dy;

        if (a <= 0)
            return double.MaxValue;

        var b = dx * cx + dy * cy;
        var c = cx * cx + cy * cy - radius * radius;
        var discriminant = b * b - a * c;

        if (discriminant < 0)
            return double.MaxValue;

        var t = (b - Math.Sqrt(discriminant)) / a;

        return t is >= 0 and <= 1 ? t : double.MaxValue;
    }

    /// <summary>
    ///     Calculates where a segment from outside first enters a rectangle, as a fraction of the segment, by clipping it
    ///     against each pair of sides in turn.
    /// </summary>
    /// <param name="left">The rectangle's least x.</param>
    /// <param name="top">The rectangle's least y.</param>
    /// <param name="right">The rectangle's greatest x.</param>
    /// <param name="bottom">The rectangle's greatest y.</param>
    /// <param name="x0">The segment start's x.</param>
    /// <param name="y0">The segment start's y.</param>
    /// <param name="dx">The segment's x extent.</param>
    /// <param name="dy">The segment's y extent.</param>
    /// <returns>
    ///     The entry fraction, or <see cref="double.MaxValue" /> when the segment misses.
    /// </returns>
    private static double CalculateRectangleEntry(
        double left,
        double top,
        double right,
        double bottom,
        double x0,
        double y0,
        double dx,
        double dy)
    {
        var enter = 0d;
        var leave = 1d;

        if (!ClipSlab(
                left - x0,
                right - x0,
                dx,
                ref enter,
                ref leave)
            || !ClipSlab(
                top - y0,
                bottom - y0,
                dy,
                ref enter,
                ref leave))
            return double.MaxValue;

        return enter;

        static bool ClipSlab(
            double low,
            double high,
            double d,
            ref double enter,
            ref double leave)
        {
            if (d == 0)
                return (low <= 0) && (high >= 0);

            var t0 = low / d;
            var t1 = high / d;

            if (t0 > t1)
                (t0, t1) = (t1, t0);

            enter = Math.Max(enter, t0);
            leave = Math.Min(leave, t1);

            return enter <= leave;
        }
    }
}