#region
using AL.Core.Extensions;
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Where a walk may stop to count as having arrived: a rectangle band inflated by a range. The set of positions that
///     opens a door is exactly this shape (the server measures a box against a box, per axis, clamped at zero), and a
///     plain destination is a zero-size band with its radius as the range.
/// </summary>
public readonly struct Reach
{
    /// <summary>
    ///     The rectangle the range is measured from.
    /// </summary>
    public Rectangle Band { get; }

    /// <summary>
    ///     How far outside the band still counts.
    /// </summary>
    public float Range { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Reach" /> struct.
    /// </summary>
    public Reach(Rectangle band, float range)
    {
        Band = band;
        Range = range;
    }

    /// <summary>
    ///     A circular reach about a point.
    /// </summary>
    public static Reach Circle(float x, float y, float radius)
        => new(
            new Rectangle(
                x,
                y,
                0f,
                0f),
            radius);

    /// <summary>
    ///     The distance from (x, y) to the band's edge, zero inside it.
    /// </summary>
    public float Distance(float x, float y) => Band.EdgeToCenterDistance(new ValuePoint(x, y));

    /// <summary>
    ///     Whether (x, y) is inside the reach.
    /// </summary>
    public bool Contains(float x, float y) => Distance(x, y) <= Range;

    /// <summary>
    ///     The point on the reach's boundary nearest to (x, y): the clamp of the point into the band, stepped back toward the
    ///     point by the range. A point already inside answers itself.
    /// </summary>
    public (float X, float Y) NearEdge(float x, float y)
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
    ///     The first point along the segment from (x0, y0) to (x1, y1) that lies inside the reach. The distance to a convex
    ///     region is convex along a line, so with the start outside and the end inside there is exactly one crossing, found by
    ///     bisection.
    /// </summary>
    public bool TryEntry(
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

        var outside = 0f;
        var inside = 1f;

        //24 halvings of a leg under 16k units is under a thousandth of a unit
        for (var i = 0; i < 24; i++)
        {
            var mid = (outside + inside) / 2f;
            var midX = x0 + (x1 - x0) * mid;
            var midY = y0 + (y1 - y0) * mid;

            if (Contains(midX, midY))
                inside = mid;
            else
                outside = mid;
        }

        entryX = x0 + (x1 - x0) * inside;
        entryY = y0 + (y1 - y0) * inside;

        return true;
    }
}