#region
using System.Runtime.CompilerServices;
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents a map's wall lines in the shape the server keeps them, with the server's own <c>can_move</c> test over
///     them.
/// </summary>
public sealed class WallLines
{
    /// <summary>
    ///     The server's own epsilon, which widens the range check.
    /// </summary>
    private const double EPS = 1e-8;

    /// <summary>
    ///     The server's own epsilon, which keeps the divisor off zero for a vertical track.
    /// </summary>
    private const double REPS = 2.220446049250313e-16;

    private readonly int[] HorizontalEnd;
    private readonly int[] HorizontalOn;
    private readonly int[] HorizontalStart;
    private readonly int[] VerticalEnd;

    /// <summary>
    ///     The coordinate each vertical line sits on, sorted ascending.
    /// </summary>
    private readonly int[] VerticalOn;

    private readonly int[] VerticalStart;

    /// <summary>How many horizontal lines this map has.</summary>
    public int HorizontalCount => HorizontalOn.Length;

    /// <summary>How many vertical lines this map has.</summary>
    public int VerticalCount => VerticalOn.Length;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WallLines" /> class.
    /// </summary>
    /// <param name="verticalLines">The map's <c>x_lines</c>.</param>
    /// <param name="horizontalLines">The map's <c>y_lines</c>.</param>
    public WallLines(IReadOnlyList<StraightLine> verticalLines, IReadOnlyList<StraightLine> horizontalLines)
    {
        (VerticalOn, VerticalStart, VerticalEnd) = SortLines(verticalLines);
        (HorizontalOn, HorizontalStart, HorizontalEnd) = SortLines(horizontalLines);
    }

    /// <summary>
    ///     Determines whether any line passes through the collision box hanging on a point. The client refuses every move out
    ///     of such a point.
    /// </summary>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <param name="boundingBase">The collision base hanging on the point.</param>
    /// <returns>
    ///     <c>true</c> if a line passes through the box; otherwise, <c>false</c> .
    /// </returns>
    public bool BoxIntersects(double x, double y, BoundingBase boundingBase)
    {
        double h = boundingBase.HalfWidth;
        double v = boundingBase.VerticalNorth;
        double vn = boundingBase.VerticalNotNorth;

        return IntersectsSpan(
                   VerticalOn,
                   VerticalStart,
                   VerticalEnd,
                   x - h,
                   x + h,
                   y - v,
                   y + vn)
               || IntersectsSpan(
                   HorizontalOn,
                   HorizontalStart,
                   HorizontalEnd,
                   y - v,
                   y + vn,
                   x - h,
                   x + h);
    }

    /// <summary>
    ///     Determines whether a character can move between two points: the four corners of the base, plus two fence tracks
    ///     along the box's leading edges at the destination.
    /// </summary>
    /// <param name="x0">The start's x.</param>
    /// <param name="y0">The start's y.</param>
    /// <param name="x1">The destination's x.</param>
    /// <param name="y1">The destination's y.</param>
    /// <param name="boundingBase">The character's collision base.</param>
    /// <returns>
    ///     <c>true</c> if no track crosses a line; otherwise, <c>false</c> .
    /// </returns>
    public bool CanMove(
        double x0,
        double y0,
        double x1,
        double y1,
        BoundingBase boundingBase)
    {
        double h = boundingBase.HalfWidth;
        double v = boundingBase.VerticalNorth;
        double vn = boundingBase.VerticalNotNorth;

        if (!CanMoveLine(
                x0 - h,
                y0 + vn,
                x1 - h,
                y1 + vn))
            return false;

        if (!CanMoveLine(
                x0 + h,
                y0 + vn,
                x1 + h,
                y1 + vn))
            return false;

        if (!CanMoveLine(
                x0 - h,
                y0 - v,
                x1 - h,
                y1 - v))
            return false;

        if (!CanMoveLine(
                x0 + h,
                y0 - v,
                x1 + h,
                y1 - v))
            return false;

        //the fence: the two box edges that lead the move, checked as tracks at the destination so a wall that
        //slips between two corner tracks still refuses the move
        var px0 = h;
        var px1 = -h;

        if (x1 > x0)
        {
            px0 = -h;
            px1 = h;
        }

        var py0 = vn;
        var py1 = -v;

        if (y1 > y0)
        {
            py0 = -v;
            py1 = vn;
        }

        if (!CanMoveLine(
                x1 + px1,
                y1 + py0,
                x1 + px1,
                y1 + py1))
            return false;

        return CanMoveLine(
            x1 + px0,
            y1 + py1,
            x1 + px1,
            y1 + py1);
    }

