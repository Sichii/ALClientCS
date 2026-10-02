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
///     Represents the cross-map graph: arrival nodes (spawns that something lands on), departure nodes (exits), and the
///     static edges between them, with walk costs funnelled once at build.
/// </summary>
internal sealed class PortalGraph
{
    private static readonly List<int> EmptyNodes = [];

    /// <summary>
    ///     The lattice a blink's landing is rounded onto, and the step the server checks around it.
    /// </summary>
    private const float BLINK_LATTICE = 10f;

    private readonly Dictionary<(string Map, int Spawn), int> ArrivalIndex = new(MapSpawnComparer.Instance);
    private readonly Dictionary<string, List<int>> ArrivalsOnMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<int>> DeparturesOnMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly int[] EdgeStart;

    private readonly IReadOnlyDictionary<string, NavMesh> Meshes;
    private readonly List<Node> Nodes = [];

    private readonly int[] ReverseEdges;
    private readonly int[] ReverseStart;

    /// <summary>
    ///     The static edges, sorted by <see cref="Edge.From" />; <see cref="EdgeStart" /> indexes into them.
    /// </summary>
    private readonly Edge[] StaticEdges;

    public PortalGraph(IReadOnlyDictionary<string, NavMesh> meshes)
    {
        Meshes = meshes;

        foreach ((var map, var mesh) in meshes)
        {
            var gMap = GameData.Maps[map];

            if (gMap is null)
                continue;

            GetMapLists(map);

            if (gMap.Spawns.Count > 0)
                AddArrival(mesh, gMap, 0);

            //a map's doors are its own whether it is irregular or not
            foreach (var exit in gMap.Exits)
            {
                if (exit.Type == ExitType.Door)
                {
                    var door = gMap.Doors.FirstOrDefault(d => IPoint.Comparer.Equals(d, exit));

                    //a locked door is skipped; its way back out lands on the entry spawn
                    if (door is { LockType: DoorLockType.AccountLocked or DoorLockType.Key })
                        continue;
                }

                if (!meshes.TryGetValue(exit.ToLocation.Map, out var toMesh))
                    continue;

                var toMap = GameData.Maps[exit.ToLocation.Map];

                if (toMap is null || (exit.ToSpawnIndex < 0) || (exit.ToSpawnIndex >= toMap.Spawns.Count))
                    continue;

                AddArrival(toMesh, toMap, exit.ToSpawnIndex);

                var triangle = mesh.FindStartTriangle(exit.X, exit.Y, out var entry);

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
                        BlinkEntry = FindBlinkLanding(mesh, reach, entry),
                        Exit = exit,
                        Reach = reach
                    });

                DeparturesOnMap[map]
                    .Add(index);
            }
        }

        (StaticEdges, EdgeStart) = BuildStaticEdges();
        (ReverseEdges, ReverseStart) = BuildReverseEdges();
    }

    private void AddArrival(NavMesh mesh, GMap map, int spawnIndex)
    {
        if (ArrivalIndex.ContainsKey((map.Accessor, spawnIndex)))
            return;

        var spawn = map.Spawns[spawnIndex];
        var triangle = mesh.FindStartTriangle(spawn.X, spawn.Y, out var entry);

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

        GetMapLists(map.Accessor)
            .Arrivals
            .Add(index);
    }

    /// <summary>
    ///     Builds the index of static edges into each node, for the backward search behind the lower bounds.
    /// </summary>
    /// <returns>
    ///     Indices into <see cref="StaticEdges" /> grouped by <see cref="Edge.To" />, and where each node's group starts.
    /// </returns>
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

                var cost = node.Mesh.CalculateWalkCost(
                    node.Entry,
                    target.Triangle,
                    target.Entry,
                    target.Reach,
                    scratch);

                //float.MaxValue is the unreachable sentinel; a pair no walk joins gets a blink-only edge, priced per search
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

            //a recall to the map's town spawn, priced per search; the server refuses one on a dungeon floor
            var gMap = GameData.Maps[map];

            if ((node.SpawnIndex != 0) && gMap is { Boundless: false, Generated: null } && ArrivalIndex.TryGetValue((map, 0), out var town))
                edges.Add(
                    new Edge(
                        index,
                        town,
                        EdgeType.Town,
                        0f));

            if ((mainSpawn >= 0) && CONSTANTS.CanLeave(map))
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
    ///     Determines whether the server would land a blink aimed at a point: the lattice cell it rounds to and the eight
    ///     around it must all be ground.
    /// </summary>
    /// <param name="mesh">The mesh of the map the blink lands on.</param>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <returns>
    ///     <c>true</c> if the blink lands; otherwise, <c>false</c> .
    /// </returns>
    private static bool CanLand(NavMesh mesh, float x, float y)
    {
        var cellX = MathF.Round(x / BLINK_LATTICE) * BLINK_LATTICE;
        var cellY = MathF.Round(y / BLINK_LATTICE) * BLINK_LATTICE;

        for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
                if (!mesh.IsWalkable(cellX + dx * BLINK_LATTICE, cellY + dy * BLINK_LATTICE))
                    return false;

        return true;
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
                EdgeType.Blink => landing,
                EdgeType.Town  => useTown ? townCost : float.PositiveInfinity,
                _              => edge.Cost
            };
    }

    /// <summary>
    ///     Finds where a blink aimed at an exit is sent: the entry, or the nearest lattice cell inside the reach that the
    ///     server will land.
    /// </summary>
    /// <param name="mesh">The mesh of the map the blink lands on.</param>
    /// <param name="reach">The exit's reach.</param>
    /// <param name="entry">The exit's entry on the mesh.</param>
    /// <returns>
    ///     The landing point, or the entry when no cell in the reach takes one.
    /// </returns>
    /// <remarks>
    ///     The server searches only three cells along the axes for a landing, so an entry on the edge of the ground can refuse
    ///     every cast aimed at it.
    /// </remarks>
    private static Point FindBlinkLanding(NavMesh mesh, Reach reach, Point entry)
    {
        //wide enough to cover a door's reach
        const int BLINK_LANDING_RINGS = 12;

        if (CanLand(mesh, entry.X, entry.Y))
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

                    if (reach.Contains(x, y) && CanLand(mesh, x, y))
                        return new Point(x, y);
                }

        //nothing in the reach takes one, so the leg keeps the entry
        return entry;
    }

    /// <summary>
    ///     Finds where a blink aimed at a route's end is sent: the edge of the end's radius nearest the caster, pulled in by a
    ///     lattice step so the server's rounding keeps it inside.
    /// </summary>
    /// <param name="mesh">The mesh of the map the blink lands on.</param>
    /// <param name="end">The route's end.</param>
    /// <param name="from">Where the caster stands.</param>
    /// <returns>The landing point.</returns>
    private static Point FindEndLanding(NavMesh mesh, ICircle end, Point from)
    {
        var reach = Reach.CreateCircle(end.X, end.Y, Math.Max(0f, end.Radius - BLINK_LATTICE));
        (var x, var y) = reach.FindNearestEdgePoint(from.X, from.Y);

        return FindBlinkLanding(mesh, reach, new Point(x, y));
    }

    /// <summary>
    ///     Finds the cheapest route from a start to any of several ends.
    /// </summary>
    /// <param name="start">Where the route starts.</param>
    /// <param name="ends">The candidate ends; the first one taken wins.</param>
    /// <param name="options">How the route is priced.</param>
    /// <typeparam name="T">The type of the ends.</typeparam>
    /// <returns>The route's legs, never empty.</returns>
    /// <remarks>
    ///     A start already inside an end's reach gets one zero-cost <see cref="EdgeType.Walk" /> leg from the start to itself.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     The start has no mesh or ground nearby, no end can be reached, or the route could not be walked.
    /// </exception>
    public IReadOnlyList<PathEdge> FindPath<T>(ILocation start, IEnumerable<T> ends, PathOptions options) where T: ILocation, ICircle
    {
        if (!Meshes.TryGetValue(start.Map, out var startMesh))
            throw new InvalidOperationException($"No mesh for the map \"{start.Map}\".");

        var scratch = SearchScratch.Rent();
        var townCost = CONSTANTS.CalculateTownCost(options.WalkSpeed ?? CONSTANTS.NOMINAL_WALK_SPEED);

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

        var startTriangle = startMesh.FindStartTriangle(startPoint.X, startPoint.Y, out var startEntry);

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

            var triangle = endMesh.FindStartTriangle(end.X, end.Y, out scratch.EndEntry[j]);

            if (triangle < 0)
                continue;

            scratch.EndMesh[j] = endMesh;
            scratch.EndTriangle[j] = triangle;
            scratch.EndReach[j] = Reach.CreateCircle(end.X, end.Y, end.Radius);
        }

        var startNode = Nodes.Count;
        var firstEnd = Nodes.Count + 1;
        scratch.ResetNodes(firstEnd + endList.Count);

        //per-search edges into each end: one search of the end's map from the end, priced reversed and read at every arrival
        for (var j = 0; j < endList.Count; j++)
        {
            if (scratch.EndMesh[j] is not { } endMesh)
                continue;

            endMesh.Search(scratch.EndTriangle[j], scratch.EndEntry[j], scratch);

            var endOffset = scratch.EndEntry[j]
                                   .Distance(endList[j]);

            foreach (var arrival in GetNodes(ArrivalsOnMap, endList[j].Map))
            {
                var node = Nodes[arrival];

                var cost = endMesh.CalculateWalkCost(
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
        var startDepartures = GetNodes(DeparturesOnMap, start.Map);

        foreach (var departure in startDepartures)
        {
            var node = Nodes[departure];

            var cost = startMesh.CalculateWalkCost(
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

            var cost = startMesh.CalculateWalkCost(
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

        //the server refuses a recall anywhere in a run with cant_escape
        if (options.UseTown
            && startMap is { Boundless: false, Generated: null }
            && ArrivalIndex.TryGetValue((start.Map, 0), out var startTown))
            scratch.SearchEdges.Add(
                new Edge(
                    startNode,
                    startTown,
                    EdgeType.Town,
                    0f));

        if (CONSTANTS.CanLeave(start.Map) && ArrivalIndex.TryGetValue(("main", 0), out var mainSpawn))
            scratch.SearchEdges.Add(
                new Edge(
                    startNode,
                    mainSpawn,
                    EdgeType.Leave,
                    CONSTANTS.TRANSPORT_HEURISTIC));

        var nodeCount = firstEnd + endList.Count;
        scratch.IndexSearchEdges(nodeCount);

        if (blinkOn)
            ComputeLowerBounds(
                scratch,
                startNode,
                firstEnd,
                nodeCount,
                options,
                townCost);

        var winner = Search(
            scratch,
            new ArrivalSearch(
                scratch,
                options,
                townCost,
                startNode),
            options,
            firstEnd);

        if (winner < 0)
            throw new InvalidOperationException($"No path from {ILocation.ToString(start)} to any of {endList.Count} end(s).");

        //its own list, since TryWalk below reuses the corridor list
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
                //a walk cast instead, or a pair no walk joins: one teleport to the target, costed at the walk it
                //replaces or the straight line where there is none
                case EdgeType.Blink:
                case EdgeType.Walk when blinked:
                {
                    if (edge.To >= firstEnd)
                    {
                        var endIndex = edge.To - firstEnd;
                        var target = endList[endIndex];
                        var ruler = cursorPoint.Distance(new Point(target.X, target.Y));
                        var landing = FindEndLanding(scratch.EndMesh[endIndex]!, target, cursorPoint);

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
                    if (!NavMesh.IsSamePoint(cursorPoint, cursorEntry))
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

                    //a leg that landed exactly on the end ends on the caller's end object, and a start already inside
                    //the reach gets one zero-cost leg that stays put
                    if (isEnd && (polyline.Count < 2))
                    {
                        if (result.Count == 0)
                            result.Add(
                                new PathEdge(
                                    EdgeType.Walk,
                                    cursor,
                                    cursor,
                                    0f));
                        else if (NavMesh.IsSamePoint(cursorPoint, targetPoint))
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
                        var next = last && isEnd && NavMesh.IsSamePoint(polyline[i], targetPoint)
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
    ///     Gets a map's node lists, creating them on first use. Build time only; a search reads them through
    ///     <see cref="GetNodes" />.
    /// </summary>
    /// <param name="map">The map's key.</param>
    /// <returns>The map's arrival and departure node ids.</returns>
    private (List<int> Arrivals, List<int> Departures) GetMapLists(string map)
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
    ///     Gets a map's node ids without writing to the dictionary, so searches on several threads can read at once.
    /// </summary>
    /// <param name="byMap">The node ids by map.</param>
    /// <param name="map">The map's key.</param>
    /// <returns>The map's node ids, or an empty list.</returns>
    private static List<int> GetNodes(Dictionary<string, List<int>> byMap, string map)
        => byMap.TryGetValue(map, out var nodes) ? nodes : EmptyNodes;

    /// <summary>
    ///     Gets the character's speed for pricing, nominal when unset or not positive, as
    ///     <see cref="CONSTANTS.CalculateTownCost" /> does.
    /// </summary>
    /// <param name="options">The search's options.</param>
    /// <returns>The walk speed.</returns>
    private static float GetSpeed(PathOptions options)
        => options.WalkSpeed is > 0f ? options.WalkSpeed.Value : CONSTANTS.NOMINAL_WALK_SPEED;

    /// <summary>
    ///     Reads the route to an arrival back through its parents into the scratch's chain list.
    /// </summary>
    /// <param name="scratch">The calling thread's search state.</param>
    /// <param name="winner">The arrival the route ends at.</param>
    /// <returns>The arrival ids from the first move to the last.</returns>
    private static List<int> ReadChain(SearchScratch scratch, int winner)
    {
        var chain = scratch.Chain;
        chain.Clear();

        for (var arrivalId = winner; scratch.Arrivals[arrivalId].Parent >= 0; arrivalId = scratch.Arrivals[arrivalId].Parent)
            chain.Add(arrivalId);

        chain.Reverse();

        return chain;
    }

    /// <summary>Runs the search from the start, cheapest first.</summary>
    /// <param name="scratch">The calling thread's search state.</param>
    /// <param name="search">The search's pricing rules.</param>
    /// <param name="options">The search's options.</param>
    /// <param name="firstEnd">
    ///     The first end node; every node from it on is an end.
    /// </param>
    /// <returns>
    ///     The first arrival taken at an end, or -1 when none can be reached.
    /// </returns>
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
    ///     Provides one search's pricing: relaxes an edge out of an arrival into the arrivals it makes, and keeps at each node
    ///     only the arrivals no other one there beats.
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

        private readonly SearchScratch Scratch;
        private readonly float Speed;
        private readonly int StartNode;
        private readonly float TownCost;
        private readonly bool UseTown;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArrivalSearch" /> struct for one search.
        /// </summary>
        /// <param name="scratch">
        ///     The calling thread's scratch, holding the arrivals and, with blink on, the lower bounds.
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
                    next.Cost = origin.Cost + edge.Cost;

                    if (BlinkOn)
                        next.State = Clock.AdvanceTime(origin.State, edge.Cost / Speed * 1000f);

                    Offer(next);

                    //a cast may replace only a walk at least the floor long
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
        ///     Adds an arrival unless one already at its node beats it, and drops every arrival there it beats. Nothing is added
        ///     at a node no end can be reached from.
        /// </summary>
        /// <param name="arrival">The arrival to add.</param>
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

            //with blink off the bound is zero and the queue is ordered by cost alone
            Scratch.ArrivalQueue.Enqueue(newId, arrival.Cost + lowerBound);
        }

        /// <summary>
        ///     Offers the cast along an edge, priced at the time it takes in walk units but never under the floor; nothing when
        ///     blink is off or the bar can never pay for it.
        /// </summary>
        /// <param name="edge">
        ///     The walk the cast replaces, or the pair it bridges.
        /// </param>
        /// <param name="next">
        ///     The walked arrival along the same edge, which the cast differs from only in price and state.
        /// </param>
        /// <param name="origin">The arrival the cast is made from.</param>
        private void OfferBlink(in Edge edge, SearchScratch.Arrival next, in SearchScratch.Arrival origin)
        {
            if (!BlinkOn)
                return;

            if (!Clock.TryBlink(origin.State, out var spentMs, out var after))
                return;

            next.Cost = origin.Cost + Math.Max(Floor, spentMs * Speed / 1000f);
            next.State = after;
            next.Blinked = true;
            Offer(next);
        }

        /// <summary>
        ///     Determines whether one arrival makes another at the same node pointless.
        /// </summary>
        /// <param name="a">The arrival that may beat.</param>
        /// <param name="b">The arrival that may be beaten.</param>
        /// <returns>
        ///     <c>true</c> if <paramref name="a" /> costs no more and is at least as ready now and at the next cast; otherwise,
        ///     <c>false</c> .
        /// </returns>
        private bool Beats(in SearchScratch.Arrival a, in SearchScratch.Arrival b)
            => (a.Cost <= b.Cost) && Clock.IsAtLeastAsWellPlaced(a.State, b.State);
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

    private sealed class Node
    {
        /// <summary>
        ///     Where a blink aimed at this node lands; the same as <see cref="Entry" /> unless the server would refuse one there.
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