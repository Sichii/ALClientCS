#region
using System.Diagnostics.CodeAnalysis;
using AL.Client.Definitions;
using AL.Client.Model;
using AL.Data;
using AL.Data.Compounds;
using AL.Data.Items;
using AL.Data.Upgrades;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the formulas behind <see cref="UpgradeHelper" />: the server's grade, base chance, one attempt's chance,
///     grace and expected cost, asked of thresholds or of an item looked up for its thresholds.
/// </summary>
/// <remarks>
///     Ignores <see cref="GItem.Grade" />, the fixed grade scrolls and offerings carry, as the server's grade rule does.
/// </remarks>
public static class UpgradeMath
{
    /// <summary>
    ///     The thresholds the server uses for an upgradeable or compoundable item that declares none of its own.
    /// </summary>
    public static readonly IReadOnlyList<int> DEFAULT_THRESHOLDS =
    [
        9,
        10,
        11,
        12
    ];

    /// <summary>
    ///     The player's failstacks at a level that add one point of grace to an upgrade.
    /// </summary>
    public const double PLAYER_FAILSTACKS_PER_POINT = 4.5;

    /// <summary>
    ///     The most grace the player's own failstacks can add to an upgrade.
    /// </summary>
    public const double PLAYER_PITY_CAP = 3;

    /// <summary>
    ///     The whole server's failstacks at a level that add one point of grace to an upgrade.
    /// </summary>
    public const double SERVER_FAILSTACKS_PER_POINT = 3.0;

    /// <summary>
    ///     The most grace everyone's failstacks can add to an upgrade.
    /// </summary>
    public const double SERVER_PITY_CAP = 6;

    /// <summary>
    ///     The offering pity counter that adds one point of grace to an upgrade.
    /// </summary>
    public const double OFFERING_PITY_PER_POINT = 3.2;

    /// <summary>
    ///     The chance each point of pooled grace is worth on a compound with no offering.
    /// </summary>
    public const double COMPOUND_GRACE_RATE = 0.007;

    /// <summary>
    ///     The chance each point of pooled grace is worth on a compound with an offering.
    /// </summary>
    public const double COMPOUND_OFFERING_GRACE_RATE = 0.027;

    /// <summary>
    ///     The grace an offering adds to a compound's pooled grace before it is priced.
    /// </summary>
    public const double COMPOUND_OFFERING_GRACE_BONUS = 0.5;

    /// <summary>
    ///     The chance the lucky slot scales the roll down rather than leaving it alone.
    /// </summary>
    public const double LUCKY_BRANCH_CHANCE = 0.6;

    /// <summary>
    ///     The factor the lucky slot multiplies the roll by when it scales it down.
    /// </summary>
    public const double LUCKY_ROLL_SCALE = 0.975;

    /// <summary>
    ///     The amount the lucky slot takes off the roll after scaling it down.
    /// </summary>
    public const double LUCKY_ROLL_OFFSET = 0.012;

    /// <summary>
    ///     The grace one offering used without a scroll banks on an upgrade item, whatever its grade.
    /// </summary>
    public const double DEPOSIT_GRACE = 0.5;

    /// <summary>
    ///     The divisor an upgrade with no offering applies to the item's grace before the low-level penalty comes off.
    /// </summary>
    public const double UPGRADE_PLAIN_GRACE_DIVISOR = 4.8;

    /// <summary>
    ///     The amount a failed upgrade with an offering adds to the player's offering pity counter.
    /// </summary>
    public const double UPGRADE_PITY_FAILURE_GAIN = 0.6;

    /// <summary>
    ///     The grace an offering at the item's own grade banks on an upgrade item.
    /// </summary>
    internal const double UPGRADE_MATCHING_OFFERING_GRACE = 0.4;

    /// <summary>
    ///     The grace an offering at the copies' own grade banks on a compound.
    /// </summary>
    internal const double COMPOUND_MATCHING_OFFERING_GRACE = 0.5;

    /// <summary>
    ///     The highest level an upgrade can reach and still bank grace from a scroll above the item's grade.
    /// </summary>
    internal const int UPGRADE_SCROLL_GRACE_MAX_LEVEL = 10;

    /// <summary>
    ///     The divisor a compound applies to the copies' grace as it merges them into the surviving item.
    /// </summary>
    internal const double COMPOUND_MERGE_GRACE_DIVISOR = 6.4;

