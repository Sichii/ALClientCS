#region
using System.Collections.Concurrent;
using AL.Client.Definitions;
using AL.Client.Helpers;
using AL.Client.Model;
#endregion

namespace AL.Client.Abstractions;

/// <summary>
///     Provides the cheapest expected-gold climb to a target level on one bench, for an item, scroll prices and offerings
///     fixed per instance, and the model every climb is priced against.
/// </summary>
/// <remarks>
///     Costs are long-run averages over climbs run back to back, with the failstack and offering pity counters moved only
///     by those climbs. The 90th-percentile climb still runs about triple the median.
/// </remarks>
internal abstract class PlannerBase
{
    private readonly ConcurrentDictionary<(int Level, long Grace), IReadOnlyList<PlanChoice>> ChoiceCache = new();
    private readonly OfferingChoice? DepositOffering;

    /// <summary>
    ///     The values that set this planner's bench apart from the other.
    /// </summary>
    public Bench Bench { get; }

    /// <summary>Whether the offering pity counter is counted.</summary>
    public bool CountsOfferingPity { get; }

    /// <summary>
    ///     Whether the player's per-level failstacks are counted. The compound bench has none.
    /// </summary>
    public bool CountsPlayerFailstacks { get; }

    /// <summary>
    ///     Whether the server's per-level failstacks are counted, starting from none and moved only by these climbs.
    /// </summary>
    public bool CountsServerFailstacks { get; }

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
    ///     Initializes a new instance of the <see cref="PlannerBase" /> class.
    /// </summary>
    /// <param name="bench">The bench the climb is made on.</param>
    /// <param name="thresholds">The item's thresholds.</param>
    /// <param name="scrollPrices">Scroll prices indexed by scroll grade.</param>
    /// <param name="offerings">The offerings available.</param>
    /// <param name="countPity">
    ///     Specifies whether the player's failstacks and the offering pity counter are counted.
    /// </param>
    /// <param name="countServerPity">
    ///     Specifies whether the server's failstacks are counted.
    /// </param>
    /// <exception cref="System.ArgumentNullException">thresholds</exception>
    /// <exception cref="System.ArgumentNullException">scrollPrices</exception>
    /// <exception cref="System.ArgumentNullException">offerings</exception>
    protected PlannerBase(
        Bench bench,
        IReadOnlyList<int> thresholds,
        IReadOnlyList<double> scrollPrices,
        IReadOnlyList<OfferingChoice> offerings,
        bool countPity,
        bool countServerPity)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        ArgumentNullException.ThrowIfNull(scrollPrices);

        ArgumentNullException.ThrowIfNull(offerings);

