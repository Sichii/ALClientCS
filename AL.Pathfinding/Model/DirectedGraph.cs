#region
using System.Diagnostics.CodeAnalysis;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding.Interfaces;
using CONSTANTS = AL.Pathfinding.Definitions.CONSTANTS;
using EdgeType = AL.Pathfinding.Definitions.EdgeType;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents the graph routes are searched over: one node mesh per map, joined by a table of walks between doors.
/// </summary>
public sealed class DirectedGraph
{
    /// <summary>
    ///     The number of stretches the funnel could not pull, which were straightened instead.
    /// </summary>
    internal static int FunnelFallbacks;

    /// <summary>
    ///     The search between maps, over the walks found at build.
    /// </summary>
    private readonly DoorTable Doors;

    /// <summary>
    ///     Every map's vertex graph, which the walks on one map are searched over.
    /// </summary>
    private readonly Dictionary<string, MeshGraph> MeshGraphs;

    /// <summary>Every map's node mesh, by map key.</summary>
    internal Dictionary<string, NodeMesh> NavMeshes { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DirectedGraph" /> class over a set of maps, finding every walk between
    ///     their doors.
    /// </summary>
    /// <param name="navMeshes">Each map's ground, by map key.</param>
    /// <param name="buildHops">
    ///     Specifies whether wall hops are built on the maps that allow them.
    /// </param>
    public DirectedGraph(IReadOnlyDictionary<string, NavMesh> navMeshes, bool buildHops)
    {
        NavMeshes = navMeshes.ToDictionary(kvp => kvp.Key, kvp => new NodeMesh(kvp.Value), StringComparer.OrdinalIgnoreCase);

        var hops = buildHops ? FindHops(navMeshes) : [];

        MeshGraphs = NavMeshes.ToDictionary(
            kvp => kvp.Key,
            kvp => new MeshGraph(kvp.Value, hops.GetValueOrDefault(kvp.Key) ?? []),
            StringComparer.OrdinalIgnoreCase);

        Doors = new DoorTable(this, MeshGraphs);
    }

    /// <summary>
    ///     Smooths each stretch of walk legs in a found route. Every other leg keeps its endpoints, except a recall, which
    ///     leaves from the start of the walks before it.
    /// </summary>
    /// <param name="path">The route as the search found it.</param>
    /// <returns>The smoothed route.</returns>
    internal List<GraphEdge> EnhancePath(IReadOnlyList<GraphEdge> path)
    {
        var enhanced = new List<GraphEdge>();

        var stretch = SmoothScratch.Rent()
                                   .Stretch;
        stretch.Clear();

        for (var i = 0; i < path.Count; i++)
        {
            var edge = path[i];

            if (edge.Type == EdgeType.Town)
            {
                //you can town from anywhere, so the walks before a recall collapse into it, at the search's price
                enhanced.Add(
                    stretch.Count == 0
                        ? edge
                        : NavMeshes[stretch[0].Start.Location.Map]
                            .ConstructEdge(
                                stretch[0].Start,
                                edge.End,
                                EdgeType.Town,
                                edge.Cost));

                stretch.Clear();
            } else if (edge.Type != EdgeType.Walk)
            {
                SmoothStretch(stretch, enhanced);
                enhanced.Add(edge);
                stretch.Clear();
            } else
            {
                stretch.Add(edge);

                //a walk stops inside a node's reach rather than on it, so the stretch ends there
                if (edge.End.Reach.Range > 0)
                {
                    SmoothStretch(stretch, enhanced);
                    stretch.Clear();
                }
            }
        }

        SmoothStretch(stretch, enhanced);

        return enhanced;
    }

    private static int FindOppositeSlot(IGenericTriangle<GraphNode> triangle, IGenericTriangle<GraphNode> neighbour)
    {
        for (var slot = 0; slot < 3; slot++)
            if (!neighbour.Vertices.Contains(triangle.Vertices[slot]))
                return slot;

        throw new InvalidOperationException("The triangles share all three corners.");
    }

    /// <summary>
    ///     Finds the cheapest route from a start to any of several ends.
    /// </summary>
    /// <param name="start">Where the route starts.</param>
    /// <param name="ends">The candidate ends; the cheapest to reach wins.</param>
    /// <param name="options">How the route is priced.</param>
    /// <typeparam name="T">The type of the ends.</typeparam>
    /// <returns>The route's legs.</returns>
    /// <exception cref="ArgumentNullException">start</exception>
    /// <exception cref="ArgumentNullException">ends</exception>
    /// <exception cref="ArgumentException">ends is empty.</exception>
    /// <exception cref="InvalidOperationException">The start has no mesh, or no end can be reached.</exception>
    public List<GraphEdge> FindPath<T>(ILocation start, IEnumerable<T> ends, PathOptions options) where T: ILocation, ICircle
    {
        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(ends);

        var endsArr = ends.ToArray();

        if (endsArr.Length == 0)
            throw new ArgumentException("No end to route to.", nameof(ends));

        //a start already inside an end gets one zero-cost leg that stays put
        if (NavMeshes.TryGetValue(start.Map, out var startMesh)
            && endsArr.Any(end => end.OnSameMapAs(start)
                                  && Reach.CreateCircle(end.X, end.Y, end.Radius)
                                          .Contains(start.X, start.Y)))
        {
            var startNode = startMesh.ConstructNode(start);

            return [startMesh.ConstructEdge(startNode, startNode, EdgeType.Walk)];
        }

        //a short hop with a clear line needs no search. Bounded by the town cost, the only same-map alternative a clear
        //straight line can lose to, or the blink floor when lower, since a cast never prices under it. One destination
        //only, or the nearest same-map candidate need not be cheapest
        var townCost = CONSTANTS.CalculateTownCost(options.WalkSpeed ?? CONSTANTS.NOMINAL_WALK_SPEED);
        var straightBound = Math.Min(townCost, options.BlinkCost ?? float.MaxValue);

        if ((endsArr.Length == 1) && NavMeshes.TryGetValue(start.Map, out var navMesh))
            foreach (var end in endsArr.Where(e => e.OnSameMapAs(start) && (start.Distance(e) < straightBound)))
            {
                var reach = Reach.CreateCircle(end.X, end.Y, end.Radius);

                //stop at the near edge of the destination rather than its centre, the same cut TrimToReach makes
                (var targetX, var targetY) = reach.FindNearestEdgePoint(start.X, start.Y);
                var target = new Location(start.Map, targetX, targetY);

                if (!navMesh.CanMove(start, target))
                    continue;

                var startNode = navMesh.ConstructNode(start);
                var endNode = navMesh.ConstructNode(target);

                return [navMesh.ConstructEdge(startNode, endNode, EdgeType.Walk)];
            }

        return FindRoute(start, endsArr, options);
    }

    /// <summary>
    ///     Builds the walk the last backward mesh search found from a node to the search's source, from the mesh edges and
    ///     hops it took.
    /// </summary>
    /// <param name="meshGraph">The map's vertex graph the search ran over.</param>
    /// <param name="scratch">The calling thread's scratch, holding the search.</param>
    /// <param name="from">The node the walk leaves.</param>
    /// <param name="seeds">
    ///     The seeds of <paramref name="from" />, read off the search.
    /// </param>
    /// <param name="to">The node the search ran backward from.</param>
    /// <returns>
    ///     The legs, empty when <paramref name="from" /> already stands in the target's reach, or null when the search reached
    ///     none of the seeds.
    /// </returns>
    internal List<GraphEdge>? BuildBackwardPiece(
        MeshGraph meshGraph,
        MeshScratch scratch,
        GraphNode from,
        List<MeshGraph.Seed> seeds,
        GraphNode to)
    {
        if (to.Reach.Contains(from.Location.X, from.Location.Y))
            return [];

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (MeshGraph.FindCheapestSeed(scratch, seeds, out var vertex) == float.MaxValue)
            return null;

        var raw = new List<GraphEdge>
        {
            meshGraph.Mesh.ConstructEdge(from, meshGraph.Vertices[vertex], EdgeType.Walk)
        };

        meshGraph.AddBackwardLegs(scratch, vertex, raw);

        return FinishPiece(meshGraph.Mesh, raw, to);
    }

    /// <summary>
    ///     Builds the walk the last forward mesh search found from the search's source to a target, from the mesh edges and
    ///     hops it took.
    /// </summary>
    /// <param name="meshGraph">The map's vertex graph the search ran over.</param>
    /// <param name="scratch">The calling thread's scratch, holding the search.</param>
    /// <param name="from">The node the search ran from.</param>
    /// <param name="seeds">
    ///     The seeds of <paramref name="to" />, read off the search.
    /// </param>
    /// <param name="to">The node the walk reaches.</param>
    /// <returns>
    ///     The legs, empty when <paramref name="from" /> already stands in the target's reach, or null when the search reached
    ///     none of the seeds.
    /// </returns>
    internal List<GraphEdge>? BuildForwardPiece(
        MeshGraph meshGraph,
        MeshScratch scratch,
        GraphNode from,
        List<MeshGraph.Seed> seeds,
        GraphNode to)
    {
        if (to.Reach.Contains(from.Location.X, from.Location.Y))
            return [];

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (MeshGraph.FindCheapestSeed(scratch, seeds, out var vertex) == float.MaxValue)
            return null;

        var raw = new List<GraphEdge>();
        meshGraph.AddForwardLegs(scratch, vertex, raw);
        var first = raw.Count > 0 ? raw[0].Start : meshGraph.Vertices[vertex];
        raw.Insert(0, meshGraph.Mesh.ConstructEdge(from, first, EdgeType.Walk));

        return FinishPiece(meshGraph.Mesh, raw, to);
    }

    /// <summary>
    ///     Closes a walk's raw legs onto its target, drops the legs that go nowhere, and smooths it.
    /// </summary>
    /// <param name="mesh">The map's mesh.</param>
    /// <param name="raw">The raw legs, changed in place.</param>
    /// <param name="to">The node the walk reaches.</param>
    /// <returns>The walk's legs.</returns>
    private List<GraphEdge> FinishPiece(NodeMesh mesh, List<GraphEdge> raw, GraphNode to)
    {
        raw.Add(mesh.ConstructEdge(raw[^1].End, to, EdgeType.Walk));
        raw.RemoveAll(leg => (leg.Type == EdgeType.Walk) && (leg.Cost <= 0f));

        return EnhancePath(raw);
    }

    /// <summary>
    ///     Calculates the price of a walk's legs: each walk at its length, each hop at its own.
    /// </summary>
    /// <param name="legs">The legs.</param>
    /// <returns>The price.</returns>
    internal static float CalculateCost(List<GraphEdge> legs)
    {
        var cost = 0f;

        foreach (var leg in legs)
            cost += leg.Cost;

        return cost;
    }

    /// <summary>
    ///     Replays the in-motion penalty score along a walk's hops, from a score carried in and decayed along every walk leg.
    /// </summary>
    /// <param name="legs">The walk's legs.</param>
    /// <param name="score">The score at the walk's start.</param>
    /// <param name="speed">The character's speed.</param>
    /// <returns>
    ///     The highest score along the hops, 0 for a walk without hops, and the score at the walk's end.
    /// </returns>
    internal static (float Peak, float End) ReplayHops(List<GraphEdge> legs, float score, float speed)
    {
        var peak = 0f;

        foreach (var leg in legs)
            if (leg.Type == EdgeType.Hop)
            {
                (var legPeak, var end) = HopRisk.Replay(
                    leg.HopPenalties,
                    leg.Cost,
                    speed,
                    score);

                peak = Math.Max(peak, legPeak);
                score = end;
            } else if (score > 0f)
                score = HopRisk.DecayWhileWalking(score, leg.Cost / speed);

        return (peak, score);
    }

    /// <summary>
    ///     Finds where a blink aimed at a target lands: at an exit the nearest cell the server lands inside its reach, and at
    ///     an end the edge of its radius nearest the caster.
    /// </summary>
    /// <param name="target">The exit or end the cast is aimed at.</param>
    /// <param name="from">Where the caster stands.</param>
    /// <returns>The landing.</returns>
    private Point FindBlinkLanding(ILocation target, ILocation from)
    {
        var mesh = NavMeshes[target.Map].NavMesh;
        var landing = new Point(target.X, target.Y);

        return target switch
        {
            Exit exit when mesh.FindStartTriangle(exit.X, exit.Y, out var entry) >= 0 => mesh.FindBlinkLanding(
                new Reach(exit.ReachBand, exit.ReachRange),
                entry),
            Exit        => landing,
            ICircle end => mesh.FindEndLanding(end, new Point(from.X, from.Y)),
            _           => landing
        };
    }

    /// <summary>
    ///     Finds the wall hops on every map that allows them.
    /// </summary>
    /// <param name="navMeshes">Each map's ground, by map key.</param>
    /// <returns>Each map's hops.</returns>
    private static Dictionary<string, List<Hop>> FindHops(IReadOnlyDictionary<string, NavMesh> navMeshes)
        => navMeshes.Where(kvp => GameData.Maps[kvp.Key] is { } gMap && HopBuilder.AllowsHops(gMap))

                    //each map's hops are its own, so they are found side by side
                    .AsParallel()
                    .Select(kvp => (Map: kvp.Key, Hops: HopBuilder.FindHops(kvp.Value)))
                    .ToList()
                    .ToDictionary(pair => pair.Map, pair => pair.Hops, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Finds the cheapest route from a start to any of several ends: the walks out of the start and into each end are
    ///     searched over their maps' vertices and priced smoothed, then joined through the walks found at build.
    /// </summary>
    /// <param name="start">Where the route starts.</param>
    /// <param name="ends">The candidate ends.</param>
    /// <param name="options">How the route is priced.</param>
    /// <typeparam name="T">The type of the ends.</typeparam>
    /// <returns>The route's legs.</returns>
    /// <exception cref="InvalidOperationException">
    ///     The start has no mesh or ground nearby, or no end can be reached.
    /// </exception>
    private List<GraphEdge> FindRoute<T>(ILocation start, T[] ends, PathOptions options) where T: ILocation, ICircle
    {
        if (!MeshGraphs.TryGetValue(start.Map, out var startGraph))
            throw new InvalidOperationException($"No mesh found for map {start.Map}");

        var startMesh = startGraph.Mesh;
        var result = new List<GraphEdge>();

        //a start the server would refuse to move from steps onto the nearest accepted point first
        if (!startMesh.IsWalkable(start) && startMesh.TryFindNearestWalkable(start, out var steppedOut))
        {
            var freeLocation = new Location(start.Map, steppedOut);

            result.Add(startMesh.ConstructEdge(startMesh.ConstructNode(start), startMesh.ConstructNode(freeLocation), EdgeType.Walk));
            start = freeLocation;
        }

        var startSeeds = new List<MeshGraph.Seed>();
        startGraph.FindSeeds(start, startSeeds);

        if (startSeeds.Count == 0)
            throw new InvalidOperationException($"No walkable ground near {ILocation.ToString(start)}.");

        var scratch = SearchScratch.Rent();
        var meshScratch = MeshScratch.Rent();
        var pieces = new Dictionary<(int From, int To), List<GraphEdge>>();

        //walks out of the start or into an end that take a hop, beside the walk without hops in pieces
        var hopPieces = new Dictionary<(int From, int To), List<GraphEdge>>();
        var targets = new List<int>();
        var hopSpeed = options.UseHops && options.WalkSpeed is > 0f ? options.WalkSpeed : null;
        var townCost = CONSTANTS.CalculateTownCost(options.WalkSpeed ?? CONSTANTS.NOMINAL_WALK_SPEED);
        var blinkOn = options.BlinkCost is not null;
        var origin = startMesh.ConstructNode(start);
        var startNode = Doors.Nodes.Count;
        var firstEnd = startNode + 1;
        var nodeCount = firstEnd + ends.Length;
        var endGraphs = new MeshGraph?[ends.Length];
        var endNodes = new GraphNode?[ends.Length];
        var endSeeds = new List<MeshGraph.Seed>[ends.Length];
        var hopped = new List<int>();
        scratch.ResetNodes(nodeCount);

        //ends: resolved onto their meshes; one that resolves nowhere is simply not a candidate
        for (var j = 0; j < ends.Length; j++)
        {
            endSeeds[j] = [];

            if (!MeshGraphs.TryGetValue(ends[j].Map, out var endGraph)
                || !TryResolveEnd(
                    endGraph,
                    ends[j],
                    endSeeds[j],
                    out var endNode))
                continue;

            endGraphs[j] = endGraph;
            endNodes[j] = endNode;
        }

        //walks into each end: one backward search of the end's map from the end, read at every arrival
        for (var j = 0; j < ends.Length; j++)
        {
            if (endGraphs[j] is not { } endGraph)
                continue;

            var arrivals = Doors.GetArrivals(ends[j].Map);
            targets.Clear();
            hopped.Clear();

            foreach (var arrival in arrivals)
                foreach (var seed in Doors.Nodes[arrival].Seeds)
                    targets.Add(seed.Vertex);

            if (hopSpeed is { } backwardSpeed)
                endGraph.SearchBackward(
                    meshScratch,
                    endSeeds[j],
                    targets,
                    backwardSpeed);
            else
                endGraph.Search(meshScratch, endSeeds[j], targets);

            foreach (var arrival in arrivals)
                if (!AddPiece(arrival, firstEnd + j, BuildEndPiece(endGraph, arrival, j)))
                    hopped.Add(arrival);

            if (hopSpeed is null || (hopped.Count == 0))
                continue;

            //a route that reaches the arrival carrying a score may not take a walk with hops checked from 0
            endGraph.Search(meshScratch, endSeeds[j], targets);

            foreach (var arrival in hopped)
                AddPiece(arrival, firstEnd + j, BuildEndPiece(endGraph, arrival, j));
        }

        //walks out of the start: one forward search of the start's map, to its exits and the ends on it
        var startTargets = new List<(int Node, List<MeshGraph.Seed> Seeds, GraphNode Target)>();

        foreach (var departure in Doors.GetDepartures(start.Map))
            startTargets.Add((departure, Doors.Nodes[departure].Seeds, Doors.Nodes[departure].Node));

        for (var j = 0; j < ends.Length; j++)
            if (ReferenceEquals(endGraphs[j], startGraph))
                startTargets.Add((firstEnd + j, endSeeds[j], endNodes[j]!));

        targets.Clear();
        hopped.Clear();

        foreach (var startTarget in startTargets)
            foreach (var seed in startTarget.Seeds)
                targets.Add(seed.Vertex);

        if (hopSpeed is { } forwardSpeed)
            startGraph.SearchForward(
                meshScratch,
                startSeeds,
                targets,
                forwardSpeed,
                options.HopScore);
        else
            startGraph.Search(meshScratch, startSeeds, targets);

        for (var i = 0; i < startTargets.Count; i++)
            if (!AddPiece(startNode, startTargets[i].Node, BuildStartPiece(i)))
                hopped.Add(i);

        //a walk without hops leaves the start carrying less score, which a hop past a door may need
        if (hopSpeed is not null && (hopped.Count > 0))
        {
            startGraph.Search(meshScratch, startSeeds, targets);

            foreach (var i in hopped)
                AddPiece(startNode, startTargets[i].Node, BuildStartPiece(i));
        }

        if (options.UseTown && Doors.TryGetTown(start.Map, out var town))
            scratch.SearchEdges.Add(
                new DoorSearch.Edge(
                    startNode,
                    town,
                    EdgeType.Town,
                    0f));

        if ((Doors.MainSpawn >= 0) && CONSTANTS.CanLeave(start.Map))
            scratch.SearchEdges.Add(
                new DoorSearch.Edge(
                    startNode,
                    Doors.MainSpawn,
                    EdgeType.Leave,
                    CONSTANTS.TRANSPORT_HEURISTIC));

        scratch.IndexSearchEdges(nodeCount);

        //the stored walks with hops are found at speed bands, up to the fastest speed a hop is built for
        var band = 0;

        var doors = hopSpeed is { } bandSpeed && DoorTable.TryFindBand(bandSpeed, out band) ? Doors.WithHops[band] : Doors.WithoutHops;

        var winner = doors.Search(
            scratch,
            options,
            townCost,
            startNode,
            firstEnd,
            nodeCount);

        if (winner < 0)
            throw new InvalidOperationException($"No path from {ILocation.ToString(start)} to any of {ends.Length} end(s).");

        var cursor = origin;

        foreach (var arrivalId in DoorSearch.ReadChain(scratch, winner))
        {
            var edge = doors.GetEdge(scratch, scratch.Arrivals[arrivalId].EdgeIndex);
            var isEnd = edge.To >= firstEnd;
            var target = isEnd ? endNodes[edge.To - firstEnd]! : Doors.Nodes[edge.To].Node;

            switch (edge.Type)
            {
                //a walk cast instead, or a pair no walk joins: one teleport to the target, costed at the walk it replaces or
                //the straight line where there is none
                case EdgeType.Blink:
                case EdgeType.Walk when scratch.Arrivals[arrivalId].Blinked:
                {
                    var landing = FindBlinkLanding(target.Location, cursor.Location);
                    var ruler = isEnd ? cursor.Location.Distance(target.Location) : cursor.Location.Distance(landing);

                    result.Add(
                        new GraphEdge
                        {
                            Start = cursor,
                            End = NavMeshes[target.Location.Map]
                                .ConstructNode(new Location(target.Location.Map, landing)),
                            Cost = edge.Type == EdgeType.Blink ? ruler : edge.Cost,
                            Type = EdgeType.Blink
                        });

                    break;
                }

                case EdgeType.Walk:
                {
                    var walks = (edge.From == startNode) || isEnd
                        ? edge.EndScore is not null ? hopPieces : pieces
                        : edge.MinSpeed > 0f
                            ? Doors.HopWalks[band]
                            : Doors.Walks;

                    var legs = walks[(edge.From, edge.To)];

                    if (hopSpeed is { } replaySpeed)
                        legs = ScoreHops(legs, scratch.Arrivals[scratch.Arrivals[arrivalId].Parent].HopScore, replaySpeed);

                    //a walk that ends exactly on the end ends on the caller's end object, and a start already inside the
                    //reach gets one zero-cost leg that stays put
                    if (isEnd && (legs.Count == 0))
                    {
                        if (result.Count == 0)
                            result.Add(
                                new GraphEdge
                                {
                                    Start = cursor,
                                    End = cursor,
                                    Type = EdgeType.Walk
                                });
                        else if (IPoint.Comparer.Equals(result[^1].End.Location, target.Location))
                            result[^1] = new GraphEdge
                            {
                                Start = result[^1].Start,
                                End = target,
                                Cost = result[^1].Cost,
                                Type = result[^1].Type,
                                HopPenalties = result[^1].HopPenalties,
                                HopScore = result[^1].HopScore
                            };
                    }

                    result.AddRange(legs);

                    break;
                }

                case EdgeType.Door:
                case EdgeType.Transport:
                case EdgeType.Leave:
                    result.Add(
                        new GraphEdge
                        {
                            Start = edge.Type == EdgeType.Leave ? cursor : Doors.Nodes[edge.From].Node,
                            End = target,
                            Cost = edge.Cost,
                            Type = edge.Type
                        });

                    break;

                case EdgeType.Town:
                    result.Add(
                        new GraphEdge
                        {
                            Start = cursor,
                            End = target,
                            Cost = townCost,
                            Type = EdgeType.Town
                        });

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Type));
            }

            cursor = target;
        }

        return result;

        List<GraphEdge>? BuildStartPiece(int index)
            => BuildForwardPiece(
                startGraph,
                meshScratch,
                origin,
                startTargets[index].Seeds,
                startTargets[index].Target);

        List<GraphEdge>? BuildEndPiece(MeshGraph endGraph, int arrival, int endIndex)
            => BuildBackwardPiece(
                endGraph,
                meshScratch,
                Doors.Nodes[arrival].Node,
                Doors.Nodes[arrival].Seeds,
                endNodes[endIndex]!);

        //adds a walk as a search edge, or a blink where there is none; a walk whose hops are refused once replayed along its
        //smoothed legs is none. False for a walk with hops or none, so an end can add the walk without hops beside it
        bool AddPiece(int from, int to, List<GraphEdge>? legs)
        {
            var fromStart = from == startNode;
            float? endScore = null;

            if (legs is not null && hopSpeed is { } replaySpeed && legs.Exists(leg => leg.Type == EdgeType.Hop))
            {
                //a walk out of the start carries the start's score; a walk into an end is checked from 0
                (var peak, var end) = ReplayHops(legs, fromStart ? options.HopScore : 0f, replaySpeed);

                if (peak >= HopRisk.SAFE_SCORE)
                    legs = null;
                else
                    endScore = end;
            }

            if (legs is null)
            {
                if (blinkOn)
                    scratch.SearchEdges.Add(
                        new DoorSearch.Edge(
                            from,
                            to,
                            EdgeType.Blink,
                            float.MaxValue));

                return false;
            }

            var checkedFromZero = endScore is not null && !fromStart;

            (endScore is not null ? hopPieces : pieces)[(from, to)] = legs;

            scratch.SearchEdges.Add(
                new DoorSearch.Edge(
                    from,
                    to,
                    EdgeType.Walk,
                    CalculateCost(legs),
                    MinSpeed: checkedFromZero ? Math.Max(1f, MathF.Floor(hopSpeed!.Value)) : 0f,
                    EndScore: endScore));

            return endScore is null;
        }
    }

