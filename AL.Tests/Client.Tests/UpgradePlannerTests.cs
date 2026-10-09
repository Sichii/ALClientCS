#region
using System.Reflection;
using AL.Client.Abstractions;
using AL.Client.Helpers;
using AL.Client.Model;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     Checks the upgrade and compound planners and the offering pity against hand computations and a simulation.
/// </summary>
/// <remarks>
///     The planner cases price off whichever chance tables are loaded, the committed snapshot or live data, so every
///     assertion is a comparison or a band that holds on both.
/// </remarks>
[NotInParallel(ParallelKeys.GAME_DATA)]
public class UpgradePlannerTests
{
    private static readonly IReadOnlyList<int> WSHIELD_THRESHOLDS =
    [
        7,
        9,
        10,
        12
    ];

    /// <summary>
    ///     The wingedboots grades: grade 0 to +4, so the early levels are where an over-grade scroll is affordable.
    /// </summary>
    private static readonly IReadOnlyList<int> BOOTS_THRESHOLDS =
    [
        4,
        8,
        10,
        12
    ];

    private static readonly IReadOnlyList<int> WBOOK_THRESHOLDS =
    [
        4,
        5,
        6,
        7
    ];

    private static readonly IReadOnlyList<double> SCROLL_PRICES =
    [
        1_000,
        40_000,
        1_600_000,
        480_000_000
    ];

    private static readonly IReadOnlyList<double> CSCROLL_PRICES =
    [
        2_400,
        100_000,
        4_000_000,
        640_000_000
    ];

    private const double UPGRADE_GAIN = 0.6;

    private const double UPGRADE_DECAY = 0.25;

    private const double UPGRADE_PLAIN = 0.005;

    private const double COMPOUND_GAIN = 0.4;

    private const double COMPOUND_DECAY = 0;

    private const double COMPOUND_PLAIN = 0.02;
    private static Dictionary<FieldInfo, object?> CapturedGameData = new();

    [Before(Class)]
    public static void EnsureGameData()

        //from the committed snapshot, so no credentials are needed
        => CapturedGameData = Fixture.LoadGameDataIfEmpty();

    private static IReadOnlyList<OfferingChoice> Offerings(
        double primling = 480_000,
        double essence = 27_420_000,
        double essenceX = 242_064_000)
        =>
        [
            new("offeringp", 1, primling),
            new("offering", 2, essence),
            new("offeringx", 3, essenceX)
        ];

    [After(Class)]
    public static void RestoreGameData() => Fixture.RestoreGameData(CapturedGameData);

    #region Upgrade planner
    [Test]
    public async Task AFreeOfferingCutsTheUpgradeCost()
    {
        var paid = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(10, 4_800);

        var free = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(1),
            true).FindCheapestPlan(10, 4_800);

        free.Steps
            .Should()
            .Contain(step => step.Offering == "offeringp");

        free.TotalCost
            .Should()
            .BeLessThan(paid.TotalCost);

