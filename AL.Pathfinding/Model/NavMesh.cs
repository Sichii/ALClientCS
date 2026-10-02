#region
using System.Runtime.InteropServices;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data.Geometry;
using AL.Data.Maps;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents one map's navigation: the exact wall test and the triangle mesh, with the per-map search over it. Every
///     method is safe to call from any thread.
/// </summary>
public sealed class NavMesh
{
    private readonly int MaxX;
    private readonly int MaxY;

    private readonly int MinX;
    private readonly int MinY;

    /// <summary>The map this mesh is for.</summary>
    public string Map { get; }

    /// <summary>The walkable ground as triangles.</summary>
    public TriangleMesh Mesh { get; }

    /// <summary>The wall lines with the local carve applied.</summary>
    public WallLines Walls { get; }

    internal NavMesh(GMap map, GGeometry geometry, TriangleMesh mesh)
    {
        Map = map.Accessor;
        Mesh = mesh;
        MinX = geometry.MinX;
        MinY = geometry.MinY;
        MaxX = geometry.MaxX;
        MaxY = geometry.MaxY;
        Walls = new WallLines(geometry.VerticalLines, geometry.HorizontalLines);
    }

    /// <summary>
    ///     Rotates round a vertex from the corridor's last triangle, appending each triangle passed, until one holds the next
    ///     vertex. Takes the way round that keeps the turn at the pivot inside the corridor, else the shorter way.
    /// </summary>
    /// <param name="corridor">
    ///     The corridor to append to.
    /// </param>
    /// <param name="previous">
    ///     The vertex before the pivot, or -1.
    /// </param>
    /// <param name="pivot">
    ///     The vertex to rotate round.
    /// </param>
    /// <param name="next">
    ///     The vertex after the pivot, or -1 to rotate until <paramref name="endTriangle" />.
    /// </param>
    /// <param name="endTriangle">
    ///     The triangle holding the walk's end.
    /// </param>
    /// <param name="end">
    ///     The walk's end.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the rotation reached its target; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    private bool AppendFan(
        List<int> corridor,
        int previous,
        int pivot,
        int next,
        int endTriangle,
        Point end)
    {
        var origin = corridor[^1];

        if (IsTarget(origin))
            return true;

        var slot = Mesh.FindVertexSlot(origin, pivot);
        var oneWay = Mesh.Neighbour(origin, (slot + 1) % 3);
        var otherWay = Mesh.Neighbour(origin, (slot + 2) % 3);
        var oneSteps = CountSteps(oneWay);
        var otherSteps = CountSteps(otherWay);

        if ((oneSteps < 0) && (otherSteps < 0))
            return false;

        int first;

        if ((oneSteps < 0) || (otherSteps < 0))
            first = oneSteps >= 0 ? oneWay : otherWay;
        else
        {
            var inside = FindInsideWay();

            first = inside >= 0
                ? inside
                : oneSteps <= otherSteps
                    ? oneWay
                    : otherWay;
        }

        var last = origin;
        var triangle = first;

        while (true)
        {
            corridor.Add(triangle);

            if (IsTarget(triangle))
                return true;

            var following = FindFollowing(triangle, last);
            last = triangle;
            triangle = following;
        }

        bool IsTarget(int candidate) => next >= 0 ? Mesh.FindVertexSlot(candidate, next) >= 0 : candidate == endTriangle;

        //the way round that keeps the turn inside the corridor, or -1 when it cannot be told
        int FindInsideWay()
        {
            if (previous < 0)
                return -1;

            var pastOne = Mesh.Corners[origin * 3 + (slot + 2) % 3];
            var pastOther = Mesh.Corners[origin * 3 + (slot + 1) % 3];

            if ((pastOne != previous) && (pastOther != previous))
                return -1;

            (var third, var pastThird, var backAcross) = pastOne == previous ? (pastOther, otherWay, oneWay) : (pastOne, oneWay, otherWay);
            var pivotPoint = Mesh.Vertices[pivot];
            var previousPoint = Mesh.Vertices[previous];
            var ahead = next >= 0 ? Mesh.Vertices[next] : end;
            var turn = CalculateTurn(previousPoint, pivotPoint, ahead);
            var side = CalculateTurn(previousPoint, pivotPoint, Mesh.Vertices[third]);

            if ((turn == 0) || (side == 0))
                return -1;

            return (turn > 0) == (side > 0) ? pastThird : backAcross;
        }

        //the z of (at - from) x (to - at), the side of the leg into at that to lies on
        static float CalculateTurn(Point from, Point at, Point to) => (at.X - from.X) * (to.Y - at.Y) - (at.Y - from.Y) * (to.X - at.X);

        //the triangle across the pivot's edge that was not entered by
        int FindFollowing(int current, int from)
        {
            var pivotSlot = Mesh.FindVertexSlot(current, pivot);
            var across = Mesh.Neighbour(current, (pivotSlot + 1) % 3);

            return across == from ? Mesh.Neighbour(current, (pivotSlot + 2) % 3) : across;
        }

        //how many triangles the rotation passes before one satisfies, or -1 off the boundary or full circle
        int CountSteps(int current)
        {
            var from = origin;
            var steps = 0;

            while ((current >= 0) && (current != origin))
            {
                steps++;

                if (IsTarget(current))
                    return steps;

                var following = FindFollowing(current, from);
                from = current;
                current = following;
            }

            return -1;
        }
    }

    /// <summary>
    ///     Determines whether a character can move in a straight line from start to end, by the server's own test.
    /// </summary>
    /// <param name="start">
    ///     The start of the move.
    /// </param>
    /// <param name="end">
    ///     The end of the move.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the move crosses no wall; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public bool CanMove(IPoint start, IPoint end)
        => Walls.CanMove(
            start.X,
            start.Y,
            end.X,
            end.Y,
            CONSTANTS.DEFAULT_BOUNDING_BASE);

    /// <summary>
    ///     <inheritdoc cref="CanMove(IPoint, IPoint)" />
    ///     <br />
    ///     The struct overload, so the search never boxes a point.
    /// </summary>
    /// <param name="start">
    ///     The start of the move.
    /// </param>
    /// <param name="end">
    ///     The end of the move.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the move crosses no wall; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    internal bool CanMove(Point start, Point end)
        => Walls.CanMove(
            start.X,
            start.Y,
            end.X,
            end.Y,
            CONSTANTS.DEFAULT_BOUNDING_BASE);

    private static void Cut(List<Point> polyline, int i, Point stop)
    {
        if (polyline.Count > (i + 1))
            polyline.RemoveRange(i + 1, polyline.Count - i - 1);

        if (!IsSamePoint(polyline[i], stop))
            polyline.Add(stop);
    }

    /// <summary>
    ///     Turns a pulled polyline into the emitted walk: reversed if asked, smoothed unless it is only a price, then cut where
    ///     it first gets inside the reach.
    /// </summary>
    /// <param name="polyline">
    ///     The pulled polyline, edited in place.
    /// </param>
    /// <param name="reach">
    ///     Where the walk counts as arrived.
    /// </param>
    /// <param name="reversed">
    ///     Specifies whether the search ran from the far end.
    /// </param>
    /// <param name="pricing">
    ///     Specifies whether the polyline is only a price.
    /// </param>
    private void Finish(
        List<Point> polyline,
        in Reach reach,
        bool reversed,
        bool pricing)
    {
        if (reversed)
            polyline.Reverse();

        if (!pricing)
            Smooth(polyline);

        TrimToReach(polyline, reach);
    }

    internal static bool IsSamePoint(Point a, Point b) => (MathF.Abs(a.X - b.X) < 0.001f) && (MathF.Abs(a.Y - b.Y) < 0.001f);

    /// <summary>
    ///     Determines whether a walk may end at a point, inside the ground the mesh was built from.
    /// </summary>
    /// <param name="x">
    ///     The point's x.
    /// </param>
    /// <param name="y">
    ///     The point's y.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the point is on the mesh; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    /// <remarks>
    ///     Water and enclosed pockets pass <see cref="IsWall(float, float)" />, so this is the test a standing point needs.
    /// </remarks>
    public bool IsWalkable(float x, float y) => Mesh.FindTriangle(x, y) >= 0;

    /// <inheritdoc cref="IsWalkable(float, float)" />
    public bool IsWalkable(IPoint point) => IsWalkable(point.X, point.Y);

    /// <summary>
    ///     Determines whether a character standing at a point has a wall inside its collision box, or is off the map.
    /// </summary>
    /// <param name="x">
    ///     The point's x.
    /// </param>
    /// <param name="y">
    ///     The point's y.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the client refuses every move out of the point; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public bool IsWall(float x, float y)
        => (x < MinX) || (x > MaxX) || (y < MinY) || (y > MaxY) || Walls.BoxIntersects(x, y, CONSTANTS.DEFAULT_BOUNDING_BASE);

    /// <inheritdoc cref="IsWall(float, float)" />
    public bool IsWall(IPoint point) => IsWall(point.X, point.Y);

    private bool AreLegsClear(List<Point> polyline)
    {
        for (var i = 1; i < polyline.Count; i++)
            if (!CanMove(polyline[i - 1], polyline[i]))
                return false;

        return true;
    }

    /// <summary>Calculates the total length of a polyline.</summary>
    /// <param name="polyline">The polyline to measure.</param>
    /// <returns>The sum of its legs.</returns>
    internal static float CalculateLength(List<Point> polyline)
    {
        var length = 0f;

        for (var i = 1; i < polyline.Count; i++)
            length += polyline[i - 1]
                .Distance(polyline[i]);

        return length;
    }

    /// <summary>
    ///     Finds the triangle a search from a point starts in. A point outside every triangle starts at the nearest vertex it
    ///     can reach in a straight line, or the nearest of all.
    /// </summary>
    /// <param name="x">
    ///     The point's x.
    /// </param>
    /// <param name="y">
    ///     The point's y.
    /// </param>
    /// <param name="entry">
    ///     The point the search starts from.
    /// </param>
    /// <returns>
    ///     The triangle's id, or -1 on an empty mesh.
    /// </returns>
    internal int FindStartTriangle(float x, float y, out Point entry)
    {
        var triangle = Mesh.FindTriangle(x, y);

        if (triangle >= 0)
        {
            entry = new Point(x, y);

            return triangle;
        }

        var origin = new Point(x, y);
        var vertex = Mesh.FindNearestVertex(x, y, index => CanMove(origin, Mesh.Vertices[index]));

        if (vertex < 0)
        {
            entry = origin;

            return -1;
        }

        entry = Mesh.Vertices[vertex];

        return Mesh.GetVertexTriangle(vertex);
    }

    private void BuildMidpointPolyline(
        List<int> corridor,
        Point start,
        Point end,
        List<Point> polyline)
    {
        polyline.Clear();
        polyline.Add(start);

        for (var i = 0; i < (corridor.Count - 1); i++)
        {
            var from = corridor[i];
            var to = corridor[i + 1];

            for (var slot = 0; slot < 3; slot++)
            {
                if (Mesh.Neighbour(from, slot) != to)
                    continue;

                var a = Mesh.Vertices[Mesh.Corners[from * 3 + (slot + 1) % 3]];
                var b = Mesh.Vertices[Mesh.Corners[from * 3 + (slot + 2) % 3]];
                polyline.Add(new Point((a.X + b.X) / 2f, (a.Y + b.Y) / 2f));

                break;
            }
        }

        polyline.Add(end);
    }

    /// <summary>
    ///     Runs Dijkstra over the mesh's vertices along triangle edges, seeded from the corners of the start triangle, into the
    ///     scratch's vertex arrays.
    /// </summary>
    /// <param name="start">
    ///     The triangle the search starts in.
    /// </param>
    /// <param name="entry">
    ///     The point the search starts from.
    /// </param>
    /// <param name="scratch">
    ///     The calling thread's search state.
    /// </param>
    /// <param name="stopTriangle">
    ///     The one target's triangle, or -1 to search the whole mesh.
    /// </param>
    /// <param name="stopPoint">
    ///     The one target's point.
    /// </param>
    internal void Search(
        int start,
        Point entry,
        SearchScratch scratch,
        int stopTriangle = -1,
        Point stopPoint = default)
    {
        scratch.ResetVertices(Mesh.Vertices.Length);
        scratch.SearchTriangle = start;

        //with one target, the search ends once nothing left in the queue can beat the best corner of its triangle
        var bound = float.MaxValue;

        for (var slot = 0; slot < 3; slot++)
        {
            var corner = Mesh.Corners[start * 3 + slot];
            var cost = entry.Distance(Mesh.Vertices[corner]);
            scratch.VertexCost[corner] = cost;
            scratch.VertexQueue.Enqueue(corner, cost);
            TightenBound(corner, cost);
        }

        while (scratch.VertexQueue.TryDequeue(out var vertex, out var cost))
        {
            //lazy deletion: a stale entry is one a cheaper push has already superseded
            if (cost > scratch.VertexCost[vertex])
                continue;

            if (cost >= bound)
                return;

            var from = Mesh.Vertices[vertex];

            foreach (var next in Mesh.GetVertexEdges(vertex))
            {
                var total = cost + from.Distance(Mesh.Vertices[next]);

                if (total >= scratch.VertexCost[next])
                    continue;

                scratch.VertexCost[next] = total;
                scratch.VertexParent[next] = vertex;
                scratch.VertexQueue.Enqueue(next, total);
                TightenBound(next, total);
            }
        }

        void TightenBound(int vertex, float cost)
        {
            if ((stopTriangle >= 0) && (Mesh.FindVertexSlot(stopTriangle, vertex) >= 0))
                bound = MathF.Min(
                    bound,
                    cost
                    + Mesh.Vertices[vertex]
                          .Distance(stopPoint));
        }
    }

    /// <summary>
    ///     Collapses the polyline onto the fewest legs the move test accepts, scanning back from the end for the farthest
    ///     vertex each one can reach in a straight line.
    /// </summary>
    /// <param name="polyline">
    ///     The polyline, edited in place.
    /// </param>
    private void Smooth(List<Point> polyline)
    {
        if (polyline.Count < 3)
            return;

        var kept = 1;

        for (var i = 0; i < (polyline.Count - 1);)
        {
            var next = i + 1;

            for (var j = polyline.Count - 1; j > next; j--)
                if (CanMove(polyline[i], polyline[j]))
                {
                    next = j;

                    break;
                }

            //compacts in place: the write index never passes the read index
            polyline[kept++] = polyline[next];
            i = next;
        }

        polyline.RemoveRange(kept, polyline.Count - kept);
    }

    /// <summary>
    ///     Cuts the polyline where it first gets inside the reach: at each vertex, the nearest reach point, else the next leg's
    ///     entry, whichever is walkable and clear first. Failing both, the polyline is left whole.
    /// </summary>
    /// <param name="polyline">
    ///     The polyline, edited in place.
    /// </param>
    /// <param name="reach">
    ///     Where the walk counts as arrived.
    /// </param>
    private void TrimToReach(List<Point> polyline, in Reach reach)
    {
        if (polyline.Count == 0)
            return;

        if (reach.Contains(polyline[0].X, polyline[0].Y))
        {
            polyline.Clear();

            return;
        }

        for (var i = 0; i < polyline.Count; i++)
        {
            var vertex = polyline[i];
            (var nx, var ny) = reach.FindNearestEdgePoint(vertex.X, vertex.Y);
            var near = new Point(nx, ny);

            if (IsWalkable(nx, ny) && CanMove(vertex, near))
            {
                Cut(polyline, i, near);

                return;
            }

            if ((i + 1) >= polyline.Count)
                break;

            var next = polyline[i + 1];

            if (reach.TryFindEntry(
                    vertex.X,
                    vertex.Y,
                    next.X,
                    next.Y,
                    out var ex,
                    out var ey))
            {
                var entry = new Point(ex, ey);

                if (IsWalkable(ex, ey) && CanMove(vertex, entry))
                {
                    Cut(polyline, i, entry);

                    return;
                }
            }
        }
    }

    /// <summary>
    ///     Builds the corridor from the search's start triangle to the end's, into the scratch list: the fan round each vertex
    ///     of the cheapest vertex path.
    /// </summary>
    /// <param name="endTriangle">
    ///     The triangle holding the end.
    /// </param>
    /// <param name="end">
    ///     The walk's end.
    /// </param>
    /// <param name="scratch">
    ///     The calling thread's search state.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if a corner of the end's triangle was reached; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    private bool TryBuildCorridor(int endTriangle, Point end, SearchScratch scratch)
    {
        var corridor = scratch.Corridor;
        corridor.Clear();
        corridor.Add(scratch.SearchTriangle);

        if (endTriangle == scratch.SearchTriangle)
            return true;

        var best = -1;
        var bestCost = float.MaxValue;

        for (var slot = 0; slot < 3; slot++)
        {
            var corner = Mesh.Corners[endTriangle * 3 + slot];

            //float.MaxValue is the unreached sentinel
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (scratch.VertexCost[corner] == float.MaxValue)
                continue;

            var cost = scratch.VertexCost[corner]
                       + Mesh.Vertices[corner]
                             .Distance(end);

            if (cost >= bestCost)
                continue;

            bestCost = cost;
            best = corner;
        }

        if (best < 0)
            return false;

        //the start's corners carry no parent, so the chain read back from the end's corner begins in the start triangle
        var chain = scratch.VertexChain;
        chain.Clear();

        for (var vertex = best; vertex >= 0; vertex = scratch.VertexParent[vertex])
            chain.Add(vertex);

        chain.Reverse();

        for (var i = 0; i < chain.Count; i++)
        {
            var previous = i > 0 ? chain[i - 1] : -1;
            var next = (i + 1) < chain.Count ? chain[i + 1] : -1;

            if (!AppendFan(
                    corridor,
                    previous,
                    chain[i],
                    next,
                    endTriangle,
                    end))
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Finds the nearest point inside the ground, within <see cref="CONSTANTS.MAX_UNSTICK_DISTANCE" />.
    /// </summary>
    /// <param name="point">
    ///     The point to start from.
    /// </param>
    /// <param name="walkable">
    ///     The nearest walkable point, or <see cref="Point.None" />.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if a walkable point was found; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public bool TryFindNearestWalkable(IPoint point, out IPoint walkable)
    {
        if (IsWalkable(point))
        {
            walkable = point;

            return true;
        }

        if (Mesh.TryFindNearestInside(
                point.X,
                point.Y,
                CONSTANTS.MAX_UNSTICK_DISTANCE,
                out var x,
                out var y))
        {
            walkable = new Point(x, y);

            return true;
        }

        walkable = Point.None;

        return false;
    }

    /// <summary>
    ///     Builds the walk <see cref="Search" /> found to an end, pulled, smoothed and trimmed to stop inside the reach.
    /// </summary>
    /// <param name="start">
    ///     The search's start point.
    /// </param>
    /// <param name="endTriangle">
    ///     The triangle holding the end.
    /// </param>
    /// <param name="end">
    ///     The entry the walk's end resolved to.
    /// </param>
    /// <param name="reach">
    ///     Where the walk counts as arrived.
    /// </param>
    /// <param name="scratch">
    ///     The calling thread's search state.
    /// </param>
    /// <param name="polyline">
    ///     The list the walk is written into; empty when the start is already inside the reach.
    /// </param>
    /// <param name="reversed">
    ///     Specifies whether the search ran from the far end, which flips the polyline first.
    /// </param>
    /// <param name="pricing">
    ///     Specifies whether the walk is only a price, which skips the smoothing and the leg check.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if the end was reached; otherwise,
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    internal bool TryWalk(
        Point start,
        int endTriangle,
        Point end,
        in Reach reach,
        SearchScratch scratch,
        List<Point> polyline,
        bool reversed = false,
        bool pricing = false)
    {
        polyline.Clear();

        if ((endTriangle < 0) || !TryBuildCorridor(endTriangle, end, scratch))
            return false;

        var corridor = scratch.Corridor;

        Funnel.Pull(
            Mesh,
            CollectionsMarshal.AsSpan(corridor),
            start,
            end,
            polyline);

        Finish(
            polyline,
            reach,
            reversed,
            pricing);

        //a leg fails only on a malformed corridor; a leg between two portal midpoints of one triangle cannot leave it
        if (pricing || AreLegsClear(polyline))
            return true;

        BuildMidpointPolyline(
            corridor,
            start,
            end,
            polyline);

        Finish(
            polyline,
            reach,
            reversed,
            pricing);

        return true;
    }

    /// <summary>
    ///     Calculates the length of the walk <see cref="Search" /> found to an end, pulled and trimmed as the emitted walk is
    ///     but not smoothed.
    /// </summary>
    /// <param name="start">
    ///     The search's start point.
    /// </param>
    /// <param name="triangle">
    ///     The triangle holding the end.
    /// </param>
    /// <param name="end">
    ///     The entry the walk's end resolved to.
    /// </param>
    /// <param name="reach">
    ///     Where the walk counts as arrived.
    /// </param>
    /// <param name="scratch">
    ///     The calling thread's search state.
    /// </param>
    /// <param name="reversed">
    ///     Specifies whether the search ran from the far end.
    /// </param>
    /// <returns>
    ///     The walk's length, or <see cref="float.MaxValue" /> when the triangle was never reached.
    /// </returns>
    internal float CalculateWalkCost(
        Point start,
        int triangle,
        Point end,
        in Reach reach,
        SearchScratch scratch,
        bool reversed = false)
        => TryWalk(
            start,
            triangle,
            end,
            reach,
            scratch,
            scratch.Polyline,
            reversed,
            true)
            ? CalculateLength(scratch.Polyline)
            : float.MaxValue;
}