#region
using System.Collections.Concurrent;
using AL.Client.Abstractions;
using AL.Client.Model;
#endregion

namespace AL.Client.Helpers;

/// <summary>
///     Provides the provably cheapest plan for one climb, and every plan no other plan beats on both copies and gold.
/// </summary>
/// <remarks>
///     A level's attempts depend on how often the levels above it fail, because a failure adds failstacks to the levels
///     below, and on the offering pity the whole plan sustains. So the search builds plans from the target down, and
///     prices each partial plan with bounds that hold for any lower levels and any pity up to a proven ceiling: a partial
///     plan is dropped only once its best case already loses to a known plan. Every plan that survives is priced exactly.
/// </remarks>
internal sealed class PlanSearch
{
    /// <summary>
    ///     The grid the offering pity carry factor is rounded up onto, so climbs that keep nearly the same share of the
    ///     counter share a state.
    /// </summary>
    private const double CARRY_GRID = 256;

    private readonly int Levels;
    private readonly PlannerBase Planner;
    private readonly double StartGrace;
    private readonly int StartLevel;
    private double? PityCeiling;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlanSearch" /> class.
    /// </summary>
    /// <param name="planner">
    ///     The planner whose model every plan is priced against.
    /// </param>
    /// <param name="startLevel">The level the climb starts from.</param>
    /// <param name="targetLevel">The level climbed to.</param>
    /// <param name="startGrace">The grace each starting copy carries.</param>
    public PlanSearch(
        PlannerBase planner,
        int startLevel,
        int targetLevel,
        double startGrace)
    {
        Planner = planner;
        StartLevel = startLevel;
        Levels = targetLevel - startLevel;
        StartGrace = startGrace;
    }

    /// <summary>
    ///     Calculates the fewest attempts a level could take, with the most helpful bump the climb could leave it.
    /// </summary>
    /// <param name="index">The level's index in the climb.</param>
    /// <param name="staked">
    ///     The grace each staked copy carries, deposits included.
    /// </param>
    /// <param name="choice">What goes on the bench.</param>
    /// <param name="ograce">The offering pity counter.</param>
    /// <returns>The fewest expected attempts.</returns>
    private double CalculateLeastAttempts(
        int index,
        double staked,
        PlanChoice choice,
        double ograce)
    {
        var level = StartLevel + index;
        (int Player, int Server) largest = (0, 0);

        //a bigger bump never costs attempts, so one at least as big as any the level can get gives the floor
        for (var above = 1; (above <= 3) && ((index + above) < Levels); above++)
            foreach (var withOffering in (bool[])
                     [
                         false,
                         true
                     ])
            {
                var bump = UpgradeMath.GetFailstackBump(level + 1, above, withOffering);
                largest = (Math.Max(largest.Player, bump.Player), Math.Max(largest.Server, bump.Server));
            }

        return Planner.CalculateAttempts(
            level,
            staked,
            choice,
            largest,
            ograce);
    }

    /// <summary>
    ///     Calculates the share of a climb's starting offering pity left at the level above, after a success here. A level
    ///     below <see cref="PlannerBase.PityStartLevel" /> leaves the share as it is.
    /// </summary>
    /// <param name="countOfferingPity">
    ///     Specifies whether the offering pity counter is counted.
    /// </param>
    /// <param name="carry">The share left at this level.</param>
    /// <param name="level">The level the attempt is made from.</param>
    /// <param name="choice">What goes on the bench.</param>
    /// <returns>The share, rounded up onto the grid.</returns>
    private double CalculateNextCarry(
        bool countOfferingPity,
        double carry,
        int level,
        PlanChoice choice)
        => countOfferingPity && (level >= Planner.PityStartLevel)
            ? RoundCarry(carry * Planner.Bench.CalculatePityDecay(level + 1, choice.Offering is not null))
            : carry;

