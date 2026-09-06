#region
using System.Runtime.CompilerServices;
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     A map's wall lines in the shape the server keeps them, with the server's own segment test over them. This is what
///     the official client clips a move against, so it is what a planned leg has to pass.
/// </summary>
public sealed class WallLines
{
    //the server's own epsilons. EPS widens the range check, REPS keeps the divisor off zero for a vertical track
    private const double EPS = 1e-8;
    private const double REPS = 2.220446049250313e-16;
    private readonly int[] HorizontalEnd;
    private readonly int[] HorizontalOn;
    private readonly int[] HorizontalStart;
    private readonly int[] VerticalEnd;

    //sorted by On. Start <= End after LineHelper.FixLines, which every geometry goes through before this is built
    private readonly int[] VerticalOn;
    private readonly int[] VerticalStart;

    /// <summary>
    ///     How many horizontal lines this map has.
    /// </summary>
    public int HorizontalCount => HorizontalOn.Length;

    /// <summary>
    ///     How many vertical lines this map has.
    /// </summary>
    public int VerticalCount => VerticalOn.Length;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WallLines" /> class.
    /// </summary>
    /// <param name="verticalLines">
    ///     The map's x_lines.
    /// </param>
    /// <param name="horizontalLines">
    ///     The map's y_lines.
    /// </param>
    public WallLines(IReadOnlyList<StraightLine> verticalLines, IReadOnlyList<StraightLine> horizontalLines)
    {
        (VerticalOn, VerticalStart, VerticalEnd) = Sorted(verticalLines);
        (HorizontalOn, HorizontalStart, HorizontalEnd) = Sorted(horizontalLines);
    }

    /// <summary>
    ///     Whether any line passes through the collision box hanging on (x, y). A point where this is true is one the client
    ///     would refuse every move out of, which is what a wall means at runtime.
    /// </summary>
    public bool BoxIntersects(double x, double y, BoundingBase boundingBase)
    {
        double h = boundingBase.HalfWidth;
        double v = boundingBase.VerticalNorth;
        double vn = boundingBase.VerticalNotNorth;

        return SpanHits(
                   VerticalOn,
                   VerticalStart,
                   VerticalEnd,
                   x - h,
                   x + h,
                   y - v,
                   y + vn)
               || SpanHits(
                   HorizontalOn,
                   HorizontalStart,
                   HorizontalEnd,
                   y - v,
                   y + vn,
                   x - h,
                   x + h);
    }

    /// <summary>
    ///     Whether a character with the given collision base can move from (x0, y0) to (x1, y1): the four corners of the base,
    ///     plus two fence tracks along the box's leading edges at the destination.
    /// </summary>
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
    ///     Whether a single track from (x0, y0) to (x1, y1) crosses no line. The server's single-track test, clause for
    ///     clause.
    /// </summary>
    public bool CanMoveLine(
        double x0,
        double y0,
        double x1,
        double y1)
        => TrackClear(
               VerticalOn,
               VerticalStart,
               VerticalEnd,
               x0,
               y0,
               x1,
               y1)
           && TrackClear(
               HorizontalOn,
               HorizontalStart,
               HorizontalEnd,
               y0,
               x0,
               y1,
               x1);

    //first index whose On is >= value; the server's own bsearch start
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LowerBound(int[] on, double value)
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

    private static (int[] On, int[] Start, int[] End) Sorted(IReadOnlyList<StraightLine> lines)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SpanHits(
        int[] on,
        int[] start,
        int[] end,
        double minA,
        double maxA,
        double minB,
        double maxB)
    {
        for (var i = LowerBound(on, minA); i < on.Length; i++)
        {
            double lineOn = on[i];

            if (maxA < lineOn)
                break;

            if ((start[i] <= maxB) && (end[i] >= minB))
                return true;
        }

        return false;
    }

    //the server's loop with the axes named generically: 'a' is the coordinate the lines sit on, 'b' the one they
    //span. For vertical lines a is x and b is y; for horizontal lines the caller swaps them
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TrackClear(
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

        //the server starts from a conservative bsearch and skips lines below minA with a continue; starting at
        //the exact lower bound is the same loop, since no skipped line can equal a1 either
        for (var i = LowerBound(on, minA); i < on.Length; i++)
        {
            double lineOn = on[i];
            double lineStart = start[i];
            double lineEnd = end[i];

            //the server's first check, before the range test: a track may not end on a line, and may not slide up
            //a line's own column from below its start to past it. A track sliding down the column is allowed,
            //which is the server's quirk and is kept
            if ((lineOn == a1) && (((lineStart <= b1) && (lineEnd >= b1)) || ((lineOn == a0) && (b0 <= lineStart) && (b1 > lineStart))))
                return false;

            if (maxA < lineOn)
                break;

            var next = b0 + (b1 - b0) * (lineOn - a0) / (a1 - a0 + REPS);

            if (!(((lineStart - EPS) <= next) && (next <= (lineEnd + EPS))))
                continue;

            return false;
        }

        return true;
    }
}