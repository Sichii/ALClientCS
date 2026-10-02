#region
using System.Reflection;
using AL.Client.Definitions;
using AL.Client.Extensions;
using AL.Client.Helpers;
using AL.Data;
using AL.SocketClient.Model;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     Checks the server's grade, chance and cost rules against values written out here rather than read off the code.
/// </summary>
[NotInParallel(ParallelKeys.GAME_DATA)]
public class UpgradeMathTests
{
    private static readonly int[] SERVER_DEFAULT_THRESHOLDS =
    [
        9,
        10,
        11,
        12
    ];

    private static Dictionary<FieldInfo, object?> CapturedGameData = new();

    [Test]
    public void ACompoundGetsDearerAsItClimbs()
    {
        //the grade two levels below the one being left: +0 through +3 leave at grade 0, then it steps
        var rows = UpgradeMath.GetCompoundLevelChances(
            Marked,
            [
                2,
                4,
                6,
                7
            ]);

        rows.Select(row => row.Level)
            .Should()
            .Equal(Enumerable.Range(1, 7));

        rows.Select(row => row.Chance)
            .Should()
            .Equal(
                1d,
                2d,
                3d,
                4d,
                105d,
                106d,
                207d);
    }

    [Test]
    public void AHigherOfferingWidensTheCompoundCaps()
    {
        //scroll one over: 0.4 x 1.1 + 0.001 = 0.441; essence two over the item: x 1.64 + 0.027 x (10 + 0.5 + 1) x 2,
        //which is 1.344 uncapped. The offering sets the widening to 1, so the caps are 0.4 x 3.6 and 0.4 + 0.25 -
        //the flat one binds at 0.65
        var odds = UpgradeMath.CalculateCompoundChance(
            0.4,
            3,
            0,
            1,
            2,
            10,
            1);

        odds.Chance
            .Should()
            .BeApproximately(0.65, 1e-9);

        odds.Uncapped
            .Should()
            .BeApproximately(1.34424, 1e-5);

        odds.FlatCap
            .Should()
            .BeApproximately(0.65, 1e-9);

        odds.MultiplierCap
            .Should()
            .BeApproximately(1.44, 1e-9);

        odds.Capped
            .Should()
            .BeTrue();

        odds.IsFlatCapLower
            .Should()
            .BeTrue();

        odds.GraceCap
            .Should()
            .BeNull();
    }

    [Test]
    public void ALevelCostsThreeOfTheOneBelow()
    {
        var copies = UpgradeMath.CalculateCopiesPerLevel(
            [
                (1, 0.99d),
                (2, 0.75d),
                (3, 0.4d)
            ],
            3);

        copies[0]
            .Should()
            .Be(1d);

        copies[3]
            .Should()
            .BeApproximately(3d / 0.99d * (3d / 0.75d) * (3d / 0.4d), 1e-9);
    }

    [Test]
    public void AMatchingOfferingBanksMoreOnACompound()
    {
        Func<int, int, double>[] getOfferingGraceFuncs =
        [
            UpgradeMath.GetUpgradeOfferingGrace,
            UpgradeMath.GetCompoundOfferingGrace
        ];

        foreach (var getOfferingGraceFunc in getOfferingGraceFuncs)
        {
            getOfferingGraceFunc(4, 1)
                .Should()
                .Be(3);

            getOfferingGraceFunc(2, 1)
                .Should()
                .Be(1);

            getOfferingGraceFunc(0, 1)
                .Should()
                .Be(0.2);

            getOfferingGraceFunc(0, 2)
                .Should()
                .Be(0.1);
        }

        UpgradeMath.GetUpgradeOfferingGrace(1, 1)
                   .Should()
                   .Be(0.4);

        UpgradeMath.GetCompoundOfferingGrace(1, 1)
                   .Should()
                   .Be(0.5);
    }