    /// <summary>
    ///     Calculates a ceiling on the offering pity any plan can sustain.
    /// </summary>
    /// <remarks>
    ///     Dinkelbach's method over the ratio of pity added to pity kept, each level's success rate free between no help and
    ///     every bump at the previous pass's ceiling.
    /// </remarks>
    /// <param name="states">
    ///     The grace and carry factor a copy can enter each level with.
    /// </param>
    /// <returns>The ceiling.</returns>
    private double CalculatePityCeiling(List<Dictionary<(long, int), (double Grace, double Carry)>> states)
    {
        const double FIRST_GUESS = 1e6;
        const int MAX_PASSES = 20;
        const double LITTLE = 1e-3;

        (var moves, var ranges, var startPosition) = CreatePityMoves(states);
        var highs = new double[moves.Length];

        double CalculatePass(double pityCap, double searchHigh)
        {
            Parallel.For(
                0,
                moves.Length,
                position =>
                {
                    var move = moves[position];

                    highs[position] = 1
                    / CalculateLeastAttempts(
                        move.Index,
                        move.Staked,
                        move.Choice,
                        pityCap * move.Carry);
                });

            return CalculatePityCeiling(
                moves,
                highs,
                ranges,
                startPosition,
                searchHigh);
        }

        var ceiling = CalculatePass(FIRST_GUESS, 1);

        for (var pass = 0; pass < MAX_PASSES; pass++)
        {
            var next = CalculatePass(ceiling, ceiling);
            var settled = next >= (ceiling * (1 - LITTLE));
            ceiling = Math.Min(ceiling, next);

            if (settled)
                break;
        }

        return ceiling;
    }

    /// <summary>
    ///     Calculates the most offering pity any plan sustains, given each move's range of success rates.
    /// </summary>
    /// <param name="moves">Every move out of every state.</param>
    /// <param name="highs">The highest success rate of each move.</param>
    /// <param name="ranges">The moves out of each state, by level then state.</param>
    /// <param name="startPosition">
    ///     The position of the starting state among the first level's.
    /// </param>
    /// <param name="searchHigh">A first guess at the answer.</param>
    /// <returns>The most pity.</returns>
    private static double CalculatePityCeiling(
        PityMove[] moves,
        double[] highs,
        (int Start, int Count)[][] ranges,
        int startPosition,
        double searchHigh)
    {
        const double TOLERANCE = 1e-6;

        //the most pity added plus lambda times pity kept, less lambda; positive while some plan sustains more than lambda
        double CalculateExcess(double lambda)
        {
            var above = Array.Empty<double>();

            for (var index = ranges.Length - 1; index >= 0; index--)
            {
                var here = new double[ranges[index].Length];

                for (var state = 0; state < here.Length; state++)
                {
                    var best = double.MinValue;
                    (var start, var count) = ranges[index][state];

                    for (var position = start; position < (start + count); position++)
                    {
                        var move = moves[position];
                        var low = move.Low;
                        var high = highs[position];
                        var later = move.Next < 0 ? lambda * move.NextCarry : above[move.Next];
                        var failed = move.Gain + lambda * move.Carry;

                        //linear in the success rate, so one end of its range is the most
                        best = Math.Max(best, Math.Max((1 - low) * failed + low * later, (1 - high) * failed + high * later));
                    }

                    here[state] = best;
                }

                above = here;
            }

            return above[startPosition] - lambda;
        }

        double bottom = 0;
        var top = searchHigh;

        while (CalculateExcess(top) > 0)
            top *= 2;

        while ((top - bottom) > (TOLERANCE * Math.Max(1, top)))
        {
            var mid = (bottom + top) / 2;

            if (CalculateExcess(mid) > 0)
                bottom = mid;
            else
                top = mid;
        }

        return top;
    }

    /// <summary>
    ///     Calculates how often a partial plan leaves each of the three levels below it a bump of each size or bigger.
    /// </summary>
    /// <remarks>
    ///     Partial plans spending offerings on the same three lowest levels list the same bump sizes in the same order.
    /// </remarks>
    /// <param name="suffix">The partial plan.</param>
    /// <param name="index">The index of its lowest level.</param>
    /// <returns>
    ///     For each level below, nearest first, the share of climbs leaving it each bump size or bigger.
    /// </returns>
    private double[] CalculateBumpTails(Suffix suffix, int index)
    {
        var tails = new List<double>();

        for (var levelsBelow = 1; (levelsBelow <= 3) && ((index - levelsBelow) >= 0); levelsBelow++)
        {
            var newLevel = StartLevel + index - levelsBelow + 1;
            var outcomes = new List<((int Player, int Server) Bump, double Weight)>();
            var rest = 1.0;

            //a failure in the partial plan's lowest levels, given the climb got past the levels in between
            for (var levelsAbove = levelsBelow; levelsAbove <= 3; levelsAbove++)
            {
                var position = levelsAbove - levelsBelow;

                outcomes.Add(
                    (UpgradeMath.GetFailstackBump(newLevel, levelsAbove, suffix.WithOffering[position]), rest * suffix.FailLow[position]));

                rest *= 1 - suffix.FailLow[position];
            }

            outcomes.Add(((0, 0), rest));

            foreach ((var size, _) in outcomes)
            {
                var tail = 0.0;

                foreach ((var bump, var weight) in outcomes)
                    if ((bump.Player >= size.Player) && (bump.Server >= size.Server))
                        tail += weight;

                tails.Add(tail);
            }
        }

        return [.. tails];
    }

