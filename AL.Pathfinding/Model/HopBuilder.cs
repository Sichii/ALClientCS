#region
using AL.Core.Extensions;
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents a straight move from one piece of a map's walkable mesh to another, or across a wall or water inside
///     one.
/// </summary>
/// <param name="FromPiece">The piece the hop leaves.</param>
/// <param name="ToPiece">The piece the hop lands on.</param>
/// <param name="Takeoff">Where the hop starts, on the piece it leaves.</param>
/// <param name="Landing">Where the hop ends, on the piece it lands on.</param>
/// <param name="Length">The distance from takeoff to landing.</param>
/// <param name="Depths">
///     The distance to the nearest ground at every <see cref="HopRisk.SAMPLE_STEP" /> from takeoff, the last on the
///     landing.
/// </param>
internal readonly record struct Hop(
    int FromPiece,
    int ToPiece,
    Point Takeoff,
    Point Landing,
    float Length,
    float[] Depths);

/// <summary>
///     Provides the candidate hops between the separate pieces of one map's walkable mesh, and the shortcuts inside each.
/// </summary>
internal sealed class HopBuilder
{
    /// <summary>
    ///     The longest hop built; a deep crossing this long fails the penalty check at any normal speed.
    /// </summary>
    private const float MAX_HOP_LENGTH = 200f;

    /// <summary>
    ///     How many times longer than a shortcut the walk between its ends must be.
    /// </summary>
    private const float MIN_WALK_RATIO = 2f;

    /// <summary>
    ///     How much longer than a shortcut the walk between its ends must be.
    /// </summary>
    private const float MIN_WALK_SAVING = 100f;

    /// <summary>
    ///     A speed above what a character reaches with strong gear and buffs; a hop that passes the server's limit even at it
    ///     is refused at every real speed, so it is not built.
    /// </summary>
    private const float MAX_SPEED = 200f;

    /// <summary>
    ///     How much longer than a hop the way through a kept hop may be before the hop counts as adding nothing.
    /// </summary>
    private const float DETOUR_ALLOWANCE = 75f;

    /// <summary>The edge vertices of every piece.</summary>
    private readonly int[][] Edges;

    private readonly NavMesh Mesh;

    /// <summary>The mesh's outer edges, two ends per edge.</summary>
    private readonly Point[] OuterEdges;

    private readonly int PieceCount;

    /// <summary>The piece of every triangle.</summary>
    private readonly int[] Pieces;

    private HopBuilder(NavMesh mesh)
    {
        Mesh = mesh;
        Pieces = LabelPieces(mesh.Mesh, out PieceCount);
        Edges = CollectEdgeVertices(out OuterEdges);
    }

    /// <summary>
    ///     Finds the hops between every two pieces of a map's mesh that come within <see cref="MAX_HOP_LENGTH" /> of each
    ///     other, then the shortcuts inside each piece.
    /// </summary>
    /// <param name="mesh">The map's mesh.</param>
    /// <returns>Every hop, each paired with its return.</returns>
    /// <exception cref="ArgumentNullException">mesh</exception>
    public static List<Hop> FindHops(NavMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        var builder = new HopBuilder(mesh);
        var hops = new List<Hop>();

        // ponytail: every edge vertex against every other piece's, O(V^2); a per-piece bounding box skips far pairs if a
        // map grows slow
        for (var from = 0; from < builder.PieceCount; from++)
            for (var to = from + 1; to < builder.PieceCount; to++)
                builder.AddWithReturns(hops, builder.FindCandidates(from, to));

        for (var piece = 0; piece < builder.PieceCount; piece++)
            builder.AddWithReturns(hops, builder.FindShortcuts(piece));

        return hops;
    }