    [Test]
    public void AMatchingOfferingScalesTheItemsGrace()
    {
        //same upgrade with a primling (grade 1 == item grade 1): 0.15 x 1.4 + grace, grace = 0.15 x 1/8 + 0.001. The
        //0.4 the offering deposits lands after the roll and is not in this figure
        var odds = UpgradeMath.CalculateUpgradeChance(
            0.15,
            8,
            1,
            0,
            1,
            1,
            0,
            0,
            0,
            0);

        odds.Chance
            .Should()
            .BeApproximately(0.22975, 1e-9);

        odds.GraceAdded
            .Should()
            .BeApproximately(0.01975, 1e-9);

        odds.Widened
            .Should()
            .BeFalse();
    }

    [Test]
    public void AMergeSumsTheCopiesOnlyWithAnOffering()
    {
        UpgradeMath.CalculateMergedGrace(1.6, true)
                   .Should()
                   .Be(3 * 1.6 / 6.4);

        UpgradeMath.CalculateMergedGrace(1.6, false)
                   .Should()
                   .Be(1.6 / 6.4);
    }

    [Test]
    public void AMissingTableEntryIsLeftOut()
    {
        //grade 3 from +0 on, and the server's tables stop at grade 2 - every level is left out rather than guessed
        UpgradeMath.GetUpgradeLevelChances(
                       Marked,
                       [
                           0,
                           0,
                           0,
                           12
                       ])
                   .Should()
                   .BeEmpty();

        //a table shorter than the thresholds answers for the levels it has and no further
        static double? Stunted(int grade, int level)
            => (grade, level) switch
            {
                (0, 1) => 0.5,
                (0, 2) => 0.25,
                _      => null
            };

        UpgradeMath.GetUpgradeLevelChances(
                       Stunted,
                       [
                           9,
                           10,
                           11,
                           12
                       ])
                   .Select(row => row.Level)
                   .Should()
                   .Equal(1, 2);
    }

    [Test]
    public void APlainCompoundAddsNothingWithNoGrace()
    {
        //wbook0 +2 -> +3 on the matching scroll: the table's 0.4 and a grace term of nothing
        var odds = UpgradeMath.CalculateCompoundChance(
            0.4,
            3,
            0,
            0,
            null,
            0,
            0);

        odds.Chance
            .Should()
            .BeApproximately(0.4, 1e-9);

        odds.GraceCap
            .Should()
            .BeApproximately(25 * 0.007, 1e-9);

        odds.Widened
            .Should()
            .BeFalse();

        odds.Parts
            .Should()
            .BeNull();
    }

    [Test]
    public void APlainUpgradeIsUncapped()
    {
        //wshield +7 -> +8 with the matching high scroll and nothing banked: base 0.15, a normal item's +1 is the whole grace
        //number, and the no-offering level penalty 0.4/(7.001^2) swallows the 0.01975/4.8 it would add
        var odds = UpgradeMath.CalculateUpgradeChance(
            0.15,
            8,
            1,
            0,
            1,
            null,
            0,
            0,
            0,
            0);

        odds.Chance
            .Should()
            .BeApproximately(0.15, 1e-9);

        odds.Uncapped
            .Should()
            .BeApproximately(0.15, 1e-9);

        odds.FlatCap
            .Should()
            .BeApproximately(0.39, 1e-9);

        odds.MultiplierCap
            .Should()
            .BeApproximately(0.30, 1e-9);

        odds.Widened
            .Should()
            .BeFalse();

        odds.Capped
            .Should()
            .BeFalse();

        odds.GraceNumber
            .Should()
            .Be(1);

        odds.GraceAdded
            .Should()
            .Be(0);

        odds.Parts!.BaseGradeGrace
            .Should()
            .Be(1);

        odds.Parts
            .OwnCap
            .Should()
            .Be(9);
    }

    [Test]
    public void APotionHasNoMaxLevel()
        => UpgradeMath.TryGetMaxLevel("hpot0", out _)
                      .Should()
                      .BeFalse();

    [Test]
    public void AnItemsOwnThresholdsWin()
    {
        //a compoundable item steps far lower than the default set does
        int[] thresholds =
        [
            2,
            3,
            4,
            5
        ];

        UpgradeMath.CalculateGrade(thresholds, 2)
                   .Should()
                   .Be(1);

        UpgradeMath.CalculateGrade(thresholds, 5)
                   .Should()
                   .Be(4);

        //and the default set must not be reaching in behind them
        UpgradeMath.CalculateGrade(thresholds, 1)
                   .Should()
                   .Be(0);
    }

