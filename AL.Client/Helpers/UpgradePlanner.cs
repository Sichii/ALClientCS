#region
using AL.Client.Abstractions;
using AL.Client.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the cheapest expected-gold upgrade climb for one item, with its thresholds, scroll prices, offerings and
///     slot fixed per instance.
/// </summary>
internal sealed class UpgradePlanner : PlannerBase
{
    private readonly int GradeAtZero;
    private readonly bool LuckySlot;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpgradePlanner" /> class.
    /// </summary>
    /// <param name="thresholds">
    ///     The item's thresholds. The last one ends the track, and an empty track has nothing to plan.
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
    /// <param name="countPity">
    ///     Specifies whether the player's failstacks and the offering pity counter are counted.
    /// </param>
    /// <param name="countServerPity">
    ///     Specifies whether the server's failstacks are counted, starting from none and moved only by these climbs.
    /// </param>
    /// <param name="pityStartLevel">
    ///     The lowest level whose attempts count failstacks and offering pity.
    /// </param>
    /// <exception cref="System.ArgumentNullException">thresholds</exception>
    /// <exception cref="System.ArgumentNullException">scrollPrices</exception>
    /// <exception cref="System.ArgumentNullException">offerings</exception>
    public UpgradePlanner(
        IReadOnlyList<int> thresholds,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        bool luckySlot,
        bool countPity = true,
        bool countServerPity = false,
        int pityStartLevel = 0)
        : base(
            Bench.UPGRADE,
            thresholds,
            scrollPrices,
            offerings,
            countPity,
            countServerPity,
            pityStartLevel)
    {
        GradeAtZero = UpgradeMath.CalculateGrade(thresholds, 0);
        LuckySlot = luckySlot;
    }

    /// <inheritdoc />
    protected override double CalculateChance(
        double baseChance,
        int newLevel,
        int itemGrade,
        int scrollGrade,
        int? offeringGrade,
        double grace,
        double playerFailstacks,
        double serverFailstacks,
        double ograce)
    {
        var chance = UpgradeMath.CalculateUpgradeChance(
                                    baseChance,
                                    newLevel,
                                    itemGrade,
                                    GradeAtZero,
                                    scrollGrade,
                                    offeringGrade,
                                    grace,
                                    playerFailstacks,
                                    serverFailstacks,
                                    ograce)
                                .Chance;

        return LuckySlot ? UpgradeMath.CalculateLuckySlotChance(chance) : chance;
    }

    /// <inheritdoc />
    internal override bool TryGetBaseChance(int level, out double chance)
        => UpgradeMath.TryGetUpgradeBaseChance(Thresholds, level, out chance);
}