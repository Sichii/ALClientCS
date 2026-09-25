#region
using System.Runtime.InteropServices;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Definitions;
using Chaos.Extensions.Common;
using DoorLockType = AL.Core.Definitions.DoorLockType;
using ExitType = AL.Core.Definitions.ExitType;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     The cross-map graph: arrival nodes (spawns that something lands on), departure nodes (exits), and the static edges
///     between them, with walk costs funnelled once at build. A search adds a virtual start and the ends, runs Dijkstra
///     over the nodes, and expands the winning chain into legs.
/// </summary>
internal sealed class PortalGraph
{
    /// <summary>
    ///     The most maps one graph can hold: the floor rules keep the maps a route has visited as bits of a
    ///     <see cref="UInt128" />.
    /// </summary>
    private const int MAX_MAPS = 128;

    private static readonly List<int> EmptyNodes = [];

    /// <summary>
    ///     Each map's index, the bit it takes in the floor rules' visited and closed sets.
    /// </summary>
    private readonly Dictionary<string, int> MapIndex = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Each node's map, as its <see cref="MapIndex" />.</summary>
    private readonly int[] NodeMap;

    private readonly int[] ReverseEdges;
    private readonly int[] ReverseStart;

    private readonly Dictionary<(string Map, int Spawn), int> ArrivalIndex = new(MapSpawnComparer.Instance);
    private readonly Dictionary<string, List<int>> ArrivalsOnMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<int>> DeparturesOnMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly int[] EdgeStart;

    private readonly IReadOnlyDictionary<string, NavMesh> Meshes;
    private readonly List<Node> Nodes = [];

    /// <summary>
    ///     The static edges in CSR form by <see cref="Edge.From" />.
    /// </summary>
    private readonly Edge[] StaticEdges;

    public PortalGraph(IReadOnlyDictionary<string, NavMesh> meshes)
    {
        Meshes = meshes;

        if (meshes.Count > MAX_MAPS)
            throw new InvalidOperationException(
                $"The portal graph holds {meshes.Count} maps; the blink floor rules track at most {MAX_MAPS}.");

        foreach (var map in meshes.Keys)
            MapIndex[map] = MapIndex.Count;

        foreach ((var map, var mesh) in meshes)
        {
            var gMap = GameData.Maps[map];

            if (gMap is null)
                continue;

            Lists(map);

            if (gMap.Spawns.Count > 0)
                AddArrival(mesh, gMap, 0);

            //a map's doors are its own whether it is irregular or not
            foreach (var exit in gMap.Exits)
            {
                if (exit.Type == ExitType.Door)
                {
                    var door = gMap.Doors.FirstOrDefault(d => IPoint.Comparer.Equals(d, exit));

                    //a key door is not a walk, and the way back out lands on the entry spawn - so left in, the round
                    //trip prices as the cheapest route to the spot in front of the door
                    if (door is { LockType: DoorLockType.AccountLocked or DoorLockType.Key })
                        continue;
                }

                if (!meshes.TryGetValue(exit.ToLocation.Map, out var toMesh))
                    continue;

                var toMap = GameData.Maps[exit.ToLocation.Map];

                if (toMap is null || (exit.ToSpawnIndex < 0) || (exit.ToSpawnIndex >= toMap.Spawns.Count))
                    continue;

                AddArrival(toMesh, toMap, exit.ToSpawnIndex);

                var triangle = mesh.Locate(exit.X, exit.Y, out var entry);

                if (triangle < 0)
                    continue;

                var index = Nodes.Count;
                var reach = new Reach(exit.ReachBand, exit.ReachRange);

                Nodes.Add(
                    new Node
                    {
                        Mesh = mesh,
                        Location = exit,
                        Triangle = triangle,
                        Entry = entry,
                        BlinkEntry = BlinkLanding(mesh, reach, entry),
                        Exit = exit,
                        Reach = reach
                    });

                DeparturesOnMap[map]
                    .Add(index);
            }
        }

        (StaticEdges, EdgeStart) = BuildStaticEdges();
        (ReverseEdges, ReverseStart) = BuildReverseEdges();
        NodeMap = new int[Nodes.Count];

        for (var i = 0; i < Nodes.Count; i++)
            NodeMap[i] = MapIndex[Nodes[i].Location.Map];
    }