        Bench = bench;
        Thresholds = thresholds;
        ScrollPrices = scrollPrices;
        Offerings = offerings;
        CountsOfferingPity = countPity;
        CountsPlayerFailstacks = countPity && bench.TracksFailstacks;
        CountsServerFailstacks = countServerPity && bench.TracksFailstacks;
        DepositOffering = bench.TakesDeposits && UpgradeMath.TryFindCheapestOffering(offerings, out var cheapest) ? cheapest : null;
    }

    /// <summary>
    ///     Calculates the expected attempts one level takes per success, a run of attempts ending at its first success.
    /// </summary>
    /// <remarks>
    ///     Each failure in the run adds a failstack, so the chance rises until the counters pass their caps.
    /// </remarks>
    /// <param name="level">The level the attempts are made from.</param>
    /// <param name="staked">
    ///     The grace each staked copy carries, deposits included.
    /// </param>
    /// <param name="choice">What goes on the bench.</param>
    /// <param name="bump">The failstacks the counters start the run with.</param>
    /// <param name="ograce">The offering pity counter.</param>
    /// <returns>
    ///     The expected attempts, or infinity when no attempt can succeed.
    /// </returns>
    internal double CalculateAttempts(
        int level,
        double staked,
        PlanChoice choice,
        (int Player, int Server) bump,
        double ograce)
    {
        const double PLAYER_CAP = UpgradeMath.PLAYER_PITY_CAP * UpgradeMath.PLAYER_FAILSTACKS_PER_POINT;
        const double SERVER_CAP = UpgradeMath.SERVER_PITY_CAP * UpgradeMath.SERVER_FAILSTACKS_PER_POINT;

        if (!TryGetBaseChance(level, out var baseChance))
            return double.PositiveInfinity;

        var player = CountsPlayerFailstacks ? bump.Player : 0;
        var server = CountsServerFailstacks ? bump.Server : 0;
        double total = 0;
        double survive = 1;

        while (true)
        {
            var chance = CalculateChance(
                baseChance,
                level,
                choice,
                staked,
                player,
                server,
                ograce);

            //past both caps the chance stops moving, so the rest of the run is a plain geometric series
            if ((!CountsPlayerFailstacks || (player >= PLAYER_CAP)) && (!CountsServerFailstacks || (server >= SERVER_CAP)))
                return chance > 0 ? total + survive / chance : double.PositiveInfinity;

            total += survive;
            survive *= 1 - chance;

            if (CountsPlayerFailstacks)
                player++;

            if (CountsServerFailstacks)
                server++;
        }
    }

    /// <summary>
    ///     Calculates the grace a copy carries into the level above after a success.
    /// </summary>
    /// <param name="level">The level the attempt is made from.</param>
    /// <param name="staked">
    ///     The grace each staked copy carries, deposits included.
    /// </param>
    /// <param name="choice">What goes on the bench.</param>
    /// <returns>The grace carried up.</returns>
    internal double CalculateCarriedGrace(int level, double staked, PlanChoice choice)
    {
        var itemGrade = UpgradeMath.CalculateGrade(Thresholds, level);

        //the grace the scroll and offering bank lands after the roll, so only a success carries it
        return (choice.Offering is not null ? Bench.CopiesPerAttempt * staked : staked) / Bench.GraceMergeDivisor
               + UpgradeMath.GetScrollGrace(
                   choice.ScrollGrade,
                   itemGrade,
                   level + 1,
                   Bench.ScrollGraceMaxLevel)
               + (choice.Offering is { } offering
                   ? UpgradeMath.GetOfferingGrace(offering.Grade, itemGrade, Bench.MatchingOfferingGrace)
                   : 0);
    }

    /// <summary>Calculates one attempt's chance on this bench.</summary>
    /// <param name="baseChance">The attempt's base chance.</param>
    /// <param name="newLevel">The level being reached.</param>
    /// <param name="itemGrade">The item's grade at the level it leaves.</param>
    /// <param name="scrollGrade">The scroll's grade.</param>
    /// <param name="offeringGrade">The offering's grade, or null for none.</param>
    /// <param name="grace">The grace each staked copy carries.</param>
    /// <param name="playerFailstacks">
    ///     The player's failstacks at the level being reached.
    /// </param>
    /// <param name="serverFailstacks">
    ///     The server's failstacks at the level being reached.
    /// </param>
    /// <param name="ograce">The offering pity counter.</param>
    /// <returns>The attempt's chance.</returns>
    protected abstract double CalculateChance(
        double baseChance,
        int newLevel,
        int itemGrade,
        int scrollGrade,
        int? offeringGrade,
        double grace,
        double playerFailstacks,
        double serverFailstacks,
        double ograce);

    private double CalculateChance(
        double baseChance,
        int level,
        PlanChoice choice,
        double staked,
        double playerFailstacks,
        double serverFailstacks,
        double ograce)
        => CalculateChance(
            baseChance,
            level + 1,
            UpgradeMath.CalculateGrade(Thresholds, level),
            choice.ScrollGrade,
            choice.Offering?.Grade,
            staked,
            playerFailstacks,
            serverFailstacks,
            ograce);

    /// <summary>
    ///     Creates a priced plan's steps, each with the expected cost to own one copy at the level it reaches.
    /// </summary>
    /// <param name="priced">The priced plan.</param>
    /// <param name="startLevel">The level the climb starts from.</param>
    /// <param name="copyPrice">
    ///     The price of one copy at <paramref name="startLevel" />.
    /// </param>
    /// <returns>One step per level climbed, in level order.</returns>
    private List<UpgradePlanStep> CreateSteps(PricedPlan priced, int startLevel, double copyPrice)
    {
        var steps = new List<UpgradePlanStep>(priced.Choices.Count);
        var expected = copyPrice;

        for (var index = 0; index < priced.Choices.Count; index++)
        {
            var choice = priced.Choices[index];
            var attempts = priced.Attempts[index];
            expected = attempts * (Bench.CopiesPerAttempt * expected + choice.Fees);

            steps.Add(
                new UpgradePlanStep(
                    startLevel + index,
                    choice.ScrollGrade,
                    choice.Offering?.Name,
                    choice.Deposits,
                    priced.ItemGrace[index],
                    1 / attempts,
                    expected));
        }

        return steps;
    }

    /// <summary>
    ///     Finds the cheapest expected climb from <paramref name="startLevel" /> to <paramref name="targetLevel" />.
    /// </summary>
    /// <remarks>
    ///     Every copy is assumed to carry <paramref name="startGrace" />, which is slightly optimistic when the graced copy is
    ///     one of a kind.
    /// </remarks>
    /// <param name="targetLevel">The level to climb to.</param>
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
        if (!TryCheckClimb(
                targetLevel,
                startLevel,
                forced,
                out var unreachable))
            return new UpgradePlan([], 0, unreachable);

        startGrace = Math.Max(0, startGrace);

        IReadOnlyList<PlanChoice> choices;

        if (forced is null)
            choices = new PlanSearch(
                this,
                startLevel,
                targetLevel,
                startGrace).FindCheapest(copyPrice);
        else if (!TryReadForced(
                     forced,
                     startLevel,
                     targetLevel,
                     out choices,
                     out unreachable))
            return new UpgradePlan([], 0, unreachable);

        var steps = CreateSteps(Price(choices, startLevel, startGrace), startLevel, copyPrice);

        //the baseline uses the matching scroll alone and never an offering
        var baseline = Enumerable.Range(startLevel, targetLevel - startLevel)
                                 .Select(level => UpgradeMath.CalculateGrade(Thresholds, level))
                                 .Select(grade => new PlanChoice(
                                     grade,
                                     ScrollPrices[grade],
                                     null,
                                     0,
                                     0))
                                 .ToList();

        var baselineCost = CreateSteps(Price(baseline, startLevel, startGrace), startLevel, copyPrice)[^1].ExpectedCost;

        return new UpgradePlan(steps, baselineCost, null);
    }

    /// <summary>
    ///     Gets every choice worth putting on the bench at a level, for a copy carrying the given grace.
    /// </summary>
    /// <remarks>
    ///     Deposits stop once the chance can rise no further, or the item's own grace is capped: grace for the levels above is
    ///     cheaper deposited there, where only the copies that got that far pay for it.
    /// </remarks>
    /// <param name="level">The level the attempts are made from.</param>
    /// <param name="grace">The grace each copy carries before any deposit.</param>
    /// <returns>
    ///     The choices, or none when the level has no base chance or no priced scroll covers it.
    /// </returns>
    internal IReadOnlyList<PlanChoice> GetChoices(int level, double grace)
    {
        const double PAST_EVERY_CAP = 1e6;

        var key = (level, (long)Math.Round(grace * 1e6));

        if (ChoiceCache.TryGetValue(key, out var cached))
            return cached;

        var choices = new List<PlanChoice>();

        if (!TryGetBaseChance(level, out var baseChance))
            return choices;

        var itemGrade = UpgradeMath.CalculateGrade(Thresholds, level);

        //own grace, the grade at +0's share included, is capped one past the level being reached; only the bench that takes
        //deposits has that share
        var ownGrace = grace + UpgradeMath.GetBaseGradeGrace(UpgradeMath.CalculateGrade(Thresholds, 0));
        var depositCeiling = DepositOffering is null ? 0 : (int)Math.Ceiling(Math.Max(0, level + 2 - ownGrace) / UpgradeMath.DEPOSIT_GRACE);

        var scrolls = Bench.ScrollGrades[itemGrade]
                           .Where(scroll => (scroll < ScrollPrices.Count) && (ScrollPrices[scroll] > 0))
                           .ToList();

        if (Bench.ScrollsAboveAlike)
            scrolls =
            [
                .. scrolls.Where(scroll => scroll == itemGrade),
                .. scrolls.Where(scroll => scroll > itemGrade)
                          .OrderBy(scroll => ScrollPrices[scroll])
                          .Take(1)
            ];

        foreach (var scroll in scrolls)
            foreach (var offering in Offerings.Where(candidate => candidate.Price > 0)
                                              .Prepend(null))
            {
                var bare = new PlanChoice(
                    scroll,
                    ScrollPrices[scroll],
                    offering,
                    0,
                    DepositOffering?.Price ?? 0);

                var most = CalculateChance(
                    baseChance,
                    level,
                    bare,
                    PAST_EVERY_CAP,
                    PAST_EVERY_CAP,
                    PAST_EVERY_CAP,
                    PAST_EVERY_CAP);

                for (var deposits = 0; deposits <= depositCeiling; deposits++)
                {
                    choices.Add(
                        bare with
                        {
                            Deposits = deposits
                        });

                    var chance = CalculateChance(
                        baseChance,
                        level,
                        bare,
                        grace + UpgradeMath.DEPOSIT_GRACE * deposits,
                        0,
                        0,
                        0);

                    if (chance >= most)
                        break;
                }
            }

        return ChoiceCache.GetOrAdd(key, choices);
    }

    /// <summary>
    ///     Calculates a level's expected attempts per success from how often the next three levels fail.
    /// </summary>
    /// <remarks>
    ///     A run starts after a success, with whatever bump the same climb's next failure leaves: a failure one, two or three
    ///     levels up, in that order of likelihood, or none when the climb gets past all three.
    /// </remarks>
    /// <param name="failOne">How often the level above fails.</param>
    /// <param name="failTwo">How often the level two above fails.</param>
    /// <param name="failThree">How often the level three above fails.</param>
    /// <param name="attempts">
    ///     The level's expected attempts with no bump, then with the bump from a failure one, two and three levels up.
    /// </param>
    /// <returns>The expected attempts.</returns>
    internal static double MixBumps(
        double failOne,
        double failTwo,
        double failThree,
        IReadOnlyList<double> attempts)
    {
        var one = failOne;
        var two = (1 - failOne) * failTwo;
        var three = (1 - failOne) * (1 - failTwo) * failThree;

        return (1 - one - two - three) * attempts[0]
               + (one > 0 ? one * attempts[1] : 0)
               + (two > 0 ? two * attempts[2] : 0)
               + (three > 0 ? three * attempts[3] : 0);
    }

    /// <summary>
    ///     Prices a plan as a long-run average, its offering pity settled at the value the plan itself sustains.
    /// </summary>
    /// <param name="choices">One choice per level climbed, in level order.</param>
    /// <param name="startLevel">The level the climb starts from.</param>
    /// <param name="startGrace">The grace each starting copy carries.</param>
    /// <param name="countOfferingPity">
    ///     Specifies whether the offering pity counter is counted, or null to follow <see cref="CountsOfferingPity" />.
    /// </param>
    /// <returns>The priced plan.</returns>
    internal PricedPlan Price(
        IReadOnlyList<PlanChoice> choices,
        int startLevel,
        double startGrace,
        bool? countOfferingPity = null)
    {
        var levels = choices.Count;
        var staked = new double[levels];
        var carried = new double[levels];
        var withOffering = new bool[levels];
        var grace = startGrace;
        var carry = 1.0;

        for (var index = 0; index < levels; index++)
        {
            var level = startLevel + index;
            staked[index] = grace + UpgradeMath.DEPOSIT_GRACE * choices[index].Deposits;
            carried[index] = carry;
            withOffering[index] = choices[index].Offering is not null;
            grace = CalculateCarriedGrace(level, staked[index], choices[index]);
            carry *= Bench.CalculatePityDecay(level + 1, withOffering[index]);
        }

        //each level's attempts depend on how often the levels above it fail, so they are worked out from the top down
        double[] CalculateAllAttempts(double pity)
        {
            var attempts = new double[levels];

            for (var index = levels - 1; index >= 0; index--)
            {
                var level = startLevel + index;
                var ograce = pity * carried[index];

                double GetFailRate(int above) => (index + above) < levels ? 1 - 1 / attempts[index + above] : 0;

                var bumps = new double[4];

                for (var above = 0; above <= 3; above++)
                    bumps[above] = (above == 0) || (GetFailRate(above) > 0)
                        ? CalculateAttempts(
                            level,
                            staked[index],
                            choices[index],
                            above == 0 ? (0, 0) : UpgradeMath.GetFailstackBump(level + 1, above, withOffering[index + above]),
                            ograce)
                        : 0;

                attempts[index] = MixBumps(
                    GetFailRate(1),
                    GetFailRate(2),
                    GetFailRate(3),
                    bumps);
            }

            return attempts;
        }

        var settled = 0.0;

        if (countOfferingPity ?? CountsOfferingPity)
            settled = SolveOfferingPity(pity => SettleOfferingPity(CalculateAllAttempts(pity), withOffering, startLevel));

        var final = CalculateAllAttempts(settled);
        double copies = 1;
        double gold = 0;

        for (var index = 0; index < levels; index++)
        {
            copies *= Bench.CopiesPerAttempt * final[index];
            gold = final[index] * (Bench.CopiesPerAttempt * gold + choices[index].Fees);
        }

        return new PricedPlan(
            choices,
            copies,
            gold,
            settled,
            staked,
            final);
    }

    /// <summary>
    ///     Calculates the offering pity a climb starts with, for a plan priced as though climbs started with
    ///     <paramref name="attempts" />' pity.
    /// </summary>
    /// <param name="attempts">
    ///     The expected attempts each level takes per success.
    /// </param>
    /// <param name="withOffering">Whether each level spends an offering.</param>
    /// <param name="startLevel">The level the climb starts from.</param>
    /// <returns>The offering pity a climb starts with.</returns>
    private double SettleOfferingPity(IReadOnlyList<double> attempts, bool[] withOffering, int startLevel)
    {
        var ograce = new double[attempts.Count];

        Bench.UpdateOfferingPity(
            ograce,
            [.. attempts.Select(count => 1 / count)],
            withOffering,
            startLevel);

        return ograce.Length == 0 ? 0 : ograce[0];
    }

    /// <summary>
    ///     Solves for the offering pity a plan sustains: the value that, priced at, produces itself.
    /// </summary>
    /// <remarks>
    ///     More pity can produce more pity, since better luck low in a climb sends more climbs to the offering levels above.
    /// </remarks>
    /// <param name="settleFunc">
    ///     The pity a climb starts with, when priced at the given pity.
    /// </param>
    /// <returns>The sustained offering pity.</returns>
    private static double SolveOfferingPity(Func<double, double> settleFunc)
    {
        const int MAX_STEPS = 100;
        const double TOLERANCE = 1e-13;

        var low = 0.0;
        var lowExcess = settleFunc(0);
        var high = lowExcess;
        var highExcess = settleFunc(high) - high;

        while (highExcess > 0)
        {
            low = high;
            lowExcess = highExcess;
            high = high * 2 + 1e-9;
            highExcess = settleFunc(high) - high;
        }

        var side = 0;

        for (var step = 0; (step < MAX_STEPS) && ((high - low) > (TOLERANCE * Math.Max(1, high))); step++)
        {
            var mid = lowExcess == highExcess ? (low + high) / 2 : (low * highExcess - high * lowExcess) / (highExcess - lowExcess);

            if ((mid <= low) || (mid >= high))
                mid = (low + high) / 2;

            var midExcess = settleFunc(mid) - mid;

            if (midExcess == 0)
                return mid;

            if (midExcess > 0)
            {
                low = mid;
                lowExcess = midExcess;

                if (side == 1)
                    highExcess /= 2;

                side = 1;
            } else
            {
                high = mid;
                highExcess = midExcess;

                if (side == -1)
                    lowExcess /= 2;

                side = -1;
            }
        }

        return (low + high) / 2;
    }

    /// <summary>
    ///     Tries to confirm the climb can be planned at all.
    /// </summary>
    /// <param name="targetLevel">The level to climb to.</param>
    /// <param name="startLevel">The level the climb starts from.</param>
    /// <param name="forced">A plan to price, or null.</param>
    /// <param name="unreachable">
    ///     Why the climb cannot be planned, or null when it can.
    /// </param>
    /// <returns>
    ///     <c>true</c> if every level has a base chance and a priced scroll; otherwise, <c>false</c>.
    /// </returns>
    internal bool TryCheckClimb(
        int targetLevel,
        int startLevel,
        IReadOnlyList<ForcedStep>? forced,
        out string? unreachable)
    {
        unreachable = null;

        if (targetLevel < 1)
            unreachable = "Nothing to plan below +1.";
        else if (forced is not null && (forced.Count != targetLevel))
            unreachable = $"The plan code covers {forced.Count} levels, not the {targetLevel} this climb needs.";
        else if ((startLevel < 0) || (startLevel >= targetLevel))
            unreachable = "The starting level must sit below the target.";
        else if (Thresholds.Count == 0)
            unreachable = $"The item has no {Bench.Name} track.";
        else if (targetLevel > UpgradeMath.GetMaxLevel(Thresholds))
            unreachable = $"The track ends at +{UpgradeMath.GetMaxLevel(Thresholds)}.";
        else
            for (var level = startLevel; level < targetLevel; level++)
            {
                var itemGrade = UpgradeMath.CalculateGrade(Thresholds, level);

                if (!TryGetBaseChance(level, out _))
                {
                    unreachable = $"No base figure exists for +{level + 1} at this item's grade - " + "the server's table stops there.";

                    break;
                }

                if (forced is null
                    && !Bench.ScrollGrades[itemGrade]
                             .Any(grade => (grade < ScrollPrices.Count) && (ScrollPrices[grade] > 0)))
                {
                    unreachable = $"No priced scroll covers grade {itemGrade} at +{level}.";

                    break;
                }
            }

        return unreachable is null;
    }

    /// <summary>
    ///     Tries to get an attempt's base chance from the level it leaves.
    /// </summary>
    /// <param name="level">The level the attempt leaves.</param>
    /// <param name="chance">The base chance.</param>
    /// <returns>
    ///     <c>true</c> if the server's table has a figure for the level; otherwise, <c>false</c>.
    /// </returns>
    internal abstract bool TryGetBaseChance(int level, out double chance);

    /// <summary>
    ///     Tries to read a plan code's steps into choices, priced at this planner's prices.
    /// </summary>
    /// <param name="forced">One step per level from +0.</param>
    /// <param name="startLevel">The level the climb starts from.</param>
    /// <param name="targetLevel">The level climbed to.</param>
    /// <param name="choices">
    ///     One choice per level climbed, or null when a step cannot be priced.
    /// </param>
    /// <param name="unreachable">
    ///     Why the plan cannot be priced, or null when it can.
    /// </param>
    /// <returns>
    ///     <c>true</c> if every step from <paramref name="startLevel" /> up can be priced; otherwise, <c>false</c>.
    /// </returns>
    private bool TryReadForced(
        IReadOnlyList<ForcedStep> forced,
        int startLevel,
        int targetLevel,
        out IReadOnlyList<PlanChoice> choices,
        out string? unreachable)
    {
        var read = new List<PlanChoice>();
        choices = read;
        unreachable = null;

        for (var level = startLevel; level < targetLevel; level++)
        {
            var step = forced[level];

            if ((step.ScrollGrade >= ScrollPrices.Count) || (ScrollPrices[step.ScrollGrade] <= 0))
            {
                unreachable = $"No priced scroll covers grade {UpgradeMath.CalculateGrade(Thresholds, level)} at +{level}.";

                return false;
            }

            var offering = step.Offering is { } name ? Offerings.FirstOrDefault(candidate => candidate.Name == name) : null;

            //a step may name its own deposit offering
            var deposit = step.DepositOffering is { } named
                ? Offerings.FirstOrDefault(candidate => candidate.Name == named)
                : DepositOffering;

            read.Add(
                new PlanChoice(
                    step.ScrollGrade,
                    ScrollPrices[step.ScrollGrade],
                    offering,
                    Bench.TakesDeposits ? step.Deposits : 0,
                    deposit?.Price ?? 0));
        }

        return true;
    }
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
/// <param name="ScrollsAboveAlike">
///     Whether every scroll above the item's grade adds the same, so only the cheapest of them is worth trying.
/// </param>
/// <param name="CopiesPerAttempt">The copies one attempt consumes.</param>
/// <param name="TakesDeposits">
///     Whether offerings can be used without a scroll before an attempt to bank grace.
/// </param>
/// <param name="TracksFailstacks">
///     Whether a failed attempt adds failstacks that raise later attempts' chances.
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
    bool ScrollsAboveAlike,
    int CopiesPerAttempt,
    bool TakesDeposits,
    bool TracksFailstacks,
    double GraceMergeDivisor,
    int ScrollGraceMaxLevel,
    double MatchingOfferingGrace,
    double PityFailureGain,
    double PityOfferingDecay,
    double PityPlainDecayPerLevel)
{
    /// <summary>
    ///     The upgrade bench. By item grade it tries the matching scroll and the cheapest priced one above it, since every
    ///     scroll above the item's grade adds the same.
    /// </summary>
    internal static readonly Bench UPGRADE = new(
        "upgrade",
        [
            [
                0,
                1,
                2,
                3,
                4
            ],
            [
                1,
                2,
                3,
                4
            ],
            [
                2,
                3,
                4
            ],
            [
                3,
                4
            ],
            [4]
        ],
        true,
        1,
        true,
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
        false,
        CONSTANTS.ITEMS_PER_COMPOUND,
        false,
        false,
        UpgradeMath.COMPOUND_MERGE_GRACE_DIVISOR,
        int.MaxValue,
        UpgradeMath.COMPOUND_MATCHING_OFFERING_GRACE,
        0.4,
        0,
        0.02);

    /// <summary>
    ///     Calculates the share of the offering pity counter a success leaves.
    /// </summary>
    /// <param name="newLevel">The level the success reached.</param>
    /// <param name="withOffering">Specifies whether the success spent an offering.</param>
    /// <returns>The share left.</returns>
    internal double CalculatePityDecay(int newLevel, bool withOffering)
        => withOffering ? PityOfferingDecay : Math.Max(0, 1 - newLevel * PityPlainDecayPerLevel);

    /// <summary>
    ///     Updates <paramref name="ograce" /> to the average offering pity counter the plan leaves entering each level.
    /// </summary>
    /// <param name="ograce">
    ///     The counter entering each level, overwritten in place. Its length sets the climb's.
    /// </param>
    /// <param name="chances">The plan's chance at each level.</param>
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
            carried[step + 1] = carried[step] * CalculatePityDecay(startLevel + step + 1, withOffering[step]);

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