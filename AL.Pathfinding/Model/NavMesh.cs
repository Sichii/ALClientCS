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
///     One map's navigation: the exact wall test and the triangle mesh, with the per-map
///     search over it. Every method is safe to call from any thread; search state lives in
///     <see cref="SearchScratch" />.
/// </summary>
public sealed class NavMesh
{
    /// <summary>The map this mesh is for.</summary>
    public string Map { get; }

    /// <summary>The walkable ground as triangles.</summary>
    public TriangleMesh Mesh { get; }

    /// <summary>The wall lines with the local carve applied.</summary>
    public WallLines Walls { get; }

    private readonly int MinX;
    private readonly int MinY;
    private readonly int MaxX;
    private readonly int MaxY;

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
    ///     Whether a character standing at (x, y) has a wall inside its collision box, or is off the map. A point
    ///     where this is true is one the client refuses every move out of.
    /// </summary>
    public bool IsWall(float x, float y)
        => (x < MinX) || (x > MaxX) || (y < MinY) || (y > MaxY) || Walls.BoxIntersects(x, y, CONSTANTS.DEFAULT_BOUNDING_BASE);

    /// <inheritdoc cref="IsWall(float, float)" />
    public bool IsWall(IPoint point) => IsWall(point.X, point.Y);

    /// <summary>
    ///     Whether a walk may end at (x, y): inside the ground the mesh was built from. Water and enclosed pockets
    ///     have no line to cross but are not ground either, so this, not <see cref="IsWall(float, float)" />, is the
    ///     test a standing point needs.
    /// </summary>
    public bool IsWalkable(float x, float y) => Mesh.TriangleAt(x, y) >= 0;

    /// <inheritdoc cref="IsWalkable(float, float)" />
    public bool IsWalkable(IPoint point) => IsWalkable(point.X, point.Y);

    /// <summary>
    ///     Whether a character can move in a straight line from start to end: the server's own test.
    /// </summary>
    public bool CanMove(IPoint start, IPoint end)
        => Walls.CanMove(
            start.X,
            start.Y,
            end.X,
            end.Y,
            CONSTANTS.DEFAULT_BOUNDING_BASE);

    //the struct overload, so the search never boxes a point to ask
    internal bool CanMove(Point start, Point end)
        => Walls.CanMove(
            start.X,
            start.Y,
            end.X,
            end.Y,
            CONSTANTS.DEFAULT_BOUNDING_BASE);