    /// <summary>
    ///     Determines whether a walk may legitimately stop on the near edge of a destination's radius, coming from a point.
    /// </summary>
    /// <param name="mesh">The destination's mesh.</param>
    /// <param name="from">Where the walk comes from.</param>
    /// <param name="end">The destination.</param>
    /// <returns>
    ///     <c>true</c> if the destination has a radius whose near edge is walkable and clear of <paramref name="from" />;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     An NPC recorded off the flood fill still has ordinary ground all around it, so a centre nothing can walk to is not
    ///     on its own a reason to discard the radius.
    /// </remarks>
    private static bool HasReachableEdge(NodeMesh mesh, ILocation from, ILocation end)
    {
        if (end is not ICircle { Radius: > 0 } circle)
            return false;

        (var x, var y) = Reach.CreateCircle(circle.X, circle.Y, circle.Radius)
                              .FindNearestEdgePoint(from.X, from.Y);
        var nearEdge = new Location(end.Map, x, y);

        return mesh.IsWalkable(nearEdge) && mesh.CanMove(from, nearEdge);
    }

    /// <summary>
    ///     Copies a walk's legs with each hop leg carrying the in-motion penalty score it lands with, replayed from a score
    ///     carried in.
    /// </summary>
    /// <param name="legs">The walk's legs.</param>
    /// <param name="score">The score at the walk's start.</param>
    /// <param name="speed">The character's speed.</param>
    /// <returns>
    ///     The scored legs, or <paramref name="legs" /> itself when the walk takes no hop.
    /// </returns>
    private static List<GraphEdge> ScoreHops(List<GraphEdge> legs, float score, float speed)
    {
        if (legs.TrueForAll(leg => leg.Type != EdgeType.Hop))
            return legs;

        var scored = new List<GraphEdge>(legs.Count);

        foreach (var leg in legs)
        {
            if (leg.Type != EdgeType.Hop)
            {
                if (score > 0f)
                    score = HopRisk.DecayWhileWalking(score, leg.Cost / speed);

                scored.Add(leg);

                continue;
            }

            (_, score) = HopRisk.Replay(
                leg.HopPenalties,
                leg.Cost,
                speed,
                score);

            scored.Add(
                new GraphEdge
                {
                    Start = leg.Start,
                    End = leg.End,
                    Cost = leg.Cost,
                    Type = leg.Type,
                    HopPenalties = leg.HopPenalties,
                    HopScore = score
                });
        }

        return scored;
    }

