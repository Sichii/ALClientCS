#region
using AL.Core.Geometry;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     The route clock against timelines worked by hand, so the comparisons built on it rest on checked numbers.
/// </summary>
public class RouteClockTests
{
    private static readonly ClockSettings RESTED_UNTRACKED = new(
        45f,
        0f,
        0f,
        null,
        0f,
        0f,
        0f);

    private static PathEdge Leg(EdgeType type, float cost = 0f)
        => new(
            type,
            new Location("main", 0, 0),
            new Location("main", 0, 0),
            cost);

    /// <summary>
    ///     450 units at speed 45 is 10s, priced at the distance walked.
    /// </summary>
    [Test]
    public void ALoneWalkIsItsDistanceOverSpeed()
    {
        (var seconds, var price) = RouteClock.Measure([Leg(EdgeType.Walk, 450f)], RESTED_UNTRACKED);

        seconds.Should()
               .BeApproximately(10, 1e-9);

        price.Should()
             .BeApproximately(450, 1e-9);
    }

    /// <summary>
    ///     A door takes no time; its leg cost of 50 lands on the price beside the 10s walk.
    /// </summary>
    [Test]
    public void ADoorAddsItsCostAndNoTime()
    {
        (var seconds, var price) = RouteClock.Measure(
            [
                Leg(EdgeType.Door, 50f),
                Leg(EdgeType.Walk, 450f)
            ],
            RESTED_UNTRACKED);

        seconds.Should()
               .BeApproximately(10, 1e-9);

        price.Should()
             .BeApproximately(500, 1e-9);
    }

    /// <summary>
    ///     Blink, door, blink, door, blink from rest at speed 45. Cast 1 at t=0, ready in 1200; lands t=200 with ready 1000
    ///     and penalty 812; the door lifts penalty to 4012. Cast 2 waits 1000 to t=1200, penalty 3012, so ready in 4212; lands
    ///     t=1400 with ready 4012 and penalty 2812 + 812 = 3624; the door lifts it to 6824. Cast 3 waits 4012 to t=5412 and
    ///     lands t=5612. Price 5.612 × 45 + 100.
    /// </summary>
    [Test]
    public void AThreeMapChainWaitsOutEachDoorsPenalty()
    {
        PathEdge[] legs =
        [
            Leg(EdgeType.Blink),
            Leg(EdgeType.Door, 50f),
            Leg(EdgeType.Blink),
            Leg(EdgeType.Door, 50f),
            Leg(EdgeType.Blink)
        ];

        (var seconds, var price) = RouteClock.Measure(legs, RESTED_UNTRACKED);

        seconds.Should()
               .BeApproximately(5.612, 1e-9);

        price.Should()
             .BeApproximately(5.612 * 45 + 100, 1e-6);
    }

    /// <summary>
    ///     A tracked bar of 1000 with no regen can never reach the cast's 1600.
    /// </summary>
    [Test]
    public void AShortBarThatNeverRefillsIsInfinite()
    {
        var settings = RESTED_UNTRACKED with
        {
            MpPerSecond = 0f,
            Mp = 1000f,
            MaxMp = 2000f
        };

        (var seconds, var price) = RouteClock.Measure([Leg(EdgeType.Blink)], settings);

        seconds.Should()
               .Be(double.PositiveInfinity);

        price.Should()
             .Be(double.PositiveInfinity);
    }

    /// <summary>
    ///     A tracked bar of 1000 at 250 mp/s waits 2.4s for the cast's 1600, then lands 0.2s later.
    /// </summary>
    [Test]
    public void AShortBarWaitsForRegen()
    {
        var settings = RESTED_UNTRACKED with
        {
            MpPerSecond = 250f,
            Mp = 1000f,
            MaxMp = 2000f
        };

        (var seconds, var price) = RouteClock.Measure([Leg(EdgeType.Blink)], settings);

        seconds.Should()
               .BeApproximately(2.6, 1e-9);

        price.Should()
             .BeApproximately(2.6 * 45, 1e-6);
    }

    /// <summary>
    ///     The 3s recall channel at speed 50 is 150 walk units, carrying a 1.2 risk premium to 180.
    /// </summary>
    [Test]
    public void TownIsItsChannelTimesTheRiskPremium()
    {
        var settings = RESTED_UNTRACKED with
        {
            Speed = 50f
        };

        (var seconds, var price) = RouteClock.Measure([Leg(EdgeType.Town)], settings);

        seconds.Should()
               .BeApproximately(3, 1e-9);

        price.Should()
             .BeApproximately(180, 1e-6);
    }
}