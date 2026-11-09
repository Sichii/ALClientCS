#region
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Definitions;
using Chaos.Extensions.Common;
using DoorLockType = AL.Core.Definitions.DoorLockType;
using Edge = AL.Pathfinding.Model.DoorSearch.Edge;
using ExitType = AL.Core.Definitions.ExitType;
using Seed = AL.Pathfinding.Model.MeshGraph.Seed;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents the search between maps: arrival nodes (spawns that something lands on), departure nodes (exits), and
///     the walk from every arrival to every exit on its map, found over the mesh vertices and smoothed once at build.
/// </summary>
/// <remarks>
///     A walk that takes a hop has its hops checked from a score of 0, so only a route carrying no score takes it. The
///     server keeps the score through doors, so the score the walk ends with is carried on.
/// </remarks>
internal sealed class DoorTable
{
    /// <summary>
    ///     The speeds the walks with hops are found at, slowest first; the last is the fastest a hop is built for.
    /// </summary>
    private static readonly float[] HOP_BANDS =
    [
        40f,
        60f,
        90f,
        HopBuilder.MAX_SPEED
    ];

    private static readonly List<int> EmptyNodes = [];

    private readonly Dictionary<(string Map, int Spawn), int> ArrivalIndex = new(MapSpawnComparer.Instance);
    private readonly Dictionary<string, List<int>> ArrivalsOnMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<int>> DeparturesOnMap = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Per speed band, the search over the walks found with hops on.
    /// </summary>
    public DoorSearch[] WithHops { get; }

    /// <summary>
    ///     Per speed band, the walks that take a hop, by arrival and exit, kept only where they beat the walk without one.
    /// </summary>
    public Dictionary<(int From, int To), List<GraphEdge>>[] HopWalks { get; }

    /// <summary>
    ///     The arrival at <c>main</c>'s first spawn, where a leave lands; -1 when there is none.
    /// </summary>
    public int MainSpawn { get; }

    public List<DoorNode> Nodes { get; } = [];

    /// <summary>The search over the walks found with hops off.</summary>
    public DoorSearch WithoutHops { get; }

    /// <summary>
    ///     The walks found with hops off, by arrival and exit.
    /// </summary>
    public Dictionary<(int From, int To), List<GraphEdge>> Walks { get; } = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="DoorTable" /> class, finding and smoothing every walk.
    /// </summary>
    /// <param name="graph">The graph that smooths the walks.</param>
    /// <param name="meshGraphs">Every map's vertex graph.</param>
    public DoorTable(DirectedGraph graph, Dictionary<string, MeshGraph> meshGraphs)
    {
        //name order, so two builds number the nodes alike
        var maps = meshGraphs.Keys
                             .Order(StringComparer.OrdinalIgnoreCase)
                             .ToList();

        foreach (var map in maps)
            AddNodes(meshGraphs, map);

        MainSpawn = ArrivalIndex.TryGetValue(("main", 0), out var main) ? main : -1;

        var found = new List<FoundWalk>[maps.Count];

        Parallel.For(0, maps.Count, i => found[i] = FindWalks(graph, meshGraphs[maps[i]]));

        HopWalks = HOP_BANDS.Select(_ => new Dictionary<(int From, int To), List<GraphEdge>>())
                            .ToArray();

        foreach (var mapWalks in found)
            foreach (var walk in mapWalks)
                if (walk.MinSpeed > 0f)
                    HopWalks[walk.Band][(walk.From, walk.To)] = walk.Legs;
                else
                    Walks[(walk.From, walk.To)] = walk.Legs;

        var edges = BuildEdges();
        WithoutHops = new DoorSearch([.. edges], Nodes.Count);
        WithHops = new DoorSearch[HOP_BANDS.Length];

        for (var band = 0; band < HOP_BANDS.Length; band++)
        {
            if (HopWalks[band].Count == 0)
            {
                WithHops[band] = WithoutHops;

                continue;
            }

            var bandEdges = new List<Edge>(edges);

            foreach (var walk in found.SelectMany(mapWalks => mapWalks)
                                      .Where(walk => (walk.MinSpeed > 0f) && (walk.Band == band)))
                bandEdges.Add(
                    new Edge(
                        walk.From,
                        walk.To,
                        EdgeType.Walk,
                        DirectedGraph.CalculateCost(walk.Legs),
                        MinSpeed: walk.MinSpeed,
                        EndScore: walk.EndScore));

            WithHops[band] = new DoorSearch(bandEdges, Nodes.Count);
        }
    }

