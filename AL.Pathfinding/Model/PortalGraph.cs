#region
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
    private static readonly List<int> EmptyNodes = [];
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

        //no cast is worth more than any walk, so a price nothing exceeds is the same as no blink at all
        var blinkCost = options.BlinkCost ?? float.MaxValue;
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

        //dijkstra over the nodes; the first end settled is the cheapest
        scratch.NodeCost[startNode] = 0f;
        scratch.NodeQueue.Enqueue(startNode, 0f);
        var winner = -1;

        while (scratch.NodeQueue.TryDequeue(out var node, out var cost))
        {
            if (cost > scratch.NodeCost[node])
                continue;

            if (node >= firstEnd)
            {
                winner = node;

                break;
            }

            if (node < Nodes.Count)
                for (var i = EdgeStart[node]; i < EdgeStart[node + 1]; i++)
                    Relax(
                        StaticEdges[i],
                        i,
                        cost,
                        townCost,
                        options.UseTown,
                        blinkCost,
                        scratch);

            for (var i = 0; i < scratch.SearchEdges.Count; i++)
                if (scratch.SearchEdges[i].From == node)
                    Relax(
                        scratch.SearchEdges[i],
                        StaticEdges.Length + i,
                        cost,
                        townCost,
                        options.UseTown,
                        blinkCost,
                        scratch);
        }

        if (winner < 0)
            throw new InvalidOperationException($"No path from {ILocation.ToString(start)} to any of {endList.Count} end(s).");

        //the chain of nodes from the start to the winner, read back through the parents; each node names the edge that
        //reached it and whether that edge was cast rather than walked. Its own list, since TryWalk below reuses the
        //corridor list
        var chain = scratch.Chain;
        chain.Clear();

        for (var node = winner; node != startNode; node = scratch.NodeParent[node])
            chain.Add(node);

        chain.Reverse();

        var cursorPoint = startPoint;
        var cursorMesh = startMesh;
        var cursorTriangle = startTriangle;
        var cursorEntry = startEntry;

        //the scratch still holds the start map's search, which the first walk is read from
        var startSearchHeld = true;

        foreach (var node in chain)
        {
            var edgeIndex = scratch.NodeParentEdge[node];
            var edge = edgeIndex < StaticEdges.Length ? StaticEdges[edgeIndex] : scratch.SearchEdges[edgeIndex - StaticEdges.Length];
            var blinked = scratch.NodeBlinked[node];

            switch (edge.Type)
            {
                //a walk the search charged at the cast's price, or a pair no walk joins at all: one teleport from
                //wherever the cursor stands to the target, carrying the walked length it replaces - or, where there is
                //no walk, the ruler between the two. no funnel, since the server resolves a landing against the point
                //asked for rather than walking there. nothing walks out of a landing: an exit's only edges are its
                //door or transporter, and an end is the last node
                case EdgeType.Blink:
                case EdgeType.Walk when blinked:
                {
                    if (edge.To >= firstEnd)
                    {
                        var target = endList[edge.To - firstEnd];
                        var ruler = cursorPoint.Distance(new Point(target.X, target.Y));

                        result.Add(
                            new PathEdge(
                                EdgeType.Blink,
                                cursor,
                                target,
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

    private static void Relax(
        in Edge edge,
        int edgeIndex,
        float costSoFar,
        float townCost,
        bool useTown,
        float blinkCost,
        SearchScratch scratch)
    {
        //recall off means no recall anywhere on the route: the static town edges out of every arrival node are the
        //same move as the one out of the start, and a caller that cannot recall here cannot recall there either
        if (!useTown && (edge.Type == EdgeType.Town))
            return;

        //a pair only a cast joins stays unjoined while blink is off
        if ((edge.Type == EdgeType.Blink) && (blinkCost >= float.MaxValue))
            return;

        //a blink stands in for a walk on the same map, and the walk edges already join the pairs a cast can reach - so
        //rather than edges of its own, a walk dearer than the cast is charged the cast instead. the mark rides the node,
        //so the expansion emits a teleport rather than funnelling the walk
        var blinked = (edge.Type == EdgeType.Blink) || ((edge.Type == EdgeType.Walk) && (edge.Cost > blinkCost));

        var cost = costSoFar
                   + edge.Type switch
                   {
                       EdgeType.Town  => townCost,
                       _ when blinked => blinkCost,
                       _              => edge.Cost
                   };

        if (cost >= scratch.NodeCost[edge.To])
            return;

        scratch.NodeCost[edge.To] = cost;
        scratch.NodeParent[edge.To] = edge.From;
        scratch.NodeParentEdge[edge.To] = edgeIndex;
        scratch.NodeBlinked[edge.To] = blinked;
        scratch.NodeQueue.Enqueue(edge.To, cost);
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