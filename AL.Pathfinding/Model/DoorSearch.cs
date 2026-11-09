#region
using System.Runtime.InteropServices;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Provides the search between maps over a fixed set of nodes and static edges, plus the per-search edges a caller
///     puts in the scratch: arrivals that nothing beats, priced with recall, blink and hop score.
/// </summary>
internal sealed class DoorSearch
{
    /// <summary>
    ///     Where each node's edges that a search with blink alone takes begin, inside its run of <see cref="StaticEdges" />.
    /// </summary>
    private readonly int[] BlinkOnlyStart;

    private readonly int[] EdgeStart;

    private readonly int NodeCount;
    private readonly int[] ReverseEdges;
    private readonly int[] ReverseStart;

    /// <summary>
    ///     The static edges, sorted by <see cref="Edge.From" />; <see cref="EdgeStart" /> indexes into them.
    /// </summary>
    private readonly Edge[] StaticEdges;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DoorSearch" /> class, sorting the edges in place.
    /// </summary>
    /// <param name="edges">
    ///     The static edges, sorted in place by node with the blink-only edges last.
    /// </param>
    /// <param name="nodeCount">The number of static nodes.</param>
    public DoorSearch(List<Edge> edges, int nodeCount)
    {
        NodeCount = nodeCount;

        //each node's edges a search without blink takes come first
        edges.Sort((a, b) => a.From != b.From ? a.From.CompareTo(b.From) : a.BlinkOnly.CompareTo(b.BlinkOnly));
        var start = new int[nodeCount + 1];
        var blinkOnlyStart = new int[nodeCount];

        foreach (var edge in edges)
            start[edge.From + 1]++;

        for (var i = 1; i < start.Length; i++)
            start[i] += start[i - 1];

        for (var node = 0; node < nodeCount; node++)
        {
            var i = start[node];

            while ((i < start[node + 1]) && !edges[i].BlinkOnly)
                i++;

            blinkOnlyStart[node] = i;
        }

        StaticEdges = edges.ToArray();
        EdgeStart = start;
        BlinkOnlyStart = blinkOnlyStart;
        (ReverseEdges, ReverseStart) = BuildReverseEdges();
    }

    /// <summary>
    ///     Builds the index of static edges into each node, for the backward search behind the lower bounds.
    /// </summary>
    /// <returns>
    ///     Indices into <see cref="StaticEdges" /> grouped by <see cref="Edge.To" />, and where each node's group starts.
    /// </returns>
    private (int[] Edges, int[] Start) BuildReverseEdges()
    {
        var start = new int[NodeCount + 1];

        foreach (var edge in StaticEdges)
            start[edge.To + 1]++;

        for (var i = 1; i < start.Length; i++)
            start[i] += start[i - 1];

        var edges = new int[StaticEdges.Length];
        var next = (int[])start.Clone();

        for (var i = 0; i < StaticEdges.Length; i++)
            edges[next[StaticEdges[i].To]++] = i;

        return (edges, start);
    }