    /// <summary>
    ///     Finds the speed band a search with hops uses: the fastest band at or below its speed, or the slowest band for a
    ///     search slower than every band.
    /// </summary>
    /// <param name="speed">The search's speed.</param>
    /// <param name="band">
    ///     The band's index into <see cref="WithHops" /> and <see cref="HopWalks" />.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the speed is no faster than the fastest a hop is built for; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryFindBand(float speed, out int band)
    {
        band = 0;

        if (speed > HopBuilder.MAX_SPEED)
            return false;

        while (((band + 1) < HOP_BANDS.Length) && (HOP_BANDS[band + 1] <= speed))
            band++;

        return true;
    }

    private void AddArrival(MeshGraph graph, GMap map, int spawnIndex)
    {
        if (ArrivalIndex.ContainsKey((map.Accessor, spawnIndex)))
            return;

        var location = new Location(map.Accessor, map.Spawns[spawnIndex]);
        var seeds = new List<Seed>();
        graph.FindSeeds(location, seeds);

        if (seeds.Count == 0)
            return;

        var index = Nodes.Count;

        Nodes.Add(
            new DoorNode
            {
                Node = graph.Mesh.ConstructNode(location),
                Seeds = seeds,
                SpawnIndex = spawnIndex
            });

        ArrivalIndex[(map.Accessor, spawnIndex)] = index;

        GetMapList(ArrivalsOnMap, map.Accessor)
            .Add(index);
    }

