#region
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Holds the hop penalty replay to the server's in-motion rule: each tick the score decays by 0.99 and gains the
///     lattice value under the character less one, and passing 60 costs half the character's hp.
/// </summary>
public class HopRiskTests
{
    [Test]
    [Arguments(0, 0f)]
    [Arguments(1, 0f)]
    [Arguments(2, 1f)]
    [Arguments(8, 7f)]
    public void ThePenaltyGrowsOnePerLatticeStepPastTheFirst(int level, float penalty)
        => HopRisk.CalculatePenalty(level)
                  .Should()
                  .Be(penalty);

    [Test]
    public void ASlowCrossingOfDeepWaterIsUnsafe()
    {
        //40 units at 3.75 per tick is 11 ticks of 7: 7 * (1 - 0.99^11) / 0.01
        var penalties = Enumerable.Repeat(7f, 41)
                                  .ToArray();

        HopRisk.CalculatePeakScore(penalties, 40f, 50f)
               .Should()
               .BeApproximately(73.3f, 0.1f);

        HopRisk.IsSafe(penalties, 40f, 50f)
               .Should()
               .BeFalse();
    }

    [Test]
    public void AFastCrossingOfDeepWaterIsSafe()
    {
        //40 units at 15 per tick is 3 ticks of 7: 7 + 6.93 + 6.8607
        var penalties = Enumerable.Repeat(7f, 41)
                                  .ToArray();

        HopRisk.CalculatePeakScore(penalties, 40f, 200f)
               .Should()
               .BeApproximately(20.79f, 0.01f);

        HopRisk.IsSafe(penalties, 40f, 200f)
               .Should()
               .BeTrue();
    }

    [Test]
    public void ACrossingThatNeverLeavesTheGroundCostsNothing()
        => HopRisk.CalculatePeakScore(new float[41], 40f, 10f)
                  .Should()
                  .Be(0f);

    [Test]
    public void NoSpeedIsNeverSafe()
        => HopRisk.IsSafe(new float[41], 40f, 0f)
                  .Should()
                  .BeFalse();

    [Test]
    [Arguments(0f)]
    [Arguments(-50f)]
    [Arguments(float.NaN)]
    [Arguments(1e-30f)]
    public void AReplayWithoutRealSpeedEnds(float speed)
    {
        var penalties = Enumerable.Repeat(7f, 41)
                                  .ToArray();

        HopRisk.Replay(
                   penalties,
                   40f,
                   speed,
                   0f)
               .Peak
               .Should()
               .Be(float.PositiveInfinity);

        HopRisk.CalculatePeakScore(penalties, 40f, speed)
               .Should()
               .Be(float.PositiveInfinity);
    }

    [Test]
    public void AReplayWhoseStepUnderflowsEnds()
        => HopRisk.Replay(
                      new float[1],
                      0f,
                      float.Epsilon,
                      0f)
                  .Peak
                  .Should()
                  .Be(float.PositiveInfinity);

    [Test]
    public void TheLimitLeavesFiveUnderTheServers()
        => HopRisk.SAFE_SCORE
                  .Should()
                  .Be(55f);

    [Test]
    public void AReplayFromNothingPeaksWhereThePeakScoreDoes()
    {
        var penalties = Enumerable.Repeat(7f, 41)
                                  .ToArray();

        HopRisk.Replay(
                   penalties,
                   40f,
                   50f,
                   0f)
               .Peak
               .Should()
               .Be(HopRisk.CalculatePeakScore(penalties, 40f, 50f));
    }

    [Test]
    public void AReplayOverGroundOnlyDecaysTheCarriedScore()
    {
        //40 units at 3.75 per tick is 11 ticks; the first decays before anything is added
        (var peak, var end) = HopRisk.Replay(
            new float[41],
            40f,
            50f,
            20f);

        peak.Should()
            .BeApproximately(19.8f, 0.001f);

        end.Should()
           .BeApproximately(20f * MathF.Pow(0.99f, 11), 0.001f);
    }

    [Test]
    public void WalkingDecaysTheScoreOncePer83Ms()
        => HopRisk.DecayWhileWalking(30f, 2f)
                  .Should()
                  .BeApproximately(30f * MathF.Pow(0.99f, 2f / 0.083f), 0.001f);

    [Test]
    public void WalkingRoundsAScoreUnderOneToZero()
        => HopRisk.DecayWhileWalking(1.05f, 1f)
                  .Should()
                  .Be(0f);
}