#region
using System.Diagnostics;
using AL.Core.Geometry;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Holds the closed-form segment entry on <see cref="Reach" /> against the bisection it replaced, over seeded random
///     bands, ranges and segments.
/// </summary>
public class ReachEntryTests
{
    private const int CASES = 10_000;
    private const float TOLERANCE = 0.001f;

    [Test]
    public void ClosedFormAgreesWithBisection()
    {
        var cases = BuildCases(new Random(20261109));
        var compared = 0;
        var outsideByRounding = 0;

        foreach ((var reach, var x0, var y0, var x1, var y1) in cases)
        {
            var bisected = TryBisectEntry(
                reach,
                x0,
                y0,
                x1,
                y1,
                out var bx,
                out var by);

            var closed = reach.TryFindEntry(
                x0,
                y0,
                x1,
                y1,
                out var cx,
                out var cy);

            closed.Should()
                  .Be(
                      bisected,
                      "{0} from ({1}, {2}) to ({3}, {4})",
                      Describe(reach),
                      x0,
                      y0,
                      x1,
                      y1);

            if (!bisected)
                continue;

            compared++;

            MathF.Sqrt((bx - cx) * (bx - cx) + (by - cy) * (by - cy))
                 .Should()
                 .BeLessThan(
                     TOLERANCE,
                     "{0} from ({1}, {2}) to ({3}, {4})",
                     Describe(reach),
                     x0,
                     y0,
                     x1,
                     y1);

            reach.Distance(cx, cy)
                 .Should()
                 .BeLessThanOrEqualTo(reach.Range + TOLERANCE);

            if (!reach.Contains(cx, cy))
            {
                outsideByRounding++;

                Console.WriteLine(
                    $"OUTSIDE {Describe(reach)} ({x0}, {y0})->({x1}, {y1}) entry ({cx}, {cy}) d={reach.Distance(cx, cy) - reach.Range}");
            }
        }

        Console.WriteLine($"REACH ENTRY cases={cases.Count} compared={compared} outsideByRounding={outsideByRounding}");

        compared.Should()
                .BeGreaterThan(CASES / 4);

        outsideByRounding.Should()
                         .Be(0);
    }

    /// <summary>
    ///     A segment whose both ends are outside can still pass through the reach. The reference is the first of 2,000 even
    ///     samples inside, bisected back to the edge.
    /// </summary>
    [Test]
    public void FirstEntryFindsChordsThroughTheReach()
    {
        const int SAMPLES = 2_000;

        var cases = BuildCases(new Random(911));
        var chords = 0;

        foreach ((var reach, var x0, var y0, var x1, var y1) in cases)
        {
            if (reach.Contains(x0, y0) || reach.Contains(x1, y1))
                continue;

            var hit = reach.TryFindFirstEntry(
                x0,
                y0,
                x1,
                y1,
                out var cx,
                out var cy);
            var firstInside = -1;

            for (var i = 1; i < SAMPLES; i++)
            {
                var t = i / (float)SAMPLES;

                if (!reach.Contains(x0 + (x1 - x0) * t, y0 + (y1 - y0) * t))
                    continue;

                firstInside = i;

                break;
            }

            if (firstInside < 0)
            {
                //a miss, or a graze thinner than the sampling, which still has to sit on the edge
                if (hit)
                    reach.Distance(cx, cy)
                         .Should()
                         .BeLessThanOrEqualTo(reach.Range + TOLERANCE);

                continue;
            }

            chords++;

            hit.Should()
               .BeTrue();

            var sx = x0 + (x1 - x0) * ((firstInside - 1) / (float)SAMPLES);
            var sy = y0 + (y1 - y0) * ((firstInside - 1) / (float)SAMPLES);
            var ex = x0 + (x1 - x0) * (firstInside / (float)SAMPLES);
            var ey = y0 + (y1 - y0) * (firstInside / (float)SAMPLES);

            TryBisectEntry(
                reach,
                sx,
                sy,
                ex,
                ey,
                out var bx,
                out var by);

            MathF.Sqrt((bx - cx) * (bx - cx) + (by - cy) * (by - cy))
                 .Should()
                 .BeLessThan(
                     TOLERANCE,
                     "{0} from ({1}, {2}) to ({3}, {4})",
                     Describe(reach),
                     x0,
                     y0,
                     x1,
                     y1);
        }

        Console.WriteLine($"REACH CHORDS cases={cases.Count} chords={chords}");

        chords.Should()
              .BeGreaterThan(100);
    }

    /// <summary>
    ///     Segments built to touch a circle only at a tangent, or to pass through a zero-range point, where the exact entry
    ///     can round outside and no nudge lands inside. An entry is answered only where it tests inside.
    /// </summary>
    [Test]
    public void AGrazeIsAnEntryOnlyWhereItTestsInside()
    {
        var rng = new Random(77);
        var answered = 0;

        for (var i = 0; i < CASES; i++)
        {
            var cx = Draw(rng, -500, 500);
            var cy = Draw(rng, -500, 500);
            var range = (i % 2) == 0 ? 0f : Draw(rng, 1, 120);
            var angle = Draw(rng, 0, MathF.Tau);
            var length = Draw(rng, 1, 400);
            var tx = cx + range * MathF.Cos(angle);
            var ty = cy + range * MathF.Sin(angle);
            var px = -MathF.Sin(angle) * length;
            var py = MathF.Cos(angle) * length;
            var reach = Reach.CreateCircle(cx, cy, range);

            if (reach.TryFindFirstEntry(
                    tx - px,
                    ty - py,
                    tx + px,
                    ty + py,
                    out var ex,
                    out var ey))
            {
                answered++;

                reach.Contains(ex, ey)
                     .Should()
                     .BeTrue($"{Describe(reach)} entry ({ex}, {ey})");
            } else
            {
                ex.Should()
                  .Be(tx - px);

                ey.Should()
                  .Be(ty - py);
            }
        }

        Console.WriteLine($"REACH GRAZES cases={CASES} answered={answered}");
    }

