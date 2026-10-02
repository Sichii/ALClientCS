#region
using System.Diagnostics.CodeAnalysis;
using AL.Client.Model;
using AL.Data.Items;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides an item's upgrade and compound chance per attempt, its cheapest plan and its frontier.
/// </summary>
public static class UpgradeHelper
{
    /// <summary>
    ///     Tries to calculate the chance of one upgrade attempt on the item.
    /// </summary>
    /// <param name="item">
    ///     The item.
    /// </param>
    /// <param name="level">
    ///     The level the item is raised from.
    /// </param>
    /// <param name="scrollGrade">
    ///     The scroll's grade.
    /// </param>
    /// <param name="offeringGrade">
    ///     The offering's grade, or null for none.
    /// </param>
    /// <param name="itemGrace">
    ///     The grace stored on the item.
    /// </param>
    /// <param name="playerPity">
    ///     The player's failstacks at this level.
    /// </param>
    /// <param name="serverPity">
    ///     Everyone's failstacks at this level.
    /// </param>
    /// <param name="ograce">
    ///     The player's offering pity counter.
    /// </param>
    /// <param name="breakdown">
    ///     The attempt's chance and the figures it was built from, or null when there is none.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the item has a level and the server's table has a base chance for the attempt; otherwise,
    ///     <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     item
    /// </exception>
    public static bool TryCalculateUpgradeChance(
        GItem item,
        int level,
        int scrollGrade,
        int? offeringGrade,
        double itemGrace,
        double playerPity,
        double serverPity,
        double ograce,
        [NotNullWhen(true)] out UpgradeMath.Breakdown? breakdown)
    {
        ArgumentNullException.ThrowIfNull(item);

        breakdown = null;

        if (!UpgradeMath.TryGetGradeThresholds(item, out var thresholds)
            || !UpgradeMath.TryGetUpgradeBaseChance(thresholds, level, out var baseChance))
            return false;

        breakdown = UpgradeMath.CalculateUpgradeChance(
            baseChance,
            level + 1,
            UpgradeMath.CalculateGrade(thresholds, level),
            UpgradeMath.CalculateGrade(thresholds, 0),
            scrollGrade,
            offeringGrade,
            itemGrace,
            playerPity,
            serverPity,
            ograce);

        return true;
    }

    /// <summary>
    ///     Tries to calculate the chance of one compound attempt over three copies of the item, with their grace pooled.
    /// </summary>
    /// <param name="item">
    ///     The item.
    /// </param>
    /// <param name="level">
    ///     The level the copies are raised from.
    /// </param>
    /// <param name="scrollGrade">
    ///     The scroll's grade.
    /// </param>
    /// <param name="offeringGrade">
    ///     The offering's grade, or null for none.
    /// </param>
    /// <param name="pooledGrace">
    ///     The three copies' grace summed.
    /// </param>
    /// <param name="ograce">
    ///     The player's offering pity counter.
    /// </param>
    /// <param name="breakdown">
    ///     The attempt's chance and the figures it was built from, or null when there is none.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the item has a level and the server's table has a base chance for the attempt; otherwise,
    ///     <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     item
    /// </exception>
    public static bool TryCalculateCompoundChance(
        GItem item,
        int level,
        int scrollGrade,
        int? offeringGrade,
        double pooledGrace,
        double ograce,
        [NotNullWhen(true)] out UpgradeMath.Breakdown? breakdown)
    {
        ArgumentNullException.ThrowIfNull(item);

        breakdown = null;

        if (!UpgradeMath.TryGetGradeThresholds(item, out var thresholds)
            || !UpgradeMath.TryGetCompoundBaseChance(
                thresholds,
                level,
                out var baseChance,
                item.Accessor))
            return false;

        breakdown = UpgradeMath.CalculateCompoundChance(
            baseChance,
            level + 1,
            UpgradeMath.CalculateGrade(thresholds, level),
            scrollGrade,
            offeringGrade,
            pooledGrace,
            ograce);

        return true;
    }