    private static (long, int) CreateKey(double grace, double carry)
        => ((long)Math.Round(grace * 1e6), (int)Math.Round(carry * CARRY_GRID));

    /// <summary>
    ///     Creates every move out of every state, flattened once so each pass of the pity ceiling is plain arithmetic.
    /// </summary>
    /// <param name="states">
    ///     The grace and carry factor a copy can enter each level with.
    /// </param>
    /// <returns>
    ///     The moves, the moves out of each state by level then state, and the starting state's position.
    /// </returns>
    private (PityMove[] Moves, (int Start, int Count)[][] Ranges, int StartPosition) CreatePityMoves(
        List<Dictionary<(long, int), (double Grace, double Carry)>> states)
    {
        var positions = states.Select(level => level.Keys
                                                    .Select((key, position) => (key, position))
                                                    .ToDictionary(pair => pair.key, pair => pair.position))
                              .ToList();

        var moves = new List<PityMove>();
        var ranges = new (int Start, int Count)[Levels][];

        for (var index = 0; index < Levels; index++)
        {
            var level = StartLevel + index;
            var levelIndex = index;

            //each state's moves in parallel, kept in state order
            var found = states[index]
                        .Values
                        .AsParallel()
                        .AsOrdered()
                        .Select(entry =>
                        {
                            (var grace, var carry) = entry;
                            var stateMoves = new List<PityMove>();

                            foreach (var choice in Planner.GetChoices(level, grace))
                            {
                                var staked = grace + UpgradeMath.DEPOSIT_GRACE * choice.Deposits;
                                var withOffering = choice.Offering is not null;
                                var counted = level >= Planner.PityStartLevel;

                                var nextCarry = counted
                                    ? RoundCarry(carry * Planner.Bench.CalculatePityDecay(level + 1, withOffering))
                                    : carry;
                                var next = -1;

                                if ((levelIndex < (Levels - 1))
                                    && !positions[levelIndex + 1]
                                        .TryGetValue(CreateKey(Planner.CalculateCarriedGrace(level, staked, choice), nextCarry), out next))
                                    continue;

                                var low = 1
                                    / Planner.CalculateAttempts(
                                        level,
                                        staked,
                                        choice,
                                        (0, 0),
                                        0);

                                stateMoves.Add(
                                    new PityMove(
                                        levelIndex,
                                        staked,
                                        choice,
                                        carry,
                                        nextCarry,
                                        withOffering && counted ? Planner.Bench.PityFailureGain : 0,
                                        low,
                                        next));
                            }

                            return stateMoves;
                        })
                        .ToList();

            ranges[index] = new (int, int)[found.Count];

            for (var state = 0; state < found.Count; state++)
            {
                ranges[index][state] = (moves.Count, found[state].Count);
                moves.AddRange(found[state]);
            }
        }

        return ([.. moves], ranges, positions[0][CreateKey(StartGrace, 1)]);
    }

    /// <summary>Finds the cheapest plan at the given copy price.</summary>
    /// <param name="copyPrice">The price of one copy at the starting level.</param>
    /// <returns>One choice per level climbed, in level order.</returns>
    public IReadOnlyList<PlanChoice> FindCheapest(double copyPrice)
    {
        var seed = FindSeedPlan(copyPrice, Planner.CountsOfferingPity);

        if (!Planner.CountsOfferingPity)
            return FindCheapest(copyPrice, seed, false);

        //the cheapest plan with the offering pity left out, as a bound
        var withoutPity = FindCheapest(copyPrice, seed, false);

        var known = new[]
        {
            seed,
            withoutPity
        }.MinBy(plan => Planner.Price(plan, StartLevel, StartGrace)
                               .CalculateCost(copyPrice))!;

        return FindCheapest(copyPrice, known, true);
    }