    /// <summary>
    ///     Calculates the chance of one compound attempt over three matching copies, with their grace pooled.
    /// </summary>
    /// <param name="baseChance">
    ///     The base chance of the level being reached, from <see cref="TryGetCompoundBaseChance" />.
    /// </param>
    /// <param name="newLevel">The level being reached.</param>
    /// <param name="itemGrade">The copies' grade at the level they are leaving.</param>
    /// <param name="scrollGrade">The scroll's grade.</param>
    /// <param name="offeringGrade">The offering's grade, or null for none.</param>
    /// <param name="pooledGrace">The three copies' grace summed.</param>
    /// <param name="ograce">The player's offering pity counter.</param>
    /// <returns>
    ///     The attempt's chance and the figures it was built from.
    /// </returns>
    public static Breakdown CalculateCompoundChance(
        double baseChance,
        int newLevel,
        int itemGrade,
        int scrollGrade,
        int? offeringGrade,
        double pooledGrace,
        double ograce)
    {
        var probability = baseChance;
        var widening = 0.0;

        if (scrollGrade > itemGrade)
        {
            probability = probability * 1.1 + 0.001;
            widening = scrollGrade - itemGrade;
        }

        double grace;
        double graceAdded;
        double? graceCap = null;

        if (offeringGrade is { } offered)
        {
            grace = COMPOUND_OFFERING_GRACE_RATE * (pooledGrace + COMPOUND_OFFERING_GRACE_BONUS + ograce);

            if (offered > (itemGrade + 1))
            {
                probability *= 1.64;
                graceAdded = grace * 2;
                widening = 1;
            } else if (offered > itemGrade)
            {
                probability *= 1.48;
                graceAdded = grace;
                widening = 1;
            } else if (offered == itemGrade)
            {
                probability *= 1.36;
                graceCap = 30 * COMPOUND_OFFERING_GRACE_RATE;
                graceAdded = Math.Min(graceCap.Value, grace);
            } else if (offered == (itemGrade - 1))
            {
                probability *= 1.15;
                graceCap = 25 * 0.019;
                graceAdded = Math.Min(graceCap.Value, grace) / GetCompoundGraceDivisor(newLevel, itemGrade, offered);
            } else
            {
                probability *= 1.08;
                graceCap = 15 * 0.015;
                graceAdded = Math.Min(graceCap.Value, grace) / GetCompoundGraceDivisor(newLevel, itemGrade, offered);
            }
        } else
        {
            grace = COMPOUND_GRACE_RATE * (pooledGrace + ograce);
            graceCap = 25 * COMPOUND_GRACE_RATE;
            graceAdded = Math.Min(graceCap.Value, grace) / GetCompoundGraceDivisor(newLevel, itemGrade, null);
        }

        probability += graceAdded;

        var multiplierCap = baseChance * (3 + widening * 0.6);
        var flatCap = baseChance + 0.2 + widening * 0.05;
        var chance = Math.Clamp(Math.Min(probability, Math.Min(flatCap, multiplierCap)), 0, 1);

        return new Breakdown(
            chance,
            probability,
            baseChance,
            flatCap,
            multiplierCap,
            widening > 0,
            grace,
            graceAdded,
            graceCap,
            null);
    }

    /// <summary>
    ///     Calculates the copies at +0 it takes to own one at the end of a chain of attempts with nothing else on the bench.
    /// </summary>
    /// <param name="chances">
    ///     The chance of each attempt in the chain, in level order.
    /// </param>
    /// <param name="copiesPerAttempt">
    ///     The copies one attempt consumes, from <see cref="GetCopiesPerAttempt" />.
    /// </param>
    /// <returns>The copies needed.</returns>
    /// <exception cref="System.ArgumentNullException">chances</exception>
    public static double CalculateCopiesNeeded(IEnumerable<double> chances, int copiesPerAttempt)
    {
        ArgumentNullException.ThrowIfNull(chances);

        var copies = 1d;

        foreach (var chance in chances)
            copies = copies * copiesPerAttempt / chance;

        return copies;
    }

    /// <summary>
    ///     Calculates the copies at +0 one copy at each level costs, given each level's chance.
    /// </summary>
    /// <remarks>
    ///     Stops at the first level the chances skip, so nothing past a gap is priced.
    /// </remarks>
    /// <param name="levelChances">
    ///     Each level and its base chance in level order, as <see cref="GetLevelChances(GItem)" /> returns them.
    /// </param>
    /// <param name="copiesPerAttempt">The copies one attempt consumes.</param>
    /// <returns>The copies keyed by level, from +0.</returns>
    internal static IReadOnlyDictionary<int, double> CalculateCopiesPerLevel(
        IReadOnlyList<(int Level, double Chance)> levelChances,
        int copiesPerAttempt)
    {
        var copies = new Dictionary<int, double>
        {
            [0] = 1d
        };

        var running = 1d;
        var next = 1;

        foreach ((var level, var chance) in levelChances)
        {
            if ((level != next) || (chance <= 0))
                break;

            running *= copiesPerAttempt / chance;
            copies[level] = running;
            next++;
        }

        return copies;
    }

    /// <summary>
    ///     Calculates the copies at +0 one copy of the item at each level costs at base chance.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///     The copies keyed by level, from +0 up to the first level with no base chance.
    /// </returns>
    public static IReadOnlyDictionary<int, double> CalculateCopiesPerLevel(GItem? item)
        => CalculateCopiesPerLevel(GetLevelChances(item), GetCopiesPerAttempt(item?.CompoundModifiers is not null));

