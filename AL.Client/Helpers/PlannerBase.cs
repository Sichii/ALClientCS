#region
using AL.Client.Definitions;
using AL.Client.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the cheapest expected-gold climb to a target level on one bench, for an item, scroll prices and offerings
///     fixed per instance.
/// </summary>
/// <remarks>
///     The cost is an average under averaged pity, not a guarantee: the 90th-percentile climb runs about triple the
///     median.
/// </remarks>
internal abstract class PlannerBase
{
    private readonly OfferingChoice? DepositOffering;

    /// <summary>
    ///     The values that set this planner's bench apart from the other.
    /// </summary>
    public Bench Bench { get; }

    /// <summary>
    ///     The offerings available. Only those priced above zero are used.
    /// </summary>
    public IReadOnlyList<OfferingChoice> Offerings { get; }

    /// <summary>
    ///     Scroll prices indexed by scroll grade. A grade priced at zero, or past the end of the list, is skipped.
    /// </summary>
    public IReadOnlyList<double> ScrollPrices { get; }

    /// <summary>
    ///     The item's thresholds. The last one ends the track, and an empty track has nothing to plan.
    /// </summary>
    protected IReadOnlyList<int> Thresholds { get; }

    /// <summary>
    ///     The passes spent settling failure pity and offering pity. One pass counts neither, since each starts at zero.
    /// </summary>
    protected int PityPasses { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlannerBase" /> class.
    /// </summary>
    /// <param name="bench">
    ///     The bench the climb is made on.
    /// </param>
    /// <param name="thresholds">
    ///     The item's thresholds.
    /// </param>
    /// <param name="scrollPrices">
    ///     Scroll prices indexed by scroll grade.
    /// </param>
    /// <param name="offerings">
    ///     The offerings available.
    /// </param>
    /// <param name="countPity">
    ///     Specifies whether failure pity and offering pity are counted.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     thresholds
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     scrollPrices
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     offerings
    /// </exception>
    protected PlannerBase(
        Bench bench,
        IReadOnlyList<int> thresholds,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        bool countPity)
    {
        const int PITY_PASSES = 4;

        ArgumentNullException.ThrowIfNull(thresholds);

        ArgumentNullException.ThrowIfNull(scrollPrices);

        ArgumentNullException.ThrowIfNull(offerings);

        Bench = bench;
        Thresholds = thresholds;
        ScrollPrices = scrollPrices;
        Offerings = offerings;
        PityPasses = countPity ? PITY_PASSES : 1;
        DepositOffering = bench.TakesDeposits && UpgradeMath.TryFindCheapestOffering(offerings, out var cheapest) ? cheapest : null;
    }

    /// <summary>
    ///     Finds the cheapest expected climb from <paramref name="startLevel" /> to <paramref name="targetLevel" />.
    /// </summary>
    /// <remarks>
    ///     Every copy is assumed to carry <paramref name="startGrace" />, which is slightly optimistic when the graced copy is
    ///     one of a kind.
    /// </remarks>
    /// <param name="targetLevel">
    ///     The level to climb to.
    /// </param>
    /// <param name="copyPrice">
    ///     The price of one copy at <paramref name="startLevel" />.
    /// </param>
    /// <param name="startLevel">
    ///     The level the climb starts from, below <paramref name="targetLevel" />.
    /// </param>
    /// <param name="startGrace">
    ///     The grace each starting copy already carries. A negative figure is read as none.
    /// </param>
    /// <param name="forced">
    ///     A plan to price instead of searching for one, one step per level from +0, or null to search. Its deposits are
    ///     ignored on a bench that takes none.
    /// </param>
    /// <returns>
    ///     The plan, or an empty one with <see cref="UpgradePlan.Unreachable" /> set when the climb cannot be planned.
    /// </returns>
    public UpgradePlan FindCheapestPlan(
        int targetLevel,
        double copyPrice,
        int startLevel = 0,
        double startGrace = 0,
        IReadOnlyList<ForcedStep>? forced = null)
    {
        if (targetLevel < 1)
            return new UpgradePlan([], 0, "Nothing to plan below +1.");

        if (forced is not null && (forced.Count != targetLevel))
            return new UpgradePlan([], 0, $"The plan code covers {forced.Count} levels, not the {targetLevel} this climb needs.");

        if ((startLevel < 0) || (startLevel >= targetLevel))
            return new UpgradePlan([], 0, "The starting level must sit below the target.");

        if (Thresholds.Count == 0)
            return new UpgradePlan([], 0, $"The item has no {Bench.Name} track.");

        if (targetLevel > UpgradeMath.GetMaxLevel(Thresholds))
            return new UpgradePlan([], 0, $"The track ends at +{UpgradeMath.GetMaxLevel(Thresholds)}.");

        if (!Bench.TakesDeposits)
            forced = forced?.Select(step => step with
                           {
                               Deposits = 0
                           })
                           .ToList();

        var levels = targetLevel - startLevel;
        var steps = new List<UpgradePlanStep>(levels);

        //the offering pity entering each level, re-solved against the plan's own failures until it stops moving
        var ograce = new double[levels];
        var chances = new double[levels];
        var withOffering = new bool[levels];

        var baseline = copyPrice;

        for (var pass = 0; pass < PityPasses; pass++)
        {
            steps.Clear();

            baseline = copyPrice;
            var baselineGrace = Math.Max(0, startGrace);

            //one open plan per grace an earlier level could have banked
            var open = new List<PartialPlan>
            {
                new(
                    null,
                    0,
                    null,
                    0,
                    0,
                    copyPrice,
                    Math.Max(0, startGrace))
            };

            for (var level = startLevel; level < targetLevel; level++)
            {
                var newLevel = level + 1;

                if (!TryGetBaseChance(level, out var baseChance))
                    return new UpgradePlan(
                        [],
                        0,
                        $"No base figure exists for +{newLevel} at this item's grade - the server's table stops there.");

                var itemGrade = UpgradeMath.CalculateGrade(Thresholds, level);

                var reached = FindReachedPlans(
                    open,
                    level,
                    baseChance,
                    itemGrade,
                    ograce[level - startLevel],
                    forced?[level]);

                if (reached.Count == 0)
                    return new UpgradePlan([], 0, $"No priced scroll covers grade {itemGrade} at +{level}.");

                open = FindUnbeatenPlans(reached.Values);

                //the baseline uses the matching scroll alone and never an offering, so it banks no offering pity
                var baselineChance = CalculateChance(
                    baseChance,
                    newLevel,
                    itemGrade,
                    itemGrade,
                    null,
                    baselineGrace,
                    0);

                baseline = UpgradeMath.CalculateNextLevelCost(
                    baseline,
                    Bench.CopiesPerAttempt,
                    ScrollPrices[itemGrade],
                    0,
                    0,
                    baselineChance);

                baselineGrace /= Bench.GraceMergeDivisor;
            }

            var cheapest = open.MinBy(plan => plan.Expected)!;
            var walked = new PartialPlan[levels];
            var cursor = levels;

            for (var node = cheapest; node.Parent is not null; node = node.Parent)
                walked[--cursor] = node;

            for (var index = 0; index < levels; index++)
            {
                var node = walked[index];

                chances[index] = node.Chance;
                withOffering[index] = node.Offering is not null;

                steps.Add(
                    new UpgradePlanStep(
                        startLevel + index,
                        node.Scroll,
                        node.Offering?.Name,
                        node.Deposits,
                        node.Parent!.Grace + UpgradeMath.DEPOSIT_GRACE * node.Deposits,
                        node.Chance,
                        node.Expected));
            }

            if (!Bench.UpdateOfferingPity(
                    ograce,
                    chances,
                    withOffering,
                    startLevel))
                break;
        }

        return new UpgradePlan(steps, baseline, null);
    }

    /// <summary>
    ///     Tries to get an attempt's base chance from the level it leaves.
    /// </summary>
    /// <param name="level">
    ///     The level the attempt leaves.
    /// </param>
    /// <param name="chance">
    ///     The base chance.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the server's table has a figure for the level; otherwise, <c>false</c>.
    /// </returns>
    protected abstract bool TryGetBaseChance(int level, out double chance);

    /// <summary>
    ///     Calculates one attempt's chance on this bench, from the grace each staked copy carries.
    /// </summary>
    /// <param name="baseChance">
    ///     The attempt's base chance.
    /// </param>
    /// <param name="newLevel">
    ///     The level being reached.
    /// </param>
    /// <param name="itemGrade">
    ///     The item's grade at the level it leaves.
    /// </param>
    /// <param name="scrollGrade">
    ///     The scroll's grade.
    /// </param>
    /// <param name="offeringGrade">
    ///     The offering's grade, or null for none.
    /// </param>
    /// <param name="grace">
    ///     The grace each staked copy carries.
    /// </param>
    /// <param name="ograce">
    ///     The offering pity counter entering the attempt.
    /// </param>
    /// <returns>
    ///     The attempt's chance.
    /// </returns>
    protected abstract double CalculateChance(
        double baseChance,
        int newLevel,
        int itemGrade,
        int scrollGrade,
        int? offeringGrade,
        double grace,
        double ograce);

    private Dictionary<long, PartialPlan> FindReachedPlans(
        List<PartialPlan> open,
        int level,
        double baseChance,
        int itemGrade,
        double ograce,
        ForcedStep? forcedStep)
    {
        var newLevel = level + 1;

        //a forced level is a search over one candidate
        var scrollChoices = forcedStep is not null ? [forcedStep.ScrollGrade] : Bench.ScrollGrades[itemGrade];

        List<OfferingChoice?> offeringChoices;

        if (forcedStep is not null)
            offeringChoices = [forcedStep.Offering is { } name ? Offerings.FirstOrDefault(candidate => candidate.Name == name) : null];
        else
        {
            offeringChoices = [null];
            offeringChoices.AddRange(Offerings.Where(candidate => candidate.Price > 0));
        }

        //a forced step may name its own deposit offering
        var depositPrice = (forcedStep is { DepositOffering: { } named }
                               ? Offerings.FirstOrDefault(candidate => candidate.Name == named)
                               : DepositOffering)?.Price
                           ?? 0;

        //one roll per scroll, offering and staked grace, shared by every open plan bringing that grace
        var rolls = new Dictionary<(int Scroll, int Offering, long Grace), double>();
        var reached = new Dictionary<long, PartialPlan>();

        foreach (var plan in open)
        {
            var depositFloor = forcedStep?.Deposits ?? 0;

            //grace past newLevel + 3 buys nothing
            var depositCeiling = forcedStep is not null
                ? forcedStep.Deposits
                : DepositOffering is null
                    ? 0
                    : (int)Math.Ceiling(Math.Max(0, newLevel + 3 - plan.Grace) / UpgradeMath.DEPOSIT_GRACE);

            foreach (var scroll in scrollChoices)
            {
                if ((scroll >= ScrollPrices.Count) || (ScrollPrices[scroll] <= 0))
                    continue;

                foreach (var offering in offeringChoices)
                    for (var deposits = depositFloor; deposits <= depositCeiling; deposits++)
                    {
                        var staked = plan.Grace + UpgradeMath.DEPOSIT_GRACE * deposits;
                        var roll = (scroll, offering?.Grade ?? -1, RoundGrace(staked));

                        if (!rolls.TryGetValue(roll, out var chance))
                            rolls[roll] = chance = CalculateChance(
                                baseChance,
                                newLevel,
                                itemGrade,
                                scroll,
                                offering?.Grade,
                                staked,
                                ograce);

                        if (chance <= 0)
                            continue;

                        var cost = UpgradeMath.CalculateNextLevelCost(
                            plan.Expected,
                            Bench.CopiesPerAttempt,
                            ScrollPrices[scroll],
                            offering?.Price ?? 0,
                            deposits * depositPrice,
                            chance);

                        //the grace the scroll and offering bank lands after the roll, so only a success carries it
                        var carried = (offering is not null ? Bench.CopiesPerAttempt * staked : staked) / Bench.GraceMergeDivisor
                                      + UpgradeMath.GetScrollGrace(
                                          scroll,
                                          itemGrade,
                                          newLevel,
                                          Bench.ScrollGraceMaxLevel)
                                      + (offering is { } used
                                          ? UpgradeMath.GetOfferingGrace(used.Grade, itemGrade, Bench.MatchingOfferingGrace)
                                          : 0);

                        var key = RoundGrace(carried);

                        if (!reached.TryGetValue(key, out var incumbent) || (cost < incumbent.Expected))
                            reached[key] = new PartialPlan(
                                plan,
                                scroll,
                                offering,
                                deposits,
                                chance,
                                cost,
                                carried);
                    }
            }
        }

        return reached;
    }

    private static List<PartialPlan> FindUnbeatenPlans(IEnumerable<PartialPlan> reached)
    {
        var kept = new List<PartialPlan>();
        var cheapest = double.MaxValue;

        //a plan beaten on grace and on gold at once can never come back
        foreach (var plan in reached.OrderByDescending(plan => plan.Grace)
                                    .ThenBy(plan => plan.Expected))
            if (plan.Expected < cheapest)
            {
                kept.Add(plan);
                cheapest = plan.Expected;
            }

        return kept;
    }

    /// <summary>
    ///     Rounds grace to a millionth, so two plans that banked the same grace by different routes share one key.
    /// </summary>
    /// <param name="grace">
    ///     The grace.
    /// </param>
    /// <returns>
    ///     The grace in millionths.
    /// </returns>
    private static long RoundGrace(double grace) => (long)Math.Round(grace * 1_000_000);

    /// <summary>
    ///     Represents a plan that has reached one level: its last attempt, the expected cost of one copy by then, and the
    ///     grace that copy carries on.
    /// </summary>
    private sealed record PartialPlan(
        PartialPlan? Parent,
        int Scroll,
        OfferingChoice? Offering,
        int Deposits,
        double Chance,
        double Expected,
        double Grace);
}

/// <summary>
///     Represents the values that set one bench apart from the other, with one instance per bench.
/// </summary>
/// <param name="Name">
///     The bench's name, as an unplannable climb reports it.
/// </param>
/// <param name="ScrollGrades">
///     The scroll grades worth trying, indexed by item grade.
/// </param>
/// <param name="CopiesPerAttempt">
///     The copies one attempt consumes.
/// </param>
/// <param name="TakesDeposits">
///     Whether offerings can be used without a scroll before an attempt to bank grace.
/// </param>
/// <param name="GraceMergeDivisor">
///     The divisor the staked grace takes as it carries into the item reached.
/// </param>
/// <param name="ScrollGraceMaxLevel">
///     The highest level being reached that still banks grace from a scroll above the item's grade.
/// </param>
/// <param name="MatchingOfferingGrace">
///     The grace an offering at the item's own grade banks.
/// </param>
/// <param name="PityFailureGain">
///     The amount a failed attempt with an offering adds to the offering pity counter.
/// </param>
/// <param name="PityOfferingDecay">
///     The share of the counter a success with an offering leaves.
/// </param>
/// <param name="PityPlainDecayPerLevel">
///     The share of the counter a success without an offering takes, per level reached.
/// </param>
internal sealed record Bench(
    string Name,
    int[][] ScrollGrades,
    int CopiesPerAttempt,
    bool TakesDeposits,
    double GraceMergeDivisor,
    int ScrollGraceMaxLevel,
    double MatchingOfferingGrace,
    double PityFailureGain,
    double PityOfferingDecay,
    double PityPlainDecayPerLevel)
{
    /// <summary>
    ///     The upgrade bench. By item grade it tries the matching scroll and one above it, since further above costs more for
    ///     the same bonus.
    /// </summary>
    internal static readonly Bench UPGRADE = new(
        "upgrade",
        [
            [
                0,
                1
            ],
            [
                1,
                2
            ],
            [
                2,
                3
            ],
            [3],
            [4]
        ],
        1,
        true,
        1,
        UpgradeMath.UPGRADE_SCROLL_GRACE_MAX_LEVEL,
        UpgradeMath.UPGRADE_MATCHING_OFFERING_GRACE,
        UpgradeMath.UPGRADE_PITY_FAILURE_GAIN,
        0.25,
        0.005);

    /// <summary>
    ///     The compound bench. By item grade it tries every scroll from the matching one up, since the compound cap widens
    ///     with how far over it is.
    /// </summary>
    internal static readonly Bench COMPOUND = new(
        "compound",
        [
            [
                0,
                1,
                2,
                3
            ],
            [
                1,
                2,
                3
            ],
            [
                2,
                3
            ],
            [3],
            []
        ],
        CONSTANTS.ITEMS_PER_COMPOUND,
        false,
        UpgradeMath.COMPOUND_MERGE_GRACE_DIVISOR,
        int.MaxValue,
        UpgradeMath.COMPOUND_MATCHING_OFFERING_GRACE,
        0.4,
        0,
        0.02);

    /// <summary>
    ///     Updates <paramref name="ograce" /> to the average offering pity counter the plan leaves entering each level.
    /// </summary>
    /// <param name="ograce">
    ///     The counter entering each level, overwritten in place. Its length sets the climb's.
    /// </param>
    /// <param name="chances">
    ///     The plan's chance at each level.
    /// </param>
    /// <param name="withOffering">
    ///     Whether the plan spends an offering at each level.
    /// </param>
    /// <param name="startLevel">
    ///     The level the climb restarts from after a failure.
    /// </param>
    /// <returns>
    ///     <c>true</c> if any level's counter moved by more than a millionth; otherwise, <c>false</c>.
    /// </returns>
    internal bool UpdateOfferingPity(
        double[] ograce,
        IReadOnlyList<double> chances,
        IReadOnlyList<bool> withOffering,
        int startLevel)
    {
        const double MIN_CHANGE = 1e-6;

        var levels = ograce.Length;

        //what an unbroken run of successes up to each level leaves of the counter it started with
        var carried = new double[levels + 1];
        carried[0] = 1;

        for (var step = 0; step < levels; step++)
        {
            var decay = withOffering[step] ? PityOfferingDecay : Math.Max(0, 1 - (startLevel + step + 1) * PityPlainDecayPerLevel);

            carried[step + 1] = carried[step] * decay;
        }

        //a climb ends at its first failure or at the target; what it leaves and adds, averaged over where it ends
        var reached = 1.0;
        var shareLeft = 0.0;
        var pityAdded = 0.0;

        for (var step = 0; step < levels; step++)
        {
            var failing = reached * (1 - chances[step]);

            shareLeft += failing * carried[step];

            if (withOffering[step])
                pityAdded += failing * PityFailureGain;

            reached *= chances[step];
        }

        shareLeft += reached * carried[levels];

        //the floor keeps a degenerate plan from dividing by zero
        var averagePity = pityAdded / Math.Max(1e-9, 1 - shareLeft);

        var moved = false;

        for (var step = 0; step < levels; step++)
        {
            var settled = averagePity * carried[step];

            if (Math.Abs(settled - ograce[step]) > MIN_CHANGE)
                moved = true;

            ograce[step] = settled;
        }

        return moved;
    }
}