    /// <summary>
    ///     Finds the cheapest plan at the given copy price, given a known plan to beat.
    /// </summary>
    /// <param name="copyPrice">The price of one copy at the starting level.</param>
    /// <param name="known">A plan to beat.</param>
    /// <param name="countOfferingPity">
    ///     Specifies whether the offering pity counter is counted.
    /// </param>
    /// <returns>One choice per level climbed, in level order.</returns>
    private IReadOnlyList<PlanChoice> FindCheapest(double copyPrice, IReadOnlyList<PlanChoice> known, bool countOfferingPity)
    {
        var bound = Planner.Price(
                               known,
                               StartLevel,
                               StartGrace,
                               countOfferingPity)
                           .CalculateCost(copyPrice)
                    * (1 + 1e-9);

        //a known plan rules itself out of the search, so it rejoins the survivors here
        return FindSurvivors(countOfferingPity, (_, _, cost) => cost > bound, copyPrice)
               .Prepend(known)
               .AsParallel()
               .AsOrdered()
               .Select(plan => Planner.Price(
                   plan,
                   StartLevel,
                   StartGrace,
                   countOfferingPity))
               .ToList()
               .MinBy(priced => priced.CalculateCost(copyPrice))!.Choices;
    }

    private List<PricedPlan> FindEdge(List<IReadOnlyList<PlanChoice>> known, bool countOfferingPity)
    {
        var pricedKnown = known.Select(plan => Planner.Price(
                                   plan,
                                   StartLevel,
                                   StartGrace,
                                   countOfferingPity))
                               .ToList();

        var edge = SelectEdge(pricedKnown);

        var survivors = FindSurvivors(countOfferingPity, (copies, gold, _) => IsBeaten(edge, copies, gold), 0)
                        .AsParallel()
                        .AsOrdered()
                        .Select(plan => Planner.Price(
                            plan,
                            StartLevel,
                            StartGrace,
                            countOfferingPity))
                        .ToList();

        return SelectEdge(pricedKnown.Concat(survivors));
    }

    /// <summary>
    ///     Finds a good plan quickly, to give the exact search a cost to beat.
    /// </summary>
    /// <remarks>
    ///     Searches level by level from the bottom with every level's feedback - how often the levels above fail, and the
    ///     offering pity - held at what the previous round's plan produced, until the plan stops changing.
    /// </remarks>
    /// <param name="copyPrice">The price of one copy at the starting level.</param>
    /// <param name="countOfferingPity">
    ///     Specifies whether the offering pity counter is counted.
    /// </param>
    /// <returns>One choice per level climbed, in level order.</returns>
    private IReadOnlyList<PlanChoice> FindSeedPlan(double copyPrice, bool countOfferingPity)
    {
        const int MAX_ROUNDS = 10;

        var failRates = new double[Levels];
        var withOffering = new bool[Levels];
        var pity = new double[Levels];
        IReadOnlyList<PlanChoice>? previous = null;
        PricedPlan? best = null;

        for (var round = 0; round < MAX_ROUNDS; round++)
        {
            var plan = FindSeedPlan(
                copyPrice,
                failRates,
                withOffering,
                pity);

            var priced = Planner.Price(
                plan,
                StartLevel,
                StartGrace,
                countOfferingPity);

            if (best is null || (priced.CalculateCost(copyPrice) < best.CalculateCost(copyPrice)))
                best = priced;

            if (previous is not null && previous.SequenceEqual(plan))
                break;

            previous = plan;
            var carry = 1.0;

            for (var index = 0; index < Levels; index++)
            {
                failRates[index] = 1 - 1 / priced.Attempts[index];
                withOffering[index] = plan[index].Offering is not null;
                pity[index] = priced.OfferingPity * carry;

                if ((StartLevel + index) >= Planner.PityStartLevel)
                    carry *= Planner.Bench.CalculatePityDecay(StartLevel + index + 1, withOffering[index]);
            }
        }

        return best!.Choices;
    }