    /// <summary>
    ///     The nearest point inside the ground, within <see cref="CONSTANTS.MAX_UNSTICK_DISTANCE" />.
    /// </summary>
    public bool TryFindNearestWalkable(IPoint point, out IPoint walkable)
    {
        if (IsWalkable(point))
        {
            walkable = point;

            return true;
        }

        if (Mesh.TryNearestInside(
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
    ///     The triangle a search from (x, y) starts in, and the point it starts from. A point inside a triangle
    ///     starts there; one outside every triangle starts at the nearest vertex it can reach in a straight line,
    ///     or the nearest of all when none can be reached.
    /// </summary>
    internal int Locate(float x, float y, out Point entry)
    {
        var triangle = Mesh.TriangleAt(x, y);

        if (triangle >= 0)
        {
            entry = new Point(x, y);

            return triangle;
        }

        var origin = new Point(x, y);
        var vertex = Mesh.NearestVertex(x, y, index => CanMove(origin, Mesh.Vertices[index]));

        if (vertex < 0)
        {
            entry = origin;

            return -1;
        }

        entry = Mesh.Vertices[vertex];

        return Mesh.TriangleOfVertex(vertex);
    }

    /// <summary>
    ///     Dijkstra over the mesh's vertices along triangle edges, seeded from the corners of <paramref name="start" />,
    ///     into the scratch's vertex arrays. The shortest walk through a polygon bends only at its vertices, so a
    ///     path along triangle edges goes round the right side of every obstacle, and the funnel straightens it
    ///     inside the corridor it traces. A search over the triangles themselves, priced portal to portal, settles a
    ///     big triangle on whichever side is entered first, which sent walks the long way round an arena. Given
    ///     <paramref name="stopTriangle" /> and <paramref name="stopPoint" /> the search stops as soon as that one
    ///     target is settled; without them it runs the whole mesh, for a search read at many targets.
    /// </summary>
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
            Bound(corner, cost);
        }

        while (scratch.VertexQueue.TryDequeue(out var vertex, out var cost))
        {
            //lazy deletion: a stale entry is one a cheaper push has already superseded
            if (cost > scratch.VertexCost[vertex])
                continue;

            if (cost >= bound)
                return;

            var from = Mesh.Vertices[vertex];

            foreach (var next in Mesh.EdgesFrom(vertex))
            {
                var total = cost + from.Distance(Mesh.Vertices[next]);

                if (total >= scratch.VertexCost[next])
                    continue;

                scratch.VertexCost[next] = total;
                scratch.VertexParent[next] = vertex;
                scratch.VertexQueue.Enqueue(next, total);
                Bound(next, total);
            }
        }

        void Bound(int vertex, float cost)
        {
            if ((stopTriangle >= 0) && (Mesh.SlotOfVertex(stopTriangle, vertex) >= 0))
                bound = MathF.Min(bound, cost + Mesh.Vertices[vertex].Distance(stopPoint));
        }
    }

    /// <summary>
    ///     The length of the walk <see cref="Search" /> found to <paramref name="end" /> in triangle
    ///     <paramref name="triangle" />: pulled and trimmed as the emitted walk is, so it is a real walk's length
    ///     and not the search's estimate, and a stop short of a door is priced as the stop and not the door.
    ///     Infinity when the triangle was never reached. Every cost the portal search compares comes from here, at
    ///     build and per search. Not smoothed: the smoothing takes about the same few percent off every walk and
    ///     costs half of what pricing one takes, so the price is the pulled length the way the vertex graph's was.
    /// </summary>
    internal float WalkCost(
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
            pricing: true)
            ? Length(scratch.Polyline)
            : float.MaxValue;

    /// <summary>
    ///     The corridor from the search's start triangle to <paramref name="end" /> in <paramref name="endTriangle" />,
    ///     into the scratch list: the start triangle, then the fan round each vertex of the cheapest vertex path as
    ///     far as the triangle holding the edge to the next vertex, ending in the end's own triangle. False when no
    ///     corner of the end's triangle was reached.
    /// </summary>
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

            if (scratch.VertexCost[corner] == float.MaxValue)
                continue;

            var cost = scratch.VertexCost[corner] + Mesh.Vertices[corner].Distance(end);

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

            if (!AppendFan(corridor, previous, chain[i], next, endTriangle, end))
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Rotates round <paramref name="pivot" /> from the corridor's last triangle, appending each triangle passed,
    ///     until one holds <paramref name="next" />, or is <paramref name="endTriangle" /> when there is no next
    ///     vertex. Of the two ways round, the one keeping the turn at the pivot inside the corridor is taken, so the
    ///     funnel pulls straight past it; the shorter way when the turn cannot be told, the only way at a boundary.
    /// </summary>
    private bool AppendFan(
        List<int> corridor,
        int previous,
        int pivot,
        int next,
        int endTriangle,
        Point end)
    {
        var origin = corridor[^1];

        if (Satisfies(origin))
            return true;

        var slot = Mesh.SlotOfVertex(origin, pivot);
        var oneWay = Mesh.Neighbour(origin, (slot + 1) % 3);
        var otherWay = Mesh.Neighbour(origin, (slot + 2) % 3);
        var oneSteps = Steps(oneWay);
        var otherSteps = Steps(otherWay);

        if ((oneSteps < 0) && (otherSteps < 0))
            return false;

        int first;

        if ((oneSteps < 0) || (otherSteps < 0))
            first = oneSteps >= 0 ? oneWay : otherWay;
        else
        {
            var inside = InsideWay();
            first = inside >= 0 ? inside : oneSteps <= otherSteps ? oneWay : otherWay;
        }

        var last = origin;
        var triangle = first;

        while (true)
        {
            corridor.Add(triangle);

            if (Satisfies(triangle))
                return true;

            var following = Following(triangle, last);
            last = triangle;
            triangle = following;
        }

        bool Satisfies(int triangle) => next >= 0 ? Mesh.SlotOfVertex(triangle, next) >= 0 : triangle == endTriangle;

        //the way round on which the funnel pulls straight past the pivot: the origin's corner that is neither the
        //pivot nor the previous vertex lies on the same side of the incoming leg as the point ahead, so rotating
        //past that corner keeps the turn inside the corridor; otherwise the way back across the incoming edge does.
        //-1 with no previous vertex, or when the turn cannot be told
        int InsideWay()
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
            var turn = Turn(previousPoint, pivotPoint, ahead);
            var side = Turn(previousPoint, pivotPoint, Mesh.Vertices[third]);

            if ((turn == 0) || (side == 0))
                return -1;

            return (turn > 0) == (side > 0) ? pastThird : backAcross;
        }

        //the z of (at - from) x (to - at): which side of the leg into at the point to lies on
        static float Turn(Point from, Point at, Point to)
            => (at.X - from.X) * (to.Y - at.Y) - (at.Y - from.Y) * (to.X - at.X);

        //the pivot's other edge in the triangle, which is the one not entered by
        int Following(int triangle, int from)
        {
            var pivotSlot = Mesh.SlotOfVertex(triangle, pivot);
            var across = Mesh.Neighbour(triangle, (pivotSlot + 1) % 3);

            return across == from ? Mesh.Neighbour(triangle, (pivotSlot + 2) % 3) : across;
        }

        //how many triangles the rotation passes before one satisfies, or -1 off the boundary or full circle
        int Steps(int triangle)
        {
            var from = origin;
            var steps = 0;

            while ((triangle >= 0) && (triangle != origin))
            {
                steps++;

                if (Satisfies(triangle))
                    return steps;

                var following = Following(triangle, from);
                from = triangle;
                triangle = following;
            }

            return -1;
        }
    }