    /// <summary>
    ///     Resolves an end onto its map's vertices: the end itself where a walk can reach it, else the nearest standable
    ///     point, else the nearest vertex, which is as close as a character can legitimately get.
    /// </summary>
    /// <param name="meshGraph">The end's map's vertex graph.</param>
    /// <param name="end">The end.</param>
    /// <param name="seeds">Filled with the vertices the end is joined to.</param>
    /// <param name="target">The node a walk to the end stops on.</param>
    /// <returns>
    ///     <c>true</c> if the mesh has a vertex to join the end to; otherwise, <c>false</c>.
    /// </returns>
    private static bool TryResolveEnd(
        MeshGraph meshGraph,
        ILocation end,
        List<MeshGraph.Seed> seeds,
        [NotNullWhen(true)] out GraphNode? target)
    {
        target = null;
        var mesh = meshGraph.Mesh;
        meshGraph.FindSeeds(end, seeds);

        if (seeds.Count == 0)
            return false;

        if (mesh.GetContainingTriangle(end) is not null)
        {
            target = mesh.ConstructNode(end);

            return true;
        }

        var nearest = meshGraph.Vertices[seeds[0].Vertex];

        //the radius is lost on a standable point, so the walk runs to it rather than stopping at the destination's near edge
        if (mesh.CanMove(nearest.Location, end) || HasReachableEdge(mesh, nearest.Location, end))
            target = mesh.ConstructNode(end);
        else if (mesh.TryFindNearestWalkable(end, out var standable) && mesh.CanMove(nearest.Location, standable))
            target = mesh.ConstructNode(new Location(end.Map, standable));
        else
        {
            target = nearest;

            seeds[0] = seeds[0] with
            {
                Cost = 0f
            };
        }

        return true;
    }

