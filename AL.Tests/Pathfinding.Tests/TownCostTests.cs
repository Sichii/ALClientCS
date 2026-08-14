#region
using AL.Pathfinding.Definitions;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     What a town channel is priced at, against the arithmetic written out here rather than read from the constants
///     the code uses.
/// </summary>
/// <remarks>
///     The rule being restated: the channel is a fixed three seconds and every other edge is a distance, so what the
///     channel is worth is the ground this character would have covered on foot in those three seconds - times a
///     premium, because a walk of the same length has made progress by the time it ends and an interrupted channel has
///     not. Needs no mesh, which is the whole reason it is a test.
/// </remarks>
public class TownCostTests
{
    [Test]
    public void ThreeSecondsOfWalkingIsWhatTheChannelCosts()
        => CONSTANTS.TownCost(50f)
                    .Should()
                    .Be(3f * 50f * CONSTANTS.TOWN_RISK_PREMIUM);

    /// <summary>Twice the speed covers twice the ground, so the channel it replaces is worth twice as much.</summary>
    [Test]
    public void TwiceTheSpeedDoublesTheCost()
        => CONSTANTS.TownCost(100f)
                    .Should()
                    .Be(2f * CONSTANTS.TownCost(50f));

    /// <summary>A slow character reaches for the channel more readily, because walking buys it less.</summary>
    [Test]
    public void ASlowerCharacterPricesTheChannelLower()
        => CONSTANTS.TownCost(30f)
                    .Should()
                    .BeLessThan(CONSTANTS.TownCost(50f));

    [Test]
    public void TheNominalCostIsTheCostAtTheNominalSpeed()
        => CONSTANTS.NOMINAL_TOWN_COST
                    .Should()
                    .Be(CONSTANTS.TownCost(CONSTANTS.NOMINAL_WALK_SPEED));

    /// <summary>
    ///     A frame that has not filled in yet reports no speed, and read literally that would make the channel free
    ///     and win every search it appeared in.
    /// </summary>
    [Test]
    public void AnUnreadableSpeedIsPricedAtTheNominalOne()
    {
        CONSTANTS.TownCost(0f)
                 .Should()
                 .Be(CONSTANTS.NOMINAL_TOWN_COST);

        CONSTANTS.TownCost(-5f)
                 .Should()
                 .Be(CONSTANTS.NOMINAL_TOWN_COST);
    }
}
