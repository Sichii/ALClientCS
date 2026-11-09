#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using EdgeType = AL.Pathfinding.Definitions.EdgeType;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents one map's mesh vertices and wall hop ends as dense ids with flat edge arrays, for searches that write
///     only to a <see cref="MeshScratch" />.
/// </summary>
internal sealed class MeshGraph
{
    private readonly float[] EdgeCost;

    /// <summary>
    ///     The walk leg behind each walk edge, built once so every walk found over it shares the leg.
    /// </summary>
    private readonly GraphEdge[] EdgeLegs;

    private readonly int[] EdgeStart;

    /// <summary>
    ///     The leg behind each hop, by hop id, built once so every walk found over it shares the leg.
    /// </summary>
    private readonly GraphEdge[] HopLegs;

    private readonly int[] EdgeTo;
    private readonly int[] HopIndex;
    private readonly int[] HopStart;
    private readonly int[] HopTo;
    private readonly Dictionary<GraphNode, int> Ids = new(ReferenceEqualityComparer.Instance);
    private readonly int[] ReverseHopIndex;
    private readonly int[] ReverseHopStart;
    private readonly int[] ReverseHopTo;

    /// <summary>
    ///     The hops whose two ends stand on this mesh, indexed by the hop ids the searches record.
    /// </summary>
    public Hop[] Hops { get; }

    public NodeMesh Mesh { get; }

    /// <summary>
    ///     The number of mesh vertices; every id from it on is a hop end, which a search without hops never steps onto.
    /// </summary>
    public int MeshVertexCount { get; }