    [Test]
    [Explicit]
    public void TimeClosedFormAgainstBisection()
    {
        const int ROUNDS = 20;

        var cases = BuildCases(new Random(4242));
        var sink = 0f;

        //warm up both
        foreach ((var reach, var x0, var y0, var x1, var y1) in cases)
        {
            TryBisectEntry(
                reach,
                x0,
                y0,
                x1,
                y1,
                out var bx,
                out _);

            reach.TryFindEntry(
                x0,
                y0,
                x1,
                y1,
                out var cx,
                out _);
            sink += bx + cx;
        }

        var bisection = Stopwatch.StartNew();

        for (var round = 0; round < ROUNDS; round++)
            foreach ((var reach, var x0, var y0, var x1, var y1) in cases)
            {
                TryBisectEntry(
                    reach,
                    x0,
                    y0,
                    x1,
                    y1,
                    out var bx,
                    out _);
                sink += bx;
            }

        bisection.Stop();

        var closed = Stopwatch.StartNew();

        for (var round = 0; round < ROUNDS; round++)
            foreach ((var reach, var x0, var y0, var x1, var y1) in cases)
            {
                reach.TryFindEntry(
                    x0,
                    y0,
                    x1,
                    y1,
                    out var cx,
                    out _);
                sink += cx;
            }

        closed.Stop();

        var calls = (double)ROUNDS * cases.Count;

        Console.WriteLine(
            $"REACH ENTRY TIMING calls={calls} bisection={bisection.Elapsed.TotalMilliseconds * 1e6 / calls:F0}ns closed={closed.Elapsed.TotalMilliseconds * 1e6 / calls:F0}ns");

        sink.Should()
            .NotBe(float.NaN);
    }

    /// <summary>
    ///     Builds random reaches and segments: some bands a point or a line, some ranges zero, some starts and ends drawn
    ///     inside.
    /// </summary>
    /// <param name="rng">The seeded source of the draws.</param>
    /// <returns>The cases.</returns>
    private static List<(Reach Reach, float X0, float Y0, float X1, float Y1)> BuildCases(Random rng)
    {
        var cases = new List<(Reach, float, float, float, float)>(CASES);

        for (var i = 0; i < CASES; i++)
        {
            var left = Draw(rng, -500, 500);
            var top = Draw(rng, -500, 500);
            var shape = rng.NextDouble();
            var width = shape < 0.25 ? 0f : Draw(rng, 0, 200);
            var height = (shape < 0.25) || (shape > 0.85) ? 0f : Draw(rng, 0, 200);
            var range = rng.NextDouble() < 0.15 ? 0f : Draw(rng, 0, 120);
            var reach = new Reach(new Rectangle(new Point(left, top), new Point(left + width, top + height)), range);

            (var x0, var y0) = DrawPoint(rng, reach, rng.NextDouble() < 0.2);
            (var x1, var y1) = DrawPoint(rng, reach, rng.NextDouble() < 0.4);

            cases.Add((reach, x0, y0, x1, y1));
        }

        return cases;
    }

    private static string Describe(Reach reach)
        => $"band ({reach.Band.Left}, {reach.Band.Top})-({reach.Band.Right}, {reach.Band.Bottom}) range {reach.Range}";

    private static float Draw(Random rng, float low, float high) => low + (float)rng.NextDouble() * (high - low);

    private static (float X, float Y) DrawPoint(Random rng, Reach reach, bool inside)
    {
        if (inside)
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var x = Draw(rng, reach.Band.Left - reach.Range, reach.Band.Right + reach.Range);
                var y = Draw(rng, reach.Band.Top - reach.Range, reach.Band.Bottom + reach.Range);

                if (reach.Contains(x, y))
                    return (x, y);
            }

        return (Draw(rng, reach.Band.Left - 400, reach.Band.Right + 400), Draw(rng, reach.Band.Top - 400, reach.Band.Bottom + 400));
    }

    /// <summary>
    ///     The bisection <see cref="Reach.TryFindEntry" /> used before the closed form: 24 halvings between an outside start
    ///     and an inside end.
    /// </summary>
    /// <param name="reach">The reach.</param>
    /// <param name="x0">The segment start's x.</param>
    /// <param name="y0">The segment start's y.</param>
    /// <param name="x1">The segment end's x.</param>
    /// <param name="y1">The segment end's y.</param>
    /// <param name="entryX">The entry point's x, or the start's.</param>
    /// <param name="entryY">The entry point's y, or the start's.</param>
    /// <returns>
    ///     <c>true</c> if the start or the end is inside the reach; otherwise, <c>false</c> .
    /// </returns>
    private static bool TryBisectEntry(
        Reach reach,
        float x0,
        float y0,
        float x1,
        float y1,
        out float entryX,
        out float entryY)
    {
        entryX = x0;
        entryY = y0;

        if (reach.Contains(x0, y0))
            return true;

        if (!reach.Contains(x1, y1))
            return false;

        var outside = 0f;
        var inside = 1f;

        for (var i = 0; i < 24; i++)
        {
            var mid = (outside + inside) / 2f;

            if (reach.Contains(x0 + (x1 - x0) * mid, y0 + (y1 - y0) * mid))
                inside = mid;
            else
                outside = mid;
        }

        entryX = x0 + (x1 - x0) * inside;
        entryY = y0 + (y1 - y0) * inside;

        return true;
    }
}