    /// <summary>
    ///     Determines whether a single track between two points crosses no line, by the server's single-track test.
    /// </summary>
    /// <param name="x0">The start's x.</param>
    /// <param name="y0">The start's y.</param>
    /// <param name="x1">The end's x.</param>
    /// <param name="y1">The end's y.</param>
    /// <returns>
    ///     <c>true</c> if the track crosses no line; otherwise, <c>false</c> .
    /// </returns>
    public bool CanMoveLine(
        double x0,
        double y0,
        double x1,
        double y1)
        => IsTrackClear(
               VerticalOn,
               VerticalStart,
               VerticalEnd,
               x0,
               y0,
               x1,
               y1)
           && IsTrackClear(
               HorizontalOn,
               HorizontalStart,
               HorizontalEnd,
               y0,
               x0,
               y1,
               x1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindLowerBound(int[] on, double value)
    {
        var lo = 0;
        var hi = on.Length;

        while (lo < hi)
        {
            var mid = (lo + hi) >> 1;

            if (on[mid] < value)
                lo = mid + 1;
            else
                hi = mid;
        }

        return lo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IntersectsSpan(
        int[] on,
        int[] start,
        int[] end,
        double minA,
        double maxA,
        double minB,
        double maxB)
    {
        for (var i = FindLowerBound(on, minA); i < on.Length; i++)
        {
            double lineOn = on[i];

            if (maxA < lineOn)
                break;

            if ((start[i] <= maxB) && (end[i] >= minB))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Determines whether a track crosses none of one orientation's lines. For vertical lines a is x and b is y; for
    ///     horizontal lines the caller swaps them.
    /// </summary>
    /// <param name="on">
    ///     The coordinate each line sits on, sorted ascending.
    /// </param>
    /// <param name="start">The low end of each line's span.</param>
    /// <param name="end">The high end of each line's span.</param>
    /// <param name="a0">The start's coordinate across the lines.</param>
    /// <param name="b0">The start's coordinate along the lines.</param>
    /// <param name="a1">The end's coordinate across the lines.</param>
    /// <param name="b1">The end's coordinate along the lines.</param>
    /// <returns>
    ///     <c>true</c> if the track crosses no line; otherwise, <c>false</c> .
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTrackClear(
        int[] on,
        int[] start,
        int[] end,
        double a0,
        double b0,
        double a1,
        double b1)
    {
        var minA = Math.Min(a0, a1);
        var maxA = Math.Max(a0, a1);

        for (var i = FindLowerBound(on, minA); i < on.Length; i++)
        {
            double lineOn = on[i];
            double lineStart = start[i];
            double lineEnd = end[i];

            //a track may not end on a line, nor slide up a line's own column past its start; sliding down is allowed
            //the server compares exactly here
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if ((lineOn == a1) && (((lineStart <= b1) && (lineEnd >= b1)) || ((lineOn == a0) && (b0 <= lineStart) && (b1 > lineStart))))
                return false;

            // ReSharper restore CompareOfFloatsByEqualityOperator

            if (maxA < lineOn)
                break;

            var next = b0 + (b1 - b0) * (lineOn - a0) / (a1 - a0 + REPS);

            if (!(((lineStart - EPS) <= next) && (next <= (lineEnd + EPS))))
                continue;

            return false;
        }

        return true;
    }

    private static (int[] On, int[] Start, int[] End) SortLines(IReadOnlyList<StraightLine> lines)
    {
        var count = lines.Count;
        var on = new int[count];
        var start = new int[count];
        var end = new int[count];
        var order = new int[count];

        for (var i = 0; i < count; i++)
            order[i] = i;

        Array.Sort(
            order,
            (a, b) => lines[a]
                      .On
                      .CompareTo(lines[b].On));

        for (var i = 0; i < count; i++)
        {
            var line = lines[order[i]];
            on[i] = line.On;
            start[i] = Math.Min(line.Start, line.End);
            end[i] = Math.Max(line.Start, line.End);
        }

        return (on, start, end);
    }
}