    /// <summary>
    ///     Calculates a build's expected cost at each level, at the given copy price.
    /// </summary>
    /// <param name="steps">The build's steps.</param>
    /// <param name="compound">
    ///     Specifies whether the item compounds rather than upgrades.
    /// </param>
    /// <param name="copyPrice">The price of one copy.</param>
    /// <param name="scrollPrices">Scroll prices indexed by scroll grade.</param>
    /// <param name="offerings">
    ///     The offerings available. The cheapest prices the deposits.
    /// </param>
    /// <returns>
    ///     The expected gold to own one copy at each level reached, in step order.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">steps</exception>
    /// <exception cref="System.ArgumentNullException">scrollPrices</exception>
    /// <exception cref="System.ArgumentNullException">offerings</exception>
    public static IReadOnlyList<double> CalculateExpectedTotals(
        IReadOnlyList<UpgradeBuildStep> steps,
        bool compound,
        double copyPrice,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings)
    {
        ArgumentNullException.ThrowIfNull(steps);

        ArgumentNullException.ThrowIfNull(scrollPrices);

        ArgumentNullException.ThrowIfNull(offerings);

        return UpgradeFrontier.CalculateExpectedTotals(
            steps,
            GetCopiesPerAttempt(compound),
            copyPrice,
            scrollPrices,
            offerings);
    }

    /// <summary>
    ///     Calculates the grade a level reaches against the given thresholds.
    /// </summary>
    /// <param name="thresholds">
    ///     The item's thresholds, or null for an item with none.
    /// </param>
    /// <param name="level">The item's level.</param>
    /// <returns>
    ///     The grade, 0 through 4, or 0 when there are no thresholds.
    /// </returns>
    public static int CalculateGrade(IReadOnlyList<int>? thresholds, int level)
    {
        const int MAX_GRADE = 4;

        if (thresholds is null)
            return 0;

        //the highest of the first four thresholds the level clears is the grade
        for (var index = Math.Min(thresholds.Count, MAX_GRADE) - 1; index >= 0; index--)
            if (level >= thresholds[index])
                return index + 1;

        return 0;
    }

    /// <summary>
    ///     Calculates the grade of the named item at the given level.
    /// </summary>
    /// <param name="itemName">The item's key.</param>
    /// <param name="level">The item's level.</param>
    /// <returns>
    ///     The grade, 0 through 4, or 0 when the name is unknown or the item has no level.
    /// </returns>
    public static int CalculateGrade(string? itemName, int level)
        => TryGetGradeThresholds(itemName, out var thresholds) ? CalculateGrade(thresholds, level) : 0;

    /// <summary>
    ///     Calculates an upgrade's chance with the lucky slot, which scales the roll down 60% of the time.
    /// </summary>
    /// <param name="chance">The upgrade's chance without the lucky slot.</param>
    /// <returns>The chance with the lucky slot.</returns>
    internal static double CalculateLuckySlotChance(double chance)
        => LUCKY_BRANCH_CHANCE * Math.Min(1, (chance + LUCKY_ROLL_OFFSET) / LUCKY_ROLL_SCALE) + (1 - LUCKY_BRANCH_CHANCE) * chance;

    /// <summary>
    ///     Calculates the grace a compound's surviving item carries out of the merge, before the attempt's own grace lands.
    /// </summary>
    /// <param name="copyGrace">The grace each of the three copies carries.</param>
    /// <param name="withOffering">
    ///     Specifies whether the compound uses an offering, which sums the copies' grace rather than keeping one.
    /// </param>
    /// <returns>The merged grace.</returns>
    internal static double CalculateMergedGrace(double copyGrace, bool withOffering)
        => (withOffering ? CONSTANTS.ITEMS_PER_COMPOUND * copyGrace : copyGrace) / COMPOUND_MERGE_GRACE_DIVISOR;

    /// <summary>
    ///     Calculates the expected cost of a copy one level up, which is what the attempt consumes divided by its chance.
    /// </summary>
    /// <param name="expected">
    ///     The expected cost of one copy at the level being left.
    /// </param>
    /// <param name="copiesPerAttempt">
    ///     The copies one attempt consumes, from <see cref="GetCopiesPerAttempt" />.
    /// </param>
    /// <param name="scrollPrice">The price of the attempt's scroll.</param>
    /// <param name="offeringPrice">
    ///     The price of the attempt's offering, or 0 for none.
    /// </param>
    /// <param name="depositCost">
    ///     The cost of the offerings banked without a scroll before the attempt, or 0 for none.
    /// </param>
    /// <param name="chance">The attempt's chance of success.</param>
    /// <returns>
    ///     The expected cost of one copy at the level reached.
    /// </returns>
    internal static double CalculateNextLevelCost(
        double expected,
        int copiesPerAttempt,
        double scrollPrice,
        double offeringPrice,
        double depositCost,
        double chance)
        => (copiesPerAttempt * expected + scrollPrice + offeringPrice + depositCost) / chance;