    /// <summary>
    ///     Adds a map's town spawn and its exits, and the spawn each exit lands on.
    /// </summary>
    /// <param name="meshGraphs">Every map's vertex graph.</param>
    /// <param name="map">The map's key.</param>
    private void AddNodes(Dictionary<string, MeshGraph> meshGraphs, string map)
    {
        var gMap = GameData.Maps[map];

        if (gMap is null)
            return;

        var graph = meshGraphs[map];
        GetMapList(ArrivalsOnMap, map);
        GetMapList(DeparturesOnMap, map);

        if (gMap.Spawns.Count > 0)
            AddArrival(graph, gMap, 0);

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

            if (!meshGraphs.TryGetValue(exit.ToLocation.Map, out var toGraph))
                continue;

            var toMap = GameData.Maps[exit.ToLocation.Map];

            if (toMap is null || (exit.ToSpawnIndex < 0) || (exit.ToSpawnIndex >= toMap.Spawns.Count))
                continue;

            AddArrival(toGraph, toMap, exit.ToSpawnIndex);

            var seeds = new List<Seed>();
            graph.FindSeeds(exit, seeds);

            if (seeds.Count == 0)
                continue;

            DeparturesOnMap[map]
                .Add(Nodes.Count);

            Nodes.Add(
                new DoorNode
                {
                    Node = graph.Mesh.ConstructNode(exit),
                    Seeds = seeds,
                    Exit = exit
                });
        }
    }

    /// <summary>
    ///     Builds the edges every search takes: doors, the walks found with hops off, a blink-only edge where no walk joins an
    ///     arrival to an exit, recalls and leaves.
    /// </summary>
    /// <returns>The edges, in node order.</returns>
    private List<Edge> BuildEdges()
    {
        var edges = new List<Edge>();

        for (var index = 0; index < Nodes.Count; index++)
        {
            var node = Nodes[index];
            var map = node.Node.Location.Map;

            if (node.Exit is { } exit)
            {
                //an arrival that resolved onto no vertex was never added, and an exit landing on it goes nowhere
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

            foreach (var departure in DeparturesOnMap[map])
                edges.Add(
                    Walks.TryGetValue((index, departure), out var legs)
                        ? new Edge(
                            index,
                            departure,
                            EdgeType.Walk,
                            DirectedGraph.CalculateCost(legs))
                        : new Edge(
                            index,
                            departure,
                            EdgeType.Blink,
                            float.MaxValue,
                            true));

            //a recall to the map's town spawn, priced per search; the server refuses one on a dungeon floor
            if ((node.SpawnIndex != 0) && TryGetTown(map, out var town))
                edges.Add(
                    new Edge(
                        index,
                        town,
                        EdgeType.Town,
                        0f));

            if ((MainSpawn >= 0) && CONSTANTS.CanLeave(map))
                edges.Add(
                    new Edge(
                        index,
                        MainSpawn,
                        EdgeType.Leave,
                        CONSTANTS.TRANSPORT_HEURISTIC));
        }

        return edges;
    }

    /// <summary>
    ///     Finds the slowest whole speed from which a walk's hops stay safe at every whole speed up to the fastest a hop is
    ///     built for, replayed from a score of 0.
    /// </summary>
    /// <param name="legs">The walk's legs.</param>
    /// <param name="minSpeed">The slowest safe speed.</param>
    /// <param name="endScore">
    ///     The highest score the walk ends with at any speed from <paramref name="minSpeed" /> up.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the walk is safe at the fastest speed; otherwise, <c>false</c>.
    /// </returns>
    private static bool TryFindMinSpeed(List<GraphEdge> legs, out float minSpeed, out float endScore)
    {
        minSpeed = 0f;
        endScore = 0f;

        for (var speed = HopBuilder.MAX_SPEED; speed >= 1f; speed--)
        {
            (var peak, var end) = DirectedGraph.ReplayHops(legs, 0f, speed);

            if (peak >= HopRisk.SAFE_SCORE)
                break;

            minSpeed = speed;
            endScore = Math.Max(endScore, end);
        }

        return minSpeed > 0f;
    }

    /// <summary>
    ///     Finds and smooths the walk from every arrival on a map to every exit on it, with hops off, and with hops on at each
    ///     speed band where the map has any.
    /// </summary>
    /// <param name="graph">The graph that smooths the walks.</param>
    /// <param name="meshGraph">The map's vertex graph.</param>
    /// <returns>
    ///     Each walk found, with the slowest speed it may be taken at; 0 for a walk without hops.
    /// </returns>
    private List<FoundWalk> FindWalks(DirectedGraph graph, MeshGraph meshGraph)
    {
        var walks = new List<FoundWalk>();
        var map = meshGraph.Mesh.Map;
        var departures = GetNodes(DeparturesOnMap, map);
        var scratch = MeshScratch.Rent();

        var targets = departures.SelectMany(departure => Nodes[departure].Seeds)
                                .Select(seed => seed.Vertex)
                                .ToList();

        foreach (var arrival in GetNodes(ArrivalsOnMap, map))
        {
            var from = Nodes[arrival];
            var walked = new float[departures.Count];

            meshGraph.Search(scratch, from.Seeds, targets);

            for (var i = 0; i < departures.Count; i++)
            {
                var legs = BuildWalk(departures[i]);
                walked[i] = legs is null ? float.MaxValue : DirectedGraph.CalculateCost(legs);

                if (legs is not null)
                    walks.Add(new FoundWalk(arrival, departures[i], legs));
            }

            if (meshGraph.Hops.Length == 0)
                continue;

            //each band's hops are taken at its speed, and each walk then finds the slowest it holds at
            for (var band = 0; band < HOP_BANDS.Length; band++)
            {
                meshGraph.SearchForward(
                    scratch,
                    from.Seeds,
                    targets,
                    HOP_BANDS[band]);

                for (var i = 0; i < departures.Count; i++)
                {
                    var legs = BuildWalk(departures[i]);

                    if (legs is null
                        || legs.TrueForAll(leg => leg.Type != EdgeType.Hop)
                        || (DirectedGraph.CalculateCost(legs) >= walked[i]))
                        continue;

                    if (TryFindMinSpeed(legs, out var minSpeed, out var endScore))
                        walks.Add(
                            new FoundWalk(arrival, departures[i], legs)
                            {
                                Band = band,
                                MinSpeed = minSpeed,
                                EndScore = endScore
                            });
                }
            }

            continue;

            List<GraphEdge>? BuildWalk(int departure)
                => graph.BuildForwardPiece(
                    meshGraph,
                    scratch,
                    from.Node,
                    Nodes[departure].Seeds,
                    Nodes[departure].Node);
        }

        return walks;
    }

    private static List<int> GetMapList(Dictionary<string, List<int>> byMap, string map)
    {
        if (!byMap.TryGetValue(map, out var nodes))
        {
            nodes = [];
            byMap[map] = nodes;
        }

        return nodes;
    }

    /// <summary>
    ///     Gets a map's arrival node ids without writing to the dictionary, so searches on several threads can read at once.
    /// </summary>
    /// <param name="map">The map's key.</param>
    /// <returns>The map's arrival node ids, or an empty list.</returns>
    public List<int> GetArrivals(string map) => GetNodes(ArrivalsOnMap, map);

    /// <summary>
    ///     Gets a map's exit node ids without writing to the dictionary, so searches on several threads can read at once.
    /// </summary>
    /// <param name="map">The map's key.</param>
    /// <returns>The map's exit node ids, or an empty list.</returns>
    public List<int> GetDepartures(string map) => GetNodes(DeparturesOnMap, map);

    private static List<int> GetNodes(Dictionary<string, List<int>> byMap, string map)
        => byMap.TryGetValue(map, out var nodes) ? nodes : EmptyNodes;

    /// <summary>
    ///     Gets the arrival at a map's town spawn, where a recall lands; the server refuses one on a dungeon floor and where
    ///     it keeps no walkable area.
    /// </summary>
    /// <param name="map">The map's key.</param>
    /// <param name="town">The town spawn's node id.</param>
    /// <returns>
    ///     <c>true</c> if a recall on the map lands somewhere; otherwise, <c>false</c>.
    /// </returns>
    public bool TryGetTown(string map, out int town)
    {
        town = -1;

        return GameData.Maps[map] is { Boundless: false, Generated: null } && ArrivalIndex.TryGetValue((map, 0), out town);
    }

    /// <summary>Represents one walk found at build.</summary>
    /// <param name="From">The arrival it leaves.</param>
    /// <param name="To">The exit it reaches.</param>
    /// <param name="Legs">The smoothed legs.</param>
    private readonly record struct FoundWalk(int From, int To, List<GraphEdge> Legs)
    {
        /// <summary>The speed band a walk with hops was found at.</summary>
        public int Band { get; init; }

        /// <summary>
        ///     The highest hop score the walk ends with at a speed it may be taken at.
        /// </summary>
        public float EndScore { get; init; }

        /// <summary>
        ///     The slowest speed the walk may be taken at; 0 for a walk without hops.
        /// </summary>
        public float MinSpeed { get; init; }
    }

    private sealed class MapSpawnComparer : IEqualityComparer<(string Map, int Spawn)>
    {
        public static readonly MapSpawnComparer Instance = new();

        public bool Equals((string Map, int Spawn) x, (string Map, int Spawn) y) => (x.Spawn == y.Spawn) && x.Map.EqualsI(y.Map);

        public int GetHashCode((string Map, int Spawn) obj)
            => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Map), obj.Spawn);
    }

    /// <summary>
    ///     Represents a node of the table: an arrival, or an exit with <see cref="Exit" /> set.
    /// </summary>
    internal sealed class DoorNode
    {
        public Exit? Exit { get; init; }

        /// <summary>
        ///     The node a route's legs stand on: the spawn, or the exit with its reach.
        /// </summary>
        public required GraphNode Node { get; init; }

        /// <summary>The mesh vertices the node is joined to.</summary>
        public required List<Seed> Seeds { get; init; }

        public int SpawnIndex { get; init; } = -1;
    }
}