    /// <summary>
    ///     Fills the scratch's lower bounds: per node, the least the rest of any route to an end can cost, from one backward
    ///     search over every edge at its cheapest.
    /// </summary>
    /// <param name="scratch">The calling thread's search state.</param>
    /// <param name="startNode">The search's virtual start node.</param>
    /// <param name="firstEnd">
    ///     The first end node; every node from it on is an end.
    /// </param>
    /// <param name="nodeCount">The number of nodes in the search.</param>
    /// <param name="options">The search's options.</param>
    /// <param name="townCost">The price of a recall at the character's speed.</param>
    private void ComputeLowerBounds(
        SearchScratch scratch,
        int startNode,
        int firstEnd,
        int nodeCount,
        PathOptions options,
        float townCost)
    {
        var bounds = scratch.LowerBound;
        var queue = scratch.LowerBoundQueue;
        var floor = options.BlinkCost ?? float.MaxValue;
        var blinkOff = options.BlinkCost is null;
        var landing = Math.Max(options.BlinkCost ?? 0f, CONSTANTS.BLINK_LANDING_MS * GetSpeed(options) / 1000f);
        var useTown = options.UseTown;

        Array.Fill(
            bounds,
            float.PositiveInfinity,
            0,
            firstEnd);

        Array.Fill(
            bounds,
            0f,
            firstEnd,
            nodeCount - firstEnd);
        queue.Clear();

        //the edges into the ends seed the search; the start's own edges are read last, since nothing leads into it
        foreach (var edge in scratch.SearchEdges)
        {
            if ((edge.To < firstEnd) || (edge.From == startNode))
                continue;

            var bound = CalculateCheapest(edge);

            if (bound < bounds[edge.From])
            {
                bounds[edge.From] = bound;
                queue.Enqueue(edge.From, bound);
            }
        }

        while (queue.TryDequeue(out var node, out var bound))
        {
            if (bound > bounds[node])
                continue;

            for (var i = ReverseStart[node]; i < ReverseStart[node + 1]; i++)
            {
                var edge = StaticEdges[ReverseEdges[i]];

                if (edge.BlinkOnly && blinkOff)
                    continue;

                var through = bound + CalculateCheapest(edge);

                if (through < bounds[edge.From])
                {
                    bounds[edge.From] = through;
                    queue.Enqueue(edge.From, through);
                }
            }
        }

        for (var i = scratch.SearchEdgeStart[startNode]; i < scratch.SearchEdgeStart[startNode + 1]; i++)
        {
            var edge = scratch.SearchEdges[i];
            bounds[startNode] = Math.Min(bounds[startNode], CalculateCheapest(edge) + bounds[edge.To]);
        }

        return;

        float CalculateCheapest(in Edge edge)
            => edge.Type switch
            {
                EdgeType.Walk  => edge.Cost >= floor ? Math.Min(edge.Cost, landing) : edge.Cost,
                EdgeType.Blink => blinkOff ? float.PositiveInfinity : landing,
                EdgeType.Town  => useTown ? townCost : float.PositiveInfinity,
                _              => edge.Cost
            };
    }

    /// <summary>Gets the edge an arrival was made by.</summary>
    /// <param name="scratch">The calling thread's search state.</param>
    /// <param name="edgeIndex">
    ///     The arrival's <see cref="SearchScratch.Arrival.EdgeIndex" />: a static edge's index, or the static count plus a
    ///     search edge's.
    /// </param>
    /// <returns>The edge.</returns>
    public Edge GetEdge(SearchScratch scratch, int edgeIndex)
        => edgeIndex < StaticEdges.Length ? StaticEdges[edgeIndex] : scratch.SearchEdges[edgeIndex - StaticEdges.Length];

    /// <summary>
    ///     Gets the character's speed for pricing, nominal when unset or not positive, as
    ///     <see cref="CONSTANTS.CalculateTownCost" /> does.
    /// </summary>
    /// <param name="options">The search's options.</param>
    /// <returns>The walk speed.</returns>
    public static float GetSpeed(PathOptions options) => options.WalkSpeed is > 0f ? options.WalkSpeed.Value : CONSTANTS.NOMINAL_WALK_SPEED;

    /// <summary>
    ///     Reads the route to an arrival back through its parents into the scratch's chain list.
    /// </summary>
    /// <param name="scratch">The calling thread's search state.</param>
    /// <param name="winner">The arrival the route ends at.</param>
    /// <returns>The arrival ids from the first move to the last.</returns>
    public static List<int> ReadChain(SearchScratch scratch, int winner)
    {
        var chain = scratch.Chain;
        chain.Clear();

        for (var arrivalId = winner; scratch.Arrivals[arrivalId].Parent >= 0; arrivalId = scratch.Arrivals[arrivalId].Parent)
            chain.Add(arrivalId);

        chain.Reverse();

        return chain;
    }

