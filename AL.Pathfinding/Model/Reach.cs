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

        var outside = 0f;
        var inside = 1f;

        //the reach is convex, so one crossing lies between an outside start and an inside end
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