    /// <summary>Calculates the chance of one upgrade attempt.</summary>
    /// <remarks>
    ///     The grace the offering banks lands after the roll and is not part of this chance.
    /// </remarks>
    /// <param name="baseChance">
    ///     The base chance of the level being reached, from <see cref="TryGetUpgradeBaseChance" />.
    /// </param>
    /// <param name="newLevel">The level being reached.</param>
    /// <param name="itemGrade">The item's grade at the level it is leaving.</param>
    /// <param name="gradeAtZero">The item's grade at +0.</param>
    /// <param name="scrollGrade">The scroll's grade.</param>
    /// <param name="offeringGrade">The offering's grade, or null for none.</param>
    /// <param name="itemGrace">The grace stored on the item.</param>
    /// <param name="playerPity">The player's failstacks at this level.</param>
    /// <param name="serverPity">Everyone's failstacks at this level.</param>
    /// <param name="ograce">The player's offering pity counter.</param>
    /// <returns>
    ///     The attempt's chance and the figures it was built from.
    /// </returns>
    public static Breakdown CalculateUpgradeChance(
        double baseChance,
        int newLevel,
        int itemGrade,
        int gradeAtZero,
        int scrollGrade,
        int? offeringGrade,
        double itemGrace,
        double playerPity,
        double serverPity,
        double ograce)
    {
        var player = Math.Min(PLAYER_PITY_CAP, playerPity / PLAYER_FAILSTACKS_PER_POINT);
        var server = Math.Min(SERVER_PITY_CAP, serverPity / SERVER_FAILSTACKS_PER_POINT);
        var pity = ograce / OFFERING_PITY_PER_POINT;
        var baseGradeGrace = GetBaseGradeGrace(gradeAtZero);
        var own = itemGrace + player + baseGradeGrace;
        var ownCap = newLevel + 1;

        var graceNumber = Math.Max(0, Math.Min(ownCap, own) + server + pity);
        var grace = baseChance * graceNumber / newLevel + graceNumber / 1000.0;

        var probability = baseChance;
        var widened = false;

        if ((scrollGrade > itemGrade) && (newLevel <= 10))
        {
            probability = probability * 1.2 + 0.01;
            widened = true;
        }

        double graceAdded;

        if (offeringGrade is { } offered)
        {
            if (offered > (itemGrade + 1))
            {
                probability *= 1.7;
                graceAdded = grace * 4;
                widened = true;
            } else if (offered > itemGrade)
            {
                probability *= 1.5;
                graceAdded = grace * 1.2;
                widened = true;
            } else if (offered == itemGrade)
            {
                probability *= 1.4;
                graceAdded = grace;
            } else if (offered == (itemGrade - 1))
            {
                probability *= 1.15;
                graceAdded = grace / 3.2;
            } else
            {
                probability *= 1.08;
                graceAdded = grace / 4;
            }
        } else
            graceAdded = Math.Max(0, grace / UPGRADE_PLAIN_GRACE_DIVISOR - 0.4 / ((newLevel - 0.999) * (newLevel - 0.999)));

        probability += graceAdded;

        var flatCap = baseChance + (widened ? 0.36 : 0.24);
        var multiplierCap = baseChance * (widened ? 3 : 2);
        var chance = Math.Clamp(Math.Min(probability, Math.Min(flatCap, multiplierCap)), 0, 1);

        return new Breakdown(
            chance,
            probability,
            baseChance,
            flatCap,
            multiplierCap,
            widened,
            graceNumber,
            graceAdded,
            null,
            new GraceParts(
                own,
                ownCap,
                player,
                server,
                pity,
                baseGradeGrace));
    }

    /// <summary>
    ///     Gets the grace an upgrade item gets from its grade at +0: a normal item +1, a high one -1, a rare one -2.
    /// </summary>
    /// <param name="gradeAtZero">The item's grade at +0.</param>
    /// <returns>The grace, or 0 for any other grade.</returns>
    internal static double GetBaseGradeGrace(int gradeAtZero)
        => gradeAtZero switch
        {
            0 => 1.0,
            1 => -1.0,
            2 => -2.0,
            _ => 0.0
        };

    /// <summary>
    ///     Gets the chance table row a compound below +3 uses: the grade at +0, except the lost earring, which the server pins
    ///     to row 2.
    /// </summary>
    /// <remarks>
    ///     Only the chance row is pinned; <see cref="CalculateGrade(IReadOnlyList{int}, int)" /> still reads the thresholds.
    /// </remarks>
    /// <param name="itemName">The item's key.</param>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <returns>The chance table row.</returns>
    internal static int GetCompoundBaseRow(string? itemName, IReadOnlyList<int>? thresholds)
    {
        const string PINNED_ITEM = "lostearring";
        const int PINNED_ROW = 2;

        return itemName?.EqualsI(PINNED_ITEM) == true ? PINNED_ROW : CalculateGrade(thresholds, 0);
    }

