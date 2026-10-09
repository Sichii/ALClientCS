#region
using System.Reflection;
using AL.Client.Helpers;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Data.Items;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     Checks the copies-against-gold builds against the planner.
/// </summary>
[NotInParallel(ParallelKeys.GAME_DATA)]
public sealed class UpgradeFrontierTests
{
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

    [Test]
    public async Task ABuildMatchesThePlannersTotal()
    {
        const double COPY_PRICE = 480_000;

        var planned = new UpgradePlanner(
            WSHIELD_THRESHOLDS,
            SCROLL_PRICES,
            Offerings(),
            true).FindCheapestPlan(8, COPY_PRICE);

        var builds = new UpgradeFrontier(
            new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true)).Generate(8, COPY_PRICE);

        var build = builds.Single(candidate => candidate.Steps
                                                        .Select(step => (step.ScrollGrade, step.Offering, step.Deposits))
                                                        .SequenceEqual(
                                                            planned.Steps.Select(step => (step.ScrollGrade, step.Offering,
                                                                step.Deposits))));

        (build.Gold + build.Copies * COPY_PRICE).Should()
                                                .BeApproximately(planned.TotalCost, planned.TotalCost * 1e-9);

        //replaying the build at the same price ends on the planner's total
        UpgradeMath.CalculateExpectedTotals(
                       new GItem
                       {
                           UpgradeModifiers = new Dictionary<ALAttribute, float>(),
                           Grades = WSHIELD_THRESHOLDS
                       },
                       build.Steps,
                       false,
                       0,
                       COPY_PRICE,
                       SCROLL_PRICES,
                       Offerings())[^1]
                   .Should()
                   .BeApproximately(planned.TotalCost, planned.TotalCost * 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AStatPrimeBuildReplaysToThePlannersTotal()
    {
        const double COPY_PRICE = 100_000_000;
        const int START_LEVEL = 8;

        var item = new GItem
        {
            Stat = 1,
            UpgradeModifiers = new Dictionary<ALAttribute, float>(),
            Grades = WSHIELD_THRESHOLDS
        };

        (var planned, var builds) = UpgradeHelper.FindCheapestUpgradePlanAndFrontier(
            item,
            9,
            SCROLL_PRICES,
            Offerings(),
            false,
            COPY_PRICE,
            START_LEVEL);

        planned.Steps[0]
               .StatPrimes
               .Should()
               .BeGreaterThan(0);

        var build = builds.Single(candidate => candidate.Steps
                                                        .Select(step => (step.ScrollGrade, step.Offering, step.Deposits, step.StatPrimes))
                                                        .SequenceEqual(
                                                            planned.Steps.Select(step => (step.ScrollGrade, step.Offering, step.Deposits,
                                                                step.StatPrimes))));

        UpgradeMath.CalculateExpectedTotals(
                       item,
                       build.Steps,
                       false,
                       START_LEVEL,
                       COPY_PRICE,
                       SCROLL_PRICES,
                       Offerings())[^1]
                   .Should()
                   .BeApproximately(planned.TotalCost, planned.TotalCost * 1e-9);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ABuildPlansFromTheStartLevel()
    {
        var builds = new UpgradeFrontier(
            new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true)).Generate(
            8,
            0,
            5,
            2);

        builds.Should()
              .NotBeEmpty();

        foreach (var build in builds)
            build.Steps
                 .Should()
                 .HaveCount(3, "the plan covers +5 to +8 only");

        await Task.CompletedTask;
    }

    [Test]
    public async Task ACompoundBuildStakesThreeCopies()
    {
        var builds = new UpgradeFrontier(new CompoundPlanner(WBOOK_THRESHOLDS, CSCROLL_PRICES, Offerings())).Generate(3);

        builds.Should()
              .NotBeEmpty();

        //three copies staked per attempt over three levels is 27 even if every attempt succeeded
        foreach (var build in builds)
            build.Copies
                 .Should()
                 .BeGreaterThanOrEqualTo(27);

        await Task.CompletedTask;
    }

    [Test]
    public async Task ATargetPastTheTrackHasNoBuilds()
    {
        var builds = new UpgradeFrontier(
            new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true)).Generate(13);

        builds.Should()
              .BeEmpty();

        await Task.CompletedTask;
    }

    [Before(Class)]
    public static void EnsureGameData() => CapturedGameData = Fixture.LoadGameDataIfEmpty();

    [Test]
    public async Task MoreCopiesCostLessGold()
    {
        var builds = new UpgradeFrontier(
            new UpgradePlanner(
                WSHIELD_THRESHOLDS,
                SCROLL_PRICES,
                Offerings(),
                true)).Generate(8);

        builds.Count
              .Should()
              .BeGreaterThan(1, "real offering prices leave more than one build worth weighing");

        for (var index = 1; index < builds.Count; index++)
        {
            builds[index]
                .Copies
                .Should()
                .BeGreaterThan(builds[index - 1].Copies);

            builds[index]
                .Gold
                .Should()
                .BeLessThan(builds[index - 1].Gold);
        }

        foreach (var build in builds)
            build.Steps
                 .Should()
                 .HaveCount(8);

        //no two builds put the same things on the bench at every level
        builds.Select(build => string.Join('|', build.Steps.Select(step => $"{step.ScrollGrade}:{step.Offering}:{step.Deposits}")))
              .Should()
              .OnlyHaveUniqueItems();

        await Task.CompletedTask;
    }

    private static IReadOnlyList<OfferingChoice> Offerings()
        =>
        [
            new("offeringp", 1, 480_000),
            new("offering", 2, 27_420_000),
            new("offeringx", 3, 242_064_000)
        ];

    [After(Class)]
    public static void RestoreGameData() => Fixture.RestoreGameData(CapturedGameData);
}