    private void AddWithReturns(List<Hop> hops, List<Hop> found)
    {
        foreach (var hop in found)
        {
            var depths = BuildProfile(hop);

            var returnDepths = depths.Reverse()
                                     .ToArray();

            if ((HopRisk.CalculatePeakScore(depths, hop.Length, MAX_SPEED) >= HopRisk.SERVER_LIMIT)
                && (HopRisk.CalculatePeakScore(returnDepths, hop.Length, MAX_SPEED) >= HopRisk.SERVER_LIMIT))
                continue;

            hops.Add(
                hop with
                {
                    Depths = depths
                });

            hops.Add(
                new Hop(
                    hop.ToPiece,
                    hop.FromPiece,
                    hop.Landing,
                    hop.Takeoff,
                    hop.Length,
                    returnDepths));
        }
    }

    /// <summary>
    ///     Builds a hop's depth profile: 0 on walkable ground, otherwise the distance to the nearest outer edge of the mesh.
    /// </summary>
    /// <param name="hop">The hop to profile.</param>
    /// <returns>
    ///     The depth at every <see cref="HopRisk.SAMPLE_STEP" /> from takeoff, the last on the landing.
    /// </returns>
    private float[] BuildProfile(Hop hop)
    {
        var depths = new float[(int)(hop.Length / HopRisk.SAMPLE_STEP) + 1];
        var dx = (hop.Landing.X - hop.Takeoff.X) / hop.Length;
        var dy = (hop.Landing.Y - hop.Takeoff.Y) / hop.Length;

        for (var i = 0; i < depths.Length; i++)
        {
            var sample = i == (depths.Length - 1)
                ? hop.Landing
                : new Point(hop.Takeoff.X + dx * i * HopRisk.SAMPLE_STEP, hop.Takeoff.Y + dy * i * HopRisk.SAMPLE_STEP);

            depths[i] = Mesh.IsWalkable(sample) ? 0f : CalculateDistanceToEdges(sample);
        }

        return depths;
    }

    // ponytail: every sample against every outer edge; a grid of edges if hop counts grow
    private float CalculateDistanceToEdges(Point point)
    {
        var best = float.MaxValue;

        for (var i = 0; i < OuterEdges.Length; i += 2)
        {
            var start = OuterEdges[i];
            var end = OuterEdges[i + 1];
            var ex = end.X - start.X;
            var ey = end.Y - start.Y;
            var sqr = ex * ex + ey * ey;
            var t = sqr > 0f ? Math.Clamp(((point.X - start.X) * ex + (point.Y - start.Y) * ey) / sqr, 0f, 1f) : 0f;
            var dx = start.X + t * ex - point.X;
            var dy = start.Y + t * ey - point.Y;

            best = MathF.Min(best, dx * dx + dy * dy);
        }

        return MathF.Sqrt(best);
    }

    private static float CalculateRequiredWalk(float length) => MathF.Max(length * MIN_WALK_RATIO, length + MIN_WALK_SAVING);

    /// <summary>
    ///     Calculates the length of the walk the pathfinder prices between a hop's ends, the shorter of the two directions;
    ///     each is pulled through the corridor of a vertex search from its own start, so they can differ.
    /// </summary>
    /// <param name="hop">The hop whose ends to walk between.</param>
    /// <param name="scratch">The calling thread's search scratch.</param>
    /// <returns>
    ///     The shorter pulled walk's length, the first alone if it is already too short for a shortcut, or
    ///     <see cref="float.MaxValue" /> if no walk joins the ends.
    /// </returns>
    private float CalculateWalkLength(Hop hop, SearchScratch scratch)
    {
        var walk = CalculateWalkLength(hop.Takeoff, hop.Landing, scratch);

        if (walk < CalculateRequiredWalk(hop.Length))
            return walk;

        return MathF.Min(walk, CalculateWalkLength(hop.Landing, hop.Takeoff, scratch));
    }

    private float CalculateWalkLength(Point start, Point end, SearchScratch scratch)
    {
        var startTriangle = Mesh.Mesh.FindTriangle(start.X, start.Y);
        var endTriangle = Mesh.Mesh.FindTriangle(end.X, end.Y);

        Mesh.Search(
            startTriangle,
            start,
            scratch,
            endTriangle,
            end);

        return Mesh.CalculateWalkCost(
            start,
            endTriangle,
            end,
            Reach.CreateCircle(end.X, end.Y, 0f),
            scratch);
    }

