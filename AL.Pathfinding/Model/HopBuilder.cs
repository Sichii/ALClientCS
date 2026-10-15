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
    ///     How far back along the hop an end may be pulled to find ground the server accepts.
    /// </summary>
    private const float MAX_INSET = 20f;

    private const float INSET_STEP = 2f;

    /// <summary>
    ///     The most hops kept per direction between two pieces.
    /// </summary>
    private const int MAX_CANDIDATES = 8;

    /// <summary>The most shortcuts kept inside one piece.</summary>
    private const int MAX_SHORTCUTS_PER_PIECE = 64;

    /// <summary>
    ///     How many times longer than a shortcut the walk between its ends must be.
    /// </summary>
    private const float MIN_WALK_RATIO = 2f;

    /// <summary>
    ///     How much longer than a shortcut the walk between its ends must be.
    /// </summary>
    private const float MIN_WALK_SAVING = 100f;

    /// <summary>
    ///     The least distance between the takeoffs, and between the landings, of two hops kept between the same pieces.
    /// </summary>
    private const float CANDIDATE_SPACING = 50f;

    private const float LATTICE_STEP = 10f;

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
    ///     other, up to <see cref="MAX_CANDIDATES" /> per direction, spread along the gap, then the shortcuts inside each
    ///     piece.
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
                    depths.Reverse()
                          .ToArray()));
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

    /// <summary>
    ///     Calculates how far a line stays on the mesh from its start, to the unit, checking every
    ///     <see cref="HopRisk.SAMPLE_STEP" /> for where it leaves.
    /// </summary>
    /// <param name="start">The line's start.</param>
    /// <param name="end">The line's end.</param>
    /// <returns>
    ///     The distance to the last walkable point before the line leaves the mesh, or its length if no sample is off it.
    /// </returns>
    private float CalculateGroundRun(Point start, Point end)
    {
        const float TRIM_STEP = 1f;
        var length = start.Distance(end);
        var dx = (end.X - start.X) / length;
        var dy = (end.Y - start.Y) / length;

        //a coarse pass finds the first sample off the mesh, a fine one the last ground before it
        for (var distance = HopRisk.SAMPLE_STEP; distance < length; distance += HopRisk.SAMPLE_STEP)
        {
            if (IsWalkableAlong(distance))
                continue;

            var run = distance - HopRisk.SAMPLE_STEP;

            while (IsWalkableAlong(run + TRIM_STEP))
                run += TRIM_STEP;

            return run;
        }

        return length;

        bool IsWalkableAlong(float distance) => Mesh.IsWalkable(start.X + dx * distance, start.Y + dy * distance);
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
    ///     Finds the shortest crossings between two pieces, each trimmed to the stretch that leaves the mesh and its ends
    ///     pulled back onto ground the server accepts, and keeps the shortest ones whose takeoffs and landings are both
    ///     <see cref="CANDIDATE_SPACING" /> from every kept one.
    /// </summary>
    /// <param name="from">The piece the hops leave.</param>
    /// <param name="to">The piece the hops land on.</param>
    /// <returns>
    ///     The kept hops from <paramref name="from" /> to <paramref name="to" />, with no profile yet.
    /// </returns>
    private List<Hop> FindCandidates(int from, int to)
    {
        var vertices = Mesh.Mesh.Vertices;
        var candidates = new List<Hop>();

        //the nearest crossing seen from each side, so a gap one side lines with few vertices is still spread out
        foreach (var vertex in Edges[from])
            if (TryFindNearest(vertices[vertex], Edges[to], out var nearest)
                && TryTrim(
                    vertices[vertex],
                    nearest,
                    out var takeoff,
                    out var landing)
                && TryBuildCandidate(
                    from,
                    to,
                    takeoff,
                    landing,
                    out var hop))
                candidates.Add(hop);

        foreach (var vertex in Edges[to])
            if (TryFindNearest(vertices[vertex], Edges[from], out var nearest)
                && TryTrim(
                    nearest,
                    vertices[vertex],
                    out var takeoff,
                    out var landing)
                && TryBuildCandidate(
                    from,
                    to,
                    takeoff,
                    landing,
                    out var hop))
                candidates.Add(hop);

        candidates.Sort((a, b) => a.Length.CompareTo(b.Length));

        var kept = new List<Hop>();

        foreach (var candidate in candidates)
        {
            if (kept.Count == MAX_CANDIDATES)
                break;

            if (kept.All(hop => (hop.Takeoff.Distance(candidate.Takeoff) >= CANDIDATE_SPACING)
                                && (hop.Landing.Distance(candidate.Landing) >= CANDIDATE_SPACING)))
                kept.Add(candidate);
        }

        return kept;
    }

    /// <summary>
    ///     Finds the straight moves between two edge points of one piece that leave the mesh, trimmed to the stretch that
    ///     leaves it, that save a long walk, and keeps the ones that save the most whose ends are all
    ///     <see cref="CANDIDATE_SPACING" /> from every kept one's.
    /// </summary>
    /// <param name="piece">The piece the shortcuts leave and land on.</param>
    /// <returns>
    ///     The kept shortcuts, one direction each, with no profile yet.
    /// </returns>
    private List<Hop> FindShortcuts(int piece)
    {
        var vertices = Mesh.Mesh.Vertices;
        var edges = Edges[piece];
        var scratch = SearchScratch.Rent();
        var queue = new PriorityQueue<(Point Start, Point End, Hop? Shortcut), float>(Comparer<float>.Create((a, b) => b.CompareTo(a)));

        //an inset end lies within MAX_INSET of its trimmed end, so the ends' walk is at most this longer than the trimmed ends'
        const float INSET_SLACK = 2f * MAX_INSET;
        var maxCost = CalculateRequiredWalk(MAX_HOP_LENGTH) - INSET_SLACK;

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

                if ((start.Distance(end) > MAX_HOP_LENGTH)
                    || !TryTrim(
                        start,
                        end,
                        out var takeoff,
                        out var landing))
                    continue;

                //vertex-search cost bounds the pulled walk between the vertices from above, and each trimmed stretch is ground
                var walk = scratch.VertexCost[edges[j]] + start.Distance(takeoff) + end.Distance(landing);
                var length = takeoff.Distance(landing);

                if (walk >= (CalculateRequiredWalk(length) - INSET_SLACK))
                    queue.Enqueue((takeoff, landing, null), walk - length);
            }
        }

        //an inset lengthens a shortcut and its walk alike, so a measured shortcut taken from the queue saves at least as much
        //as any bound still in it
        var kept = new List<Hop>();

        while ((kept.Count < MAX_SHORTCUTS_PER_PIECE) && queue.TryDequeue(out var pair, out _))
        {
            if (pair.Shortcut is { } measured)
            {
                if (IsSpaced(kept, measured))
                    kept.Add(measured);

                continue;
            }

            if (!TryBuildCandidate(
                    piece,
                    piece,
                    pair.Start,
                    pair.End,
                    out var hop)
                || !IsSpaced(kept, hop)
                || !LeavesMesh(hop.Takeoff, hop.Landing))
                continue;

            var walk = CalculateWalkLength(hop, scratch);

            if ((walk < float.MaxValue) && (walk >= CalculateRequiredWalk(hop.Length)))
                queue.Enqueue((pair.Start, pair.End, hop), walk - hop.Length);
        }

        return kept;
    }

    private bool IsOnPiece(int piece, float x, float y)
    {
        var triangle = Mesh.Mesh.FindTriangle(x, y);

        return (triangle >= 0) && (Pieces[triangle] == piece);
    }

    /// <summary>
    ///     Determines whether both ends of a shortcut lie <see cref="CANDIDATE_SPACING" /> from both ends of every kept one.
    /// </summary>
    /// <param name="kept">The shortcuts kept so far.</param>
    /// <param name="candidate">The shortcut to check.</param>
    /// <returns>
    ///     <c>true</c> if no end of the candidate is near an end of a kept shortcut; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsSpaced(List<Hop> kept, Hop candidate)
        => kept.All(hop
            => (hop.Takeoff.Distance(candidate.Takeoff) >= CANDIDATE_SPACING)
               && (hop.Takeoff.Distance(candidate.Landing) >= CANDIDATE_SPACING)
               && (hop.Landing.Distance(candidate.Takeoff) >= CANDIDATE_SPACING)
               && (hop.Landing.Distance(candidate.Landing) >= CANDIDATE_SPACING));

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
    ///     Determines whether a straight line leaves the mesh at any sample its depth profile takes between its ends.
    /// </summary>
    /// <param name="start">The line's start.</param>
    /// <param name="end">The line's end.</param>
    /// <returns>
    ///     <c>true</c> if a sample between the ends is off the mesh; otherwise, <c>false</c>.
    /// </returns>
    private bool LeavesMesh(Point start, Point end)
    {
        var length = start.Distance(end);
        var dx = (end.X - start.X) / length;
        var dy = (end.Y - start.Y) / length;

        for (var distance = HopRisk.SAMPLE_STEP; distance <= (length - HopRisk.SAMPLE_STEP); distance += HopRisk.SAMPLE_STEP)
            if (!Mesh.IsWalkable(start.X + dx * distance, start.Y + dy * distance))
                return true;

        return false;
    }

    private bool TryBuildCandidate(
        int from,
        int to,
        Point takeoff,
        Point landing,
        out Hop hop)
    {
        hop = default;

        if (!TryInset(
                from,
                takeoff,
                landing,
                out var insetTakeoff)
            || !TryInset(
                to,
                landing,
                takeoff,
                out var insetLanding))
            return false;

        var length = insetTakeoff.Distance(insetLanding);

        if (length > MAX_HOP_LENGTH)
            return false;

        hop = new Hop(
            from,
            to,
            insetTakeoff,
            insetLanding,
            length,
            []);

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

    /// <summary>
    ///     Pulls a hop's end back along the hop, away from the other end, until it and its lattice rounding both lie on its
    ///     piece.
    /// </summary>
    /// <param name="piece">The piece the end belongs on.</param>
    /// <param name="end">The end to pull back.</param>
    /// <param name="other">The hop's other end.</param>
    /// <param name="inset">The pulled-back end.</param>
    /// <returns>
    ///     <c>true</c> if a point within <see cref="MAX_INSET" /> qualifies; otherwise, <c>false</c>.
    /// </returns>
    private bool TryInset(
        int piece,
        Point end,
        Point other,
        out Point inset)
    {
        inset = default;

        var length = end.Distance(other);

        if (length <= 0f)
            return false;

        var dx = (end.X - other.X) / length;
        var dy = (end.Y - other.Y) / length;

        for (var distance = 0f; distance <= MAX_INSET; distance += INSET_STEP)
        {
            var x = end.X + dx * distance;
            var y = end.Y + dy * distance;

            if (IsOnPiece(piece, x, y)
                && IsOnPiece(piece, MathF.Round(x / LATTICE_STEP) * LATTICE_STEP, MathF.Round(y / LATTICE_STEP) * LATTICE_STEP))
            {
                inset = new Point(x, y);

                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Trims a line between two points on the mesh to the stretch from where it first leaves the mesh to where it last
    ///     returns; ground the line touches in between stays inside.
    /// </summary>
    /// <param name="start">The line's start.</param>
    /// <param name="end">The line's end.</param>
    /// <param name="takeoff">
    ///     The last walkable sample before the line first leaves the mesh.
    /// </param>
    /// <param name="landing">
    ///     The first walkable sample after the line last returns to the mesh.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the line leaves the mesh between two separate ground runs; otherwise, <c>false</c>.
    /// </returns>
    private bool TryTrim(
        Point start,
        Point end,
        out Point takeoff,
        out Point landing)
    {
        takeoff = default;
        landing = default;

        var length = start.Distance(end);
        var startRun = CalculateGroundRun(start, end);

        if (startRun >= length)
            return false;

        var endRun = CalculateGroundRun(end, start);

        if ((startRun + endRun) >= length)
            return false;

        var dx = (end.X - start.X) / length;
        var dy = (end.Y - start.Y) / length;

        takeoff = new Point(start.X + dx * startRun, start.Y + dy * startRun);
        landing = new Point(end.X - dx * endRun, end.Y - dy * endRun);

        return true;
    }
}