    /// <summary>
    ///     Finds the cheapest expected upgrade climb for the item from <paramref name="startLevel" /> to
    ///     <paramref name="targetLevel" />.
    /// </summary>
    /// <remarks>
    ///     The cost is an average under averaged pity, not a guarantee: the 90th-percentile climb runs about triple the
    ///     median.
    /// </remarks>
    /// <param name="item">
    ///     The item. One with no level has nothing to plan.
    /// </param>
    /// <param name="targetLevel">
    ///     The level to climb to.
    /// </param>
    /// <param name="itemCost">
    ///     The price of one more copy at <paramref name="startLevel" />.
    /// </param>
    /// <param name="scrollPrices">
    ///     Scroll prices indexed by scroll grade. A grade priced at zero, or past the end of the list, is skipped.
    /// </param>
    /// <param name="offerings">
    ///     The offerings available. Only those priced above zero are used, and the cheapest prices the grace deposits.
    /// </param>
    /// <param name="luckySlot">
    ///     Specifies whether the attempts are made in the lucky slot.
    /// </param>
    /// <param name="startLevel">
    ///     The level the climb starts from, below <paramref name="targetLevel" />.
    /// </param>
    /// <param name="startGrace">
    ///     The grace already on the starting copy. A negative figure is read as none.
    /// </param>
    /// <param name="forced">
    ///     A plan to price instead of searching for one, one step per level from +0, or null to search.
    /// </param>
    /// <param name="countPity">
    ///     Specifies whether failure pity and offering pity are counted.
    /// </param>
    /// <returns>
    ///     The plan, or an empty one with <see cref="UpgradePlan.Unreachable" /> set when the climb cannot be planned.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     item
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     scrollPrices
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     offerings
    /// </exception>
    public static UpgradePlan FindCheapestUpgradePlan(
        GItem item,
        int targetLevel,
        double itemCost,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        bool luckySlot,
        int startLevel = 0,
        double startGrace = 0,
        IReadOnlyList<ForcedStep>? forced = null,
        bool countPity = true)
    {
        ArgumentNullException.ThrowIfNull(item);

        ArgumentNullException.ThrowIfNull(scrollPrices);

        ArgumentNullException.ThrowIfNull(offerings);

        var planner = new UpgradePlanner(
            GetPlanThresholds(item),
            scrollPrices,
            offerings,
            luckySlot,
            countPity);

        return planner.FindCheapestPlan(
            targetLevel,
            itemCost,
            startLevel,
            startGrace,
            forced);
    }

    /// <summary>
    ///     Finds the cheapest expected compound climb for the item from <paramref name="startLevel" /> to
    ///     <paramref name="targetLevel" />.
    /// </summary>
    /// <param name="item">
    ///     The item. One with no level has nothing to plan.
    /// </param>
    /// <param name="targetLevel">
    ///     The level to climb to.
    /// </param>
    /// <param name="itemCost">
    ///     The price of one copy at <paramref name="startLevel" />.
    /// </param>
    /// <param name="scrollPrices">
    ///     Scroll prices indexed by scroll grade. A grade priced at zero, or past the end of the list, is skipped.
    /// </param>
    /// <param name="offerings">
    ///     The offerings available. Only those priced above zero are used.
    /// </param>
    /// <param name="startLevel">
    ///     The level the climb starts from, below <paramref name="targetLevel" />.
    /// </param>
    /// <param name="startGrace">
    ///     The grace each staked copy already carries. A negative figure is read as none.
    /// </param>
    /// <param name="forced">
    ///     A plan to price instead of searching for one, one step per level from +0, or null to search. Its deposits are
    ///     ignored, since the compound bench takes none.
    /// </param>
    /// <param name="countPity">
    ///     Specifies whether failure pity and offering pity are counted.
    /// </param>
    /// <returns>
    ///     The plan, or an empty one with <see cref="UpgradePlan.Unreachable" /> set when the climb cannot be planned.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     item
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     scrollPrices
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     offerings
    /// </exception>
    public static UpgradePlan FindCheapestCompoundPlan(
        GItem item,
        int targetLevel,
        double itemCost,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        int startLevel = 0,
        double startGrace = 0,
        IReadOnlyList<ForcedStep>? forced = null,
        bool countPity = true)
    {
        ArgumentNullException.ThrowIfNull(item);

        ArgumentNullException.ThrowIfNull(scrollPrices);

        ArgumentNullException.ThrowIfNull(offerings);

        var planner = new CompoundPlanner(
            GetPlanThresholds(item),
            scrollPrices,
            offerings,
            item.Accessor,
            countPity);

        return planner.FindCheapestPlan(
            targetLevel,
            itemCost,
            startLevel,
            startGrace,
            forced);
    }