    private int[][] CollectEdgeVertices(out Point[] outerEdges)
    {
        var mesh = Mesh.Mesh;
        var sets = new HashSet<int>[PieceCount];
        var ends = new List<Point>();

        for (var piece = 0; piece < PieceCount; piece++)
            sets[piece] = [];

        //neighbour slot i is the edge opposite corner i
        for (var t = 0; t < mesh.TriangleCount; t++)
            for (var slot = 0; slot < 3; slot++)
                if (mesh.Neighbours[t * 3 + slot] < 0)
                {
                    var start = mesh.Corners[t * 3 + (slot + 1) % 3];
                    var end = mesh.Corners[t * 3 + (slot + 2) % 3];

                    sets[Pieces[t]]
                        .Add(start);

                    sets[Pieces[t]]
                        .Add(end);
                    ends.Add(mesh.Vertices[start]);
                    ends.Add(mesh.Vertices[end]);
                }

        outerEdges = ends.ToArray();

        return sets.Select(set => set.ToArray())
                   .ToArray();
    }

    /// <summary>
    ///     Finds the crossing from every edge vertex of either piece to the nearest edge vertex of the other, where the line
    ///     between them stays off the mesh, and keeps the shortest that no kept one covers.
    /// </summary>
    /// <param name="from">The piece the hops leave.</param>
    /// <param name="to">The piece the hops land on.</param>
    /// <returns>
    ///     The hops from <paramref name="from" /> to <paramref name="to" />, with no profile yet.
    /// </returns>
    private List<Hop> FindCandidates(int from, int to)
    {
        var vertices = Mesh.Mesh.Vertices;
        var pairs = new HashSet<(Point Takeoff, Point Landing)>();

        //the nearest crossing seen from each side, so a gap one side lines with few vertices is still covered
        foreach (var vertex in Edges[from])
            if (TryFindNearest(vertices[vertex], Edges[to], out var nearest) && IsOffMeshBetween(vertices[vertex], nearest))
                pairs.Add((vertices[vertex], nearest));

        foreach (var vertex in Edges[to])
            if (TryFindNearest(vertices[vertex], Edges[from], out var nearest) && IsOffMeshBetween(nearest, vertices[vertex]))
                pairs.Add((nearest, vertices[vertex]));

        var kept = new List<Hop>();

        foreach ((var takeoff, var landing) in pairs.OrderBy(pair => pair.Takeoff.Distance(pair.Landing)))
        {
            var hop = new Hop(
                from,
                to,
                takeoff,
                landing,
                takeoff.Distance(landing),
                []);

            if (!IsCoveredByKept(kept, hop))
                kept.Add(hop);
        }

        return kept;
    }

    /// <summary>
    ///     Finds the straight moves between two edge vertices of one piece that stay off the mesh between them and save a
    ///     long walk, and keeps the shortest that no kept one covers.
    /// </summary>
    /// <param name="piece">The piece the shortcuts leave and land on.</param>
    /// <returns>
    ///     The shortcuts, one direction each, with no profile yet.
    /// </returns>
    private List<Hop> FindShortcuts(int piece)
    {
        var vertices = Mesh.Mesh.Vertices;
        var edges = Edges[piece];
        var scratch = SearchScratch.Rent();
        var candidates = new List<Hop>();

        var maxCost = CalculateRequiredWalk(MAX_HOP_LENGTH);

        for (var i = 0; i < edges.Length; i++)
        {
            var start = vertices[edges[i]];

            Mesh.Search(
                Mesh.Mesh.GetVertexTriangle(edges[i]),
                start,
                scratch,
                maxCost: maxCost);

            for (var j = i + 1; j < edges.Length; j++)
            {
                var end = vertices[edges[j]];
                var length = start.Distance(end);

                //vertex-search cost bounds the pulled walk between the vertices from above
                if ((length <= MAX_HOP_LENGTH)
                    && (scratch.VertexCost[edges[j]] >= CalculateRequiredWalk(length))
                    && IsOffMeshBetween(start, end))
                    candidates.Add(
                        new Hop(
                            piece,
                            piece,
                            start,
                            end,
                            length,
                            []));
            }
        }

        candidates.Sort((a, b) => a.Length.CompareTo(b.Length));

        var kept = new List<Hop>();

        foreach (var hop in candidates)
        {
            if (IsCoveredByKept(kept, hop))
                continue;

            var walk = CalculateWalkLength(hop, scratch);

            if ((walk < float.MaxValue) && (walk >= CalculateRequiredWalk(hop.Length)))
                kept.Add(hop);
        }

        return kept;
    }