    private static float CalculateLength(List<GraphNode> line)
    {
        var length = 0f;

        for (var i = 1; i < line.Count; i++)
            length += line[i - 1]
                      .Location
                      .Distance(line[i].Location);

        return length;
    }

    private static int CountSharedCorners(IGenericTriangle<GraphNode> triangle, IGenericTriangle<GraphNode> other)
    {
        var shared = 0;

        for (var i = 0; i < 3; i++)
            if (other.Vertices.Contains(triangle.Vertices[i]))
                shared++;

        return shared;
    }

    /// <summary>
    ///     Slides each bend of a line along the triangle edges at its corner and along its two legs, to the farthest point
    ///     from which both legs meeting there stay clear, when that shortens the line.
    /// </summary>
    /// <param name="navMesh">The mesh the line is on.</param>
    /// <param name="line">The line, changed in place.</param>
    private static void SlideBends(NodeMesh navMesh, List<GraphNode> line)
    {
        for (var b = 1; (b + 1) < line.Count; b++)
        {
            var bend = line[b];
            var previous = line[b - 1].Location;
            var next = line[b + 1].Location;
            var best = (IPoint)bend.Location;
            var bestLength = previous.Distance(bend.Location) + bend.Location.Distance(next);

            //the triangle edges at the bend, and the two legs meeting there
            var directions = navMesh.FindTrianglesAt(bend)
                                    .SelectMany(triangle => triangle.Vertices)
                                    .Where(corner => !ReferenceEquals(corner, bend))
                                    .Select(corner => (IPoint)corner.Location)
                                    .Append(previous)
                                    .Append(next);

            foreach (var direction in directions)
            {
                var candidate = FindFarthestClearPoint(
                    navMesh,
                    previous,
                    bend.Location,
                    direction,
                    next);

                var length = previous.Distance(candidate) + candidate.Distance(next);

                if (length >= bestLength)
                    continue;

                best = candidate;
                bestLength = length;
            }

            if (!ReferenceEquals(best, bend.Location))
                line[b] = navMesh.ConstructNode(new Location(bend.Location.Map, best));
        }
    }

