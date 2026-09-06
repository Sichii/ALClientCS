#region
using System.Collections;
using System.Runtime.CompilerServices;
using AL.Core.Geometry;
using AL.Pathfinding.Definitions;
using Poly2Tri;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     A map's walkable ground as a flat triangle mesh: unique vertices, three corner indices per triangle, three
///     neighbour ids per triangle, and a uniform grid for point location. Neighbour slot i is the triangle across the edge
///     opposite corner i, or -1 where that edge is a wall.
/// </summary>
public sealed class TriangleMesh
{
    private readonly int[] CellStart;
    private readonly int[] CellTriangles;
    private readonly int GridColumns;

    //uniform grid in CSR form: CellStart[c]..CellStart[c+1] index into CellTriangles
    private readonly int GridMinX;
    private readonly int GridMinY;
    private readonly int GridRows;

    //vertex adjacency in CSR form: VertexEdgeStart[v]..VertexEdgeStart[v+1] index into VertexEdgeTo
    private readonly int[] VertexEdgeStart;
    private readonly int[] VertexEdgeTo;

    private readonly int[] VertexTriangle;

    /// <summary>
    ///     Three vertex indices per triangle.
    /// </summary>
    public int[] Corners { get; }

    /// <summary>
    ///     Three neighbour triangle ids per triangle, -1 for none.
    /// </summary>
    public int[] Neighbours { get; }

    /// <summary>
    ///     The unique vertices, in map coordinates.
    /// </summary>
    public Point[] Vertices { get; }

    /// <summary>
    ///     How many triangles the mesh has.
    /// </summary>
    public int TriangleCount => Corners.Length / 3;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TriangleMesh" /> class.
    /// </summary>
    public TriangleMesh(Point[] vertices, int[] corners, int[] neighbours)
    {
        if ((corners.Length % 3) != 0)
            throw new ArgumentException("Three corners per triangle.", nameof(corners));

        if (neighbours.Length != corners.Length)
            throw new ArgumentException("Three neighbours per triangle.", nameof(neighbours));

        Vertices = vertices;
        Corners = corners;
        Neighbours = neighbours;

        VertexTriangle = new int[vertices.Length];
        Array.Fill(VertexTriangle, -1);

        for (var i = 0; i < corners.Length; i++)
            if (VertexTriangle[corners[i]] < 0)
                VertexTriangle[corners[i]] = i / 3;

        (VertexEdgeStart, VertexEdgeTo) = BuildVertexEdges();

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var vertex in vertices)
        {
            minX = MathF.Min(minX, vertex.X);
            minY = MathF.Min(minY, vertex.Y);
            maxX = MathF.Max(maxX, vertex.X);
            maxY = MathF.Max(maxY, vertex.Y);
        }

        if (vertices.Length == 0)
            minX = minY = maxX = maxY = 0;

        GridMinX = (int)MathF.Floor(minX);
        GridMinY = (int)MathF.Floor(minY);
        GridColumns = ((int)MathF.Ceiling(maxX) - GridMinX) / CONSTANTS.MESH_GRID_CELL + 1;
        GridRows = ((int)MathF.Ceiling(maxY) - GridMinY) / CONSTANTS.MESH_GRID_CELL + 1;

        //two passes: count triangles per cell, then fill
        var counts = new int[GridColumns * GridRows + 1];

        for (var triangle = 0; triangle < TriangleCount; triangle++)
        {
            Bounds(
                triangle,
                out var c0,
                out var r0,
                out var c1,
                out var r1);

            for (var row = r0; row <= r1; row++)
                for (var column = c0; column <= c1; column++)
                    counts[row * GridColumns + column + 1]++;
        }

        for (var i = 1; i < counts.Length; i++)
            counts[i] += counts[i - 1];

        CellStart = counts;
        CellTriangles = new int[counts[^1]];
        var fill = new int[GridColumns * GridRows];