    [Test]
    public void AnOverGradeScrollStopsPastTenOnUpgrades()
    {
        UpgradeMath.GetUpgradeScrollGrace(2, 1, 11)
                   .Should()
                   .Be(0);

        UpgradeMath.GetUpgradeScrollGrace(2, 1, 10)
                   .Should()
                   .Be(0.4);

        UpgradeMath.GetCompoundScrollGrace(2, 1)
                   .Should()
                   .Be(0.4);

        UpgradeMath.GetCompoundScrollGrace(1, 1)
                   .Should()
                   .Be(0);
    }

    [Test]
    public void AnUncappedAttemptHasNoCappedValue()

        //no offering at +7 -> +8: even twelve grace on the item leaves a plain upgrade under the cap
        => UpgradeMath.TryFindFirstCappedValue(
                          grace => UpgradeMath.CalculateUpgradeChance(
                              0.15,
                              8,
                              1,
                              0,
                              1,
                              null,
                              grace,
                              0,
                              0,
                              0),
                          0,
                          12,
                          0.05,
                          out _)
                      .Should()
                      .BeFalse();

    [Test]
    public void AnUnknownNameHasNoGrade()
    {
        UpgradeMath.CalculateGrade("notanitem", 12)
                   .Should()
                   .Be(0);

        UpgradeMath.CalculateGrade((string?)null, 12)
                   .Should()
                   .Be(0);

        UpgradeMath.HasLevel("notanitem")
                   .Should()
                   .BeFalse();

        UpgradeMath.HasLevel(null)
                   .Should()
                   .BeFalse();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("notanitem")]
    public void AnUnknownNameHasNoMaxLevel(string? name)
        => UpgradeMath.TryGetMaxLevel(name, out _)
                      .Should()
                      .BeFalse();

    [Test]
    public void AnUnupgradeableItemHasNoChances()
    {
        UpgradeMath.GetLevelChances(null)
                   .Should()
                   .BeEmpty();

        UpgradeMath.GetLevelChances(GameData.Items["hpot0"])
                   .Should()
                   .BeEmpty("a stack of potions is not sitting at +0");
    }

    [Test]
    public void AnUnupgradeableItemHasNoGrade()
    {
        UpgradeMath.TryGetGradeThresholds("cdragon", out _)
                   .Should()
                   .BeFalse();

        UpgradeMath.CalculateGrade("cdragon", 12)
                   .Should()
                   .Be(0);

        UpgradeMath.HasLevel("cdragon")
                   .Should()
                   .BeFalse();
    }

    [Test]
    public void AnUpgradeUsesTheGradeAtZero()
    {
        //grade 1 from +0 on, because the first threshold is 0
        var rows = UpgradeMath.GetUpgradeLevelChances(
            Marked,
            [
                0,
                8,
                10,
                12
            ]);

        rows.Select(row => row.Level)
            .Should()
            .Equal(Enumerable.Range(1, 12), "the track runs to the level grade 4 begins at");

        rows.Select(row => row.Chance)
            .Should()
            .Equal(
                Enumerable.Range(1, 12)
                          .Select(level => 100d + level),
                "an upgrade reads one grade row for the whole climb, however high the item gets");
    }

    [Test]
    public void BaseGradeGraceFollowsTheGradeAtZero()
    {
        UpgradeMath.GetBaseGradeGrace(0)
                   .Should()
                   .Be(1);

        UpgradeMath.GetBaseGradeGrace(1)
                   .Should()
                   .Be(-1);

        UpgradeMath.GetBaseGradeGrace(2)
                   .Should()
                   .Be(-2);

        UpgradeMath.GetBaseGradeGrace(3)
                   .Should()
                   .Be(0);
    }

    [Test]
    public void ChancesStopWhereGradeFourBegins()
    {
        UpgradeMath.GetUpgradeLevelChances(
                       Marked,
                       [
                           9,
                           10,
                           11,
                           12
                       ])
                   .Select(row => row.Level)
                   .Should()
                   .Equal(Enumerable.Range(1, 12));

        UpgradeMath.GetUpgradeLevelChances(
                       Marked,
                       [
                           2,
                           3,
                           4,
                           5
                       ])
                   .Select(row => row.Level)
                   .Should()
                   .Equal(Enumerable.Range(1, 5), "an item the server calls grade 4 at +5 is not upgraded past it");
    }

