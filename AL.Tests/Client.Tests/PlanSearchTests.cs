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
///     Checks the plan search against every plan a short climb has, and the priced model against the server's rules played
///     out attempt by attempt.
/// </summary>
[NotInParallel(ParallelKeys.GAME_DATA)]
public sealed class PlanSearchTests
{
    private const double COPY_PRICE = 480_000;

    private static readonly IReadOnlyList<int> WSHIELD_THRESHOLDS =
    [
        7,
        9,
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

    private static Dictionary<FieldInfo, object?> CapturedGameData = new();

    private static void CheckAgainstEveryPlan(
        PlannerBase planner,
        int startLevel,
        int targetLevel,
        double copyPrice)
    {
        var every = new List<PricedPlan>();
        var plan = new PlanChoice[targetLevel - startLevel];

        void Walk(int index, double grace)
        {
            if (index == plan.Length)
            {
                every.Add(planner.Price([.. plan], startLevel, 0));

                return;
            }

            var level = startLevel + index;

            foreach (var choice in planner.GetChoices(level, grace))
            {
                var staked = grace + UpgradeMath.DEPOSIT_GRACE * choice.Deposits;
                plan[index] = choice;
                Walk(index + 1, planner.CalculateCarriedGrace(level, staked, choice));
            }
        }

        Walk(0, 0);

        var cheapest = every.Min(priced => priced.CalculateCost(copyPrice));

        planner.FindCheapestPlan(targetLevel, copyPrice, startLevel)
               .TotalCost
               .Should()
               .BeApproximately(cheapest, cheapest * 1e-9);

        var edge = new List<PricedPlan>();

        foreach (var priced in every.OrderBy(priced => priced.Copies)
                                    .ThenBy(priced => priced.Gold))
            if ((edge.Count == 0) || (priced.Gold < edge[^1].Gold))
                edge.Add(priced);

        var builds = new UpgradeFrontier(planner).Generate(targetLevel, copyPrice, startLevel);

        builds.Should()
              .HaveCount(edge.Count);

        foreach ((var build, var expected) in builds.Zip(edge))
        {
            build.Copies
                 .Should()
                 .BeApproximately(expected.Copies, expected.Copies * 1e-9);

            build.Gold
                 .Should()
                 .BeApproximately(expected.Gold, Math.Max(1, expected.Gold) * 1e-9);
        }
    }

    [Before(Class)]
    public static void EnsureGameData() => CapturedGameData = Fixture.LoadGameDataIfEmpty();

    private static IReadOnlyList<OfferingChoice> Offerings()
        =>
        [
            new("offeringp", 1, 480_000),
            new("offering", 2, 27_420_000),
            new("offeringx", 3, 242_064_000)
        ];

    /// <summary>
    ///     Plays a plan by the server's upgrade rules, climbs back to back with every counter carried over, and returns the
    ///     copies and gold one finished copy took on average.
    /// </summary>
    /// <remarks>
    ///     The rules are written out again here, apart from the planner's, so a mistake in one does not hide in the other. The
    ///     2.5% chance of a bonus grace is left out, as the planner leaves it out.
    /// </remarks>
    private static (double Copies, double Gold) PlayServerRules(
        UpgradePlanner planner,
        IReadOnlyList<UpgradePlanStep> steps,
        int startLevel,
        double depositPrice,
        int climbs)
    {
        const double GRADE_ZERO_GRACE = 1;

        var random = new Random(20261002);
        var player = new double[20];
        var server = new double[20];
        double ograce = 0;
        double copies = 0;
        double gold = 0;
        var offerings = Offerings();

        for (var climb = 0; climb < climbs; climb++)
        {
            var level = startLevel;
            double grace = 0;
            copies++;

            while (level < (startLevel + steps.Count))
            {
                var step = steps[level - startLevel];
                var newLevel = level + 1;
                var itemGrade = UpgradeMath.CalculateGrade(WSHIELD_THRESHOLDS, level);

                var offeringGrade = step.Offering is { } name
                    ? offerings.First(offering => offering.Name == name)
                               .Grade
                    : -1;

                grace += 0.5 * step.Deposits;

                gold += SCROLL_PRICES[step.ScrollGrade]
                        + step.Deposits * depositPrice
                        + (step.Offering is { } bought
                            ? offerings.First(offering => offering.Name == bought)
                                       .Price
                            : 0);

                planner.TryGetBaseChance(level, out var baseChance);

                var graceNumber = Math.Max(
                    0,
                    Math.Min(newLevel + 1, grace + Math.Min(3, player[newLevel] / 4.5) + GRADE_ZERO_GRACE)
                    + Math.Min(6, server[newLevel] / 3)
                    + ograce / 3.2);

                var bonus = baseChance * graceNumber / newLevel + graceNumber / 1000;
                var probability = baseChance;
                var widened = false;

                if ((step.ScrollGrade > itemGrade) && (newLevel <= 10))
                {
                    probability = probability * 1.2 + 0.01;
                    widened = true;
                    grace += 0.4;
                }

                if (offeringGrade >= 0)
                {
                    if (offeringGrade > (itemGrade + 1))
                    {
                        probability = probability * 1.7 + bonus * 4;
                        widened = true;
                        grace += 3;
                    } else if (offeringGrade > itemGrade)
                    {
                        probability = probability * 1.5 + bonus * 1.2;
                        widened = true;
                        grace += 1;
                    } else if (offeringGrade == itemGrade)
                    {
                        probability = probability * 1.4 + bonus;
                        grace += 0.4;
                    } else if (offeringGrade == (itemGrade - 1))
                    {
                        probability = probability * 1.15 + bonus / 3.2;
                        grace += 0.2;
                    } else
                    {
                        probability = probability * 1.08 + bonus / 4;
                        grace += 0.1;
                    }
                } else
                    probability += Math.Max(0, bonus / 4.8 - 0.4 / ((newLevel - 0.999) * (newLevel - 0.999)));

                probability = widened
                    ? Math.Min(probability, Math.Min(baseChance + 0.36, baseChance * 3))
                    : Math.Min(probability, Math.Min(baseChance + 0.24, baseChance * 2));

                //the lucky slot rerolls six times in ten toward success
                var roll = random.NextDouble();

                if (random.NextDouble() < 0.6)
                    roll = Math.Max(random.NextDouble() / 10000, roll * 0.975 - 0.012);

                if (roll <= probability)
                {
                    player[newLevel] = server[newLevel] = 0;
                    ograce *= offeringGrade >= 0 ? 0.25 : 1 - newLevel * 0.005;
                    level = newLevel;

                    continue;
                }

                var withOffering = offeringGrade >= 0 ? 1 : 0;
                player[newLevel - 1]++;
                server[newLevel - 1]++;
                player[newLevel]++;
                server[newLevel]++;

                if ((newLevel >= 8) && (newLevel <= 15))
                {
                    player[newLevel - 1]++;
                    server[newLevel - 1]++;
                    player[newLevel - 2] += 2 + withOffering;
                    server[newLevel - 2] += 2;
                    player[newLevel - 3] += 2 + 2 * withOffering;
                    server[newLevel - 3] += 3 + withOffering;
                }

                if (offeringGrade >= 0)
                    ograce += 0.6;

                //the copy is gone, and a fresh one starts the next attempt from the bottom
                level = startLevel;
                grace = 0;
                copies++;
            }
        }

        return (copies / climbs, gold / climbs);
    }

    [After(Class)]
    public static void RestoreGameData() => Fixture.RestoreGameData(CapturedGameData);

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task TheCompoundSearchMatchesEveryPlan(bool countPity)
    {
        CheckAgainstEveryPlan(
            new CompoundPlanner(
                WBOOK_THRESHOLDS,
                CSCROLL_PRICES,
                Offerings(),
                null,
                countPity),
            0,
            3,
            1_000_000);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ThePriceMatchesTheServerRulesPlayedOut()
    {
        const int START_LEVEL = 5;
        const int TARGET_LEVEL = 8;
        const int CLIMBS = 400_000;

        var planner = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true,
            true,
            true);

        var plan = planner.FindCheapestPlan(TARGET_LEVEL, COPY_PRICE, START_LEVEL);

        (var copies, var gold) = PlayServerRules(
            planner,
            plan.Steps,
            START_LEVEL,
            Offerings()
                .Min(offering => offering.Price),
            CLIMBS);

        var played = copies * COPY_PRICE + gold;

        //the counters are averaged rather than followed climb by climb, which costs the model about a percent
        plan.TotalCost
            .Should()
            .BeApproximately(played, played * 0.02);

        await Task.CompletedTask;
    }

    [Test]
    [Arguments(5, false, false)]
    [Arguments(5, true, false)]
    [Arguments(5, true, true)]
    [Arguments(7, false, false)]
    public async Task TheUpgradeSearchMatchesEveryPlan(int startLevel, bool countPity, bool countServerPity)
    {
        CheckAgainstEveryPlan(
            new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true,
                countPity,
                countServerPity),
            startLevel,
            startLevel + 3,
            COPY_PRICE);

        await Task.CompletedTask;
    }
}