    /// <summary>
    ///     The static edges into each node, as indices into <see cref="StaticEdges" /> in CSR form by <see cref="Edge.To" />,
    ///     for the backward search behind the lower bounds.
    /// </summary>
    private (int[] Edges, int[] Start) BuildReverseEdges()
    {
        var start = new int[Nodes.Count + 1];

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

    private void AddArrival(NavMesh mesh, GMap map, int spawnIndex)
    {
        if (ArrivalIndex.ContainsKey((map.Accessor, spawnIndex)))
            return;

        var spawn = map.Spawns[spawnIndex];
        var triangle = mesh.Locate(spawn.X, spawn.Y, out var entry);

        if (triangle < 0)
            return;

        var index = Nodes.Count;

        Nodes.Add(
            new Node
            {
                Mesh = mesh,
                Location = new Location(map.Accessor, spawn),
                Triangle = triangle,
                Entry = entry,
                BlinkEntry = entry,
                SpawnIndex = spawnIndex
            });

        ArrivalIndex[(map.Accessor, spawnIndex)] = index;

        Lists(map.Accessor)
            .Arrivals
            .Add(index);
    }

    private (Edge[] Edges, int[] Start) BuildStaticEdges()
    {
        var edges = new List<Edge>();
        var scratch = SearchScratch.Rent();
        var mainSpawn = ArrivalIndex.TryGetValue(("main", 0), out var main) ? main : -1;

        for (var index = 0; index < Nodes.Count; index++)
        {
            var node = Nodes[index];
            var map = node.Location.Map;

            if (node.Exit is { } exit)
            {
                //an arrival that resolved onto no triangle was never added, and an exit landing on it goes nowhere
                if (ArrivalIndex.TryGetValue((exit.ToLocation.Map, exit.ToSpawnIndex), out var to))
                {
                    var type = exit.Type == ExitType.Door ? EdgeType.Door : EdgeType.Transport;

                    //the bank is the maps the server mounts it on; a door between two of its floors is an ordinary door
                    var entersBank = GameData.Maps[exit.ToLocation.Map] is { Mount: true } && GameData.Maps[map] is not { Mount: true };

                    edges.Add(
                        new Edge(
                            index,
                            to,
                            type,
                            entersBank ? CONSTANTS.BANK_DOOR_COST : CONSTANTS.TRANSPORT_HEURISTIC));
                }

                continue;
            }

            //walk edges to every departure on the map, priced at the pulled length
            node.Mesh.Search(node.Triangle, node.Entry, scratch);

            foreach (var departure in DeparturesOnMap[map])
            {
                var target = Nodes[departure];

                var cost = node.Mesh.WalkCost(
                    node.Entry,
                    target.Triangle,
                    target.Entry,
                    target.Reach,
                    scratch);

                //float.MaxValue is WalkCost's unreachable sentinel, so this is an identity test, not a measurement.
                //a pair no walk joins is still one cast apart - a blink lands anywhere on the map - so it gets a
                //blink-only edge, priced per search and ignored while blink is off
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (cost == float.MaxValue)
                {
                    edges.Add(
                        new Edge(
                            index,
                            departure,
                            EdgeType.Blink,
                            float.MaxValue));

                    continue;
                }

                edges.Add(
                    new Edge(
                        index,
                        departure,
                        EdgeType.Walk,
                        node.Entry.Distance(node.Location) + cost));
            }

            //a recall from anywhere the character lands, to the map's town spawn; priced per search. Not on a dungeon
            //floor: whether the server honours a recall there is not public, and a route promising a cast the server
            //refuses fails where a longer walk does not
            var gMap = GameData.Maps[map];

            if ((node.SpawnIndex != 0) && gMap is { Boundless: false, Generated: null } && ArrivalIndex.TryGetValue((map, 0), out var town))
                edges.Add(
                    new Edge(
                        index,
                        town,
                        EdgeType.Town,
                        0f));

            if ((mainSpawn >= 0) && CONSTANTS.AcceptsLeave(map))
                edges.Add(
                    new Edge(
                        index,
                        mainSpawn,
                        EdgeType.Leave,
                        CONSTANTS.TRANSPORT_HEURISTIC));
        }

        edges.Sort((a, b) => a.From.CompareTo(b.From));
        var start = new int[Nodes.Count + 1];

        foreach (var edge in edges)
            start[edge.From + 1]++;

        for (var i = 1; i < start.Length; i++)
            start[i] += start[i - 1];

        return (edges.ToArray(), start);
    }

    /// <summary>
    ///     The cheapest route from <paramref name="start" /> to any of <paramref name="ends" />. A start that already
    ///     satisfies an end is routed as a single zero-cost <see cref="EdgeType.Walk" /> leg from the start to itself, so the
    ///     route is never empty and a caller always finds where it arrived at the last leg; a walk stops inside an end's reach
    ///     rather than on its centre, and this one has no distance left to walk.
    /// </summary>
    /// <exception cref="InvalidOperationException">No end can be reached.</exception>
    public IReadOnlyList<PathEdge> FindPath<T>(ILocation start, IEnumerable<T> ends, PathOptions options) where T: ILocation, ICircle
    {
        if (!Meshes.TryGetValue(start.Map, out var startMesh))
            throw new InvalidOperationException($"No mesh for the map \"{start.Map}\".");

        var scratch = SearchScratch.Rent();
        var townCost = CONSTANTS.TownCost(options.WalkSpeed ?? CONSTANTS.NOMINAL_WALK_SPEED);

        var blinkOn = options.BlinkCost is not null;
        var result = new List<PathEdge>();

        //a start the server would refuse to move from steps onto the nearest accepted cell first
        var startPoint = new Point(start.X, start.Y);
        var cursor = start;

        if (!startMesh.IsWalkable(startPoint.X, startPoint.Y) && startMesh.TryFindNearestWalkable(startPoint, out var unstuck))
        {
            var unstuckLocation = new Location(start.Map, unstuck);

            result.Add(
                new PathEdge(
                    EdgeType.Walk,
                    start,
                    unstuckLocation,
                    startPoint.Distance(unstuck)));
            startPoint = new Point(unstuck.X, unstuck.Y);
            cursor = unstuckLocation;
        }

        var startTriangle = startMesh.Locate(startPoint.X, startPoint.Y, out var startEntry);

        if (startTriangle < 0)
            throw new InvalidOperationException($"No walkable ground near {ILocation.ToString(start)}.");

        //ends: resolved onto their meshes; one that resolves nowhere is simply not a candidate
        var endList = ends as IReadOnlyList<T> ?? ends.ToList();
        scratch.ResetEnds(endList.Count);

        for (var j = 0; j < endList.Count; j++)
        {
            var end = endList[j];

            if (!Meshes.TryGetValue(end.Map, out var endMesh))
                continue;

            var triangle = endMesh.Locate(end.X, end.Y, out scratch.EndEntry[j]);

            if (triangle < 0)
                continue;

            scratch.EndMesh[j] = endMesh;
            scratch.EndTriangle[j] = triangle;
            scratch.EndReach[j] = Reach.Circle(end.X, end.Y, end.Radius);
        }

        var startNode = Nodes.Count;
        var firstEnd = Nodes.Count + 1;
        scratch.ResetNodes(firstEnd + endList.Count);

        //per-search edges into each end: one search of the end's map from the end, read at every arrival. The walk
        //is priced reversed, so the trim to the end's reach lands on the end the character walks toward
        for (var j = 0; j < endList.Count; j++)
        {
            if (scratch.EndMesh[j] is not { } endMesh)
                continue;

            endMesh.Search(scratch.EndTriangle[j], scratch.EndEntry[j], scratch);

            var endOffset = scratch.EndEntry[j]
                                   .Distance(endList[j]);

            foreach (var arrival in NodesOn(ArrivalsOnMap, endList[j].Map))
            {
                var node = Nodes[arrival];

                var cost = endMesh.WalkCost(
                    scratch.EndEntry[j],
                    node.Triangle,
                    node.Entry,
                    scratch.EndReach[j],
                    scratch,
                    true);

                if (cost < float.MaxValue)
                    scratch.SearchEdges.Add(
                        new Edge(
                            arrival,
                            firstEnd + j,
                            EdgeType.Walk,
                            endOffset + cost));
                else if (blinkOn)
                    scratch.SearchEdges.Add(
                        new Edge(
                            arrival,
                            firstEnd + j,
                            EdgeType.Blink,
                            float.MaxValue));
            }
        }

        //per-search edges out of the start: read off one search of the start map, run last so the first leg
        //walked below can read it too
        startMesh.Search(startTriangle, startEntry, scratch);
        var startOffset = startPoint.Distance(startEntry);
        var startDepartures = NodesOn(DeparturesOnMap, start.Map);

        foreach (var departure in startDepartures)
        {
            var node = Nodes[departure];

            var cost = startMesh.WalkCost(
                startEntry,
                node.Triangle,
                node.Entry,
                node.Reach,
                scratch);

            if (cost < float.MaxValue)
                scratch.SearchEdges.Add(
                    new Edge(
                        startNode,
                        departure,
                        EdgeType.Walk,
                        startOffset + cost));
            else if (blinkOn)
                scratch.SearchEdges.Add(
                    new Edge(
                        startNode,
                        departure,
                        EdgeType.Blink,
                        float.MaxValue));
        }

        for (var j = 0; j < endList.Count; j++)
        {
            if (!ReferenceEquals(scratch.EndMesh[j], startMesh))
                continue;

            var cost = startMesh.WalkCost(
                startEntry,
                scratch.EndTriangle[j],
                scratch.EndEntry[j],
                scratch.EndReach[j],
                scratch);

            if (cost < float.MaxValue)
                scratch.SearchEdges.Add(
                    new Edge(
                        startNode,
                        firstEnd + j,
                        EdgeType.Walk,
                        startOffset + cost));
            else if (blinkOn)
                scratch.SearchEdges.Add(
                    new Edge(
                        startNode,
                        firstEnd + j,
                        EdgeType.Blink,
                        float.MaxValue));
        }

        var startMap = GameData.Maps[start.Map];

        if (options.UseTown && startMap is { Boundless: false } && ArrivalIndex.TryGetValue((start.Map, 0), out var startTown))
            scratch.SearchEdges.Add(
                new Edge(
                    startNode,
                    startTown,
                    EdgeType.Town,
                    0f));

        if (CONSTANTS.AcceptsLeave(start.Map) && ArrivalIndex.TryGetValue(("main", 0), out var mainSpawn))
            scratch.SearchEdges.Add(
                new Edge(
                    startNode,
                    mainSpawn,
                    EdgeType.Leave,
                    CONSTANTS.TRANSPORT_HEURISTIC));

        var nodeCount = firstEnd + endList.Count;
        scratch.IndexSearchEdges(nodeCount);

        Array.Copy(NodeMap, scratch.NodeMap, Nodes.Count);
        scratch.NodeMap[startNode] = MapIndex[start.Map];

        //an end that resolved nowhere has no edges, so its map is never read
        for (var j = 0; j < endList.Count; j++)
            scratch.NodeMap[firstEnd + j] = MapIndex.GetValueOrDefault(endList[j].Map);

        if (blinkOn)
            ComputeLowerBounds(
                scratch,
                startNode,
                firstEnd,
                nodeCount,
                options,
                townCost);

        //the floor rules only ever remove routes, so a route found without them that obeys them anyway is the cheapest
        //legal one, and the dearer search that tracks them runs only when it does not
        var winner = Search(
            scratch,
            new ArrivalSearch(
                this,
                scratch,
                options,
                townCost,
                startNode,
                false),
            options,
            firstEnd);

        if (blinkOn
            && (winner >= 0)
            && !ObeysFloorRules(
                scratch,
                winner,
                startNode,
                options.BlinkCost!.Value))
        {
            scratch.ResetArrivals(nodeCount);

            winner = Search(
                scratch,
                new ArrivalSearch(
                    this,
                    scratch,
                    options,
                    townCost,
                    startNode,
                    true),
                options,
                firstEnd);
        }

        if (winner < 0)
            throw new InvalidOperationException($"No path from {ILocation.ToString(start)} to any of {endList.Count} end(s).");

        //each arrival on the chain names the edge that made it and whether that edge was cast rather than walked. Its own
        //list, since TryWalk below reuses the corridor list
        var chain = ReadChain(scratch, winner);

        var cursorPoint = startPoint;
        var cursorMesh = startMesh;
        var cursorTriangle = startTriangle;
        var cursorEntry = startEntry;

        //the scratch still holds the start map's search, which the first walk is read from
        var startSearchHeld = true;

        foreach (var arrivalId in chain)
        {
            var edgeIndex = scratch.Arrivals[arrivalId].EdgeIndex;
            var edge = edgeIndex < StaticEdges.Length ? StaticEdges[edgeIndex] : scratch.SearchEdges[edgeIndex - StaticEdges.Length];
            var blinked = scratch.Arrivals[arrivalId].Blinked;

            switch (edge.Type)
            {
                //a walk the search chose to cast instead, or a pair no walk joins at all: one teleport from
                //wherever the cursor stands to the target, carrying the walked length it replaces - or, where there is
                //no walk, the ruler between the two. no funnel, since the server resolves a landing against the point
                //asked for rather than walking there. nothing walks out of a landing: an exit's only edges are its
                //door or transporter, and an end is the last node
                case EdgeType.Blink:
                case EdgeType.Walk when blinked:
                {
                    if (edge.To >= firstEnd)
                    {
                        var endIndex = edge.To - firstEnd;
                        var target = endList[endIndex];
                        var ruler = cursorPoint.Distance(new Point(target.X, target.Y));
                        var landing = EndLanding(scratch.EndMesh[endIndex]!, target, cursorPoint);

                        result.Add(
                            new PathEdge(
                                EdgeType.Blink,
                                cursor,
                                new Location(target.Map, landing),
                                edge.Type == EdgeType.Blink ? ruler : edge.Cost));
                        cursor = target;
                    } else
                    {
                        var exit = Nodes[edge.To];

                        result.Add(
                            new PathEdge(
                                EdgeType.Blink,
                                cursor,
                                new Location(exit.Location.Map, exit.BlinkEntry),
                                edge.Type == EdgeType.Blink ? cursorPoint.Distance(exit.BlinkEntry) : edge.Cost));
                        MoveCursor(exit);
                    }

                    startSearchHeld = false;

                    break;
                }

                case EdgeType.Walk:
                {
                    var isEnd = edge.To >= firstEnd;
                    var endIndex = edge.To - firstEnd;
                    var target = isEnd ? endList[endIndex] : Nodes[edge.To].Location;
                    var targetTriangle = isEnd ? scratch.EndTriangle[endIndex] : Nodes[edge.To].Triangle;
                    var reach = isEnd ? scratch.EndReach[endIndex] : Nodes[edge.To].Reach;
                    var targetEntry = isEnd ? scratch.EndEntry[endIndex] : Nodes[edge.To].Entry;
                    var targetPoint = new Point(target.X, target.Y);

                    //a cursor outside every triangle walks to the vertex the search started from first
                    if (!NavMesh.IsSame(cursorPoint, cursorEntry))
                    {
                        var entryLocation = new Location(cursor.Map, cursorEntry);

                        result.Add(
                            new PathEdge(
                                EdgeType.Walk,
                                cursor,
                                entryLocation,
                                cursorPoint.Distance(cursorEntry)));
                        cursor = entryLocation;
                        cursorPoint = cursorEntry;
                    }

                    if (!startSearchHeld || !ReferenceEquals(cursorMesh, startMesh) || (cursorTriangle != startTriangle))
                        cursorMesh.Search(
                            cursorTriangle,
                            cursorEntry,
                            scratch,
                            targetTriangle,
                            targetEntry);

                    startSearchHeld = false;

                    if (!cursorMesh.TryWalk(
                            cursorPoint,
                            targetTriangle,
                            targetEntry,
                            reach,
                            scratch,
                            scratch.Polyline))
                        throw new InvalidOperationException($"The route to {ILocation.ToString(target)} could not be walked.");

                    var polyline = scratch.Polyline;

                    //a leg that already landed exactly on the end - a door onto the destination's own spawn - ends on
                    //the caller's end object the way a walk there would, so the destination is still found by equality.
                    //a start that already satisfies the end has no leg to rewrite and gets one zero-cost leg that
                    //stays put: it is already inside the reach, and a walk never continues on to the centre
                    if (isEnd && (polyline.Count < 2))
                    {
                        if (result.Count == 0)
                            result.Add(
                                new PathEdge(
                                    EdgeType.Walk,
                                    cursor,
                                    cursor,
                                    0f));
                        else if (NavMesh.IsSame(cursorPoint, targetPoint))
                            result[^1] = result[^1] with
                            {
                                End = target
                            };
                    }

                    for (var i = 1; i < polyline.Count; i++)
                    {
                        var last = i == (polyline.Count - 1);

                        //the last leg ends on the caller's own end object where it reaches the end exactly, so a
                        //caller can find its destination in the path by equality
                        var next = last && isEnd && NavMesh.IsSame(polyline[i], targetPoint)
                            ? target
                            : new Location(cursor.Map, polyline[i]);

                        result.Add(
                            new PathEdge(
                                EdgeType.Walk,
                                cursor,
                                next,
                                polyline[i - 1]
                                    .Distance(polyline[i])));
                        cursor = next;
                        cursorPoint = polyline[i];
                    }

                    break;
                }

                case EdgeType.Door:
                case EdgeType.Transport:
                {
                    var arrival = Nodes[edge.To];

                    result.Add(
                        new PathEdge(
                            edge.Type,
                            Nodes[edge.From].Location,
                            arrival.Location,
                            edge.Cost));
                    MoveCursor(arrival);

                    break;
                }

                case EdgeType.Town:
                {
                    var arrival = Nodes[edge.To];

                    result.Add(
                        new PathEdge(
                            EdgeType.Town,
                            cursor,
                            arrival.Location,
                            townCost));
                    MoveCursor(arrival);

                    break;
                }

                case EdgeType.Leave:
                {
                    var arrival = Nodes[edge.To];

                    result.Add(
                        new PathEdge(
                            EdgeType.Leave,
                            cursor,
                            arrival.Location,
                            edge.Cost));
                    MoveCursor(arrival);

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Type));
            }
        }

        return result;

        void MoveCursor(Node arrival)
        {
            cursor = arrival.Location;
            cursorPoint = new Point(arrival.Location.X, arrival.Location.Y);
            cursorMesh = arrival.Mesh;
            cursorTriangle = arrival.Triangle;
            cursorEntry = arrival.Entry;
        }
    }