    [Test]
    public void CoalUsesTheDefaultThresholds()
    {
        UpgradeMath.TryGetGradeThresholds("coal", out var thresholds)
                   .Should()
                   .BeTrue();

        thresholds.Should()
                  .Equal(SERVER_DEFAULT_THRESHOLDS);

        UpgradeMath.DEFAULT_THRESHOLDS
                   .Should()
                   .Equal(SERVER_DEFAULT_THRESHOLDS);

        UpgradeMath.CalculateGrade("coal", 0)
                   .Should()
                   .Be(0);

        UpgradeMath.CalculateGrade("coal", 9)
                   .Should()
                   .Be(1);
    }

    [Test]
    public void CompoundGraceDividesByTheLevel()
    {
        //+5 -> +6 leaves level 5: no offering and two or more under divide by 4, one under by 3, matching or above by 1
        UpgradeMath.GetCompoundGraceDivisor(6, 2, null)
                   .Should()
                   .Be(4);

        UpgradeMath.GetCompoundGraceDivisor(6, 2, 1)
                   .Should()
                   .Be(3);

        UpgradeMath.GetCompoundGraceDivisor(6, 2, 0)
                   .Should()
                   .Be(4);

        UpgradeMath.GetCompoundGraceDivisor(6, 2, 2)
                   .Should()
                   .Be(1);

        UpgradeMath.GetCompoundGraceDivisor(6, 2, 3)
                   .Should()
                   .Be(1);

        //low levels floor at 1 rather than dividing by zero or less
        UpgradeMath.GetCompoundGraceDivisor(2, 2, null)
                   .Should()
                   .Be(1);

        UpgradeMath.GetCompoundGraceDivisor(3, 2, 1)
                   .Should()
                   .Be(1);
    }

    [Test]
    public void CopiesNeededMatchesCopiesPerLevel()
    {
        var copies = UpgradeMath.CalculateCopiesNeeded(
            [
                0.99,
                0.75,
                0.4
            ],
            3);

        copies.Should()
              .Be(3 / 0.99 * 3 / 0.75 * 3 / 0.4);

        copies.Should()
              .BeApproximately(
                  UpgradeMath.CalculateCopiesPerLevel(
                      [
                          (1, 0.99d),
                          (2, 0.75d),
                          (3, 0.4d)
                      ],
                      3)[3],
                  1e-9);
    }

    [Test]
    public void CopiesStopAtTheFirstMissingLevel()
        => UpgradeMath.CalculateCopiesPerLevel(
                          [
                              (1, 0.99d),
                              (3, 0.4d)
                          ],
                          3)
                      .Keys
                      .Should()
                      .BeEquivalentTo(
                          [
                              0,
                              1
                          ]);

    [Before(Class)]
    public static void EnsureGameData()

        //from the committed snapshot, so no credentials are needed
        => CapturedGameData = Fixture.LoadGameDataIfEmpty();

    private static IEnumerable<double?> GetChances(Func<int, int, double?> getChanceFunc, params int[] rowPerLevel)
        => rowPerLevel.Select((row, index) => getChanceFunc(row, index + 1));

    [Test]
    public void GetGradeFollowsTheServerRule()
    {
        new Item
            {
                Name = "xpbooster",
                Level = 6
            }.GetGrade()
             .Should()
             .Be(Grade.Legendary);

        new Item
            {
                Name = "coal",
                Level = 0
            }.GetGrade()
             .Should()
             .Be(Grade.Normal);

        new Item
            {
                Name = "cdragon",
                Level = 0
            }.GetGrade()
             .Should()
             .Be(Grade.None);
    }