    /// <summary>
    ///     Binary-searches a triangle edge or leg for the farthest point from a bend at which a line through it stays clear.
    /// </summary>
    /// <param name="navMesh">The mesh the edge is on.</param>
    /// <param name="previous">The point the line comes from.</param>
    /// <param name="bend">The bend, at the edge's first end.</param>
    /// <param name="corner">The other end of the triangle edge or leg.</param>
    /// <param name="next">The point the line goes on to.</param>
    /// <returns>
    ///     The farthest clear point found, or the bend itself.
    /// </returns>
    private static IPoint FindFarthestClearPoint(
        NodeMesh navMesh,
        IPoint previous,
        IPoint bend,
        IPoint corner,
        IPoint next)
    {
        //the search stops once the interval is shorter than this many units along the edge
        const float RESOLUTION = 1f;

        var dx = corner.X - bend.X;
        var dy = corner.Y - bend.Y;
        var edgeLength = MathF.Sqrt(dx * dx + dy * dy);
        var best = bend;

        if (IsClear(corner))
            return corner;

        var lo = 0f;
        var hi = 1f;

        while (((hi - lo) * edgeLength) > RESOLUTION)
        {
            var t = (lo + hi) / 2f;
            var candidate = new Point(bend.X + dx * t, bend.Y + dy * t);

            if (IsClear(candidate))
            {
                lo = t;
                best = candidate;
            } else
                hi = t;
        }

        return best;

        bool IsClear(IPoint point) => navMesh.IsWalkable(point) && navMesh.CanMove(previous, point) && navMesh.CanMove(point, next);
    }

