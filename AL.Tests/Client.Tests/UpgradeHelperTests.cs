#region
using System.Reflection;
using AL.Client.Helpers;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Data;
using AL.Data.Items;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     Checks that the item-level entry points resolve the item's thresholds, grade and base chance the way the server
///     does.
/// </summary>
[NotInParallel(ParallelKeys.GAME_DATA)]
public sealed class UpgradeHelperTests
{
    private static readonly IReadOnlyList<double> SCROLL_PRICES =
    [
        1_000,
        40_000,
        1_600_000,
        480_000_000
    ];

    private static Dictionary<FieldInfo, object?> CapturedGameData = new();

    [Test]
    public async Task AnItemWithNoGradesPlansAgainstTheServerDefaults()
    {
        var item = new GItem
        {
            UpgradeModifiers = new Dictionary<ALAttribute, float>()
        };

        var planned = UpgradeHelper.FindCheapestUpgradePlan(
            item,
            8,
            10_000,
            SCROLL_PRICES,
            Offerings(),
            false);

        var expected = new UpgradePlanner(
            [
                9,
                10,
                11,
                12
            ],
            SCROLL_PRICES,
            Offerings(),
            false).FindCheapestPlan(8, 10_000);

        planned.Unreachable
               .Should()
               .BeNull();

        planned.TotalCost
               .Should()
               .Be(expected.TotalCost);

        await Task.CompletedTask;
    }

    [Test]
    public async Task AnItemWithNoLevelHasNothingToPlanOrRoll()
    {
        var item = new GItem
        {
            Grades =
            [
                4,
                8,
                10,
                12
            ]
        };

        UpgradeHelper.FindCheapestUpgradePlan(
                         item,
                         4,
                         10_000,
                         SCROLL_PRICES,
                         Offerings(),
                         false)
                     .Unreachable
                     .Should()
                     .NotBeNull();

        UpgradeHelper.TryCalculateUpgradeChance(
                         item,
                         0,
                         0,
                         null,
                         0,
                         0,
                         0,
                         0,
                         out _)
                     .Should()
                     .BeFalse();

        await Task.CompletedTask;
    }

    [Test]
    public async Task AnUpgradeAttemptReadsTheGradeAtItsLevelAndTheRowAtZero()
    {
        var item = new GItem
        {
            Grades =
            [
                4,
                8,
                10,
                12
            ],
            UpgradeModifiers = new Dictionary<ALAttribute, float>()
        };

        //+5 to +6 is grade 1 against [4, 8, ...], and every upgrade prices from the grade 0 row the item has at +0
        var expected = UpgradeMath.CalculateUpgradeChance(
            GameData.Upgrades.GetChance(0, 6)!.Value,
            6,
            1,
            0,
            1,
            null,
            0.5,
            2,
            3,
            0);

        UpgradeHelper.TryCalculateUpgradeChance(
                         item,
                         5,
                         1,
                         null,
                         0.5,
                         2,
                         3,
                         0,
                         out var breakdown)
                     .Should()
                     .BeTrue();

        breakdown.Should()
                 .Be(expected);

        await Task.CompletedTask;
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

    [After(Class)]
    public static void RestoreGameData() => Fixture.RestoreGameData(CapturedGameData);
}