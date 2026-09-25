#region
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     The search's clock against the server's numbers written out here: blink's 1200ms cooldown and 1600 mana, a 200ms
///     landing, 812ms of penalty after it, 3200ms for a door, and at most 10000ms of the penalty charged to a cast.
/// </summary>
public class BlinkClockTests : GameDataTestBed
{
    private static readonly PathOptions UNTRACKED = new()
    {
        BlinkCost = 400f
    };

    /// <summary>
    ///     A rested cast takes the 200ms landing, and leaves the next cast 1000ms off and 812ms of penalty pending.
    /// </summary>
    [Test]
    public void ARestedCastLeavesTheCooldownLessTheLandingAndTheEffectPenalty()
    {
        var clock = new BlinkClock(UNTRACKED);

        clock.TryBlink(default, out var spentMs, out var after)
             .Should()
             .BeTrue();

        spentMs.Should()
               .Be(200f);

        after.Should()
             .Be(new TravelState(1000f, 812f, 0f));
    }

    /// <summary>
    ///     3000ms of pending penalty is added to the cooldown: 3000 + 1200 - 200 = 4000ms to the next cast.
    /// </summary>
    [Test]
    public void APendingPenaltyIsChargedToTheNextCast()
    {
        var clock = new BlinkClock(UNTRACKED);

        clock.TryBlink(new TravelState(0f, 3000f, 0f), out _, out var after)
             .Should()
             .BeTrue();

        after.BlinkReadyInMs
             .Should()
             .Be(4000f);

        after.PenaltyMs
             .Should()
             .Be(3000f - 200f + 812f);
    }

    /// <summary>
    ///     A tracked bar of 1000 at 250 a second waits (1600 - 1000) / 250 = 2.4s for the cast, then lands 200ms later.
    /// </summary>
    [Test]
    public void AShortTrackedBarWaitsForTheRefill()
    {
        var clock = new BlinkClock(
            new PathOptions
            {
                BlinkCost = 400f,
                BlinkMpPerSecond = 250f,
                Mp = 1000f,
                MaxMp = 4000f
            });

        clock.TryBlink(new TravelState(0f, 0f, 1000f), out var spentMs, out var after)
             .Should()
             .BeTrue();

        spentMs.Should()
               .BeApproximately((1600f - 1000f) / 250f * 1000f + 200f, 0.01f);

        after.Mp
             .Should()
             .BeApproximately(0f + 250f * 0.2f, 0.01f);
    }

    /// <summary>
    ///     A tracked bar short of a cast that never refills can never cast.
    /// </summary>
    [Test]
    public void ARateOfZeroNeverCasts()
    {
        var clock = new BlinkClock(
            new PathOptions
            {
                BlinkCost = 400f,
                BlinkMpPerSecond = 0f,
                Mp = 0f,
                MaxMp = 4000f
            });

        clock.TryBlink(new TravelState(0f, 0f, 0f), out _, out _)
             .Should()
             .BeFalse();
    }

    /// <summary>
    ///     Blink, door, blink, door, blink: 200ms, then a 1000ms wait and 200ms, then a 4012ms wait and 200ms - 5612ms, the
    ///     same timeline <see cref="RouteClock" /> is held to.
    /// </summary>
    [Test]
    public void ADoorBlinkChainWaitsBeforeEachLaterCast()
    {
        var clock = new BlinkClock(UNTRACKED);
        var state = default(TravelState);
        var totalMs = 0f;

        for (var cast = 0; cast < 3; cast++)
        {
            if (cast > 0)
                state = BlinkClock.AddPenalty(state, CONSTANTS.DOOR_PENALTY_MS);

            clock.TryBlink(state, out var spentMs, out state)
                 .Should()
                 .BeTrue();

            totalMs += spentMs;
        }

        totalMs.Should()
               .BeApproximately(5612f, 0.01f);
    }

    /// <summary>
    ///     Ready now but 3000ms of penalty still pending loses to a cast 2000ms off with 3500ms pending, since 1500ms is all
    ///     that is left when that one casts: its next cast is ready 2500ms after landing against 4000ms. With 1000ms pending,
    ///     the ready one wins.
    /// </summary>
    [Test]
    public void AWaitThatRunsThePenaltyDownCanLeaveTheNextCastReadySooner()
    {
        var clock = new BlinkClock(UNTRACKED);
        var readyNow = new TravelState(0f, 3000f, 0f);
        var waiting = new TravelState(2000f, 3500f, 0f);

        clock.TryBlink(readyNow, out _, out var afterReadyNow);
        clock.TryBlink(waiting, out _, out var afterWaiting);

        afterReadyNow.BlinkReadyInMs
                     .Should()
                     .Be(4000f);

        afterWaiting.BlinkReadyInMs
                    .Should()
                    .Be(2500f);

        clock.AtLeastAsWellPlaced(readyNow, waiting)
             .Should()
             .BeFalse();

        clock.AtLeastAsWellPlaced(
                 readyNow with
                 {
                     PenaltyMs = 1000f
                 },
                 waiting)
             .Should()
             .BeTrue();
    }

    /// <summary>
    ///     One state is at least as ready as another only when neither timer is later and the bar is no lower.
    /// </summary>
    [Test]
    public void AtLeastAsReadyNeedsEveryCount()
    {
        var baseline = new TravelState(100f, 100f, 100f);

        BlinkClock.AtLeastAsReady(baseline, baseline)
                  .Should()
                  .BeTrue();

        BlinkClock.AtLeastAsReady(
                      baseline with
                      {
                          BlinkReadyInMs = 101f
                      },
                      baseline)
                  .Should()
                  .BeFalse();

        BlinkClock.AtLeastAsReady(
                      baseline with
                      {
                          PenaltyMs = 101f
                      },
                      baseline)
                  .Should()
                  .BeFalse();

        BlinkClock.AtLeastAsReady(
                      baseline with
                      {
                          Mp = 99f
                      },
                      baseline)
                  .Should()
                  .BeFalse();
    }
}