    /// <summary>
    ///     Pulls one stretch of walk legs tight and cuts it where it enters the reach of the node it ends on.
    /// </summary>
    /// <param name="stretch">Consecutive walk legs on one map.</param>
    /// <param name="legs">The list the smoothed legs are added to.</param>
    private void SmoothStretch(List<GraphEdge> stretch, List<GraphEdge> legs)
    {
        if (stretch.Count == 0)
            return;

        var smooth = SmoothScratch.Rent();
        var navMesh = NavMeshes[stretch[0].Start.Location.Map];
        var points = smooth.Points;
        points.Clear();
        points.Add(stretch[0].Start);

        foreach (var edge in stretch)
            points.Add(edge.End);

        List<GraphNode> line;
        var tighten = true;

        //the funnel, then straightened; a stretch it cannot pull is straightened alone
        if (TryPullFunnel(navMesh, points, out var pulled))
        {
            line = Straighten(navMesh, pulled);
            tighten = false;
        } else
        {
            Interlocked.Increment(ref FunnelFallbacks);
            line = Straighten(navMesh, points);
        }

        var reach = points[^1].Reach;

        //cut before sliding, so the bends ahead of the cut slide against the true final stop
        if (reach.Range > 0)
        {
            TrimToReach(navMesh, line, reach);

            //the stretch's own points can touch the reach sooner than the smoothed line does
            var cutPoints = smooth.CutPoints;
            cutPoints.Clear();
            cutPoints.AddRange(points);
            TrimToReach(navMesh, cutPoints, reach);

            if (CalculateLength(cutPoints) < CalculateLength(line))
            {
                line = cutPoints;
                tighten = true;
            }
        }

        if (tighten)
            SlideBends(navMesh, line);

        var legStart = line[0];

        //a bend slid onto its neighbour, or two raw points on one spot, would make a leg that goes nowhere
        for (var i = 1; i < line.Count; i++)
        {
            if (IPoint.Comparer.Equals(legStart.Location, line[i].Location))
                continue;

            legs.Add(navMesh.ConstructEdge(legStart, line[i], EdgeType.Walk));
            legStart = line[i];
        }
    }

    /// <summary>
    ///     Joins each point to the farthest later point it can walk to in a straight line.
    /// </summary>
    /// <param name="navMesh">The mesh the points are on.</param>
    /// <param name="points">The stretch's points in order.</param>
    /// <returns>The points kept.</returns>
    private static List<GraphNode> Straighten(NodeMesh navMesh, List<GraphNode> points)
    {
        var line = new List<GraphNode>
        {
            points[0]
        };

        for (var i = 0; (i + 1) < points.Count;)
        {
            var next = i + 1;

            //scan back from the end and stop at the first point in a straight line
            for (var j = points.Count - 1; j > next; j--)
                if (navMesh.CanMove(points[i].Location, points[j].Location))
                {
                    next = j;

                    break;
                }

            line.Add(points[next]);
            i = next;
        }

        return line;
    }