        await Task.CompletedTask;
    }

    [Test]
    public async Task CheapOfferingsAreDepositedForGrace()
    {
        //a primling at a gold apiece makes +0.5 grace nearly free while every +9 -> +10 attempt stakes a fortune,
        //so the planner banks grace before rolling - and the clamp keeps the count finite
        var offerings = Offerings(1, 0, 0);

        var result = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            offerings,
            false).FindCheapestPlan(10, 100_000_000, 9);

        result.Unreachable
              .Should()
              .BeNull();

        result.Steps
              .Should()
              .HaveCount(1);

        result.Steps[0]
              .Deposits
              .Should()
              .BeGreaterThan(0);

        //the item's grace share clamps at newLevel + 1, so past (10 + 3) / 0.5 sacrifices nothing more converts
        result.Steps[0]
              .Deposits
              .Should()
              .BeLessThanOrEqualTo(28);

        //the reported grace is what the attempt rolls with, so this level's own deposits are already in it
        result.Steps[0]
              .ItemGrace
              .Should()
              .BeApproximately(0.5 * result.Steps[0].Deposits, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AnUpgradePlansFromTheStartLevel()
    {
        var result = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(8, 10_000, 5);

        result.Unreachable
              .Should()
              .BeNull();

        result.Steps
              .Should()
              .HaveCount(3);

        result.Steps[0]
              .FromLevel
              .Should()
              .Be(5);

        result.Steps[^1]
              .FromLevel
              .Should()
              .Be(7);

        //a start at or past the target leaves nothing to plan
        new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true).FindCheapestPlan(8, 10_000, 8)
                     .Unreachable
                     .Should()
                     .NotBeNull();

        await Task.CompletedTask;
    }

    [Test]
    public async Task AnUpgradePastTheTrackIsRefused()
    {
        //the last threshold is where grade 4 begins, and the server refuses upgrades from there
        new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true).FindCheapestPlan(13, 4_800)
                     .Unreachable
                     .Should()
                     .NotBeNull();

        //luckyt's [0, 0, 0, 12] puts it at grade 3 from +0, and the chance table has no grade-3 row
        new UpgradePlanner(
                [
                    0,
                    0,
                    0,
                    12
                ],
                SCROLL_PRICES,
                Offerings(),
                true).FindCheapestPlan(1, 4_800)
                     .Unreachable
                     .Should()
                     .NotBeNull();

        await Task.CompletedTask;
    }

    [Test]
    public async Task CheapLevelsGetNoOffering()
    {
        var result = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(10, 4_800);

        result.Unreachable
              .Should()
              .BeNull();

        result.Steps
              .Should()
              .HaveCount(10);

        //matching scrolls the whole way: the over-grade bonus never repays a 40x price on this item
        result.Steps
              .Select(step => step.ScrollGrade)
              .Should()
              .Equal(
                  0,
                  0,
                  0,
                  0,
                  0,
                  0,
                  0,
                  1,
                  1,
                  2);

        //an offering can only pay where the copy at risk is worth several times the offering - never on the levels
        //where a destroyed wshield costs tens of thousands to rebuild
        result.Steps
              .Take(7)
              .Should()
              .OnlyContain(step => step.Offering == null);

        result.TotalCost
              .Should()
              .BeLessThanOrEqualTo(result.BaselineCost);

        //the ballpark a step-by-step simulation put on this grind
        result.TotalCost
              .Should()
              .BeInRange(250_000_000, 900_000_000);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AMatchingOfferingCapsAtTwiceBase()
    {
        //the +9 -> +10 wall: item grade 2, essence grade 2. Grace is large (graceNum = min(11, 10+3+1) + 6 = 17,
        //grace = 0.024x17/10 + 0.017 = 0.0578), so 0.024 x 1.4 + 0.0578 = 0.0914 - but an equal-grade offering is
        //not "high", and the cap min(base+0.24, base x 2) = 0.048 binds
        var chance = UpgradeMath.CalculateUpgradeChance(
                                    0.024,
                                    10,
                                    2,
                                    0,
                                    2,
                                    2,
                                    10,
                                    20,
                                    20,
                                    0)
                                .Chance;

        chance.Should()
              .BeApproximately(0.048, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AHigherOfferingRaisesTheCap()
    {
        //same wall with a grade-3 offering: 0.024 x 1.5 + 0.0578 x 1.2 = 0.1054, capped by the high cap
        //min(base+0.36, base x 3) = 0.072
        var chance = UpgradeMath.CalculateUpgradeChance(
                                    0.024,
                                    10,
                                    2,
                                    0,
                                    2,
                                    3,
                                    10,
                                    20,
                                    20,
                                    0)
                                .Chance;

        chance.Should()
              .BeApproximately(0.072, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AHigherScrollMultipliesTheChance()
    {
        //base 0.15 into +8 on a grade-1 item with a grade-2 scroll: 0.15 x 1.2 + 0.01 = 0.19. The grace term is
        //zero with no accumulated state - the level penalty 0.4/(n-0.999)^2 swallows it - and the high cap
        //min(base+0.36, base x 3) = 0.45 does not bind
        var chance = UpgradeMath.CalculateUpgradeChance(
                                    0.15,
                                    8,
                                    1,
                                    0,
                                    2,
                                    null,
                                    0,
                                    0,
                                    0,
                                    0)
                                .Chance;

        chance.Should()
              .BeApproximately(0.19, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task UpgradeCostGrowsWithTheTarget()
    {
        var toEight = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(8, 4_800);

        var toNine = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(9, 4_800);

        var toTen = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(10, 4_800);

        toEight.TotalCost
               .Should()
               .BeLessThan(toNine.TotalCost);

        toNine.TotalCost
              .Should()
              .BeLessThan(toTen.TotalCost);

        await Task.CompletedTask;
    }

    [Test]
    public async Task StartingGraceCutsTheCost()
    {
        //the item bucket min(newLevel + 1, grace + pity + igrace) is what a starting grace feeds; nothing else moves
        var plain = UpgradeMath.CalculateUpgradeChance(
                                   0.3,
                                   6,
                                   0,
                                   0,
                                   0,
                                   null,
                                   0,
                                   0,
                                   0,
                                   0)
                               .Chance;

        var graced = UpgradeMath.CalculateUpgradeChance(
                                    0.3,
                                    6,
                                    0,
                                    0,
                                    0,
                                    null,
                                    5,
                                    0,
                                    0,
                                    0)
                                .Chance;

        graced.Should()
              .BeGreaterThan(plain);

        //and through the planner: no offerings priced, so the grace is the only thing separating the two plans
        var none = Offerings(0, 0, 0);

        var cold = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            none,
            false).FindCheapestPlan(6, 10_000, 5);

        var warm = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            none,
            false).FindCheapestPlan(
            6,
            10_000,
            5,
            5);

        warm.TotalCost
            .Should()
            .BeLessThan(cold.TotalCost);

        //the step reports the grace its chance was computed from, and with nothing priced to deposit that is the
        //starting grace untouched
        warm.Steps[0]
            .ItemGrace
            .Should()
            .BeApproximately(5, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AnUpgradePlanBeatsTheBaseline()
    {
        foreach (var target in new[]
                 {
                     5,
                     8,
                     10
                 })
        {
            var result = new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true).FindCheapestPlan(target, 4_800);

            result.TotalCost
                  .Should()
                  .BeLessThanOrEqualTo(result.BaselineCost);
        }

        await Task.CompletedTask;
    }

    [Test]
    public async Task TheLuckySlotRaisesTheChance()
    {
        //the lucky slot works on the roll, not the chance: success needs the raw roll under (p + 0.012)/0.975, and
        //that happens 60% of the time. On the capped 0.048 above: 0.6 x 0.0615 + 0.4 x 0.048 = 0.05612
        var chance = UpgradeMath.CalculateLuckySlotChance(
            UpgradeMath.CalculateUpgradeChance(
                           0.024,
                           10,
                           2,
                           0,
                           2,
                           2,
                           10,
                           20,
                           20,
                           0)
                       .Chance);

        chance.Should()
              .BeApproximately(0.6 * (0.048 + 0.012) / 0.975 + 0.4 * 0.048, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task NoForcedPlanBeatsTheSearch()
    {
        const int TARGET = 3;
        const double COPY_PRICE = 1_000_000;

        //every plan the planner is allowed to write over three levels, with at most two deposits
        var options = new List<ForcedStep>();

        foreach (var scroll in new[]
                 {
                     0,
                     1
                 })
            foreach (var offering in new[]
                     {
                         null,
                         "offeringp",
                         "offering",
                         "offeringx"
                     })
                foreach (var deposits in new[]
                         {
                             0,
                             1,
                             2
                         })
                    options.Add(
                        new ForcedStep(
                            0,
                            scroll,
                            offering,
                            deposits,
                            0,
                            deposits > 0 ? "offeringp" : null));

        var planned = new UpgradePlanner(
            BOOTS_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(TARGET, COPY_PRICE);

        planned.Unreachable
               .Should()
               .BeNull();

        var walk = new ForcedStep[TARGET];
        var best = double.MaxValue;
        string? bestPlan = null;

        Walk(0);

        bestPlan.Should()
                .NotBeNull("a walk that priced nothing would pass this test without comparing anything");

        //a rival whose failures would bank a different offering pity may come in a hair under
        best.Should()
            .BeGreaterThan(planned.TotalCost * (1 - 1e-4), $"'{bestPlan}' beats the planned climb");

        await Task.CompletedTask;

        void Walk(int level)
        {
            if (level == TARGET)
            {
                var forced = new UpgradePlanner(
                    BOOTS_THRESHOLDS,
                    SCROLL_PRICES,
                    Offerings(),
                    true).FindCheapestPlan(TARGET, COPY_PRICE, forced: walk);

                if (forced.Unreachable is null && (forced.TotalCost < best))
                {
                    best = forced.TotalCost;
                    bestPlan = string.Join(", ", walk.Select(step => step.ToString()));
                }

                return;
            }

            foreach (var option in options)
            {
                walk[level] = option with
                {
                    FromLevel = level
                };

                Walk(level + 1);
            }
        }
    }

    [Test]
    public async Task AnEmptyTrackIsRefused()
    {
        new UpgradePlanner(
                [],
                SCROLL_PRICES,
                Offerings(),
                false).FindCheapestPlan(1, 4_800)
                      .Unreachable
                      .Should()
                      .Be("The item has no upgrade track.");

        new CompoundPlanner([], CSCROLL_PRICES, Offerings()).FindCheapestPlan(1, 12_000)
                                                            .Unreachable
                                                            .Should()
                                                            .Be("The item has no compound track.");

        await Task.CompletedTask;
    }

    [Test]
    public async Task UpgradeWithoutPity()
    {
        var offerings = Offerings();
        var gradeAtZero = UpgradeMath.CalculateGrade(WSHIELD_THRESHOLDS, 0);

        var result = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            offerings,
            false,
            false).FindCheapestPlan(10, 4_800);

        result.Unreachable
              .Should()
              .BeNull();

        foreach (var step in result.Steps)
        {
            UpgradeMath.TryGetUpgradeBaseChance(WSHIELD_THRESHOLDS, step.FromLevel, out var baseChance)
                       .Should()
                       .BeTrue();

            var expected = UpgradeMath.CalculateUpgradeChance(
                                          baseChance,
                                          step.FromLevel + 1,
                                          UpgradeMath.CalculateGrade(WSHIELD_THRESHOLDS, step.FromLevel),
                                          gradeAtZero,
                                          step.ScrollGrade,
                                          offerings.FirstOrDefault(offering => offering.Name == step.Offering)
                                                   ?.Grade,
                                          step.ItemGrace,
                                          0,
                                          0,
                                          0)
                                      .Chance;

            step.Chance
                .Should()
                .BeApproximately(expected, 1e-9);
        }

        await Task.CompletedTask;
    }

    [Test]
    public async Task StatPrimesBankGraceWhenScrollsCostLessThanAnOffering()
    {
        //at +9 the item is rare grade, so a stat prime costs 100 scrolls at 1 gold and one primling: far under two primlings
        var result = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            false,
            statScrollPrice: 1).FindCheapestPlan(10, 100_000_000, 9);

        result.Unreachable
              .Should()
              .BeNull();

        result.Steps[0]
              .StatPrimes
              .Should()
              .BeGreaterThan(0);

        //a half point left over takes one plain prime, never two
        result.Steps[0]
              .Deposits
              .Should()
              .BeLessThanOrEqualTo(1);

        //a stat prime banks a whole point, a plain prime half of one
        result.Steps[0]
              .ItemGrace
              .Should()
              .BeApproximately(result.Steps[0].StatPrimes + 0.5 * result.Steps[0].Deposits, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task NoStatPrimeIsPlannedAtLegendaryGrade()
    {
        //at +10 the item is legendary; scrolls at 1 gold would still beat an offering if they were allowed
        var result = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            false,
            statScrollPrice: 1).FindCheapestPlan(11, 100_000_000, 10);

        result.Unreachable
              .Should()
              .BeNull();

        result.Steps[0]
              .StatPrimes
              .Should()
              .Be(0);

        //grace is still worth buying here, so the zero above is the grade rule and not a lack of demand
        result.Steps[0]
              .Deposits
              .Should()
              .BeGreaterThan(0);

        await Task.CompletedTask;
    }

    [Test]
    public async Task EachHalfPointOfGraceIsOfferedOnceInTheCheaperKind()
    {
        var cheap = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            false,
            statScrollPrice: 1);

        foreach (var bench in cheap.GetChoices(9, 0)
                                   .GroupBy(choice => (choice.ScrollGrade, choice.Offering?.Name)))
        {
            bench.Should()
                 .OnlyContain(choice => choice.Deposits <= 1);

            //half points of grace 0, 1, 2, ... each appear once
            bench.Select(choice => choice.Deposits + 2 * choice.StatPrimes)
                 .Should()
                 .Equal(Enumerable.Range(0, bench.Count()));
        }

        //100 scrolls at 10,000 gold cost far more than a second primling
        var dear = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            false,
            statScrollPrice: 10_000);

        dear.GetChoices(9, 0)
            .Should()
            .OnlyContain(choice => choice.StatPrimes == 0);

        await Task.CompletedTask;
    }
    #endregion

    #region Compound planner
    [Test]
    public async Task AFreeOfferingCutsTheCompoundCost()
    {
        var paid = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(5, 12_000);

        var free = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings(1)).FindCheapestPlan(5, 12_000);

        free.Steps
            .Should()
            .Contain(step => step.Offering == "offeringp");

        free.TotalCost
            .Should()
            .BeLessThan(paid.TotalCost);

        //the compound bench takes no scroll-less deposits
        free.Steps
            .Should()
            .OnlyContain(step => step.Deposits == 0);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ACompoundPlansFromTheStartLevel()
    {
        var result = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(
            4,
            12_000,
            2,
            3);

        result.Unreachable
              .Should()
              .BeNull();

        result.Steps
              .Should()
              .HaveCount(2);

        result.Steps[0]
              .FromLevel
              .Should()
              .Be(2);

        //the first step's copies carry the starting grace, and the reported figure is each copy's share rather than
        //the pooled term the chance was built from
        result.Steps[0]
              .ItemGrace
              .Should()
              .BeApproximately(3, 1e-9);

        //a start at or past the target leaves nothing to plan
        new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(4, 12_000, 4)
                                                                          .Unreachable
                                                                          .Should()
                                                                          .NotBeNull();

        await Task.CompletedTask;
    }

    [Test]
    public async Task ACompoundPastTheTrackIsRefused()
    {
        new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(8, 12_000)
                                                                          .Unreachable
                                                                          .Should()
                                                                          .NotBeNull();

        //grade 3 from +0 has no row in the compound table either
        new CompoundPlanner(
                [
                    0,
                    0,
                    0,
                    12
                ],
                CSCROLL_PRICES,
                Offerings()).FindCheapestPlan(1, 12_000)
                            .Unreachable
                            .Should()
                            .NotBeNull();

        await Task.CompletedTask;
    }

    [Test]
    public async Task AMatchingOfferingHitsTheCompoundCap()
    {
        //pooled grace 40 converts at 0.027 x 40.5 = 1.09, clamped to 0.81 - then 0.05 x 1.36 + 0.81
        //slams into the unwidened cap min(base x 3, base + 0.2) = 0.15
        UpgradeMath.CalculateCompoundChance(
                       0.05,
                       8,
                       1,
                       1,
                       1,
                       40,
                       0)
                   .Chance
                   .Should()
                   .BeApproximately(0.15, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AHigherOfferingOverridesTheScrollsCap()
    {
        //the server's own oddity: a scroll two grades over widens the cap by 2, but the over-grade offering then
        //sets the widening to 1 rather than adding - so the cap is base x 3.6 = 0.18, not base x 4.2
        UpgradeMath.CalculateCompoundChance(
                       0.05,
                       6,
                       0,
                       2,
                       2,
                       40,
                       0)
                   .Chance
                   .Should()
                   .BeApproximately(0.18, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AHigherScrollAddsASmallBonus()
    {
        //base 0.4 into +3 with a grade-1 scroll on a grade-0 item: 0.4 x 1.1 + 0.001 = 0.441, and the widened cap
        //min(base x 3.6, base + 0.25) = 0.65 does not bind
        UpgradeMath.CalculateCompoundChance(
                       0.4,
                       3,
                       0,
                       1,
                       null,
                       0,
                       0)
                   .Chance
                   .Should()
                   .BeApproximately(0.441, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ACompoundStakesThreeCopies()
    {
        var toOne = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(1, 12_000);

        var toTwo = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(2, 12_000);

        //an attempt consumes three copies of the level below, so each level costs more than triple the one before
        toTwo.TotalCost
             .Should()
             .BeGreaterThan(3 * toOne.TotalCost);

        await Task.CompletedTask;
    }

    [Test]
    public async Task PlainCompoundGraceIsClamped()
    {
        //0.007 x 30 = 0.21, clamped to 0.175, divided by max(level-1, 1) = 1: 0.4 + 0.175 = 0.575, under the cap
        //min(1.2, 0.6)
        UpgradeMath.CalculateCompoundChance(
                       0.4,
                       3,
                       0,
                       0,
                       null,
                       30,
                       0)
                   .Chance
                   .Should()
                   .BeApproximately(0.575, 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ACompoundPlanBeatsTheBaseline()
    {
        foreach (var target in new[]
                 {
                     3,
                     5,
                     7
                 })
        {
            var result = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(target, 12_000);

            result.Unreachable
                  .Should()
                  .BeNull();

            result.TotalCost
                  .Should()
                  .BeLessThanOrEqualTo(result.BaselineCost);
        }

        await Task.CompletedTask;
    }

    [Test]
    public async Task TheLostEarringCostsMore()
    {
        //the server pins the lost earring's grade at 2, a dearer row than its thresholds read on their own
        IReadOnlyList<int> earring =
        [
            0,
            2,
            6,
            7
        ];

        var pinned = new CompoundPlanner(
            earring,
            CSCROLL_PRICES,
            Offerings(),
            "lostearring").FindCheapestPlan(3, 12_000);

        var unpinned = new CompoundPlanner(earring, CSCROLL_PRICES, Offerings()).FindCheapestPlan(3, 12_000);

        pinned.Unreachable
              .Should()
              .BeNull();

        pinned.TotalCost
              .Should()
              .BeGreaterThan(unpinned.TotalCost);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ACompoundIgnoresDeposits()
    {
        //the compound bench refuses a scroll-less call, so a forced step's deposits price nothing and report as none
        ForcedStep[] plain =
        [
            new(0, 0, null),
            new(1, 0, null)
        ];

        ForcedStep[] stuffed =
        [
            new(
                0,
                0,
                null,
                2,
                0,
                "offeringp"),
            new(
                1,
                0,
                null,
                2,
                0,
                "offeringp")
        ];

        var withoutDeposits = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(2, 12_000, forced: plain);

        var withDeposits = new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings()).FindCheapestPlan(2, 12_000, forced: stuffed);

        withDeposits.Steps
                    .Should()
                    .Equal(withoutDeposits.Steps);

        await Task.CompletedTask;
    }

    [Test]
    public async Task CompoundWithoutPity()
    {
        //offerings at a gold apiece get used, so offering pity would otherwise build up
        var offerings = Offerings(1, 1, 1);

        var result = new CompoundPlanner(
            WBOOK_THRESHOLDS,
            CSCROLL_PRICES,
            offerings,
            countPity: false).FindCheapestPlan(4, 12_000);

        result.Unreachable
              .Should()
              .BeNull();

        result.Steps
              .Should()
              .Contain(step => step.Offering != null);

        foreach (var step in result.Steps)
        {
            UpgradeMath.TryGetCompoundBaseChance(WBOOK_THRESHOLDS, step.FromLevel, out var baseChance)
                       .Should()
                       .BeTrue();

            var expected = UpgradeMath.CalculateCompoundChance(
                                          baseChance,
                                          step.FromLevel + 1,
                                          UpgradeMath.CalculateGrade(WBOOK_THRESHOLDS, step.FromLevel),
                                          step.ScrollGrade,
                                          offerings.FirstOrDefault(offering => offering.Name == step.Offering)
                                                   ?.Grade,
                                          3 * step.ItemGrace,
                                          0)
                                      .Chance;

            step.Chance
                .Should()
                .BeApproximately(expected, 1e-9);
        }

        await Task.CompletedTask;
    }
    #endregion

    #region Offering pity
    [Test]
    public async Task ACertainClimbBanksNoPity()
    {
        //no failures means no offering is ever wasted, so there is nothing for the counter to hold
        double[] chances =
        [
            1,
            1,
            1
        ];

        bool[] withOffering =
        [
            true,
            true,
            true
        ];

        var settled = new double[chances.Length];

        Bench.UPGRADE.UpdateOfferingPity(
            settled,
            chances,
            withOffering,
            0);

        settled.Should()
               .OnlyContain(value => value == 0);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ACompoundSuccessClearsThePity()
    {
        //the compound bench zeroes the counter on any success that spent an offering, and no level is reachable
        //without a success below it
        double[] chances =
        [
            0.6,
            0.4,
            0.25,
            0.15
        ];

        bool[] withOffering =
        [
            true,
            true,
            true,
            true
        ];

        var settled = new double[chances.Length];

        Bench.COMPOUND.UpdateOfferingPity(
            settled,
            chances,
            withOffering,
            0);

        settled[0]
            .Should()
            .BeGreaterThan(0);

        settled.Skip(1)
               .Should()
               .OnlyContain(value => value == 0);

        var ground = SimulateOfferingPity(
            chances,
            withOffering,
            0,
            COMPOUND_GAIN,
            COMPOUND_DECAY,
            COMPOUND_PLAIN);

        settled[0]
            .Should()
            .BeApproximately(ground[0], 0.01 * ground[0]);

        await Task.CompletedTask;
    }

    [Test]
    public async Task NoOfferingBanksNoPity()
    {
        double[] chances =
        [
            0.5,
            0.3,
            0.2
        ];

        bool[] withOffering =
        [
            false,
            false,
            false
        ];

        var settled = new double[chances.Length];

        var moved = Bench.UPGRADE.UpdateOfferingPity(
            settled,
            chances,
            withOffering,
            0);

        settled.Should()
               .OnlyContain(value => value == 0);

        //nothing moved, so the caller stops re-solving after the first pass
        moved.Should()
             .BeFalse();

        await Task.CompletedTask;
    }

    [Test]
    public async Task AnUpgradeSuccessQuartersThePity()
    {
        double[] chances =
        [
            0.8,
            0.6,
            0.4,
            0.25
        ];

        bool[] withOffering =
        [
            true,
            true,
            true,
            true
        ];

        var settled = new double[chances.Length];

        Bench.UPGRADE.UpdateOfferingPity(
            settled,
            chances,
            withOffering,
            0);

        for (var level = 1; level < chances.Length; level++)
            settled[level]
                .Should()
                .BeApproximately(settled[level - 1] * UPGRADE_DECAY, 1e-12);

        //which leaves the top of a four-level climb with under a hundredth of what the bottom carries
        settled[^1]
            .Should()
            .BeLessThan(settled[0] / 50);

        await Task.CompletedTask;
    }

    [Test]
    public async Task PityMatchesASimulationWithSomeOfferings()
    {
        //the cheap levels go bare and the wall gets an offering, which is the shape the planner actually picks
        double[] chances =
        [
            0.95,
            0.8,
            0.5,
            0.22
        ];

        bool[] withOffering =
        [
            false,
            false,
            true,
            true
        ];

        var settled = new double[chances.Length];

        Bench.UPGRADE.UpdateOfferingPity(
            settled,
            chances,
            withOffering,
            4);

        var ground = SimulateOfferingPity(
            chances,
            withOffering,
            4,
            UPGRADE_GAIN,
            UPGRADE_DECAY,
            UPGRADE_PLAIN);

        for (var level = 0; level < chances.Length; level++)
            settled[level]
                .Should()
                .BeApproximately(ground[level], Math.Max(0.01 * ground[level], 1e-4));

        await Task.CompletedTask;
    }

    [Test]
    public async Task PityMatchesASimulation()
    {
        double[] chances =
        [
            0.9,
            0.7,
            0.45,
            0.3,
            0.18
        ];

        bool[] withOffering =
        [
            true,
            true,
            true,
            true,
            true
        ];

        var settled = new double[chances.Length];

        Bench.UPGRADE.UpdateOfferingPity(
            settled,
            chances,
            withOffering,
            0);

        var ground = SimulateOfferingPity(
            chances,
            withOffering,
            0,
            UPGRADE_GAIN,
            UPGRADE_DECAY,
            UPGRADE_PLAIN);

        for (var level = 0; level < chances.Length; level++)
            settled[level]
                .Should()
                .BeApproximately(ground[level], Math.Max(0.01 * ground[level], 1e-4));

        await Task.CompletedTask;
    }

    private static double[] SimulateOfferingPity(
        IReadOnlyList<double> chances,
        IReadOnlyList<bool> withOffering,
        int startLevel,
        double gain,
        double decay,
        double plainPerLevel,
        int legs = 400_000)
    {
        var levels = chances.Count;
        var totals = new double[levels];
        var counts = new long[levels];
        var random = new Random(20260823);
        var counter = 0.0;

        //every failure restarts from the bottom, and the counter rides through because it sits on the character
        for (var leg = 0; leg < legs; leg++)
            for (var level = 0; level < levels; level++)
            {
                totals[level] += counter;
                counts[level]++;

                if (random.NextDouble() >= chances[level])
                {
                    if (withOffering[level])
                        counter += gain;

                    break;
                }

                counter *= withOffering[level] ? decay : 1 - (startLevel + level + 1) * plainPerLevel;
            }

        return
        [
            .. Enumerable.Range(0, levels)
                         .Select(level => totals[level] / counts[level])
        ];
    }
    #endregion
}