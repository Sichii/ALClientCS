#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     The goal region restated by hand: a band from (0, 0) to (100, 50) with a range of 10 is that rectangle inflated by
///     10 with rounded corners, and a circle is a zero-size band with the radius as range.
/// </summary>
public class ReachTests
{
    [Test]
    public void ACircleIsAZeroSizeBand()
    {
        var circle = Reach.Circle(0, 0, 40);

        circle.Contains(30, 0)
              .Should()
              .BeTrue();

        circle.Contains(41, 0)
              .Should()
              .BeFalse();

        circle.Contains(28, 28)
              .Should()
              .BeTrue();

        //euclidean, not per-axis: max(30, 30) is 30 and would wrongly stay inside, but the real distance is 42.4
        circle.Contains(30, 30)
              .Should()
              .BeFalse();
    }

    private static Reach Band() => new(new Rectangle(new Point(0, 0), new Point(100, 50)), 10f);

    [Test]
    [Arguments(50f, 25f, true)]
    [Arguments(105f, 25f, true)]
    [Arguments(111f, 25f, false)]
    [Arguments(107f, 57f, true)]
    [Arguments(108f, 58f, false)]
    [Arguments(-10f, 0f, true)]
    public void ContainsIsTheInflatedRoundedRectangle(float x, float y, bool inside)
        => Band()
           .Contains(x, y)
           .Should()
           .Be(inside);

    [Test]
    public void NearEdgeOfAPointAlreadyInsideIsThePointItself()
    {
        var band = Band();

        (var x, var y) = band.NearEdge(50, 25);

        x.Should()
         .Be(50f);

        y.Should()
         .Be(25f);

        //an interior point is not on the boundary, so only Contains holds here, not the Range distance the stepped-back cases satisfy
        band.Contains(x, y)
            .Should()
            .BeTrue();
    }

    [Test]
    public void NearEdgeStepsBackFromTheBandByTheRange()
    {
        var band = Band();

        (var x, var y) = band.NearEdge(150, 25);

        x.Should()
         .BeApproximately(110f, 0.001f);

        y.Should()
         .BeApproximately(25f, 0.001f);

        band.Contains(x, y)
            .Should()
            .BeTrue();

        band.Band
            .EdgeToCenterDistance(new Point(x, y))
            .Should()
            .BeApproximately(band.Range, 0.001f);

        //past a corner the step is along the diagonal from the corner
        (x, y) = band.NearEdge(150, 100);

        x.Should()
         .BeApproximately(107.071f, 0.01f);

        y.Should()
         .BeApproximately(57.071f, 0.01f);

        band.Contains(x, y)
            .Should()
            .BeTrue();

        band.Band
            .EdgeToCenterDistance(new Point(x, y))
            .Should()
            .BeApproximately(band.Range, 0.001f);
    }

    [Test]
    public void TheQueriesDoNotAllocate()
    {
        var band = Band();
        var circle = Reach.Circle(0, 0, 40);

        //warm up
        var sink = band.Contains(50, 25) ? 1f : 0f;
        sink += band.Distance(150, 25);
        (var bx, var by) = band.NearEdge(150, 25);
        sink += bx + by;

        band.TryEntry(
            150,
            25,
            50,
            25,
            out bx,
            out by);
        sink += bx + by;

        sink += circle.Contains(30, 0) ? 1f : 0f;
        sink += circle.Distance(41, 0);
        (var cx, var cy) = circle.NearEdge(50, 0);
        sink += cx + cy;

        circle.TryEntry(
            50,
            0,
            0,
            0,
            out cx,
            out cy);
        sink += cx + cy;

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 10_000; i++)
        {
            var y = 25 + i % 30;

            sink += band.Contains(150, y) ? 1f : 0f;
            sink += band.Distance(150, y);

            (bx, by) = band.NearEdge(150, y);
            sink += bx + by;

            band.TryEntry(
                150,
                y,
                50,
                y,
                out bx,
                out by);
            sink += bx + by;

            sink += circle.Contains(30, i % 40) ? 1f : 0f;
            sink += circle.Distance(41, i % 40);

            (cx, cy) = circle.NearEdge(50, i % 40);
            sink += cx + cy;

            circle.TryEntry(
                50,
                i % 40,
                0,
                0,
                out cx,
                out cy);
            sink += cx + cy;
        }

        (GC.GetAllocatedBytesForCurrentThread() - before).Should()
                                                         .Be(0);

        //keep the accumulator live so the loop body is not elided
        sink.Should()
            .NotBe(0f);
    }

    [Test]
    public void TryEntryFindsTheFirstPointInside()
    {
        Band()
            .TryEntry(
                150,
                25,
                50,
                25,
                out var x,
                out var y)
            .Should()
            .BeTrue();

        x.Should()
         .BeApproximately(110f, 0.05f);

        y.Should()
         .BeApproximately(25f, 0.05f);
    }

    [Test]
    public void TryEntryFromInsideIsTheStart()
    {
        Band()
            .TryEntry(
                50,
                25,
                150,
                25,
                out var x,
                out var y)
            .Should()
            .BeTrue();

        x.Should()
         .Be(50f);

        y.Should()
         .Be(25f);
    }

    [Test]
    public void TryEntryIsFalseWhenTheEndIsOutside()
    {
        Band()
            .TryEntry(
                150,
                25,
                200,
                25,
                out _,
                out _)
            .Should()
            .BeFalse();

        //the method only answers segments that end inside; a chord dipping through the band with both ends outside still answers false
        Band()
            .TryEntry(
                -50,
                25,
                150,
                25,
                out _,
                out _)
            .Should()
            .BeFalse();
    }
}