    /// <summary>
    ///     Cuts a line where it first reaches a reach: straight to the reach's nearest point when that is clear, otherwise
    ///     where a leg first crosses into it.
    /// </summary>
    /// <param name="navMesh">The mesh the line is on.</param>
    /// <param name="line">The line, changed in place.</param>
    /// <param name="reach">Where the line counts as arrived.</param>
    internal static void TrimToReach(NodeMesh navMesh, List<GraphNode> line, Reach reach)
    {
        for (var i = 0; (i + 1) < line.Count; i++)
        {
            var current = line[i].Location;

            if (reach.Contains(current.X, current.Y))
            {
                line.RemoveRange(i + 1, line.Count - i - 1);

                return;
            }

            (var nearX, var nearY) = reach.FindNearestEdgePoint(current.X, current.Y);
            var nearest = new Location(current.Map, nearX, nearY);

            if (navMesh.CanMove(current, nearest))
            {
                line.RemoveRange(i + 1, line.Count - i - 1);
                line.Add(navMesh.ConstructNode(nearest));

                return;
            }

            var following = line[i + 1].Location;

            if (reach.TryFindFirstEntry(
                    current.X,
                    current.Y,
                    following.X,
                    following.Y,
                    out var entryX,
                    out var entryY))
            {
                line.RemoveRange(i + 1, line.Count - i - 1);
                line.Add(navMesh.ConstructNode(new Location(current.Map, entryX, entryY)));

                return;
            }
        }
    }

    /// <summary>
    ///     Pulls a stretch through the triangles it crosses with <see cref="Funnel" />, as far as the triangles can be
    ///     chained, and straightens the rest.
    /// </summary>
    /// <param name="navMesh">The mesh the points are on.</param>
    /// <param name="points">
    ///     The stretch's points in order; every one but the first and last is a mesh vertex.
    /// </param>
    /// <param name="line">The pulled line.</param>
    /// <returns>
    ///     <c>true</c> if at least the first leg's triangles could be chained and the line is clear and no longer than the
    ///     stretch; otherwise, <c>false</c> .
    /// </returns>
    private static bool TryPullFunnel(NodeMesh navMesh, List<GraphNode> points, out List<GraphNode> line)
    {
        line = [];

        if (!navMesh.TryFindTriangle(points[0].Location, out var first))
            return false;

        var smooth = SmoothScratch.Rent();
        var corridor = smooth.Corridor;
        var extended = smooth.Extended;
        corridor.Clear();
        corridor.Add(first);

        //from the first point no triangle can be chained to, such as a door on the wall, the rest is straightened instead
        var pulledCount = 1;

        while (pulledCount < points.Count)
        {
            var target = points[pulledCount].Location;

            //extended on a copy, since a failed extension can leave it partway along the leg
            if (!NodeMesh.ContainsPoint(corridor[^1], target))
            {
                extended.Clear();
                extended.AddRange(corridor);

                if (!TryExtendCorridor(
                        navMesh,
                        points[pulledCount - 1],
                        target,
                        extended))
                    break;

                (corridor, extended) = (extended, corridor);
            }

            pulledCount++;
        }

        smooth.Corridor = corridor;
        smooth.Extended = extended;

        if (pulledCount < 2)
            return false;

        var start = points[0].Location;
        var end = points[pulledCount - 1].Location;
        var pulled = smooth.Pulled;

        smooth.EnsurePortals(corridor.Count);
        smooth.Corners.Clear();

        foreach (var triangle in corridor)
            for (var c = 0; c < 3; c++)
            {
                var corner = triangle.Vertices[c];
                smooth.Corners.TryAdd((corner.Location.X, corner.Location.Y), corner);
            }

        //the portal into each next triangle is the edge opposite the corner it does not share
        for (var t = 0; (t + 1) < corridor.Count; t++)
        {
            var corners = corridor[t].Vertices;
            var slot = FindOppositeSlot(corridor[t], corridor[t + 1]);
            var a = corners[(slot + 1) % 3].Location;
            var b = corners[(slot + 2) % 3].Location;

            Funnel.SetPortal(
                smooth.Lefts,
                smooth.Rights,
                t,
                (corners[0].Location.X + corners[1].Location.X + corners[2].Location.X) / 3f,
                (corners[0].Location.Y + corners[1].Location.Y + corners[2].Location.Y) / 3f,
                new Point(a.X, a.Y),
                new Point(b.X, b.Y));
        }

        Funnel.PullThroughPortals(
            new Point(start.X, start.Y),
            new Point(end.X, end.Y),
            smooth.Lefts.AsSpan(0, corridor.Count),
            smooth.Rights.AsSpan(0, corridor.Count),
            pulled);

        line.Add(points[0]);

        //every bend the funnel pulls is a corridor corner
        for (var i = 1; (i + 1) < pulled.Count; i++)
            line.Add(
                smooth.Corners.TryGetValue((pulled[i].X, pulled[i].Y), out var corner)
                    ? corner
                    : navMesh.ConstructNode(new Location(start.Map, pulled[i])));

        line.Add(points[pulledCount - 1]);

        for (var i = 1; i < line.Count; i++)
            if (!navMesh.CanMove(line[i - 1].Location, line[i].Location))
            {
                line = [];

                return false;
            }

        //the tail is the stretch's own legs, which straightening joins to the pulled line
        if (pulledCount < points.Count)
        {
            for (var i = pulledCount; i < points.Count; i++)
                line.Add(points[i]);

            line = Straighten(navMesh, line);
        }

        //a corridor that lost a loop no longer holds the stretch, so its shortest line can be the longer way round
        if (CalculateLength(line) <= CalculateLength(points))
            return true;

        line = [];

        return false;
    }