    /// <summary>
    ///     Gets the chance table row a compound from <paramref name="level" /> uses: the grade two levels below the one it
    ///     leaves.
    /// </summary>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <param name="level">The level the copies are raised from.</param>
    /// <param name="itemName">
    ///     The item's key, for <see cref="GetCompoundBaseRow" />.
    /// </param>
    /// <returns>The chance table row.</returns>
    internal static int GetCompoundChanceRow(IReadOnlyList<int> thresholds, int level, string? itemName = null)
        => level >= 3 ? CalculateGrade(thresholds, level - 2) : GetCompoundBaseRow(itemName, thresholds);

    /// <summary>
    ///     Gets the divisor a compound applies to its capped grace, which grows with the level unless the offering matches or
    ///     outranks the copies.
    /// </summary>
    /// <param name="newLevel">The level being reached.</param>
    /// <param name="itemGrade">The copies' grade at the level they are leaving.</param>
    /// <param name="offeringGrade">The offering's grade, or null for none.</param>
    /// <returns>The divisor, at least 1.</returns>
    public static int GetCompoundGraceDivisor(int newLevel, int itemGrade, int? offeringGrade)
    {
        var level = newLevel - 1;

        return offeringGrade switch
        {
            null                                        => Math.Max(level - 1, 1),
            { } offered when offered >= itemGrade       => 1,
            { } offered when offered == (itemGrade - 1) => Math.Max(level - 2, 1),
            _                                           => Math.Max(level - 1, 1)
        };
    }

    /// <summary>
    ///     Gets every compound level the given thresholds have a base chance for in one chance table.
    /// </summary>
    /// <param name="getChanceFunc">
    ///     The table lookup by row and level, such as <see cref="CompoundsDatum.GetChance" />.
    /// </param>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <param name="itemName">
    ///     The item's key, for <see cref="GetCompoundBaseRow" />.
    /// </param>
    /// <returns>Each level and its base chance in level order.</returns>
    internal static IReadOnlyList<(int Level, double Chance)> GetCompoundLevelChances(
        Func<int, int, double?> getChanceFunc,
        IReadOnlyList<int> thresholds,
        string? itemName = null)
        => GetLevelChances(level => getChanceFunc(GetCompoundChanceRow(thresholds, level, itemName), level + 1), thresholds);

    /// <summary>
    ///     Gets the grace an offering banks on a compound per attempt, by the offering's grade against the copies'.
    /// </summary>
    /// <param name="offeringGrade">The offering's grade.</param>
    /// <param name="itemGrade">The copies' grade at the level they are leaving.</param>
    /// <returns>The banked grace.</returns>
    internal static double GetCompoundOfferingGrace(int offeringGrade, int itemGrade)
        => GetOfferingGrace(offeringGrade, itemGrade, COMPOUND_MATCHING_OFFERING_GRACE);

    /// <summary>
    ///     Gets the grace a scroll above the copies' grade banks on a compound, at any level.
    /// </summary>
    /// <param name="scrollGrade">The scroll's grade.</param>
    /// <param name="itemGrade">The copies' grade at the level they are leaving.</param>
    /// <returns>
    ///     The banked grace, or 0 when the scroll does not outrank the copies.
    /// </returns>
    internal static double GetCompoundScrollGrace(int scrollGrade, int itemGrade)
        => GetScrollGrace(
            scrollGrade,
            itemGrade,
            0,
            int.MaxValue);

    /// <summary>
    ///     Gets the copies one attempt consumes: three compounding, one upgrading.
    /// </summary>
    /// <param name="compound">
    ///     Specifies whether the attempt is a compound rather than an upgrade.
    /// </param>
    /// <returns>The copies consumed.</returns>
    public static int GetCopiesPerAttempt(bool compound) => compound ? CONSTANTS.ITEMS_PER_COMPOUND : 1;

    /// <summary>
    ///     Gets the failstacks a failed upgrade adds to the counters an attempt at a lower level reads, beyond the one each
    ///     failure adds to its own level's counters.
    /// </summary>
    /// <remarks>
    ///     Only the level below gains outside +8 to +15; inside it, the three levels below all gain, and the player's counters
    ///     gain more when the failed attempt spent an offering.
    /// </remarks>
    /// <param name="newLevel">The level the lower attempt reaches.</param>
    /// <param name="levelsAbove">
    ///     How far above <paramref name="newLevel" /> the failed attempt reached, 1 to 3.
    /// </param>
    /// <param name="withOffering">
    ///     Specifies whether the failed attempt spent an offering.
    /// </param>
    /// <returns>
    ///     The failstacks added to the player's and the server's counters.
    /// </returns>
    internal static (int Player, int Server) GetFailstackBump(int newLevel, int levelsAbove, bool withOffering)
    {
        const int FIRST_DEEP_LEVEL = 8;
        const int LAST_DEEP_LEVEL = 15;

        var failedLevel = newLevel + levelsAbove;
        var deep = (failedLevel >= FIRST_DEEP_LEVEL) && (failedLevel <= LAST_DEEP_LEVEL);
        var offering = withOffering ? 1 : 0;

        return levelsAbove switch
        {
            1           => deep ? (2, 2) : (1, 1),
            2 when deep => (2 + offering, 2),
            3 when deep => (2 + 2 * offering, 3 + offering),
            _           => (0, 0)
        };
    }