    private IReadOnlyList<PlanChoice> FindSeedPlan(
        double copyPrice,
        double[] failRates,
        bool[] withOffering,
        double[] pity)
    {
        var open = new List<SeedNode>
        {
            new(
                null,
                null,
                copyPrice,
                StartGrace)
        };

        for (var index = 0; index < Levels; index++)
        {
            var level = StartLevel + index;
            var reached = new Dictionary<long, SeedNode>();

            double GetFailRate(int above) => (index + above) < Levels ? failRates[index + above] : 0;

            foreach (var node in open)
                foreach (var choice in Planner.GetChoices(level, node.Grace))
                {
                    var staked = node.Grace + UpgradeMath.DEPOSIT_GRACE * choice.Deposits;
                    var bumps = new double[4];

                    for (var above = 0; above <= 3; above++)
                        bumps[above] = (above == 0) || (GetFailRate(above) > 0)
                            ? Planner.CalculateAttempts(
                                level,
                                staked,
                                choice,
                                above == 0 ? (0, 0) : UpgradeMath.GetFailstackBump(level + 1, above, withOffering[index + above]),
                                pity[index])
                            : 0;

                    var attempts = PlannerBase.MixBumps(
                        GetFailRate(1),
                        GetFailRate(2),
                        GetFailRate(3),
                        bumps);

                    if (double.IsPositiveInfinity(attempts))
                        continue;

                    var cost = attempts * (Planner.Bench.CopiesPerAttempt * node.Cost + choice.Fees);
                    var grace = Planner.CalculateCarriedGrace(level, staked, choice);
                    var key = (long)Math.Round(grace * 1e6);

                    if (!reached.TryGetValue(key, out var incumbent) || (cost < incumbent.Cost))
                        reached[key] = new SeedNode(
                            node,
                            choice,
                            cost,
                            grace);
                }

            //a node beaten on grace and on cost at once can never come back
            open = [];
            var cheapest = double.MaxValue;

            foreach (var node in reached.Values
                                        .OrderByDescending(node => node.Grace)
                                        .ThenBy(node => node.Cost))
                if (node.Cost < cheapest)
                {
                    open.Add(node);
                    cheapest = node.Cost;
                }
        }

        var plan = new PlanChoice[Levels];
        var walked = open.MinBy(node => node.Cost)!;

        for (var index = Levels - 1; index >= 0; index--)
        {
            plan[index] = walked.Choice!;
            walked = walked.Parent!;
        }

        return plan;
    }

