#region
using AL.Client.Abstractions;
using AL.Client.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the builds that no other build beats on both copies consumed and gold spent, for the item, bench and
///     prices of one planner.
/// </summary>
internal sealed class UpgradeFrontier
{
    private readonly PlannerBase Planner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpgradeFrontier" /> class.
    /// </summary>
    /// <param name="planner">
    ///     The planner whose item, bench and prices every build is planned with.
    /// </param>
    /// <exception cref="System.ArgumentNullException">planner</exception>
    public UpgradeFrontier(PlannerBase planner)
    {
        ArgumentNullException.ThrowIfNull(planner);

        Planner = planner;
    }

    /// <summary>
    ///     Calculates a build's expected cost at each level, at the given copy price.
    /// </summary>
    /// <param name="steps">The build's steps.</param>
    /// <param name="copiesPerAttempt">The copies one attempt consumes.</param>
    /// <param name="copyPrice">The price of one copy.</param>
    /// <param name="scrollPrices">Scroll prices indexed by scroll grade.</param>
    /// <param name="offerings">
    ///     The offerings available. The cheapest prices the deposits.
    /// </param>
    /// <returns>
    ///     The expected gold to own one copy at each level reached, in step order.
    /// </returns>
    internal static IReadOnlyList<double> CalculateExpectedTotals(
        IReadOnlyList<UpgradeBuildStep> steps,
        int copiesPerAttempt,
        double copyPrice,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings)
    {
        var depositPrice = UpgradeMath.TryFindCheapestOffering(offerings, out var cheapestOffering) ? cheapestOffering.Price : 0;
        var expected = copyPrice;
        var totals = new List<double>(steps.Count);

        foreach (var step in steps)
        {
            expected = UpgradeMath.CalculateNextLevelCost(
                expected,
                copiesPerAttempt,
                scrollPrices[step.ScrollGrade],
                GetOfferingPrice(offerings, step.Offering),
                step.Deposits * depositPrice,
                step.Chance);

            totals.Add(expected);
        }

        return totals;
    }

    private static UpgradeBuild CreateBuild(PricedPlan priced)
        => new(
            priced.Copies,
            priced.Gold,
            [
                .. priced.Choices.Select((choice, index) => new UpgradeBuildStep(
                    choice.ScrollGrade,
                    choice.Offering?.Name,
                    choice.Deposits,
                    priced.ItemGrace[index],
                    1 / priced.Attempts[index]))
            ]);

    /// <summary>
    ///     Generates the builds that no other build beats on both copies and gold.
    /// </summary>
    /// <param name="targetLevel">The level every build has to reach.</param>
    /// <param name="copyPrice">
    ///     A copy price whose cheapest build is among the results.
    /// </param>
    /// <param name="startLevel">The level a copy starts at.</param>
    /// <param name="startGrace">The grace each staked copy already carries.</param>
    /// <returns>
    ///     The builds, fewest copies first so gold falls down the list, or none when the target is unreachable.
    /// </returns>
    public IReadOnlyList<UpgradeBuild> Generate(
        int targetLevel,
        double copyPrice = 0,
        int startLevel = 0,
        double startGrace = 0)
    {
        if (!Planner.TryCheckClimb(
                targetLevel,
                startLevel,
                null,
                out _))
            return [];

        return
        [
            .. new PlanSearch(
                    Planner,
                    startLevel,
                    targetLevel,
                    Math.Max(0, startGrace)).GenerateEdge(copyPrice)
                                            .Select(CreateBuild)
        ];
    }

    private static double GetOfferingPrice(IReadOnlyList<OfferingChoice> offerings, string? offering)
    {
        if (offering is null)
            return 0;

        return offerings.First(choice => choice.Name == offering)
                        .Price;
    }
}