    /// <summary>
    ///     Finds the cheapest route from the start node to any end, over the static edges and the scratch's search edges.
    /// </summary>
    /// <param name="scratch">
    ///     The calling thread's search state, with its nodes reset and its search edges added and indexed.
    /// </param>
    /// <param name="options">The search's options.</param>
    /// <param name="townCost">The price of a recall at the character's speed.</param>
    /// <param name="startNode">The search's virtual start node.</param>
    /// <param name="firstEnd">
    ///     The first end node; every node from it on is an end.
    /// </param>
    /// <param name="nodeCount">The number of nodes in the search.</param>
    /// <returns>
    ///     The first arrival taken at an end, or -1 when none can be reached.
    /// </returns>
    public int Search(
        SearchScratch scratch,
        PathOptions options,
        float townCost,
        int startNode,
        int firstEnd,
        int nodeCount)
    {
        ComputeLowerBounds(
            scratch,
            startNode,
            firstEnd,
            nodeCount,
            options,
            townCost);

        var search = new ArrivalSearch(
            scratch,
            options,
            townCost,
            startNode);

        search.OfferStart(options);

        var blinkOn = options.BlinkCost is not null;

        while (scratch.ArrivalQueue.TryDequeue(out var arrivalId, out _))
        {
            var from = scratch.Arrivals[arrivalId];

            if (from.Dropped)
                continue;

            if (from.Node >= firstEnd)
                return arrivalId;

            if (from.Node < NodeCount)
                for (var i = EdgeStart[from.Node]; i < (blinkOn ? EdgeStart[from.Node + 1] : BlinkOnlyStart[from.Node]); i++)
                    search.Relax(
                        StaticEdges[i],
                        i,
                        arrivalId,
                        from);

            for (var i = scratch.SearchEdgeStart[from.Node]; i < scratch.SearchEdgeStart[from.Node + 1]; i++)
                search.Relax(
                    scratch.SearchEdges[i],
                    StaticEdges.Length + i,
                    arrivalId,
                    from);
        }

        return -1;
    }

