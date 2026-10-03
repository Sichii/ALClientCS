#region
using AL.Client.Abstractions;
using AL.Client.Definitions;
using AL.Client.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the cheapest expected-gold compound climb for one item, with its thresholds, scroll prices and offerings
///     fixed per instance.
/// </summary>
internal sealed class CompoundPlanner : PlannerBase
{
    private readonly string? ItemName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompoundPlanner" /> class.
    /// </summary>
    /// <param name="thresholds">
    ///     The item's thresholds. The last one ends the track, and an empty track has nothing to plan.
    /// </param>
    /// <param name="scrollPrices">
    ///     Scroll prices indexed by scroll grade. A grade priced at zero, or past the end of the list, is skipped.
    /// </param>
    /// <param name="offerings">
    ///     The offerings available. Only those priced above zero are used.
    /// </param>
    /// <param name="itemName">
    ///     The item's key, read only for the grade the server pins - see <see cref="UpgradeMath.GetCompoundBaseRow" />.
    /// </param>
    /// <param name="countPity">
    ///     Specifies whether the offering pity counter is counted. The compound bench keeps no failstacks.
    /// </param>
    /// <exception cref="System.ArgumentNullException">thresholds</exception>
    /// <exception cref="System.ArgumentNullException">scrollPrices</exception>
    /// <exception cref="System.ArgumentNullException">offerings</exception>
    public CompoundPlanner(
        IReadOnlyList<int> thresholds,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        string? itemName = null,
        bool countPity = true)
        : base(
            Bench.COMPOUND,
            thresholds,
            scrollPrices,
            offerings,
            countPity,
            false)
        => ItemName = itemName;

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
        => UpgradeMath.CalculateCompoundChance(
                          baseChance,
                          newLevel,
                          itemGrade,
                          scrollGrade,
                          offeringGrade,
                          CONSTANTS.ITEMS_PER_COMPOUND * grace,
                          ograce)
                      .Chance;

    /// <inheritdoc />
    internal override bool TryGetBaseChance(int level, out double chance)
        => UpgradeMath.TryGetCompoundBaseChance(
            Thresholds,
            level,
            out chance,
            ItemName);
}