    /// <summary>
    ///     Extends a corridor through the fan of triangles round a vertex, to the nearest one holding a target.
    /// </summary>
    /// <param name="navMesh">The mesh the vertex is on.</param>
    /// <param name="pivot">
    ///     The vertex the last triangle of the corridor has as a corner.
    /// </param>
    /// <param name="target">The point to reach.</param>
    /// <param name="corridor">
    ///     The corridor, extended in place. A triangle it already holds cuts it back to that triangle.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a triangle of the fan holds the target; otherwise, <c>false</c> .
    /// </returns>
    private static bool TryRotateAround(
        NodeMesh navMesh,
        GraphNode pivot,
        IPoint target,
        List<IGenericTriangle<GraphNode>> corridor)
    {
        var fan = navMesh.FindTrianglesAt(pivot);
        var from = corridor[^1];

        if (!fan.Contains(from))
            return false;

        var smooth = SmoothScratch.Rent();
        var parents = smooth.Parents;
        var queue = smooth.Queue;
        parents.Clear();
        queue.Clear();
        parents[from] = from;
        queue.Enqueue(from);

        while (queue.TryDequeue(out var current))
        {
            if (NodeMesh.ContainsPoint(current, target))
            {
                var chain = smooth.Chain;
                chain.Clear();

                for (var triangle = current; !ReferenceEquals(triangle, from); triangle = parents[triangle])
                    chain.Add(triangle);

                for (var i = chain.Count - 1; i >= 0; i--)
                    AppendTriangle(corridor, chain[i]);

                return true;
            }

            for (var f = 0; f < fan.Count; f++)
            {
                var next = fan[f];

                if (!parents.ContainsKey(next) && (CountSharedCorners(current, next) == 2))
                {
                    parents[next] = current;
                    queue.Enqueue(next);
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Extends a corridor across the triangles one leg of a stretch crosses, ending in the triangle that holds the leg's
    ///     end.
    /// </summary>
    /// <param name="navMesh">The mesh the leg is on.</param>
    /// <param name="from">
    ///     The leg's start, which the corridor's last triangle holds.
    /// </param>
    /// <param name="target">The leg's end.</param>
    /// <param name="corridor">The corridor, extended in place.</param>
    /// <returns>
    ///     <c>true</c> if the corridor reached a triangle holding the target; otherwise, <c>false</c> .
    /// </returns>
    private static bool TryExtendCorridor(
        NodeMesh navMesh,
        GraphNode from,
        IPoint target,
        List<IGenericTriangle<GraphNode>> corridor)
    {
        //the distance along the leg at which the triangle it leaves a vertex through is found
        const float PROBE_DISTANCE = 0.5f;

        if (TryRotateAround(
                navMesh,
                from,
                target,
                corridor))
            return true;

        var start = from.Location;
        var dx = target.X - start.X;
        var dy = target.Y - start.Y;
        var length = MathF.Sqrt(dx * dx + dy * dy);

        if (length <= PROBE_DISTANCE)
            return false;

        //an end that is not in the fan round a vertex: leave the fan the way the leg does, then follow the leg
        if (navMesh.FindTrianglesAt(from)
                   .Count
            > 0)
        {
            var probe = new Point(start.X + dx / length * PROBE_DISTANCE, start.Y + dy / length * PROBE_DISTANCE);

            if (!TryRotateAround(
                    navMesh,
                    from,
                    probe,
                    corridor))
                return false;
        }

        return TryFollowLeg(
            navMesh,
            start,
            target,
            corridor);
    }

    /// <summary>
    ///     Steps a corridor from triangle to triangle across each edge a leg crosses, until a triangle holds the leg's end.
    /// </summary>
    /// <param name="navMesh">The mesh the leg is on.</param>
    /// <param name="start">The leg's start.</param>
    /// <param name="end">The leg's end.</param>
    /// <param name="corridor">
    ///     The corridor, extended in place; its last triangle lies on the leg.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a triangle holding the end was reached; otherwise, <c>false</c> .
    /// </returns>
    private static bool TryFollowLeg(
        NodeMesh navMesh,
        IPoint start,
        IPoint end,
        List<IGenericTriangle<GraphNode>> corridor)
    {
        //a leg on a valid mesh crosses far fewer triangles; this only bounds a malformed one
        const int MAX_STEPS = 4096;

        for (var step = 0; step < MAX_STEPS; step++)
        {
            var current = corridor[^1];

            if (NodeMesh.ContainsPoint(current, end))
                return true;

            var previous = corridor.Count > 1 ? corridor[^2] : null;
            IGenericTriangle<GraphNode>? next = null;

            for (var slot = 0; (slot < 3) && next is null; slot++)
            {
                var u = current.Vertices[(slot + 1) % 3];
                var v = current.Vertices[(slot + 2) % 3];

                if (!CrossesProperly(
                        start,
                        end,
                        u.Location,
                        v.Location))
                    continue;

                var fan = navMesh.FindTrianglesAt(u);

                for (var f = 0; (f < fan.Count) && next is null; f++)
                    if (!ReferenceEquals(fan[f], current)
                        && !ReferenceEquals(fan[f], previous)
                        && fan[f]
                           .Vertices
                           .Contains(v))
                        next = fan[f];
            }

            //the leg leaves the mesh, or passes exactly through a corner
            if (next is null)
                return false;

            AppendTriangle(corridor, next);
        }

        return false;
    }

    /// <summary>
    ///     Determines whether two segments cross at a point inside both, not merely touching.
    /// </summary>
    /// <param name="a1">The first segment's start.</param>
    /// <param name="a2">The first segment's end.</param>
    /// <param name="b1">The second segment's start.</param>
    /// <param name="b2">The second segment's end.</param>
    /// <returns>
    ///     <c>true</c> if the segments cross; otherwise, <c>false</c> .
    /// </returns>
    private static bool CrossesProperly(
        IPoint a1,
        IPoint a2,
        IPoint b1,
        IPoint b2)
    {
        var d1 = CalculateCross(b1, b2, a1);
        var d2 = CalculateCross(b1, b2, a2);
        var d3 = CalculateCross(a1, a2, b1);
        var d4 = CalculateCross(a1, a2, b2);

        return (((d1 > 0) && (d2 < 0)) || ((d1 < 0) && (d2 > 0))) && (((d3 > 0) && (d4 < 0)) || ((d3 < 0) && (d4 > 0)));

        static double CalculateCross(IPoint o, IPoint a, IPoint b) => (double)(a.X - o.X) * (b.Y - o.Y) - (double)(a.Y - o.Y) * (b.X - o.X);
    }

    /// <summary>
    ///     Adds a triangle to the end of a corridor, or cuts the corridor back to it when it already holds it.
    /// </summary>
    /// <param name="corridor">The corridor, changed in place.</param>
    /// <param name="triangle">
    ///     A triangle sharing an edge with the corridor's last.
    /// </param>
    private static void AppendTriangle(List<IGenericTriangle<GraphNode>> corridor, IGenericTriangle<GraphNode> triangle)
    {
        var index = corridor.IndexOf(triangle);

        if (index >= 0)
            corridor.RemoveRange(index + 1, corridor.Count - index - 1);
        else
            corridor.Add(triangle);
    }
}