    /// <summary>
    ///     Gets every level the item has a base chance for, with the chance of reaching it.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    ///     Each level and its base chance in level order, or empty when the item neither upgrades nor compounds.
    /// </returns>
    public static IReadOnlyList<(int Level, double Chance)> GetLevelChances(GItem? item)
    {
        if (item is null || !TryGetGradeThresholds(item, out var thresholds))
            return [];

        return item.CompoundModifiers is not null
            ? GetCompoundLevelChances(GameData.Compounds.GetChance, thresholds, item.Accessor)
            : GetUpgradeLevelChances(GameData.Upgrades.GetChance, thresholds);
    }

    private static IReadOnlyList<(int Level, double Chance)> GetLevelChances(
        Func<int, double?> getBaseChanceFunc,
        IReadOnlyList<int> thresholds)
    {
        var rows = new List<(int Level, double Chance)>();

        if (thresholds.Count == 0)
            return rows;

        for (var level = 0; level < GetMaxLevel(thresholds); level++)
        {
            //the tables have no row past grade 2, so a level with no entry is left out
            if (getBaseChanceFunc(level) is not { } chance)
                continue;

            rows.Add((level + 1, chance));
        }

        return rows;
    }

    /// <summary>
    ///     Gets the highest level an item with the given thresholds can reach, which is its last threshold.
    /// </summary>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <returns>The last threshold.</returns>
    /// <exception cref="System.ArgumentNullException">thresholds</exception>
    public static int GetMaxLevel(IReadOnlyList<int> thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        return thresholds[^1];
    }

    /// <summary>
    ///     Gets the grace an offering banks per attempt, by the offering's grade against the item's.
    /// </summary>
    /// <param name="offeringGrade">The offering's grade.</param>
    /// <param name="itemGrade">The item's grade at the level it is leaving.</param>
    /// <param name="matchingGrace">
    ///     The grace an offering at the item's own grade banks.
    /// </param>
    /// <returns>The banked grace.</returns>
    internal static double GetOfferingGrace(int offeringGrade, int itemGrade, double matchingGrace)
        => offeringGrade > (itemGrade + 1)
            ? 3
            : offeringGrade > itemGrade
                ? 1
                : offeringGrade == itemGrade
                    ? matchingGrace
                    : offeringGrade == (itemGrade - 1)
                        ? 0.2
                        : 0.1;

    /// <summary>
    ///     Gets the grace a scroll above the item's grade banks on it, up to a cutoff level.
    /// </summary>
    /// <param name="scrollGrade">The scroll's grade.</param>
    /// <param name="itemGrade">The item's grade at the level it is leaving.</param>
    /// <param name="newLevel">The level being reached.</param>
    /// <param name="maxLevel">
    ///     The highest level being reached that still banks the grace.
    /// </param>
    /// <returns>
    ///     The banked grace, or 0 when the scroll does not outrank the item or the level is past the cutoff.
    /// </returns>
    internal static double GetScrollGrace(
        int scrollGrade,
        int itemGrade,
        int newLevel,
        int maxLevel)
    {
        const double OVER_GRADE_SCROLL_GRACE = 0.4;

        return (scrollGrade > itemGrade) && (newLevel <= maxLevel) ? OVER_GRADE_SCROLL_GRACE : 0;
    }

    private static int GetUpgradeChanceRow(IReadOnlyList<int> thresholds) => CalculateGrade(thresholds, 0);

    /// <summary>
    ///     Gets every upgrade level the given thresholds have a base chance for in one chance table.
    /// </summary>
    /// <param name="getChanceFunc">
    ///     The table lookup by row and level, such as <see cref="UpgradesDatum.GetChance" />.
    /// </param>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <returns>Each level and its base chance in level order.</returns>
    internal static IReadOnlyList<(int Level, double Chance)> GetUpgradeLevelChances(
        Func<int, int, double?> getChanceFunc,
        IReadOnlyList<int> thresholds)
        => GetLevelChances(level => getChanceFunc(GetUpgradeChanceRow(thresholds), level + 1), thresholds);

    /// <summary>
    ///     Gets the grace an offering banks on an upgrade item per attempt, by the offering's grade against the item's.
    /// </summary>
    /// <param name="offeringGrade">The offering's grade.</param>
    /// <param name="itemGrade">The item's grade at the level it is leaving.</param>
    /// <returns>The banked grace.</returns>
    internal static double GetUpgradeOfferingGrace(int offeringGrade, int itemGrade)
        => GetOfferingGrace(offeringGrade, itemGrade, UPGRADE_MATCHING_OFFERING_GRACE);