    /// <summary>
    ///     Build time only: grows the per-map lists. The search reads them through <see cref="NodesOn" /> and never touches
    ///     the dictionaries.
    /// </summary>
    private (List<int> Arrivals, List<int> Departures) Lists(string map)
    {
        if (!ArrivalsOnMap.TryGetValue(map, out var arrivals))
        {
            arrivals = [];
            ArrivalsOnMap[map] = arrivals;
        }

        if (!DeparturesOnMap.TryGetValue(map, out var departures))
        {
            departures = [];
            DeparturesOnMap[map] = departures;
        }

        return (arrivals, departures);
    }

    /// <summary>
    ///     A map that got no nodes has an empty list; searches on several threads read these at once.
    /// </summary>
    private static List<int> NodesOn(Dictionary<string, List<int>> byMap, string map)
        => byMap.TryGetValue(map, out var nodes) ? nodes : EmptyNodes;

    /// <summary>
    ///     The character's speed for pricing, nominal when unset or not positive, the same rule
    ///     <see cref="CONSTANTS.TownCost" /> applies.
    /// </summary>
    private static float SpeedOf(PathOptions options) => options.WalkSpeed is > 0f ? options.WalkSpeed.Value : CONSTANTS.NOMINAL_WALK_SPEED;

    /// <summary>
    ///     The route to <paramref name="winner" /> as arrival ids from the first move to the last, read back through the
    ///     parents into the scratch's chain list.
    /// </summary>
    private static List<int> ReadChain(SearchScratch scratch, int winner)
    {
        var chain = scratch.Chain;
        chain.Clear();

        for (var arrivalId = winner; scratch.Arrivals[arrivalId].Parent >= 0; arrivalId = scratch.Arrivals[arrivalId].Parent)
            chain.Add(arrivalId);

        chain.Reverse();

        return chain;
    }