        for (var triangle = 0; triangle < TriangleCount; triangle++)
        {
            Bounds(
                triangle,
                out var c0,
                out var r0,
                out var c1,
                out var r1);

            for (var row = r0; row <= r1; row++)
                for (var column = c0; column <= c1; column++)
                {
                    var cell = row * GridColumns + column;
                    CellTriangles[CellStart[cell] + fill[cell]] = triangle;
                    fill[cell]++;
                }
        }
    }

    private void Bounds(
        int triangle,
        out int column0,
        out int row0,
        out int column1,
        out int row1)
    {
        var a = Vertices[Corners[triangle * 3]];
        var b = Vertices[Corners[triangle * 3 + 1]];
        var c = Vertices[Corners[triangle * 3 + 2]];

        var minX = MathF.Min(a.X, MathF.Min(b.X, c.X));
        var minY = MathF.Min(a.Y, MathF.Min(b.Y, c.Y));
        var maxX = MathF.Max(a.X, MathF.Max(b.X, c.X));
        var maxY = MathF.Max(a.Y, MathF.Max(b.Y, c.Y));

        CellRange(
            minX,
            minY,
            maxX,
            maxY,
            out column0,
            out row0,
            out column1,
            out row1);
    }

    //every triangle edge once, in both directions, each vertex's targets ascending. a vertex whose triangles do not
    //form one fan (a pinch: two sectors touching only at the point) gets no edges either way, so no chain passes
    //through it and the corridor can rotate round every chain vertex in one sweep; the search may still seed it as
    //a corner of the start triangle
    private (int[] Start, int[] To) BuildVertexEdges()
    {
        var incident = new int[Vertices.Length];

        foreach (var corner in Corners)
            incident[corner]++;

        var pinched = new bool[Vertices.Length];

        for (var vertex = 0; vertex < pinched.Length; vertex++)
            pinched[vertex] = (incident[vertex] > 0) && (FanSize(vertex) != incident[vertex]);

        var edges = new HashSet<(int From, int To)>();

        for (var i = 0; i < Corners.Length; i += 3)
            for (var slot = 0; slot < 3; slot++)
            {
                var from = Corners[i + slot];
                var to = Corners[i + (slot + 1) % 3];

                if (pinched[from] || pinched[to])
                    continue;

                edges.Add((from, to));
                edges.Add((to, from));
            }

        //sorted by (from, to), so the targets already sit in csr order
        var sorted = edges.ToArray();
        Array.Sort(sorted);

        var start = new int[Vertices.Length + 1];

        foreach ((var from, _) in sorted)
            start[from + 1]++;

        for (var i = 1; i < start.Length; i++)
            start[i] += start[i - 1];

        var targets = new int[sorted.Length];

        for (var i = 0; i < sorted.Length; i++)
            targets[i] = sorted[i].To;

        return (start, targets);
    }

    private void CellRange(
        float minX,
        float minY,
        float maxX,
        float maxY,
        out int column0,
        out int row0,
        out int column1,
        out int row1)
    {
        column0 = Math.Clamp(((int)MathF.Floor(minX) - GridMinX) / CONSTANTS.MESH_GRID_CELL, 0, GridColumns - 1);
        row0 = Math.Clamp(((int)MathF.Floor(minY) - GridMinY) / CONSTANTS.MESH_GRID_CELL, 0, GridRows - 1);
        column1 = Math.Clamp(((int)MathF.Ceiling(maxX) - GridMinX) / CONSTANTS.MESH_GRID_CELL, 0, GridColumns - 1);
        row1 = Math.Clamp(((int)MathF.Ceiling(maxY) - GridMinY) / CONSTANTS.MESH_GRID_CELL, 0, GridRows - 1);
    }

    /// <summary>
    ///     The mean of the triangle's corners.
    /// </summary>
    public (float X, float Y) Centroid(int triangle)
    {
        var a = Vertices[Corners[triangle * 3]];
        var b = Vertices[Corners[triangle * 3 + 1]];
        var c = Vertices[Corners[triangle * 3 + 2]];

        return ((a.X + b.X + c.X) / 3f, (a.Y + b.Y + c.Y) / 3f);
    }

    /// <summary>
    ///     Whether (x, y) is inside the triangle, edges included.
    /// </summary>
    public bool Contains(int triangle, float x, float y)
    {
        var a = Vertices[Corners[triangle * 3]];
        var b = Vertices[Corners[triangle * 3 + 1]];
        var c = Vertices[Corners[triangle * 3 + 2]];

        var d1 = Sign(
            x,
            y,
            a,
            b);

        var d2 = Sign(
            x,
            y,
            b,
            c);

        var d3 = Sign(
            x,
            y,
            c,
            a);

        //a hair of tolerance so a point on an edge belongs to both triangles rather than neither
        const float TOLERANCE = 1e-3f;
        var hasNegative = (d1 < -TOLERANCE) || (d2 < -TOLERANCE) || (d3 < -TOLERANCE);
        var hasPositive = (d1 > TOLERANCE) || (d2 > TOLERANCE) || (d3 > TOLERANCE);

        return !(hasNegative && hasPositive);
    }

    /// <summary>
    ///     The vertices joined to <paramref name="vertex" /> by a triangle edge.
    /// </summary>
    internal ReadOnlySpan<int> EdgesFrom(int vertex)
        => VertexEdgeTo.AsSpan(VertexEdgeStart[vertex], VertexEdgeStart[vertex + 1] - VertexEdgeStart[vertex]);

    //how many of the triangles at the vertex are reached from its first one by stepping across the edges that meet
    //there, both ways round; equal to the incident count exactly when they form one sector
    private int FanSize(int vertex)
    {
        var first = VertexTriangle[vertex];
        var firstSlot = SlotOfVertex(first, vertex);
        var count = 1;

        for (var way = 1; way <= 2; way++)
        {
            var last = first;
            var triangle = Neighbour(first, (firstSlot + way) % 3);

            while ((triangle >= 0) && (triangle != first))
            {
                count++;
                var slot = SlotOfVertex(triangle, vertex);
                var across = Neighbour(triangle, (slot + 1) % 3);
                var following = across == last ? Neighbour(triangle, (slot + 2) % 3) : across;
                last = triangle;
                triangle = following;
            }

            //back round to the first triangle: the fan is closed and the other way would only repeat it
            if (triangle == first)
                break;
        }

        return count;
    }

    /// <summary>
    ///     Builds a mesh from Poly2Tri's output, translating raster coordinates back into map coordinates.
    /// </summary>
    internal static TriangleMesh FromPoly2Tri(IReadOnlyList<DelaunayTriangle> triangles, int xOffset, int yOffset)
    {
        var vertexIndex = new Dictionary<(double X, double Y), int>();
        var vertices = new List<Point>();
        var triangleIndex = new Dictionary<DelaunayTriangle, int>(ReferenceEqualityComparer.Instance);

        for (var i = 0; i < triangles.Count; i++)
            triangleIndex[triangles[i]] = i;

        var corners = new int[triangles.Count * 3];
        var neighbours = new int[triangles.Count * 3];

        for (var t = 0; t < triangles.Count; t++)
        {
            var triangle = triangles[t];

            for (var i = 0; i < 3; i++)
            {
                var point = triangle.Points[i];
                var key = (point.X, point.Y);

                if (!vertexIndex.TryGetValue(key, out var index))
                {
                    index = vertices.Count;
                    vertexIndex[key] = index;
                    vertices.Add(new Point((float)point.X - xOffset, (float)point.Y - yOffset));
                }

                corners[t * 3 + i] = index;

                //a constrained edge is a polygon edge, which is a wall; a neighbour outside the walkable set is
                //one Poly2Tri made in the exterior and is not in the index
                var across = triangle.Neighbors[i];

                neighbours[t * 3 + i]
                    = !triangle.EdgeIsConstrained[i] && across is not null && triangleIndex.TryGetValue(across, out var ni) ? ni : -1;
            }
        }

        return new TriangleMesh(vertices.ToArray(), corners, neighbours);
    }

    /// <summary>
    ///     The nearest vertex to (x, y) that <paramref name="accept" /> allows, trying the nearest
    ///     <see cref="CONSTANTS.NEAREST_VERTEX_CANDIDATES" /> in distance order; the nearest of all when none is allowed. -1
    ///     on an empty mesh.
    /// </summary>
    public int NearestVertex(float x, float y, Func<int, bool> accept)
    {
        if (Vertices.Length == 0)
            return -1;

        Span<float> bestDistance = stackalloc float[CONSTANTS.NEAREST_VERTEX_CANDIDATES];
        Span<int> bestIndex = stackalloc int[CONSTANTS.NEAREST_VERTEX_CANDIDATES];
        var count = 0;

        //insertion into a sorted fixed buffer: the mesh has a thousand or so vertices, so this is cheaper than
        //a sort and allocates nothing
        for (var i = 0; i < Vertices.Length; i++)
        {
            var dx = Vertices[i].X - x;
            var dy = Vertices[i].Y - y;
            var distance = dx * dx + dy * dy;

            if ((count == CONSTANTS.NEAREST_VERTEX_CANDIDATES) && (distance >= bestDistance[count - 1]))
                continue;

            var slot = count < CONSTANTS.NEAREST_VERTEX_CANDIDATES ? count : count - 1;

            while ((slot > 0) && (bestDistance[slot - 1] > distance))
            {
                bestDistance[slot] = bestDistance[slot - 1];
                bestIndex[slot] = bestIndex[slot - 1];
                slot--;
            }

            bestDistance[slot] = distance;
            bestIndex[slot] = i;

            if (count < CONSTANTS.NEAREST_VERTEX_CANDIDATES)
                count++;
        }

        for (var i = 0; i < count; i++)
            if (accept(bestIndex[i]))
                return bestIndex[i];

        return bestIndex[0];
    }

    /// <summary>
    ///     The neighbour across the edge opposite corner <paramref name="slot" />, or -1.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Neighbour(int triangle, int slot) => Neighbours[triangle * 3 + slot];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Sign(
        float x,
        float y,
        Point a,
        Point b)
        => (x - b.X) * (a.Y - b.Y) - (a.X - b.X) * (y - b.Y);

    /// <summary>
    ///     The corner slot <paramref name="vertex" /> occupies in <paramref name="triangle" />, or -1.
    /// </summary>
    internal int SlotOfVertex(int triangle, int vertex)
    {
        for (var slot = 0; slot < 3; slot++)
            if (Corners[triangle * 3 + slot] == vertex)
                return slot;

        return -1;
    }

    /// <summary>
    ///     The triangle containing (x, y), or -1. A point on a shared edge answers whichever triangle the grid lists first.
    /// </summary>
    public int TriangleAt(float x, float y)
    {
        var column = ((int)MathF.Floor(x) - GridMinX) / CONSTANTS.MESH_GRID_CELL;
        var row = ((int)MathF.Floor(y) - GridMinY) / CONSTANTS.MESH_GRID_CELL;

        if ((x < GridMinX) || (y < GridMinY) || (column < 0) || (column >= GridColumns) || (row < 0) || (row >= GridRows))
            return -1;

        var cell = row * GridColumns + column;

        for (var i = CellStart[cell]; i < CellStart[cell + 1]; i++)
            if (Contains(CellTriangles[i], x, y))
                return CellTriangles[i];

        return -1;
    }

    /// <summary>
    ///     A triangle the vertex belongs to.
    /// </summary>
    public int TriangleOfVertex(int vertex) => VertexTriangle[vertex];

    /// <summary>
    ///     The nearest point inside the mesh to (x, y), within <paramref name="maxDistance" />: the point itself when it is
    ///     inside, otherwise the nearest point on a boundary edge stepped one unit into its triangle. False when no boundary
    ///     edge is within range.
    /// </summary>
    public bool TryNearestInside(
        float x,
        float y,
        float maxDistance,
        out float insideX,
        out float insideY)
    {
        if (TriangleAt(x, y) >= 0)
        {
            insideX = x;
            insideY = y;

            return true;
        }

        var bestDistance = maxDistance * maxDistance;
        var bestTriangle = -1;
        var bestX = 0f;
        var bestY = 0f;

        CellRange(
            x - maxDistance,
            y - maxDistance,
            x + maxDistance,
            y + maxDistance,
            out var column0,
            out var row0,
            out var column1,
            out var row1);

        //a triangle sits in every cell its bounds touch, so one may be seen more than once; the answer is the same
        for (var row = row0; row <= row1; row++)
            for (var column = column0; column <= column1; column++)
            {
                var cell = row * GridColumns + column;

                for (var i = CellStart[cell]; i < CellStart[cell + 1]; i++)
                {
                    var triangle = CellTriangles[i];

                    for (var slot = 0; slot < 3; slot++)
                    {
                        //a shared edge is interior; only an edge with nothing across it is the mesh's boundary
                        if (Neighbours[triangle * 3 + slot] >= 0)
                            continue;

                        //the edge across from corner "slot" joins the other two corners
                        var p = Vertices[Corners[triangle * 3 + (slot + 1) % 3]];
                        var q = Vertices[Corners[triangle * 3 + (slot + 2) % 3]];

                        var ex = q.X - p.X;
                        var ey = q.Y - p.Y;
                        var lengthSquared = ex * ex + ey * ey;
                        var t = lengthSquared <= 0f ? 0f : Math.Clamp(((x - p.X) * ex + (y - p.Y) * ey) / lengthSquared, 0f, 1f);
                        var px = p.X + t * ex;
                        var py = p.Y + t * ey;
                        var dx = px - x;
                        var dy = py - y;
                        var distance = dx * dx + dy * dy;

                        if (distance >= bestDistance)
                            continue;

                        bestDistance = distance;
                        bestTriangle = triangle;
                        bestX = px;
                        bestY = py;
                    }
                }
            }

        if (bestTriangle < 0)
        {
            insideX = x;
            insideY = y;

            return false;
        }

        //a unit step toward the centroid lands strictly inside a convex triangle; one smaller than the step takes
        //the centroid itself
        (var cx, var cy) = Centroid(bestTriangle);
        var toX = cx - bestX;
        var toY = cy - bestY;
        var length = MathF.Sqrt(toX * toX + toY * toY);

        if (length <= 1f)
        {
            insideX = cx;
            insideY = cy;

            return true;
        }

        insideX = bestX + toX / length;
        insideY = bestY + toY / length;

        return true;
    }
}