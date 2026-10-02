#region
using System.Diagnostics.CodeAnalysis;
using AL.Client.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the builds that no other build beats on both copies consumed and gold spent, for the item, bench and
///     prices of one planner.
/// </summary>
/// <remarks>
///     The builds come from rerunning the planner across copy prices, so a build that wins only under a hard cap on copies
///     is never found.
/// </remarks>
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

    private UpgradeBuild CreateBuild(IReadOnlyList<UpgradeBuildStep> steps)
        => new(
            UpgradeMath.CalculateCopiesNeeded(steps.Select(step => step.Chance), Planner.Bench.CopiesPerAttempt),
            CalculateExpectedTotals(
                steps,
                Planner.Bench.CopiesPerAttempt,
                0,
                Planner.ScrollPrices,
                Planner.Offerings)[^1],
            steps);

    /// <summary>
    ///     Generates the builds that no other build beats on both copies and gold.
    /// </summary>
    /// <param name="targetLevel">The level every build has to reach.</param>
    /// <param name="copyPrice">
    ///     An extra copy price to plan at, so the build priced at it is among the results.
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
        var buildsBySteps = new Dictionary<string, UpgradeBuild>(StringComparer.Ordinal);

        foreach (var price in GetCopyPrices(copyPrice))
        {
            var plan = Planner.FindCheapestPlan(
                targetLevel,
                price,
                startLevel,
                startGrace);

            //unreachable at one price is unreachable at every price
            if (!TryGetPlannedSteps(plan, out var steps))
                return [];

            var key = string.Join('|', steps.Select(step => $"{step.ScrollGrade}:{step.Offering}:{step.Deposits}"));

            if (!buildsBySteps.ContainsKey(key))
                buildsBySteps[key] = CreateBuild(steps);
        }

        //keep a build only if it costs less gold than every build with fewer copies
        var builds = new List<UpgradeBuild>();

        foreach (var build in buildsBySteps.Values
                                           .OrderBy(build => build.Copies)
                                           .ThenBy(build => build.Gold))
            if ((builds.Count == 0) || (build.Gold < builds[^1].Gold))
                builds.Add(build);

        return builds;
    }

    private static IEnumerable<double> GetCopyPrices(double copyPrice)
    {
        //finer steps find no new plans, since a plan changes only where an offering's price crosses what it saves
        yield return 0;
        yield return copyPrice;

        for (var decade = 100d; decade <= 1e10; decade *= 10)
        {
            yield return decade;
            yield return decade * 2;
            yield return decade * 5;
        }
    }

    private static double GetOfferingPrice(IReadOnlyList<OfferingChoice> offerings, string? offering)
    {
        if (offering is null)
            return 0;

        return offerings.First(choice => choice.Name == offering)
                        .Price;
    }

    private static bool TryGetPlannedSteps(UpgradePlan plan, [MaybeNullWhen(false)] out IReadOnlyList<UpgradeBuildStep> steps)
    {
        steps = null;

        if (plan.Unreachable is not null)
            return false;

        steps =
        [
            .. plan.Steps.Select(step => new UpgradeBuildStep(
                step.ScrollGrade,
                step.Offering,
                step.Deposits,
                step.ItemGrace,
                step.Chance))
        ];

        return true;
    }
}