    /// <summary>
    ///     Determines whether a kept hop already joins a hop's ends: going straight to it, across it and straight on is within
    ///     <see cref="DETOUR_ALLOWANCE" /> of the hop's own length.
    /// </summary>
    /// <param name="kept">The hops kept so far.</param>
    /// <param name="candidate">The hop to check.</param>
    /// <returns>
    ///     <c>true</c> if a kept hop covers the candidate; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsCoveredByKept(List<Hop> kept, Hop candidate)
        => kept.Any(hop => MathF.Min(
                               candidate.Takeoff.Distance(hop.Takeoff) + hop.Length + hop.Landing.Distance(candidate.Landing),
                               candidate.Takeoff.Distance(hop.Landing) + hop.Length + hop.Takeoff.Distance(candidate.Landing))
                           <= (candidate.Length + DETOUR_ALLOWANCE));

    private static int[] LabelPieces(TriangleMesh mesh, out int pieceCount)
    {
        var pieces = new int[mesh.TriangleCount];
        var stack = new Stack<int>();

        Array.Fill(pieces, -1);
        pieceCount = 0;

        for (var seed = 0; seed < mesh.TriangleCount; seed++)
        {
            if (pieces[seed] >= 0)
                continue;

            pieces[seed] = pieceCount;
            stack.Push(seed);

            while (stack.TryPop(out var t))
                for (var slot = 0; slot < 3; slot++)
                {
                    var neighbour = mesh.Neighbours[t * 3 + slot];

                    if ((neighbour < 0) || (pieces[neighbour] >= 0))
                        continue;

                    pieces[neighbour] = pieceCount;
                    stack.Push(neighbour);
                }

            pieceCount++;
        }

        return pieces;
    }

    /// <summary>
    ///     Determines whether a straight line stays off the mesh at every sample its depth profile takes between its ends.
    /// </summary>
    /// <param name="start">The line's start.</param>
    /// <param name="end">The line's end.</param>
    /// <returns>
    ///     <c>true</c> if there is a sample between the ends and none is on the mesh; otherwise, <c>false</c>.
    /// </returns>
    private bool IsOffMeshBetween(Point start, Point end)
    {
        var length = start.Distance(end);
        var dx = (end.X - start.X) / length;
        var dy = (end.Y - start.Y) / length;

        if (length <= (2f * HopRisk.SAMPLE_STEP))
            return false;

        for (var distance = HopRisk.SAMPLE_STEP; distance <= (length - HopRisk.SAMPLE_STEP); distance += HopRisk.SAMPLE_STEP)
            if (Mesh.IsWalkable(start.X + dx * distance, start.Y + dy * distance))
                return false;

        return true;
    }

    private bool TryFindNearest(Point point, int[] others, out Point nearest)
    {
        nearest = default;
        var best = MAX_HOP_LENGTH * MAX_HOP_LENGTH;
        var found = false;

        foreach (var other in others)
        {
            var vertex = Mesh.Mesh.Vertices[other];
            var dx = vertex.X - point.X;
            var dy = vertex.Y - point.Y;
            var sqr = dx * dx + dy * dy;

            if (sqr > best)
                continue;

            best = sqr;
            nearest = vertex;
            found = true;
        }

        return found;
    }
}