    /// <summary>
    ///     The node behind each id: the mesh vertices in coordinate order, then the hop ends.
    /// </summary>
    public GraphNode[] Vertices { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MeshGraph" /> class.
    /// </summary>
    /// <param name="mesh">The map's mesh.</param>
    /// <param name="hops">
    ///     The map's hops; one whose end stands in no triangle of the mesh is dropped.
    /// </param>
    public MeshGraph(NodeMesh mesh, List<Hop> hops)
    {
        Mesh = mesh;

        //coordinate order, so two builds of one mesh number it alike
        var vertices = mesh.Triangles
                           .SelectMany(triangle => triangle.Vertices)
                           .Distinct(ReferenceEqualityComparer.Instance)
                           .Cast<GraphNode>()
                           .OrderBy(node => node.Location.X)
                           .ThenBy(node => node.Location.Y)
                           .ToList();

        MeshVertexCount = vertices.Count;

        for (var i = 0; i < vertices.Count; i++)
            Ids[vertices[i]] = i;

        var walks = vertices.Select(_ => new List<int>())
                            .ToList();

        foreach (var node in vertices)
            foreach (var edge in node.Edges)
                if (Ids.TryGetValue(edge.End, out var to))
                    walks[Ids[node]]
                        .Add(to);

        //each hop end is a node of its own, walked to and from the corners of the triangle it stands in
        var keptHops = new List<Hop>();
        var hopEdges = new List<(int From, int To, int Hop)>();
        var ends = new Dictionary<Point, int>();

        foreach (var hop in hops)
        {
            if (!TryAddHopEnd(hop.Takeoff, out var takeoff) || !TryAddHopEnd(hop.Landing, out var landing))
                continue;

            hopEdges.Add((takeoff, landing, keptHops.Count));
            keptHops.Add(hop);
        }

        Vertices = vertices.ToArray();
        Hops = keptHops.ToArray();
        (EdgeStart, EdgeTo, EdgeCost, EdgeLegs) = BuildWalkEdges(walks);
        HopLegs = new GraphEdge[keptHops.Count];

        foreach ((var from, var to, var hop) in hopEdges)
            HopLegs[hop] = new GraphEdge
            {
                Start = Vertices[from],
                End = Vertices[to],
                Cost = Hops[hop].Length,
                Type = EdgeType.Hop,
                HopPenalties = Hops[hop].Penalties
            };
        (HopStart, HopTo, HopIndex) = BuildHopEdges(hopEdges);

        (ReverseHopStart, ReverseHopTo, ReverseHopIndex) = BuildHopEdges(
            hopEdges.Select(edge => (edge.To, edge.From, edge.Hop))
                    .ToList());

        return;

        bool TryAddHopEnd(Point point, out int id)
        {
            if (ends.TryGetValue(point, out id))
                return true;

            if (!mesh.TryFindTriangle(point, out var triangle))
                return false;

            id = vertices.Count;
            vertices.Add(mesh.ConstructNode(new Location(mesh.Map, point)));
            walks.Add([]);

            foreach (var corner in triangle.Vertices)
            {
                var cornerId = Ids[corner];

                walks[cornerId]
                    .Add(id);

                walks[id]
                    .Add(cornerId);
            }

            ends[point] = id;

            return true;
        }
    }

    private (int[] Start, int[] To, int[] Hop) BuildHopEdges(List<(int From, int To, int Hop)> hopEdges)
    {
        var start = new int[Vertices.Length + 1];
        var to = new int[hopEdges.Count];
        var hop = new int[hopEdges.Count];

        foreach (var edge in hopEdges)
            start[edge.From + 1]++;

        for (var i = 1; i < start.Length; i++)
            start[i] += start[i - 1];

        var next = (int[])start.Clone();

        foreach (var edge in hopEdges)
        {
            var slot = next[edge.From]++;
            to[slot] = edge.To;
            hop[slot] = edge.Hop;
        }

        return (start, to, hop);
    }

    private (int[] Start, int[] To, float[] Cost, GraphEdge[] Legs) BuildWalkEdges(List<List<int>> walks)
    {
        var start = new int[Vertices.Length + 1];
        var to = new List<int>();
        var cost = new List<float>();
        var legs = new List<GraphEdge>();

        for (var i = 0; i < Vertices.Length; i++)
        {
            //by id, so a search without hops meets the hop ends last and skips them without reordering the rest
            foreach (var target in walks[i]
                                   .Distinct()
                                   .Order())
            {
                to.Add(target);

                cost.Add(
                    Vertices[i]
                        .Location
                        .Distance(Vertices[target].Location));
                legs.Add(Mesh.ConstructEdge(Vertices[i], Vertices[target], EdgeType.Walk));
            }

            start[i + 1] = to.Count;
        }

        return (start, to.ToArray(), cost.ToArray(), legs.ToArray());
    }

    /// <summary>
    ///     Finds the vertices a point is joined to: the corners of the triangle it stands in, or else the nearest vertex it
    ///     can reach in a straight line.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="seeds">
    ///     Cleared, then filled with each vertex and the straight distance to it.
    /// </param>
    public void FindSeeds(ILocation point, List<Seed> seeds)
    {
        seeds.Clear();

        if (MeshVertexCount == 0)
            return;

        if (Mesh.GetContainingTriangle(point) is { } triangle)
        {
            foreach (var corner in triangle.Vertices)
                seeds.Add(new Seed(Ids[corner], corner.Location.Distance(point)));

            return;
        }

        var best = Mesh.FindBestNode(point);
        seeds.Add(new Seed(Ids[best], best.Location.Distance(point)));
    }

    /// <summary>
    ///     Finds the cheapest seed of a target the last search reached: its vertex's cost plus the distance on.
    /// </summary>
    /// <param name="scratch">
    ///     The calling thread's scratch, holding the last search over this graph.
    /// </param>
    /// <param name="seeds">The target's seeds.</param>
    /// <param name="vertex">The seed's vertex, or -1.</param>
    /// <returns>
    ///     The cost through the seed, or <see cref="float.MaxValue" /> when the search reached none.
    /// </returns>
    public static float FindCheapestSeed(MeshScratch scratch, List<Seed> seeds, out int vertex)
    {
        vertex = -1;
        var best = float.MaxValue;

        foreach (var seed in seeds)
        {
            var cost = scratch.GetCost(seed.Vertex);

            if ((cost == float.MaxValue) || ((cost + seed.Cost) >= best))
                continue;

            best = cost + seed.Cost;
            vertex = seed.Vertex;
        }

        return best;
    }

    /// <summary>
    ///     Adds the raw legs of the last backward search's path from a vertex it reached to its seeds, one per mesh edge or
    ///     hop.
    /// </summary>
    /// <param name="scratch">
    ///     The calling thread's scratch, holding the last search over this graph.
    /// </param>
    /// <param name="vertex">The vertex the path starts at.</param>
    /// <param name="legs">
    ///     The list the legs are added to, in order of travel.
    /// </param>
    public void AddBackwardLegs(MeshScratch scratch, int vertex, List<GraphEdge> legs)
    {
        for (var at = vertex; scratch.Parent[at] >= 0; at = scratch.Parent[at])
            legs.Add(CreateLeg(at, scratch.Parent[at], scratch.ParentHop[at]));
    }

    /// <summary>
    ///     Adds the raw legs of the last forward search's path from its seeds to a vertex it reached, one per mesh edge or
    ///     hop.
    /// </summary>
    /// <param name="scratch">
    ///     The calling thread's scratch, holding the last search over this graph.
    /// </param>
    /// <param name="vertex">The vertex the path ends at.</param>
    /// <param name="legs">
    ///     The list the legs are added to, in order of travel.
    /// </param>
    public void AddForwardLegs(MeshScratch scratch, int vertex, List<GraphEdge> legs)
    {
        var first = legs.Count;

        for (var at = vertex; scratch.Parent[at] >= 0; at = scratch.Parent[at])
            legs.Add(CreateLeg(scratch.Parent[at], at, scratch.ParentHop[at]));

        legs.Reverse(first, legs.Count - first);
    }

    private GraphEdge CreateLeg(int from, int to, int hop)
    {
        if (hop >= 0)
            return HopLegs[hop];

        for (var edge = EdgeStart[from]; edge < EdgeStart[from + 1]; edge++)
            if (EdgeTo[edge] == to)
                return EdgeLegs[edge];

        throw new InvalidOperationException($"No walk joins vertices {from} and {to}.");
    }

    /// <summary>
    ///     Resets the scratch for a search, marks its targets and reaches its seeds.
    /// </summary>
    /// <param name="scratch">The calling thread's scratch.</param>
    /// <param name="seeds">
    ///     The vertices the search starts from, each at its cost.
    /// </param>
    /// <param name="targets">The vertices the search stops once it has settled.</param>
    /// <param name="startScore">The penalty score at the seeds.</param>
    /// <returns>The number of distinct targets.</returns>
    private int BeginSearch(
        MeshScratch scratch,
        List<Seed> seeds,
        List<int> targets,
        float startScore)
    {
        scratch.Reset(Vertices.Length);
        var remaining = 0;

        foreach (var target in targets)
            if (scratch.MarkTarget(target))
                remaining++;

        foreach (var seed in seeds)
            if (seed.Cost < scratch.GetCost(seed.Vertex))
                scratch.MarkReached(
                    seed.Vertex,
                    seed.Cost,
                    -1,
                    -1,
                    startScore);

        return remaining;
    }

    /// <summary>
    ///     Reaches the walk neighbours of a settled vertex, decaying its penalty score along each edge.
    /// </summary>
    /// <param name="scratch">The calling thread's scratch.</param>
    /// <param name="vertex">The settled vertex.</param>
    /// <param name="limit">The first id the search may not step onto.</param>
    /// <param name="speed">The speed the score decays at.</param>
    private void RelaxWalks(
        MeshScratch scratch,
        int vertex,
        int limit,
        float speed)
    {
        var cost = scratch.Cost[vertex];
        var score = scratch.Score[vertex];

        for (var i = EdgeStart[vertex]; i < EdgeStart[vertex + 1]; i++)
        {
            var to = EdgeTo[i];

            if (to >= limit)
                continue;

            var through = cost + EdgeCost[i];

            if (through < scratch.GetCost(to))
                scratch.MarkReached(
                    to,
                    through,
                    vertex,
                    -1,
                    score > 0f ? HopRisk.DecayWhileWalking(score, EdgeCost[i] / speed) : 0f);
        }
    }

    /// <summary>
    ///     Runs a Dijkstra search over the mesh vertices alone into the calling thread's scratch. Without hops every edge goes
    ///     both ways, so the search serves either direction.
    /// </summary>
    /// <param name="scratch">The calling thread's scratch.</param>
    /// <param name="seeds">
    ///     The vertices the search starts from, each at its cost.
    /// </param>
    /// <param name="targets">
    ///     The vertices the search stops once it has settled; empty to search the whole mesh.
    /// </param>
    public void Search(MeshScratch scratch, List<Seed> seeds, List<int> targets)
    {
        var remaining = BeginSearch(
            scratch,
            seeds,
            targets,
            0f);

        while (scratch.Queue.TryDequeue(out var vertex, out _))
        {
            if (!scratch.TrySettle(vertex))
                continue;

            if (scratch.IsTarget(vertex) && (--remaining == 0))
                return;

            RelaxWalks(
                scratch,
                vertex,
                MeshVertexCount,
                1f);
        }
    }

    /// <summary>
    ///     Runs a Dijkstra search with hops against their direction into the calling thread's scratch, so its paths lead to
    ///     the seeds. Each hop is checked from a score of 0.
    /// </summary>
    /// <param name="scratch">The calling thread's scratch.</param>
    /// <param name="seeds">The vertices the paths lead to, each at its cost.</param>
    /// <param name="targets">
    ///     The vertices the search stops once it has settled; empty to search the whole mesh.
    /// </param>
    /// <param name="hopSpeed">The speed hops are replayed at.</param>
    public void SearchBackward(
        MeshScratch scratch,
        List<Seed> seeds,
        List<int> targets,
        float hopSpeed)
    {
        var remaining = BeginSearch(
            scratch,
            seeds,
            targets,
            0f);

        while (scratch.Queue.TryDequeue(out var vertex, out _))
        {
            if (!scratch.TrySettle(vertex))
                continue;

            if (scratch.IsTarget(vertex) && (--remaining == 0))
                return;

            RelaxWalks(
                scratch,
                vertex,
                Vertices.Length,
                hopSpeed);

            var cost = scratch.Cost[vertex];

            for (var i = ReverseHopStart[vertex]; i < ReverseHopStart[vertex + 1]; i++)
            {
                var hop = Hops[ReverseHopIndex[i]];

                (var peak, _) = HopRisk.Replay(
                    hop.Penalties,
                    hop.Length,
                    hopSpeed,
                    0f);

                if (peak >= HopRisk.SAFE_SCORE)
                    continue;

                var to = ReverseHopTo[i];
                var through = cost + hop.Length;

                if (through < scratch.GetCost(to))
                    scratch.MarkReached(
                        to,
                        through,
                        vertex,
                        ReverseHopIndex[i],
                        0f);
            }
        }
    }

    /// <summary>
    ///     Runs a Dijkstra search with hops into the calling thread's scratch, carrying the penalty score from the seeds and
    ///     decaying it along every walk.
    /// </summary>
    /// <param name="scratch">The calling thread's scratch.</param>
    /// <param name="seeds">
    ///     The vertices the search starts from, each at its cost.
    /// </param>
    /// <param name="targets">
    ///     The vertices the search stops once it has settled; empty to search the whole mesh.
    /// </param>
    /// <param name="hopSpeed">
    ///     The speed hops are replayed at, and the score decays at.
    /// </param>
    /// <param name="startScore">The penalty score at the seeds.</param>

    //ponytail: one cost per vertex, so a walk that loops between two hops to let the score decay before the second is never
    //found (tunnel to winter_cove is one). Per-vertex arrival lists, kept as the door search keeps them, would find it
    public void SearchForward(
        MeshScratch scratch,
        List<Seed> seeds,
        List<int> targets,
        float hopSpeed,
        float startScore = 0f)
    {
        var remaining = BeginSearch(
            scratch,
            seeds,
            targets,
            startScore);

        while (scratch.Queue.TryDequeue(out var vertex, out _))
        {
            if (!scratch.TrySettle(vertex))
                continue;

            if (scratch.IsTarget(vertex) && (--remaining == 0))
                return;

            RelaxWalks(
                scratch,
                vertex,
                Vertices.Length,
                hopSpeed);

            var cost = scratch.Cost[vertex];
            var score = scratch.Score[vertex];

            for (var i = HopStart[vertex]; i < HopStart[vertex + 1]; i++)
            {
                var hop = Hops[HopIndex[i]];

                //the server's in-motion penalty is replayed along the hop; a peak it would jail for refuses it
                (var peak, var end) = HopRisk.Replay(
                    hop.Penalties,
                    hop.Length,
                    hopSpeed,
                    score);

                if (peak >= HopRisk.SAFE_SCORE)
                    continue;

                var to = HopTo[i];
                var through = cost + hop.Length;

                if (through < scratch.GetCost(to))
                    scratch.MarkReached(
                        to,
                        through,
                        vertex,
                        HopIndex[i],
                        end);
            }
        }
    }

    /// <summary>
    ///     Represents a vertex a point is joined to, and the straight distance between them.
    /// </summary>
    /// <param name="Vertex">The vertex's id.</param>
    /// <param name="Cost">The distance.</param>
    internal readonly record struct Seed(int Vertex, float Cost);
}