    /// <summary>
    ///     Provides one search's pricing: relaxes an edge out of an arrival into the arrivals it makes, and keeps at each node
    ///     only the arrivals no other one there beats.
    /// </summary>
    private readonly struct ArrivalSearch
    {
        /// <summary>
        ///     How much more hop score an arrival may carry and still make a dearer one at the same node pointless.
        /// </summary>
        private const float HOP_SCORE_SLACK = 2f;

        /// <summary>
        ///     Whether casts are priced at all; false leaves every state the default and the queue ordered by cost alone.
        /// </summary>
        private readonly bool BlinkOn;

        /// <summary>
        ///     The clock pricing casts; read only while <see cref="BlinkOn" />.
        /// </summary>
        private readonly BlinkClock Clock;

        /// <summary>
        ///     The shortest walk worth a cast, from <see cref="PathOptions.BlinkCost" />.
        /// </summary>
        private readonly float Floor;

        /// <summary>
        ///     If populated, the speed hops are taken at; null when the search may not take a hop.
        /// </summary>
        private readonly float? HopSpeed;

        private readonly SearchScratch Scratch;
        private readonly float Speed;
        private readonly int StartNode;
        private readonly float TownCost;
        private readonly bool UseTown;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrivalSearch" /> struct for one search.
        /// </summary>
        /// <param name="scratch">
        ///     The calling thread's scratch, holding the arrivals and the lower bounds.
        /// </param>
        /// <param name="options">The search's pricing and starting state.</param>
        /// <param name="townCost">The price of a recall at the character's speed.</param>
        /// <param name="startNode">The search's virtual start node.</param>
        public ArrivalSearch(
            SearchScratch scratch,
            PathOptions options,
            float townCost,
            int startNode)
        {
            Scratch = scratch;
            BlinkOn = options.BlinkCost is not null;
            Clock = BlinkOn ? new BlinkClock(options) : default;
            Floor = options.BlinkCost ?? float.MaxValue;
            HopSpeed = options.UseHops && options.WalkSpeed is > 0f ? options.WalkSpeed : null;
            Speed = GetSpeed(options);
            StartNode = startNode;
            TownCost = townCost;
            UseTown = options.UseTown;
        }

        /// <summary>Offers the arrival every route starts from.</summary>
        /// <param name="options">The search's options.</param>
        public void OfferStart(PathOptions options)
            => Offer(
                new SearchScratch.Arrival
                {
                    Node = StartNode,
                    State = BlinkOn ? Clock.CreateInitialState(options) : default,
                    HopScore = options.HopScore,
                    Parent = -1,
                    EdgeIndex = -1
                });

        /// <summary>
        ///     Offers the arrivals one edge makes out of an arrival: one per move, and a walk at least the floor long offers both
        ///     the walk and the cast that replaces it.
        /// </summary>
        /// <param name="edge">The edge to relax.</param>
        /// <param name="edgeIndex">
        ///     The edge's index, static edges first, then search edges.
        /// </param>
        /// <param name="fromId">The id of the arrival the edge leaves from.</param>
        /// <param name="origin">The arrival the edge leaves from.</param>
        public void Relax(
            in Edge edge,
            int edgeIndex,
            int fromId,
            in SearchScratch.Arrival origin)
        {
            var next = origin with
            {
                Node = edge.To,
                Parent = fromId,
                EdgeIndex = edgeIndex,
                Blinked = false,
                Dropped = false
            };

            switch (edge.Type)
            {
                //recall off means no recall anywhere on the route
                case EdgeType.Town:
                {
                    if (!UseTown)
                        return;

                    next.Cost = origin.Cost + TownCost;

                    if (BlinkOn)
                        next.State = BlinkClock.AddPenalty(
                            Clock.AdvanceTime(origin.State, CONSTANTS.TOWN_CHANNEL_SECONDS * 1000f),
                            CONSTANTS.EFFECT_PENALTY_MS);

                    Offer(next);

                    return;
                }

                case EdgeType.Door:
                case EdgeType.Transport:
                case EdgeType.Leave:
                {
                    next.Cost = origin.Cost + edge.Cost;

                    if (BlinkOn)
                        next.State = BlinkClock.AddPenalty(origin.State, CONSTANTS.DOOR_PENALTY_MS);

                    Offer(next);

                    return;
                }

                case EdgeType.Walk:
                    //a walk whose hops were checked from a score of 0 is taken only at a speed every one of them is safe at,
                    //checked at whole speeds, and only by a route carrying no score, since the server keeps it through doors
                    if ((edge.MinSpeed > 0f)
                        && (HopSpeed is not { } speed || (MathF.Floor(speed) < edge.MinSpeed) || (origin.HopScore > 0f)))
                        return;

                    next.Cost = origin.Cost + edge.Cost;

                    if (edge.EndScore is { } endScore)
                        next.HopScore = endScore;
                    else if (origin.HopScore > 0f)
                        next.HopScore = HopRisk.DecayWhileWalking(origin.HopScore, edge.Cost / Speed);

                    if (BlinkOn)
                        next.State = Clock.AdvanceTime(origin.State, edge.Cost / Speed * 1000f);

                    Offer(next);

                    //a cast may replace only a walk at least the floor long
                    if (edge.Cost >= Floor)
                        OfferBlink(next, origin);

                    return;

                //a pair only a cast joins stays unjoined while blink is off
                case EdgeType.Blink:
                    OfferBlink(next, origin);

                    return;
            }
        }

        /// <summary>
        ///     Adds an arrival unless one already at its node beats it, and drops every arrival there it beats. Nothing is added
        ///     at a node no end can be reached from.
        /// </summary>
        /// <param name="arrival">The arrival to add.</param>
        private void Offer(in SearchScratch.Arrival arrival)
        {
            var node = arrival.Node;
            var lowerBound = Scratch.LowerBound[node];

            if (float.IsPositiveInfinity(lowerBound))
                return;

            var live = Scratch.NodeArrivals[node];
            var arrivals = CollectionsMarshal.AsSpan(Scratch.Arrivals);

            foreach (var id in live)
                if (Beats(arrivals[id], arrival))
                    return;

            for (var i = live.Count - 1; i >= 0; i--)
            {
                var id = live[i];

                if (!Beats(arrival, arrivals[id]))
                    continue;

                arrivals[id].Dropped = true;
                live.RemoveAt(i);
            }

            var newId = Scratch.Arrivals.Count;
            Scratch.Arrivals.Add(arrival);
            live.Add(newId);

            Scratch.ArrivalQueue.Enqueue(newId, arrival.Cost + lowerBound);
        }

        /// <summary>
        ///     Offers the cast along an edge, priced at the time it takes in walk units but never under the floor; nothing when
        ///     blink is off or the bar can never pay for it.
        /// </summary>
        /// <param name="next">
        ///     The walked arrival along the edge, which the cast differs from only in price and state.
        /// </param>
        /// <param name="origin">The arrival the cast is made from.</param>
        private void OfferBlink(SearchScratch.Arrival next, in SearchScratch.Arrival origin)
        {
            if (!BlinkOn)
                return;

            if (!Clock.TryBlink(origin.State, out var spentMs, out var after))
                return;

            next.Cost = origin.Cost + Math.Max(Floor, spentMs * Speed / 1000f);
            next.State = after;
            next.HopScore = origin.HopScore;
            next.Blinked = true;
            Offer(next);
        }

        /// <summary>
        ///     Determines whether one arrival makes another at the same node pointless. The cheaper one is compared after waiting
        ///     out the difference in cost, since standing still turns it into the dearer one's state.
        /// </summary>
        /// <param name="a">The arrival that may beat.</param>
        /// <param name="b">The arrival that may be beaten.</param>
        /// <returns>
        ///     <c>true</c> if <paramref name="a" /> costs no more, carries at most <see cref="HOP_SCORE_SLACK" /> more hop score,
        ///     and after that wait is at least as ready now and at the next cast; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     Without the slack, every longer walk between hop corners keeps a new arrival that costs a little more for a little
        ///     less score, and a search with no route never ends; with it a node keeps at most one arrival per slack of score.
        ///     Each arrival still carries its own score, so the slack can only miss a slightly better route, never pass an unsafe
        ///     hop.
        /// </remarks>
        private bool Beats(in SearchScratch.Arrival a, in SearchScratch.Arrival b)
            => (a.Cost <= b.Cost)
               && (a.HopScore <= (b.HopScore + HOP_SCORE_SLACK))
               && Clock.IsAtLeastAsWellPlaced(Clock.AdvanceTime(a.State, (b.Cost - a.Cost) / Speed * 1000f), b.State);
    }

    /// <summary>Represents one edge of the search.</summary>
    /// <param name="From">The node the edge leaves.</param>
    /// <param name="To">The node the edge reaches.</param>
    /// <param name="Type">How the edge is travelled.</param>
    /// <param name="Cost">The edge's price in walk units.</param>
    /// <param name="BlinkOnly">
    ///     Whether only a search with blink takes the edge: a pair no walk joins.
    /// </param>
    /// <param name="MinSpeed">
    ///     The slowest walk speed a <see cref="EdgeType.Walk" /> edge whose hops were checked from a score of 0 may be taken
    ///     at; 0 on every other edge.
    /// </param>
    /// <param name="EndScore">
    ///     If populated, the hop score a <see cref="EdgeType.Walk" /> edge with hops inside it ends with, which the next
    ///     arrival carries.
    /// </param>
    internal readonly record struct Edge(
        int From,
        int To,
        EdgeType Type,
        float Cost,
        bool BlinkOnly = false,
        float MinSpeed = 0f,
        float? EndScore = null);
}