    [Test]
    public void HeavyGraceHitsTheMultiplierCap()
    {
        //primordial x (grade 3) two above the item: 0.15 x 1.7 + grace x 4. Grace number is the item's own grace clamped
        //at level + 1 = 9 (5 + 3 + 1), plus the server's 6 (20/3 clamps), plus 3/3.2: 15.9375 -> grace 0.31465, so
        //the uncapped figure is 1.51, and of the widened caps min(0.15 + 0.36, 0.15 x 3) the multiplier binds
        var odds = UpgradeMath.CalculateUpgradeChance(
            0.15,
            8,
            1,
            0,
            1,
            3,
            5,
            14,
            20,
            3);

        odds.Chance
            .Should()
            .BeApproximately(0.45, 1e-9);

        odds.Uncapped
            .Should()
            .BeApproximately(1.5136, 1e-3);

        odds.Widened
            .Should()
            .BeTrue();

        odds.Capped
            .Should()
            .BeTrue();

        odds.IsFlatCapLower
            .Should()
            .BeFalse();

        odds.Parts!.Own
            .Should()
            .BeApproximately(9, 1e-9);

        odds.Parts
            .Player
            .Should()
            .Be(3);

        odds.Parts
            .Server
            .Should()
            .Be(6);

        odds.Parts
            .Pity
            .Should()
            .BeApproximately(3 / 3.2, 1e-9);
    }

    /// <summary>
    ///     Gets a stand-in chance whose value names its row and level: grade 2 at +7 reads 207.
    /// </summary>
    /// <param name="grade">The table row, 0 through 2.</param>
    /// <param name="level">The level reached, 1 through 12.</param>
    /// <returns>The marked chance, or null outside the table.</returns>
    private static double? Marked(int grade, int level) => grade is >= 0 and <= 2 && level is >= 1 and <= 12 ? grade * 100d + level : null;

    [Test]
    public void NoThresholdsIsGradeZero()
    {
        foreach (var level in new[]
                 {
                     0,
                     1,
                     9,
                     12,
                     20
                 })
            UpgradeMath.CalculateGrade((IReadOnlyList<int>?)null, level)
                       .Should()
                       .Be(0, $"an item that cannot be upgraded or compounded has no grade at +{level}");
    }

    [Test]
    public void OneAttemptTakesThreeOrOneCopies()
    {
        UpgradeMath.GetCopiesPerAttempt(true)
                   .Should()
                   .Be(3);

        UpgradeMath.GetCopiesPerAttempt(false)
                   .Should()
                   .Be(1);
    }

    [Test]
    public void OnlyTheFirstFourThresholdsCount()
    {
        UpgradeMath.CalculateGrade(
                       [
                           0,
                           1,
                           2,
                           3,
                           4
                       ],
                       9)
                   .Should()
                   .Be(4);

        UpgradeMath.CalculateGrade(
                       [
                           7,
                           9
                       ],
                       12)
                   .Should()
                   .Be(2);

        UpgradeMath.CalculateGrade([], 12)
                   .Should()
                   .Be(0);
    }

    [Test]
    public void OnlyUpgradeableItemsHaveALevel()
    {
        //hpot0 is a stack of potions - it neither upgrades nor compounds
        UpgradeMath.HasLevel("hpot0")
                   .Should()
                   .BeFalse();

        var upgradeable = GameData.Items.Entries.Values.First(item => item.UpgradeModifiers is not null);

        UpgradeMath.HasLevel(upgradeable.Accessor)
                   .Should()
                   .BeTrue();

        var compoundable = GameData.Items.Entries.Values.First(item => item.CompoundModifiers is not null);

        UpgradeMath.HasLevel(compoundable.Accessor)
                   .Should()
                   .BeTrue();
    }

    [After(Class)]
    public static void RestoreGameData() => Fixture.RestoreGameData(CapturedGameData);

    [Test]
    public void TheCompoundChancesAreTheServersOwn()
    {
        var rednose = GameData.Items["rednose"];

        rednose.Should()
               .NotBeNull();

        rednose!.Grades
                .Should()
                .Equal(
                    [
                        2,
                        4,
                        6,
                        7
                    ],
                    "the rows below walk grade 0 to grade 2, and that is what makes rednose walk it");

        //+0 to +2 at the grade at +0, then the grade two levels below the one being left: +1, +2, +3 and +4
        UpgradeMath.GetLevelChances(rednose)
                   .Select(row => (double?)row.Chance)
                   .Should()
                   .Equal(
                       GetChances(
                           GameData.Compounds.GetChance,
                           0,
                           0,
                           0,
                           0,
                           1,
                           1,
                           2));
    }