    /// <summary>
    ///     Finds every plan that could still beat what a bound says is already known.
    /// </summary>
    /// <param name="countOfferingPity">
    ///     Specifies whether the offering pity counter is counted.
    /// </param>
    /// <param name="isBeatenFunc">
    ///     Whether a plan whose copies, gold and cost at <paramref name="copyPrice" /> are at least the given figures is
    ///     already beaten.
    /// </param>
    /// <param name="copyPrice">
    ///     The copy price the cost handed to <paramref name="isBeatenFunc" /> is at.
    /// </param>
    /// <returns>The plans, unpriced.</returns>
    private List<PlanChoice[]> FindSurvivors(bool countOfferingPity, Func<double, double, double, bool> isBeatenFunc, double copyPrice)
    {
        var copiesPerAttempt = Planner.Bench.CopiesPerAttempt;

        //every grace and carry factor a copy can enter each level with
        var states = new List<Dictionary<(long, int), (double Grace, double Carry)>>
        {
            new()
            {
                [CreateKey(StartGrace, 1)] = (StartGrace, 1)
            }
        };

        for (var index = 0; index < (Levels - 1); index++)
        {
            var level = StartLevel + index;
            var next = new Dictionary<(long, int), (double, double)>();

            foreach ((var grace, var carry) in states[index].Values)
                foreach (var choice in Planner.GetChoices(level, grace))
                {
                    var staked = grace + UpgradeMath.DEPOSIT_GRACE * choice.Deposits;
                    var nextGrace = Planner.CalculateCarriedGrace(level, staked, choice);

                    var nextCarry = CalculateNextCarry(
                        countOfferingPity,
                        carry,
                        level,
                        choice);
                    next.TryAdd(CreateKey(nextGrace, nextCarry), (nextGrace, nextCarry));
                }

            states.Add(next);
        }

        var pityCap = countOfferingPity ? PityCeiling ??= CalculatePityCeiling(states) : 0;

        //the fewest copies, least gold and lowest cost a copy entering each state could possibly have, each on its own
        var floors = new List<Dictionary<(long, int), (double Copies, double Gold, double Cost)>>
        {
            new()
            {
                [CreateKey(StartGrace, 1)] = (1, 0, copyPrice)
            }
        };

        for (var index = 0; index < (Levels - 1); index++)
        {
            var level = StartLevel + index;
            var next = new Dictionary<(long, int), (double Copies, double Gold, double Cost)>();

            foreach ((var key, (var grace, var carry)) in states[index])
            {
                if (!floors[index]
                        .TryGetValue(key, out var floor))
                    continue;

                foreach (var choice in Planner.GetChoices(level, grace))
                {
                    var staked = grace + UpgradeMath.DEPOSIT_GRACE * choice.Deposits;

                    var attempts = CalculateLeastAttempts(
                        index,
                        staked,
                        choice,
                        pityCap * carry);

                    var nextCarry = CalculateNextCarry(
                        countOfferingPity,
                        carry,
                        level,
                        choice);
                    var nextKey = CreateKey(Planner.CalculateCarriedGrace(level, staked, choice), nextCarry);

                    var reached = (Copies: copiesPerAttempt * attempts * floor.Copies,
                        Gold: attempts * (copiesPerAttempt * floor.Gold + choice.Fees),
                        Cost: attempts * (copiesPerAttempt * floor.Cost + choice.Fees));

                    if (next.TryGetValue(nextKey, out var old))
                        reached = (Math.Min(old.Copies, reached.Copies), Math.Min(old.Gold, reached.Gold),
                            Math.Min(old.Cost, reached.Cost));

                    next[nextKey] = reached;
                }
            }

            floors.Add(next);
        }

        //the partial plans from each level up to the target, built from the top down
        var above = new ConcurrentDictionary<(long, int), List<Suffix>>();

        for (var index = Levels - 1; index >= 0; index--)
        {
            var level = StartLevel + index;
            var levelIndex = index;
            var below = above;
            var here = new ConcurrentDictionary<(long, int), List<Suffix>>();

            Parallel.ForEach(
                states[index],
                entry =>
                {
                    (var key, (var grace, var carry)) = entry;

                    if (!floors[levelIndex]
                            .TryGetValue(key, out var floor))
                        return;

                    var list = new List<Suffix>();

                    foreach (var choice in Planner.GetChoices(level, grace))
                    {
                        var staked = grace + UpgradeMath.DEPOSIT_GRACE * choice.Deposits;
                        var withOffering = choice.Offering is not null;
                        IEnumerable<Suffix> suffixes;

                        if (levelIndex == (Levels - 1))
                            suffixes = [Suffix.EMPTY];
                        else
                        {
                            var nextCarry = CalculateNextCarry(
                                countOfferingPity,
                                carry,
                                level,
                                choice);

                            var nextKey = CreateKey(Planner.CalculateCarriedGrace(level, staked, choice), nextCarry);

                            if (!below.TryGetValue(nextKey, out var found))
                                continue;

                            suffixes = found;
                        }

                        //the attempts after a failure each number of levels above, by whether that level spends an offering
                        var fewestByBump = new double[4, 2];
                        var mostByBump = new double[4, 2];

                        for (var levelsAbove = 0; (levelsAbove <= 3) && ((levelIndex + levelsAbove) < Levels); levelsAbove++)
                            for (var offered = 0; offered < (levelsAbove == 0 ? 1 : 2); offered++)
                            {
                                var bump = levelsAbove == 0
                                    ? (0, 0)
                                    : UpgradeMath.GetFailstackBump(level + 1, levelsAbove, offered == 1);

                                fewestByBump[levelsAbove, offered] = Planner.CalculateAttempts(
                                    level,
                                    staked,
                                    choice,
                                    bump,
                                    pityCap * carry);

                                mostByBump[levelsAbove, offered] = countOfferingPity
                                    ? Planner.CalculateAttempts(
                                        level,
                                        staked,
                                        choice,
                                        bump,
                                        0)
                                    : fewestByBump[levelsAbove, offered];
                            }

                        foreach (var suffix in suffixes)
                        {
                            //fewest attempts: the most pity and the most helpful failure rates above; most attempts: neither
                            var fewest = new double[4];
                            var most = new double[4];

                            for (var levelsAbove = 0; (levelsAbove <= 3) && ((levelIndex + levelsAbove) < Levels); levelsAbove++)
                            {
                                var offered = (levelsAbove > 0) && suffix.WithOffering[levelsAbove - 1] ? 1 : 0;

                                fewest[levelsAbove] = fewestByBump[levelsAbove, offered];
                                most[levelsAbove] = mostByBump[levelsAbove, offered];
                            }

                            //the mix is linear in each failure rate on its own, so its extremes over the ranges are at corners
                            var lowest = double.MaxValue;
                            var highest = double.MinValue;

                            for (var corner = 0; corner < 8; corner++)
                            {
                                double PickRate(int levelsAbove)
                                    => ((corner >> (levelsAbove - 1)) & 1) == 1
                                        ? suffix.FailHigh[levelsAbove - 1]
                                        : suffix.FailLow[levelsAbove - 1];

                                lowest = Math.Min(
                                    lowest,
                                    PlannerBase.MixBumps(
                                        PickRate(1),
                                        PickRate(2),
                                        PickRate(3),
                                        fewest));

                                highest = Math.Max(
                                    highest,
                                    PlannerBase.MixBumps(
                                        PickRate(1),
                                        PickRate(2),
                                        PickRate(3),
                                        most));
                            }

                            if (double.IsPositiveInfinity(lowest))
                                continue;

                            var scale = suffix.Scale * copiesPerAttempt * lowest;
                            var gold = suffix.Gold + suffix.Scale * lowest * choice.Fees;

                            //a partial plan whose best possible completion is already beaten can never win
                            if (isBeatenFunc(scale * floor.Copies, scale * floor.Gold + gold, scale * floor.Cost + gold))
                                continue;

                            list.Add(
                                new Suffix(
                                    scale,
                                    gold,
                                    [
                                        1 - 1 / lowest,
                                        suffix.FailLow[0],
                                        suffix.FailLow[1]
                                    ],
                                    [
                                        1 - 1 / highest,
                                        suffix.FailHigh[0],
                                        suffix.FailHigh[1]
                                    ],
                                    [
                                        withOffering,
                                        suffix.WithOffering[0],
                                        suffix.WithOffering[1]
                                    ],
                                    [
                                        choice,
                                        .. suffix.Plan
                                    ]));
                        }
                    }

                    //with a pity range the failure rates are ranges too, and no partial plan provably helps as much as another
                    here[key] = countOfferingPity ? list : RemoveDominated(list, levelIndex);
                });

            above = here;
        }

        return above.TryGetValue(CreateKey(StartGrace, 1), out var final) ? [.. final.Select(suffix => suffix.Plan)] : [];
    }