    /// <summary>
    ///     After <see cref="Search" />, the string-pulled polyline from the search's start to <paramref name="end" />,
    ///     the entry inside triangle <paramref name="endTriangle" /> that the walk's end resolved to, smoothed with
    ///     the exact move test and trimmed to stop inside <paramref name="reach" />. Empty when the start is already
    ///     inside the reach. False when the end was never reached. <paramref name="reversed" /> flips the polyline
    ///     first, for a search run from the far end: the reach is then around the point the character walks toward.
    ///     <paramref name="pricing" /> keeps the pull as it is, unsmoothed and unchecked, for a price rather than a
    ///     walk.
    /// </summary>
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

        Finish(polyline, reach, reversed, pricing);

        //the pull keeps every leg inside the corridor, which is inside the walkable polygon, so this only fires
        //on a corridor Poly2Tri shaped strangely; the portal midpoints are the fallback because a leg between two
        //midpoints of one triangle cannot leave it. A price is not checked: one wrong on a corridor like that is
        //a rounding error against what the checks cost on every exit of every search
        if (pricing || LegsAreClear(polyline))
            return true;

        MidpointPolyline(corridor, start, end, polyline);
        Finish(polyline, reach, reversed, pricing);

        return true;
    }

    //a pulled polyline into the walk that is emitted: turned round for a search run from the far end, collapsed
    //onto the legs the move test accepts unless it is only a price, then cut where it first gets inside the reach
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

    private bool LegsAreClear(List<Point> polyline)
    {
        for (var i = 1; i < polyline.Count; i++)
            if (!CanMove(polyline[i - 1], polyline[i]))
                return false;

        return true;
    }

    /// <summary>
    ///     Collapses the polyline onto the fewest legs the move test accepts, scanning back from the end for the
    ///     farthest vertex each one can reach in a straight line. The corridor is traced from a padded raster and is
    ///     more conservative than the wall lines at every corner, so a pull that hugs its staircase vertices leaves
    ///     legs a straight line would have cleared.
    /// </summary>
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

    private void MidpointPolyline(List<int> corridor, Point start, Point end, List<Point> polyline)
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
    ///     Cuts the polyline where it first gets inside the reach. From each vertex in order: the near-edge point if
    ///     it is walkable and the line to it is clear; else the first point of the next leg inside the reach, on the
    ///     same terms. Failing both, the whole polyline: it ends on the entry the end resolved to, which is on the
    ///     mesh, and is as near the end as a walk can get.
    /// </summary>
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
            (var nx, var ny) = reach.NearEdge(vertex.X, vertex.Y);
            var near = new Point(nx, ny);

            if (IsWalkable(nx, ny) && CanMove(vertex, near))
            {
                Cut(polyline, i, near);

                return;
            }

            if ((i + 1) >= polyline.Count)
                break;

            var next = polyline[i + 1];

            if (reach.TryEntry(
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

    //keep vertices 0..i, then end on the stop point
    private static void Cut(List<Point> polyline, int i, Point stop)
    {
        if (polyline.Count > (i + 1))
            polyline.RemoveRange(i + 1, polyline.Count - i - 1);

        if (!IsSame(polyline[i], stop))
            polyline.Add(stop);
    }

    internal static bool IsSame(Point a, Point b) => (MathF.Abs(a.X - b.X) < 0.001f) && (MathF.Abs(a.Y - b.Y) < 0.001f);

    /// <summary>The total length of a polyline.</summary>
    internal static float Length(List<Point> polyline)
    {
        var length = 0f;

        for (var i = 1; i < polyline.Count; i++)
            length += polyline[i - 1].Distance(polyline[i]);

        return length;
    }
}