    /// <summary>
    ///     Generates the item's upgrade frontier: the builds that no other build beats on both copies consumed and gold spent.
    /// </summary>
    /// <remarks>
    ///     The builds come from rerunning the planner across copy prices, so a build that wins only under a hard cap on copies
    ///     is never found.
    /// </remarks>
    /// <param name="item">
    ///     The item.
    /// </param>
    /// <param name="targetLevel">
    ///     The level every build has to reach.
    /// </param>
    /// <param name="scrollPrices">
    ///     Scroll prices indexed by scroll grade.
    /// </param>
    /// <param name="offerings">
    ///     The offerings available.
    /// </param>
    /// <param name="luckySlot">
    ///     Specifies whether the attempts are made in the lucky slot.
    /// </param>
    /// <param name="copyPrice">
    ///     An extra copy price to plan at, so the build priced at it is among the results.
    /// </param>
    /// <param name="startLevel">
    ///     The level a copy starts at.
    /// </param>
    /// <param name="startGrace">
    ///     The grace each staked copy already carries.
    /// </param>
    /// <param name="countPity">
    ///     Specifies whether failure pity and offering pity are counted.
    /// </param>
    /// <returns>
    ///     The builds, fewest copies first so gold falls down the list, or none when the target is unreachable.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     item
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     scrollPrices
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     offerings
    /// </exception>
    public static IReadOnlyList<UpgradeBuild> GenerateUpgradeFrontier(
        GItem item,
        int targetLevel,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        bool luckySlot,
        double copyPrice = 0,
        int startLevel = 0,
        double startGrace = 0,
        bool countPity = true)
    {
        ArgumentNullException.ThrowIfNull(item);

        ArgumentNullException.ThrowIfNull(scrollPrices);

        ArgumentNullException.ThrowIfNull(offerings);

        var planner = new UpgradePlanner(
            GetPlanThresholds(item),
            scrollPrices,
            offerings,
            luckySlot,
            countPity);

        return new UpgradeFrontier(planner).Generate(
            targetLevel,
            copyPrice,
            startLevel,
            startGrace);
    }

    /// <summary>
    ///     Generates the item's compound frontier: the builds that no other build beats on both copies consumed and gold
    ///     spent.
    /// </summary>
    /// <param name="item">
    ///     The item.
    /// </param>
    /// <param name="targetLevel">
    ///     The level every build has to reach.
    /// </param>
    /// <param name="scrollPrices">
    ///     Scroll prices indexed by scroll grade.
    /// </param>
    /// <param name="offerings">
    ///     The offerings available.
    /// </param>
    /// <param name="copyPrice">
    ///     An extra copy price to plan at, so the build priced at it is among the results.
    /// </param>
    /// <param name="startLevel">
    ///     The level a copy starts at.
    /// </param>
    /// <param name="startGrace">
    ///     The grace each staked copy already carries.
    /// </param>
    /// <param name="countPity">
    ///     Specifies whether failure pity and offering pity are counted.
    /// </param>
    /// <returns>
    ///     The builds, fewest copies first so gold falls down the list, or none when the target is unreachable.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     item
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     scrollPrices
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     offerings
    /// </exception>
    public static IReadOnlyList<UpgradeBuild> GenerateCompoundFrontier(
        GItem item,
        int targetLevel,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        double copyPrice = 0,
        int startLevel = 0,
        double startGrace = 0,
        bool countPity = true)
    {
        ArgumentNullException.ThrowIfNull(item);

        ArgumentNullException.ThrowIfNull(scrollPrices);

        ArgumentNullException.ThrowIfNull(offerings);

        var planner = new CompoundPlanner(
            GetPlanThresholds(item),
            scrollPrices,
            offerings,
            item.Accessor,
            countPity);

        return new UpgradeFrontier(planner).Generate(
            targetLevel,
            copyPrice,
            startLevel,
            startGrace);
    }

    /// <summary>
    ///     Gets the thresholds a planner climbs against, or an empty track for an item with no level, which the planner
    ///     reports as unreachable.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The item's thresholds, or an empty list.</returns>
    private static IReadOnlyList<int> GetPlanThresholds(GItem item)
        => UpgradeMath.TryGetGradeThresholds(item, out var thresholds) ? thresholds : [];
}