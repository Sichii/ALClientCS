#region
using AL.Client.Helpers;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     What a range check spends on safety, which is no longer a flat 5%. The rule is stated here in its own terms rather
///     than read back off <see cref="RangeSafety" />, because the failure it guards is a classifier that collapses to one
///     branch: sign the dot product the wrong way and every pair reads as closing, which looks exactly like the flat shave
///     it replaced and costs the whole feature silently.
/// </summary>
public class RangeSafetyTests
{
    /// <summary>
    ///     A leg whose destination is where the entity already stands. The delta loop leaves that state on an entity rather
    ///     than clearing it, so it is reachable - and it must not read as standing still, because the flag the rest of the
    ///     client acts on still says moving.
    /// </summary>
    [Test]
    public void AMovingFlagWithNowhereToGoIsNotStandingStill()
    {
        var source = Walking(
            0f,
            0f,
            0f,
            0f);
        var target = Standing(100f, 0f);

        source.MovingAwayFrom(target)
              .Should()
              .BeFalse();

        RangeSafety.MarginFor(source, target)
                   .Should()
                   .Be(0.975f);
    }

    /// <summary>
    ///     Sideways is neither, and it has to land in the middle rather than in either extreme: the gap is not opening, but
    ///     both ends are still moving under a cast in flight.
    /// </summary>
    [Test]
    public void BothWalkingAcrossTheLineBetweenThemIsTheMiddleCase()
    {
        var source = Walking(
            0f,
            0f,
            0f,
            80f);

        var target = Walking(
            100f,
            0f,
            100f,
            -80f);

        RangeSafety.MarginFor(source, target)
                   .Should()
                   .Be(0.975f);
    }

    [Test]
    public void BothWalkingApartSpendsTheWholeShave()
    {
        //one heading west from the origin, the other heading east from 100: the gap opens at both ends at once,
        //which is the fastest it can open and the only case worth the full 5%
        var source = Walking(
            0f,
            0f,
            -50f,
            0f);

        var target = Walking(
            100f,
            0f,
            150f,
            0f);

        RangeSafety.MarginFor(source, target)
                   .Should()
                   .Be(0.95f);
    }

    [Test]
    public void BothWalkingTowardsEachOtherIsTheMiddleCase()
    {
        var source = Walking(
            0f,
            0f,
            40f,
            0f);

        var target = Walking(
            100f,
            0f,
            60f,
            0f);

        RangeSafety.MarginFor(source, target)
                   .Should()
                   .Be(0.975f);
    }

    [Test]
    public void NeitherEndMovingSpendsNothing()
    {
        var source = Standing(0f, 0f);
        var target = Standing(100f, 0f);

        RangeSafety.MarginFor(source, target)
                   .Should()
                   .Be(1f);
    }

    [Test]
    public void OneWalkingAwayAndOneStandingIsTheMiddleCase()
    {
        var source = Walking(
            0f,
            0f,
            -50f,
            0f);
        var target = Standing(100f, 0f);

        RangeSafety.MarginFor(source, target)
                   .Should()
                   .Be(0.975f);
    }

    private static MovementBlock Standing(float x, float y)
        => new(
            x,
            y,
            x,
            y,
            0f,
            0,
            false,
            "main",
            "main");

    [Test]
    public void StandingStillIsNeverWalkingAway()
        => Standing(0f, 0f)
           .MovingAwayFrom(Standing(100f, 0f))
           .Should()
           .BeFalse();

    private static MovementBlock Walking(
        float x,
        float y,
        float goingX,
        float goingY)
        => new(
            x,
            y,
            goingX,
            goingY,
            0f,
            0,
            true,
            "main",
            "main");

    [Test]
    public void WalkingDirectlyBackwardsIsWalkingAway()
        => Walking(
               0f,
               0f,
               -10f,
               0f)
           .MovingAwayFrom(Standing(100f, 0f))
           .Should()
           .BeTrue();

    /// <summary>
    ///     Away is measured against the other's position, not against the walker's own heading in isolation - a step that
    ///     carries the entity past the target is closing until it goes by, and reading only the heading would call the whole
    ///     leg a retreat.
    /// </summary>
    [Test]
    public void WalkingTowardsSomethingIsNotWalkingAwayFromIt()
        => Walking(
               0f,
               0f,
               90f,
               0f)
           .MovingAwayFrom(Standing(100f, 0f))
           .Should()
           .BeFalse();
}