    /// <summary>
    ///     Generates every plan no other plan beats on both copies and gold.
    /// </summary>
    /// <remarks>
    ///     Two stages: the exact edge with the offering pity left out, then those plans priced with it as the edge every other
    ///     plan has to beat.
    /// </remarks>
    /// <param name="copyPrice">
    ///     A copy price whose cheapest plan is among the results.
    /// </param>
    /// <returns>The plans priced, fewest copies first.</returns>
    public IReadOnlyList<PricedPlan> GenerateEdge(double copyPrice)
    {
        //known plans: quick picks at the given price, then at copy prices from free to very dear
        var known = new List<IReadOnlyList<PlanChoice>>
        {
            FindSeedPlan(copyPrice, false)
        };

        known.AddRange(
            Enumerable.Range(0, 23)
                      .AsParallel()
                      .AsOrdered()
                      .Select(exponent => FindSeedPlan(exponent == 0 ? 0 : Math.Pow(10, exponent / 2.0), false)));

        //a known plan rules itself out of the search, so the known plans rejoin the survivors here
        var withoutPity = FindEdge(known, false);

        if (!Planner.CountsOfferingPity)
            return withoutPity;

        return FindEdge(
            [
                .. withoutPity.Select(priced => priced.Choices),
                .. known
            ],
            true);
    }

    /// <summary>
    ///     Determines whether one partial plan helps the levels below it at least as much as another: for each of the three
    ///     levels below and every bump size, it leaves a bump at least that size at least as often.
    /// </summary>
    /// <remarks>
    ///     A single level below can still take more attempts, but no product of attempts from a lower level up rises, and
    ///     copies and gold are such products.
    /// </remarks>
    /// <param name="first">
    ///     A partial plan's figures, from <see cref="CalculateBumpTails" />.
    /// </param>
    /// <param name="second">
    ///     Another's, spending offerings on the same three lowest levels.
    /// </param>
    /// <returns>
    ///     <c>true</c> if <paramref name="first" /> helps every level below at least as much; otherwise, <c>false</c>.
    /// </returns>
    private static bool HelpsAsMuch(double[] first, double[] second)
    {
        const double TOLERANCE = 1e-12;

        for (var position = 0; position < first.Length; position++)
            if (first[position] < (second[position] - TOLERANCE))
                return false;

        return true;
    }

    /// <summary>
    ///     Determines whether some plan on the edge uses no more copies and no more gold.
    /// </summary>
    /// <param name="edge">The edge, fewest copies first.</param>
    /// <param name="copies">The copies to compare against.</param>
    /// <param name="gold">The gold to compare against.</param>
    /// <returns>
    ///     <c>true</c> if a plan on the edge is at least as good on both; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsBeaten(List<PricedPlan> edge, double copies, double gold)
    {
        var low = 0;
        var high = edge.Count - 1;
        var found = -1;

        //the edge runs fewest copies first with gold falling, so the last plan within the copies has the least gold
        while (low <= high)
        {
            var mid = (low + high) / 2;

            if (edge[mid].Copies <= copies)
            {
                found = mid;
                low = mid + 1;
            } else
                high = mid - 1;
        }

        return (found >= 0) && (edge[found].Gold <= gold);
    }