    /// <summary>
    ///     Gets the grace a scroll above the item's grade banks on an upgrade item, which stops past
    ///     <see cref="UPGRADE_SCROLL_GRACE_MAX_LEVEL" />.
    /// </summary>
    /// <param name="scrollGrade">The scroll's grade.</param>
    /// <param name="itemGrade">The item's grade at the level it is leaving.</param>
    /// <param name="newLevel">The level being reached.</param>
    /// <returns>
    ///     The banked grace, or 0 when the scroll does not outrank the item or the level is past the cutoff.
    /// </returns>
    internal static double GetUpgradeScrollGrace(int scrollGrade, int itemGrade, int newLevel)
        => GetScrollGrace(
            scrollGrade,
            itemGrade,
            newLevel,
            UPGRADE_SCROLL_GRACE_MAX_LEVEL);

    /// <summary>
    ///     Determines whether the named item has an upgrade level at all. A potion has none.
    /// </summary>
    /// <param name="itemName">The item's key.</param>
    /// <returns>
    ///     <c>true</c> if the item upgrades or compounds; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasLevel(string? itemName) => TryGetGradeThresholds(itemName, out _);

    /// <summary>
    ///     Tries to find the cheapest priced offering, which is what a grace deposit spends.
    /// </summary>
    /// <param name="offerings">The offerings available.</param>
    /// <param name="offering">
    ///     The cheapest offering priced above zero, or null when there is none.
    /// </param>
    /// <returns>
    ///     <c>true</c> if any offering is priced above zero; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">offerings</exception>
    public static bool TryFindCheapestOffering(IReadOnlyList<OfferingChoice> offerings, [MaybeNullWhen(false)] out OfferingChoice offering)
    {
        ArgumentNullException.ThrowIfNull(offerings);

        offering = offerings.Where(candidate => candidate.Price > 0)
                            .MinBy(candidate => candidate.Price);

        if (offering is null)
            return false;

        return true;
    }

    /// <summary>
    ///     Tries to find the first value of one input at which the attempt reaches its cap, past which more of it buys
    ///     nothing.
    /// </summary>
    /// <param name="calculateAttemptFunc">
    ///     Builds the attempt with the input set to a value.
    /// </param>
    /// <param name="min">The lowest value searched.</param>
    /// <param name="max">The highest value searched, inclusive.</param>
    /// <param name="step">The distance between searched values.</param>
    /// <param name="value">
    ///     The first capped value, rounded to <paramref name="step" />, or 0 when there is none.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the attempt caps in the range; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">calculateAttemptFunc</exception>
    public static bool TryFindFirstCappedValue(
        Func<double, Breakdown> calculateAttemptFunc,
        double min,
        double max,
        double step,
        out double value)
    {
        ArgumentNullException.ThrowIfNull(calculateAttemptFunc);

        value = 0;

        if ((step <= 0) || (max < min))
            return false;

        var candidate = min;

        for (; candidate <= (max + 1e-9); candidate += step)
        {
            var breakdown = calculateAttemptFunc(candidate);

            if (breakdown.Uncapped >= (breakdown.Ceiling - 1e-9))
                break;
        }

        if (candidate > (max + 1e-9))
            return false;

        value = Math.Round(candidate / step) * step;

        return true;
    }

    /// <summary>
    ///     Tries to get the base chance of compounding to the level above <paramref name="level" /> with only the required
    ///     scroll.
    /// </summary>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <param name="level">The level the copies are raised from.</param>
    /// <param name="chance">The base chance, or 0 when there is none.</param>
    /// <param name="itemName">
    ///     The item's key, for <see cref="GetCompoundBaseRow" />.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the server's table has an entry; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">thresholds</exception>
    public static bool TryGetCompoundBaseChance(
        IReadOnlyList<int> thresholds,
        int level,
        out double chance,
        string? itemName = null)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        chance = 0;

        if (GameData.Compounds.GetChance(GetCompoundChanceRow(thresholds, level, itemName), level + 1) is not { } baseChance)
            return false;

        chance = baseChance;