    [Test]
    [Arguments(0, 0)]
    [Arguments(8, 0)]
    [Arguments(9, 1)]
    [Arguments(10, 2)]
    [Arguments(11, 3)]
    [Arguments(12, 4)]
    [Arguments(13, 4)]
    public void TheDefaultThresholdsStepAtNineToTwelve(int level, int expected)
        => UpgradeMath.CalculateGrade(SERVER_DEFAULT_THRESHOLDS, level)
                      .Should()
                      .Be(expected);

    [Test]
    public void TheFirstCappedValueIsWhereTheCapIsReached()
    {
        //wshield +7 -> +8 with a primling: 0.15 x 1.4 + grace reaches the multiplier cap 0.30 once grace = 0.09,
        //which is a grace number of 0.09 / (0.15/8 + 0.001) = 4.5, and a normal item starts one ahead -
        //so the item's own grace stops mattering at 3.5, and the walk lands on the first 0.05 step at or past it, 3.6
        UpgradeMath.TryFindFirstCappedValue(
                       grace => UpgradeMath.CalculateUpgradeChance(
                           0.15,
                           8,
                           1,
                           0,
                           1,
                           1,
                           grace,
                           0,
                           0,
                           0),
                       0,
                       12,
                       0.05,
                       out var cappedValue)
                   .Should()
                   .BeTrue();

        cappedValue.Should()
                   .BeApproximately(3.6, 0.051);
    }

    /// <summary>
    ///     The server pins the lost earring's stored grade to 2, so its first three compounds use the grade 2 row.
    /// </summary>
    [Test]
    public void TheLostEarringIsPinnedToRowTwo()
    {
        UpgradeMath.GetCompoundBaseRow(
                       "lostearring",
                       [
                           0,
                           2,
                           6,
                           7
                       ])
                   .Should()
                   .Be(2);

        UpgradeMath.GetCompoundBaseRow(
                       null,
                       [
                           0,
                           2,
                           6,
                           7
                       ])
                   .Should()
                   .Be(1, "nothing else on those thresholds is pinned");

        //from +3 the row is the grade at +1, pin or no pin
        UpgradeMath.GetCompoundChanceRow(
                       [
                           0,
                           2,
                           6,
                           7
                       ],
                       3,
                       "lostearring")
                   .Should()
                   .Be(
                       UpgradeMath.CalculateGrade(
                           [
                               0,
                               2,
                               6,
                               7
                           ],
                           1));

        //the indexing on its own: named, the first three levels read grade 2 where the thresholds alone say grade 1
        UpgradeMath.GetCompoundLevelChances(
                       Marked,
                       [
                           0,
                           2,
                           6,
                           7
                       ],
                       "lostearring")
                   .Select(row => row.Chance)
                   .Should()
                   .Equal(
                       201d,
                       202d,
                       203d,
                       104d,
                       205d,
                       206d,
                       207d);

        UpgradeMath.GetCompoundLevelChances(
                       Marked,
                       [
                           0,
                           2,
                           6,
                           7
                       ])
                   .Select(row => row.Chance)
                   .Should()
                   .Equal(
                       [
                           101d,
                           102d,
                           103d,
                           104d,
                           205d,
                           206d,
                           207d
                       ],
                       "nothing else on those thresholds is pinned");

        var earring = GameData.Items["lostearring"];

        earring.Should()
               .NotBeNull();

        earring!.Grades
                .Should()
                .Equal(
                    0,
                    2,
                    6,
                    7);

        UpgradeMath.GetLevelChances(earring)
                   .Select(row => (double?)row.Chance)
                   .Should()
                   .Equal(
                       GetChances(
                           GameData.Compounds.GetChance,
                           2,
                           2,
                           2,
                           1,
                           2,
                           2,
                           2));

        UpgradeMath.TryGetCompoundBaseChance(
                       [
                           0,
                           2,
                           6,
                           7
                       ],
                       0,
                       out var chance,
                       "lostearring")
                   .Should()
                   .BeTrue();

        chance.Should()
              .Be(GameData.Compounds.GetChance(2, 1));
    }