    /// <summary>
    ///     Removes every partial plan another beats outright: one with the same offerings on its three lowest levels that
    ///     scales a copy's copies and gold no more, and helps every level below at least as much.
    /// </summary>
    /// <param name="suffixes">The partial plans entering one state.</param>
    /// <param name="index">The index of their lowest level.</param>
    /// <returns>The partial plans kept.</returns>
    private List<Suffix> RemoveDominated(List<Suffix> suffixes, int index)
    {
        var kept = new List<Suffix>();

        var groups = suffixes.GroupBy(suffix => (suffix.WithOffering[0], suffix.WithOffering[1], suffix.WithOffering[2]));

        foreach (var group in groups)
        {
            var groupKept = new List<(Suffix Suffix, double[] Tails)>();

            foreach (var suffix in group.OrderBy(suffix => suffix.Scale)
                                        .ThenBy(suffix => suffix.Gold))
            {
                var tails = CalculateBumpTails(suffix, index);

                if (!groupKept.Any(other => (other.Suffix.Scale <= suffix.Scale)
                                            && (other.Suffix.Gold <= suffix.Gold)
                                            && HelpsAsMuch(other.Tails, tails)))
                    groupKept.Add((suffix, tails));
            }

            kept.AddRange(groupKept.Select(entry => entry.Suffix));
        }

        return kept;
    }

    private static double RoundCarry(double carry) => Math.Ceiling(carry * CARRY_GRID - 1e-9) / CARRY_GRID;

    /// <summary>
    ///     Selects the plans no other plan beats on both copies and gold. An earlier plan wins a tie.
    /// </summary>
    /// <param name="plans">The plans, priced.</param>
    /// <returns>The edge, fewest copies first.</returns>
    private static List<PricedPlan> SelectEdge(IEnumerable<PricedPlan> plans)
    {
        var edge = new List<PricedPlan>();

        foreach (var plan in plans.OrderBy(priced => priced.Copies)
                                  .ThenBy(priced => priced.Gold))
            if ((edge.Count == 0) || (plan.Gold < edge[^1].Gold))
                edge.Add(plan);

        return edge;
    }

    /// <summary>
    ///     Represents one choice out of one state, for the pity ceiling.
    /// </summary>
    /// <param name="Index">The level's index in the climb.</param>
    /// <param name="Staked">
    ///     The grace each staked copy carries, deposits included.
    /// </param>
    /// <param name="Choice">What goes on the bench.</param>
    /// <param name="Carry">
    ///     The share of the climb's starting pity left at this level.
    /// </param>
    /// <param name="NextCarry">The share left at the level above after a success.</param>
    /// <param name="Gain">The pity a failure adds.</param>
    /// <param name="Low">The lowest success rate the move can have.</param>
    /// <param name="Next">The state reached above, or -1 at the top.</param>
    private readonly record struct PityMove(
        int Index,
        double Staked,
        PlanChoice Choice,
        double Carry,
        double NextCarry,
        double Gain,
        double Low,
        int Next);

    /// <summary>
    ///     Represents a node of the seed search: the cheapest way found to reach one level with one grace.
    /// </summary>
    private sealed record SeedNode(
        SeedNode? Parent,
        PlanChoice? Choice,
        double Cost,
        double Grace);

    /// <summary>
    ///     Represents a partial plan from one level to the target, with lower bounds on how it scales a copy's cost and the
    ///     range each of its three lowest levels' failure rate can take.
    /// </summary>
    /// <param name="Scale">
    ///     The least the partial plan multiplies a copy's copies and gold by.
    /// </param>
    /// <param name="Gold">The least gold the partial plan adds on top.</param>
    /// <param name="FailLow">
    ///     The lowest failure rate of each of the three lowest levels.
    /// </param>
    /// <param name="FailHigh">
    ///     The highest failure rate of each of the three lowest levels.
    /// </param>
    /// <param name="WithOffering">
    ///     Whether each of the three lowest levels spends an offering.
    /// </param>
    /// <param name="Plan">The choices, lowest level first.</param>
    private sealed record Suffix(
        double Scale,
        double Gold,
        double[] FailLow,
        double[] FailHigh,
        bool[] WithOffering,
        PlanChoice[] Plan)
    {
        /// <summary>
        ///     The partial plan above the target: nothing to multiply, nothing to add, and no level above to fail.
        /// </summary>
        public static readonly Suffix EMPTY = new(
            1,
            0,
            [
                0,
                0,
                0
            ],
            [
                0,
                0,
                0
            ],
            [
                false,
                false,
                false
            ],
            []);
    }
}