    /// <summary>
    ///     Fills the scratch's lower bounds: per node, the least the rest of any route to an end can be priced, from one
    ///     backward search over every edge at its cheapest. A walk a cast could replace is priced at the smaller of the walk
    ///     and a cast's landing alone, a bridged pair at the landing, and doors and recalls at their fixed prices.
    /// </summary>
    /// <remarks>
    ///     A bound that never overestimates and never drops by more than an edge's price between neighbours keeps the first
    ///     arrival taken at an end the cheapest, and arrivals at one node share it, so no keep-or-drop decision changes.
    /// </remarks>
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
        var landing = CONSTANTS.BLINK_LANDING_MS * SpeedOf(options) / 1000f;
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

            var bound = Cheapest(edge);

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
                var through = bound + Cheapest(edge);

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
            bounds[startNode] = Math.Min(bounds[startNode], Cheapest(edge) + bounds[edge.To]);
        }

        return;

        float Cheapest(in Edge edge)
            => edge.Type switch
            {
                EdgeType.Walk  => edge.Cost >= floor ? Math.Min(edge.Cost, landing) : edge.Cost,
                EdgeType.Blink => landing,
                EdgeType.Town  => useTown ? townCost : float.PositiveInfinity,
                _              => edge.Cost
            };
    }

    /// <summary>
    ///     The edge an arrival names: a static edge's index, or the static count plus a search edge's.
    /// </summary>
    private Edge EdgeAt(SearchScratch scratch, int edgeIndex)
        => edgeIndex < StaticEdges.Length ? StaticEdges[edgeIndex] : scratch.SearchEdges[edgeIndex - StaticEdges.Length];

    /// <summary>
    ///     Whether the route to <paramref name="winner" />, found without the floor rules, obeys them anyway: every cast is on
    ///     a first visit to a map no earlier cast closed and lands at least <paramref name="floor" /> of walking from where
    ///     the route entered the map, and no door leads back into a closed map. The same rules the full search applies.
    /// </summary>
    private bool ObeysFloorRules(
        SearchScratch scratch,
        int winner,
        int startNode,
        float floor)
    {
        var anchor = startNode;
        var visited = UInt128.One << scratch.NodeMap[startNode];
        var closed = UInt128.Zero;
        var revisit = false;
        var at = startNode;

        foreach (var arrivalId in ReadChain(scratch, winner))
        {
            var arrival = scratch.Arrivals[arrivalId];
            var here = scratch.NodeMap[at];

            if (arrival.Blinked)
            {
                if (revisit || ((closed & (UInt128.One << here)) != UInt128.Zero) || (WalkBetween(scratch, anchor, arrival.Node) < floor))
                    return false;

                closed = visited;
            } else if (EdgeAt(scratch, arrival.EdgeIndex)
                           .Type is EdgeType.Door or EdgeType.Transport or EdgeType.Leave)
            {
                var map = scratch.NodeMap[arrival.Node];
                var bit = UInt128.One << map;

                if (map != here)
                {
                    if ((closed & bit) != UInt128.Zero)
                        return false;

                    revisit = (visited & bit) != UInt128.Zero;
                }

                visited |= bit;
                anchor = arrival.Node;
            }

            at = arrival.Node;
        }

        return true;
    }

    /// <summary>
    ///     Runs one pass of the search from the start, cheapest first, and returns the first arrival taken at an end, or -1
    ///     when none can be reached.
    /// </summary>
    private int Search(
        SearchScratch scratch,
        in ArrivalSearch search,
        PathOptions options,
        int firstEnd)
    {
        search.OfferStart(options);

        while (scratch.ArrivalQueue.TryDequeue(out var arrivalId, out _))
        {
            var from = scratch.Arrivals[arrivalId];

            if (from.Dropped)
                continue;

            if (from.Node >= firstEnd)
                return arrivalId;

            if (from.Node < Nodes.Count)
                for (var i = EdgeStart[from.Node]; i < EdgeStart[from.Node + 1]; i++)
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
    ///     The walk from <paramref name="from" /> to <paramref name="to" /> on one map, or <see cref="float.MaxValue" /> when
    ///     no walk joins them.
    /// </summary>
    private float WalkBetween(SearchScratch scratch, int from, int to)
    {
        if (from < Nodes.Count)
            for (var i = EdgeStart[from]; i < EdgeStart[from + 1]; i++)
                if ((StaticEdges[i].To == to) && (StaticEdges[i].Type == EdgeType.Walk))
                    return StaticEdges[i].Cost;

        for (var i = scratch.SearchEdgeStart[from]; i < scratch.SearchEdgeStart[from + 1]; i++)
            if ((scratch.SearchEdges[i].To == to) && (scratch.SearchEdges[i].Type == EdgeType.Walk))
                return scratch.SearchEdges[i].Cost;

        return float.MaxValue;
    }

    /// <summary>
    ///     One pass of one search: relaxes an edge out of an arrival into the arrivals it makes, applies the floor rules to
    ///     every cast, and keeps at each node only the arrivals no other one there beats.
    /// </summary>
    private readonly struct ArrivalSearch
    {
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

        private readonly PortalGraph Graph;

        /// <summary>
        ///     Whether the anchor, visited, closed and revisit rules are tracked and enforced. Rule 1, the walk a cast replaces,
        ///     holds either way.
        /// </summary>
        private readonly bool Rules;

        private readonly SearchScratch Scratch;
        private readonly float Speed;
        private readonly int StartNode;
        private readonly float TownCost;
        private readonly bool UseTown;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrivalSearch" /> struct for one pass of one search.
        /// </summary>
        /// <param name="graph">
        ///     The graph searched, whose nodes say which of them can hold an anchor.
        /// </param>
        /// <param name="scratch">
        ///     The calling thread's scratch, holding the arrivals and, with blink on, the lower bounds.
        /// </param>
        /// <param name="options">The search's pricing and starting state.</param>
        /// <param name="townCost">The price of a recall at the character's speed.</param>
        /// <param name="startNode">The search's virtual start node.</param>
        /// <param name="rules">
        ///     Specifies whether the floor rules past the first are tracked; ignored while blink is off.
        /// </param>
        public ArrivalSearch(
            PortalGraph graph,
            SearchScratch scratch,
            PathOptions options,
            float townCost,
            int startNode,
            bool rules)
        {
            Graph = graph;
            Scratch = scratch;
            BlinkOn = options.BlinkCost is not null;
            Clock = BlinkOn ? new BlinkClock(options) : default;
            Floor = options.BlinkCost ?? float.MaxValue;
            Rules = BlinkOn && rules;
            Speed = SpeedOf(options);
            StartNode = startNode;
            TownCost = townCost;
            UseTown = options.UseTown;
        }

        /// <summary>Offers the arrival every route starts from.</summary>
        public void OfferStart(PathOptions options)
            => Offer(
                new SearchScratch.Arrival
                {
                    Node = StartNode,
                    State = BlinkOn ? Clock.Start(options) : default,
                    Parent = -1,
                    EdgeIndex = -1,
                    Anchor = Rules ? StartNode : -1,
                    Visited = Rules ? MapBit(StartNode) : UInt128.Zero
                });

        /// <summary>
        ///     Offers the arrivals one edge makes out of <paramref name="origin" />: one per move, and a walk at least the floor
        ///     long offers both the walk and the cast that replaces it.
        /// </summary>
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
                Anchor = AnchorAt(edge.To, origin.Anchor),
                Blinked = false,
                Dropped = false
            };

            switch (edge.Type)
            {
                //recall off means no recall anywhere on the route: the static town edges out of every arrival node are
                //the same move as the one out of the start, and a caller that cannot recall here cannot recall there.
                //a recall stays on the map, so the anchor stays where the route entered it
                case EdgeType.Town:
                {
                    if (!UseTown)
                        return;

                    next.Cost = origin.Cost + TownCost;

                    if (BlinkOn)
                        next.State = BlinkClock.AddPenalty(
                            Clock.Pass(origin.State, CONSTANTS.TOWN_CHANNEL_SECONDS * 1000f),
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

                    if (Rules)
                    {
                        var map = Scratch.NodeMap[edge.To];
                        var bit = UInt128.One << map;

                        //a door between two floors of one map continues the same stay
                        if (map != Scratch.NodeMap[origin.Node])
                        {
                            //a map visited before the last cast is closed: coming back would let the cast skip part of
                            //a walk shorter than the floor
                            if ((origin.Closed & bit) != UInt128.Zero)
                                return;

                            next.Revisit = (origin.Visited & bit) != UInt128.Zero;
                        }

                        next.Visited = origin.Visited | bit;
                        next.Anchor = edge.To;
                    }

                    Offer(next);

                    return;
                }

                case EdgeType.Walk:
                    next.Cost = origin.Cost + edge.Cost;

                    if (BlinkOn)
                        next.State = Clock.Pass(origin.State, edge.Cost / Speed * 1000f);

                    Offer(next);

                    //rule 1: a cast may replace only a walk at least the floor long
                    if (edge.Cost >= Floor)
                        OfferBlink(edge, next, origin);

                    return;

                //a pair only a cast joins stays unjoined while blink is off
                case EdgeType.Blink:
                    OfferBlink(edge, next, origin);

                    return;
            }
        }

        /// <summary>
        ///     The anchor an arrival at <paramref name="node" /> carries. Only the start and an arrival node have a walk or a
        ///     recall out of them; a departure's only edge is its door, which moves the anchor, and an end is the last node.
        ///     Storing none there lets arrivals that differ only in a dead anchor beat each other.
        /// </summary>
        private int AnchorAt(int node, int anchor)
            => (node == StartNode) || ((node < Graph.Nodes.Count) && Graph.Nodes[node].Exit is null) ? anchor : -1;

        /// <summary>
        ///     The bit of <paramref name="node" />'s map in a visited or closed set.
        /// </summary>
        private UInt128 MapBit(int node) => UInt128.One << Scratch.NodeMap[node];

        /// <summary>
        ///     Adds <paramref name="arrival" /> unless an arrival already at its node beats it, and drops every arrival there it
        ///     beats. Nothing is added at a node no end can be reached from.
        /// </summary>
        private void Offer(in SearchScratch.Arrival arrival)
        {
            var node = arrival.Node;
            var lowerBound = BlinkOn ? Scratch.LowerBound[node] : 0f;

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

            //with blink off the queue is ordered by cost alone, so routes are exactly what they were before the bound
            Scratch.ArrivalQueue.Enqueue(newId, arrival.Cost + lowerBound);
        }

        /// <summary>
        ///     Offers the cast along <paramref name="edge" />, priced at the time it takes in walk units at the character's speed;
        ///     nothing when blink is off, a floor rule forbids it, or the bar can never pay for it.
        /// </summary>
        /// <param name="edge">
        ///     The walk the cast replaces, or the pair it bridges.
        /// </param>
        /// <param name="next">
        ///     The walked arrival along the same edge, which the cast differs from only in price, state and closed maps.
        /// </param>
        /// <param name="origin">The arrival the cast is made from.</param>
        private void OfferBlink(in Edge edge, SearchScratch.Arrival next, in SearchScratch.Arrival origin)
        {
            if (!BlinkOn)
                return;

            if (Rules)
            {
                //rule 3: no cast on a map closed by an earlier one, nor on a return to a map
                if (origin.Revisit || ((origin.Closed & MapBit(origin.Node)) != UInt128.Zero))
                    return;

                //rule 2: nor one landing nearer than the floor to where the route entered the map, which a recall or a
                //walk to somewhere else first would otherwise split into shorter pieces
                if (Graph.WalkBetween(Scratch, origin.Anchor, edge.To) < Floor)
                    return;

                next.Closed = origin.Visited;
            }

            if (!Clock.TryBlink(origin.State, out var spentMs, out var after))
                return;

            next.Cost = origin.Cost + spentMs * Speed / 1000f;
            next.State = after;
            next.Blinked = true;
            Offer(next);
        }

        /// <summary>
        ///     Whether <paramref name="a" /> makes <paramref name="b" /> at the same node pointless: no dearer, at least as ready,
        ///     the same anchor and revisit, and no map visited or closed that <paramref name="b" /> has not.
        /// </summary>
        private static bool Beats(in SearchScratch.Arrival a, in SearchScratch.Arrival b)
            => (a.Cost <= b.Cost)
               && BlinkClock.AtLeastAsReady(a.State, b.State)
               && (a.Anchor == b.Anchor)
               && (a.Revisit == b.Revisit)
               && ((a.Visited & ~b.Visited) == UInt128.Zero)
               && ((a.Closed & ~b.Closed) == UInt128.Zero);
    }

    internal readonly record struct Edge(
        int From,
        int To,
        EdgeType Type,
        float Cost);

    private sealed class MapSpawnComparer : IEqualityComparer<(string Map, int Spawn)>
    {
        public static readonly MapSpawnComparer Instance = new();

        public bool Equals((string Map, int Spawn) x, (string Map, int Spawn) y) => (x.Spawn == y.Spawn) && x.Map.EqualsI(y.Map);

        public int GetHashCode((string Map, int Spawn) obj)
            => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Map), obj.Spawn);
    }

    /// <summary>
    ///     The lattice a blink's landing is rounded onto, and the step the server checks around it.
    /// </summary>
    private const float BLINK_LATTICE = 10f;

    /// <summary>
    ///     How far out from an exit's entry a landing is looked for, in lattice steps. Wide enough to cover the door's reach,
    ///     which runs to <see cref="AL.Core.Definitions.CONSTANTS.DOOR_RANGE" /> edge-to-edge.
    /// </summary>
    private const int BLINK_LANDING_RINGS = 12;

    /// <summary>
    ///     Where a blink aimed at this exit is sent, which is not always the exit's own entry.
    /// </summary>
    /// <remarks>
    ///     The server accepts a landing only where the lattice cell it rounds to and the eight around it are all ground, and
    ///     it looks no further than three cells along the axes for one itself. An exit's entry is its position pulled onto the
    ///     edge of the ground, so on some doors every cast aimed at one is refused: Spooky Forest's door to Spooky Town is the
    ///     one that cost a character, since the only landing near it lies diagonally, which the server's own search never
    ///     tries. A door opens from anywhere inside its reach, so the landing moves to the nearest cell in there that the
    ///     server will take.
    /// </remarks>
    private static Point BlinkLanding(NavMesh mesh, Reach reach, Point entry)
    {
        if (Lands(mesh, entry.X, entry.Y))
            return entry;

        var originX = MathF.Round(entry.X / BLINK_LATTICE) * BLINK_LATTICE;
        var originY = MathF.Round(entry.Y / BLINK_LATTICE) * BLINK_LATTICE;

        for (var ring = 1; ring <= BLINK_LANDING_RINGS; ring++)
            for (var dx = -ring; dx <= ring; dx++)
                for (var dy = -ring; dy <= ring; dy++)
                {
                    if ((Math.Abs(dx) != ring) && (Math.Abs(dy) != ring))
                        continue;

                    var x = originX + dx * BLINK_LATTICE;
                    var y = originY + dy * BLINK_LATTICE;

                    if (reach.Contains(x, y) && Lands(mesh, x, y))
                        return new Point(x, y);
                }

        //nothing in the reach takes one, so the leg keeps the entry and the refusal it earns is what turns blink off here
        return entry;
    }

    /// <summary>
    ///     Where a blink aimed at a route's end is sent: the edge of the end's radius nearest the caster, pulled in by a
    ///     lattice step so the server's rounding keeps it inside.
    /// </summary>
    /// <remarks>
    ///     An end is somewhere to be within range of, not a point to stand on: aimed at the centre, a mage headed for an NPC
    ///     lands on top of it.
    /// </remarks>
    private static Point EndLanding(NavMesh mesh, ICircle end, Point from)
    {
        var reach = Reach.Circle(end.X, end.Y, Math.Max(0f, end.Radius - BLINK_LATTICE));
        (var x, var y) = reach.NearEdge(from.X, from.Y);

        return BlinkLanding(mesh, reach, new Point(x, y));
    }

    /// <summary>
    ///     Whether the server would land a blink aimed here.
    /// </summary>
    private static bool Lands(NavMesh mesh, float x, float y)
    {
        var cellX = MathF.Round(x / BLINK_LATTICE) * BLINK_LATTICE;
        var cellY = MathF.Round(y / BLINK_LATTICE) * BLINK_LATTICE;

        for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
                if (!mesh.IsWalkable(cellX + dx * BLINK_LATTICE, cellY + dy * BLINK_LATTICE))
                    return false;

        return true;
    }

    private sealed class Node
    {
        /// <summary>
        ///     Where a blink aimed at this node lands. The same as <see cref="Entry" /> unless the server would refuse one there;
        ///     see <see cref="BlinkLanding" />.
        /// </summary>
        public required Point BlinkEntry { get; init; }

        public required Point Entry { get; init; }
        public Exit? Exit { get; init; }
        public required ILocation Location { get; init; }
        public required NavMesh Mesh { get; init; }
        public Reach Reach { get; init; }
        public int SpawnIndex { get; init; } = -1;
        public required int Triangle { get; init; }
    }
}