    [Test]
    public void TheLuckySlotScalesTheRollSixTimesInTen()
        => UpgradeMath.CalculateLuckySlotChance(0.5)
                      .Should()
                      .Be(0.6 * Math.Min(1, (0.5 + 0.012) / 0.975) + 0.4 * 0.5);

    [Test]
    public void TheMaxLevelIsPerItem()
    {
        UpgradeMath.TryGetMaxLevel("firebow", out var firebowMaxLevel)
                   .Should()
                   .BeTrue();

        firebowMaxLevel.Should()
                       .Be(GameData.Items["firebow"]!.Grades![3]);

        UpgradeMath.TryGetMaxLevel("strearring", out var strearringMaxLevel)
                   .Should()
                   .BeTrue();

        strearringMaxLevel.Should()
                          .Be(GameData.Items["strearring"]!.Grades![3]);
    }

    [Test]
    public void TheMaxLevelIsTheLastThreshold()
    {
        UpgradeMath.GetMaxLevel(
                       [
                           2,
                           5,
                           6,
                           7
                       ])
                   .Should()
                   .Be(7);

        UpgradeMath.GetMaxLevel(SERVER_DEFAULT_THRESHOLDS)
                   .Should()
                   .Be(12);
    }

    [Test]
    public void TheNextLevelCostsWhatTheAttemptConsumes()
        => UpgradeMath.CalculateNextLevelCost(
                          100,
                          3,
                          10,
                          5,
                          2,
                          0.5)
                      .Should()
                      .Be((3 * 100 + 10 + 5 + 2) / 0.5);

    [Test]
    public void TheUpgradeChancesAreTheServersOwn()
    {
        var firebow = GameData.Items["firebow"];

        firebow.Should()
               .NotBeNull();

        firebow!.Grades
                .Should()
                .Equal(
                    [
                        0,
                        8,
                        10,
                        12
                    ],
                    "the figures below are the grade 1 row, and that is what puts firebow on it");

        UpgradeMath.GetLevelChances(firebow)
                   .Select(row => (double?)row.Chance)
                   .Should()
                   .Equal(
                       GetChances(
                           GameData.Upgrades.GetChance,
                           Enumerable.Repeat(1, 12)
                                     .ToArray()));

        UpgradeMath.TryGetUpgradeBaseChance(
                       [
                           0,
                           8,
                           10,
                           12
                       ],
                       11,
                       out var chance)
                   .Should()
                   .BeTrue();

        chance.Should()
              .Be(GameData.Upgrades.GetChance(1, 12));
    }

    [Test]
    public void ThresholdsAreReadTopDown()
    {
        //[0, 10, 6, 7]: +6 misses the fourth (7), clears the third (6) - grade 3, even though it misses the second (10)
        UpgradeMath.CalculateGrade(
                       [
                           0,
                           10,
                           6,
                           7
                       ],
                       6)
                   .Should()
                   .Be(3);

        UpgradeMath.CalculateGrade(
                       [
                           0,
                           10,
                           6,
                           7
                       ],
                       5)
                   .Should()
                   .Be(1);

        UpgradeMath.CalculateGrade(
                       [
                           0,
                           10,
                           6,
                           7
                       ],
                       7)
                   .Should()
                   .Be(4);
    }

    [Test]
    public void UndeclaredGradesUseTheDefaults()
    {
        var named = GameData.Items.Entries.Values.FirstOrDefault(item
            => (item.UpgradeModifiers is not null || item.CompoundModifiers is not null) && item.Grades is null);

        named.Should()
             .NotBeNull("some upgradeable or compoundable item in the game declares no grades of its own");

        //Accessor rather than Name: the item key is what the datum is filed under, Name is the display string
        UpgradeMath.CalculateGrade(named!.Accessor, 8)
                   .Should()
                   .Be(0);

        UpgradeMath.CalculateGrade(named.Accessor, 9)
                   .Should()
                   .Be(1);

        UpgradeMath.CalculateGrade(named.Accessor, 12)
                   .Should()
                   .Be(4);
    }
}