        return true;
    }

    /// <summary>
    ///     Tries to get the item's grade thresholds, its own or the defaults.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="thresholds">
    ///     The item's thresholds or the defaults, or null when it has none.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the item upgrades or compounds; otherwise, <c>false</c>, even if it declares thresholds.
    /// </returns>
    public static bool TryGetGradeThresholds(GItem? item, [NotNullWhen(true)] out IReadOnlyList<int>? thresholds)
    {
        thresholds = null;

        if (item is null or { UpgradeModifiers: null, CompoundModifiers: null })
            return false;

        thresholds = item.Grades ?? DEFAULT_THRESHOLDS;

        return true;
    }

    /// <summary>
    ///     Tries to get the named item's grade thresholds, its own or the defaults.
    /// </summary>
    /// <param name="itemName">The item's key.</param>
    /// <param name="thresholds">
    ///     The item's thresholds or the defaults, or null when it has none.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the name is known and the item upgrades or compounds; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetGradeThresholds(string? itemName, [NotNullWhen(true)] out IReadOnlyList<int>? thresholds)
    {
        thresholds = null;

        if (string.IsNullOrEmpty(itemName) || !TryGetGradeThresholds(GameData.Items[itemName], out thresholds))
            return false;

        return true;
    }

    /// <summary>
    ///     Tries to get the highest level the named item can reach.
    /// </summary>
    /// <param name="itemName">The item's key.</param>
    /// <param name="maxLevel">The item's max level, or 0 when there is none.</param>
    /// <returns>
    ///     <c>true</c> if the name is known and the item upgrades or compounds; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetMaxLevel(string? itemName, out int maxLevel)
    {
        maxLevel = 0;

        if (!TryGetGradeThresholds(itemName, out var thresholds) || (thresholds.Count == 0))
            return false;

        maxLevel = GetMaxLevel(thresholds);

        return true;
    }

    /// <summary>
    ///     Tries to get the base chance of upgrading to the level above <paramref name="level" /> with only the required
    ///     scroll.
    /// </summary>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <param name="level">The level the item is raised from.</param>
    /// <param name="chance">The base chance, or 0 when there is none.</param>
    /// <returns>
    ///     <c>true</c> if the server's table has an entry; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">thresholds</exception>
    public static bool TryGetUpgradeBaseChance(IReadOnlyList<int> thresholds, int level, out double chance)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        chance = 0;

        if (GameData.Upgrades.GetChance(GetUpgradeChanceRow(thresholds), level + 1) is not { } baseChance)
            return false;

        chance = baseChance;

        return true;
    }

    /// <summary>
    ///     Represents one attempt's chance and the figures it was built from.
    /// </summary>
    /// <param name="Chance">The chance the server rolls against.</param>
    /// <param name="Uncapped">The chance before the caps.</param>
    /// <param name="Base">The base chance the attempt started from.</param>
    /// <param name="FlatCap">
    ///     The base chance plus a fixed amount, wider when something outranks the item.
    /// </param>
    /// <param name="MultiplierCap">
    ///     A multiple of the base chance, wider when something outranks the item.
    /// </param>
    /// <param name="Widened">
    ///     Whether a scroll or offering above the item's grade widened both caps.
    /// </param>
    /// <param name="GraceNumber">
    ///     The grace the offering scaled: the clamped sum on an upgrade, the pooled grace on a compound.
    /// </param>
    /// <param name="GraceAdded">
    ///     The chance that grace added, after the offering's scaling.
    /// </param>
    /// <param name="GraceCap">
    ///     The cap on a compound's grace where it has one; null on an upgrade.
    /// </param>
    /// <param name="Parts">
    ///     The parts an upgrade's grace was built from; null on a compound.
    /// </param>
    public sealed record Breakdown(
        double Chance,
        double Uncapped,
        double Base,
        double FlatCap,
        double MultiplierCap,
        bool Widened,
        double GraceNumber,
        double GraceAdded,
        double? GraceCap,
        GraceParts? Parts)
    {
        /// <summary>
        ///     Whether the caps took anything off the uncapped chance.
        /// </summary>
        public bool Capped => Uncapped > (Ceiling + 1e-9);

        /// <summary>
        ///     The lower of the two caps, which is the one that binds.
        /// </summary>
        public double Ceiling => Math.Min(FlatCap, MultiplierCap);

        /// <summary>Whether the flat cap is the lower one.</summary>
        public bool IsFlatCapLower => FlatCap <= MultiplierCap;
    }

    /// <summary>
    ///     Represents the parts of an upgrade's grace, each after its own clamp.
    /// </summary>
    /// <param name="Own">
    ///     The item's grace, the player's failstacks and the grace from the item's grade at +0, before the level clamp.
    /// </param>
    /// <param name="OwnCap">
    ///     The level clamp on <paramref name="Own" />: the level being reached plus one.
    /// </param>
    /// <param name="Player">
    ///     The grace from the player's failstacks at this level, clamped at <see cref="PLAYER_PITY_CAP" />.
    /// </param>
    /// <param name="Server">
    ///     The grace from everyone's failstacks at this level, clamped at <see cref="SERVER_PITY_CAP" />.
    /// </param>
    /// <param name="Pity">
    ///     The grace from the offering pity counter, unclamped.
    /// </param>
    /// <param name="BaseGradeGrace">
    ///     The grace from the item's grade at +0, from <see cref="GetBaseGradeGrace" />.
    /// </param>
    public sealed record GraceParts(
        double Own,
        double OwnCap,
        double Player,
        double Server,
        double Pity,
        double BaseGradeGrace);
}