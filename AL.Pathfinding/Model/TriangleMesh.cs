#region
using System.Runtime.CompilerServices;
using AL.Core.Geometry;
using AL.Pathfinding.Definitions;
using Poly2Tri;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents a map's walkable ground as a flat triangle mesh, with a uniform grid for point location. Neighbour slot
///     i is the triangle across the edge opposite corner i, or -1 where that edge is a wall.
/// </summary>
public sealed class TriangleMesh
{
    /// <summary>
    ///     Where each grid cell's triangles start in <see cref="CellTriangles" />, plus one end entry.
    /// </summary>
    private readonly int[] CellStart;

    private readonly int[] CellTriangles;
    private readonly int GridColumns;
    private readonly int GridMinX;
    private readonly int GridMinY;
    private readonly int GridRows;

    /// <summary>
    ///     Where each vertex's edges start in <see cref="VertexEdgeTo" />, plus one end entry.
    /// </summary>
    private readonly int[] VertexEdgeStart;

    private readonly int[] VertexEdgeTo;

    private readonly int[] VertexTriangle;

    /// <summary>Three vertex indices per triangle.</summary>
    public int[] Corners { get; }

    /// <summary>
    ///     Three neighbour triangle ids per triangle, -1 for none.
    /// </summary>
    public int[] Neighbours { get; }

    /// <summary>The unique vertices, in map coordinates.</summary>
    public Point[] Vertices { get; }

    /// <summary>How many triangles the mesh has.</summary>
    public int TriangleCount => Corners.Length / 3;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TriangleMesh" /> class.
    /// </summary>
    /// <param name="vertices">The unique vertices, in map coordinates.</param>
    /// <param name="corners">Three vertex indices per triangle.</param>
    /// <param name="neighbours">
    ///     Three neighbour triangle ids per triangle, -1 for none.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     corners is not a multiple of three, or neighbours is not the same length.
    /// </exception>
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
            CalculateCellBounds(
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
            CalculateCellBounds(
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

    /// <summary>
    ///     Builds every triangle edge once, in both directions, each vertex's targets ascending.
    /// </summary>
    /// <returns>
    ///     Where each vertex's targets start, plus one end entry, and the targets themselves.
    /// </returns>
    /// <remarks>
    ///     A vertex whose triangles do not form one fan, two sectors touching only at the point, gets no edges, so the
    ///     corridor can rotate round every chain vertex in one sweep.
    /// </remarks>
    private (int[] Start, int[] To) BuildVertexEdges()
    {
        var incident = new int[Vertices.Length];

        foreach (var corner in Corners)
            incident[corner]++;

        var pinched = new bool[Vertices.Length];

        for (var vertex = 0; vertex < pinched.Length; vertex++)
            pinched[vertex] = (incident[vertex] > 0) && (CalculateFanSize(vertex) != incident[vertex]);

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

    private void CalculateCellBounds(
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

        CalculateCellRange(
            minX,
            minY,
            maxX,
            maxY,
            out column0,
            out row0,
            out column1,
            out row1);
    }

    private void CalculateCellRange(
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

    /// <summary>Calculates the mean of the triangle's corners.</summary>
    /// <param name="triangle">The triangle's id.</param>
    /// <returns>The triangle's centroid.</returns>
    public (float X, float Y) CalculateCentroid(int triangle)
    {
        var a = Vertices[Corners[triangle * 3]];
        var b = Vertices[Corners[triangle * 3 + 1]];
        var c = Vertices[Corners[triangle * 3 + 2]];

        return ((a.X + b.X + c.X) / 3f, (a.Y + b.Y + c.Y) / 3f);
    }

    /// <summary>
    ///     Calculates how many of the triangles at a vertex are reached from its first one by stepping across the edges that
    ///     meet there, both ways round.
    /// </summary>
    /// <param name="vertex">The vertex's index.</param>
    /// <returns>
    ///     The fan's size, equal to the incident count exactly when the triangles form one sector.
    /// </returns>
    private int CalculateFanSize(int vertex)
    {
        var first = VertexTriangle[vertex];
        var firstSlot = FindVertexSlot(first, vertex);
        var count = 1;

        for (var way = 1; way <= 2; way++)
        {
            var last = first;
            var triangle = Neighbour(first, (firstSlot + way) % 3);

            while ((triangle >= 0) && (triangle != first))
            {
                count++;
                var slot = FindVertexSlot(triangle, vertex);
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
    ///     Determines whether a point is inside the triangle, edges included.
    /// </summary>
    /// <param name="triangle">The triangle's id.</param>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <returns>
    ///     <c>true</c> if the point is inside the triangle or on its edge; otherwise, <c>false</c> .
    /// </returns>
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
    ///     Finds the nearest vertex to a point that a filter allows, trying the nearest
    ///     <see cref="CONSTANTS.NEAREST_VERTEX_CANDIDATES" /> in distance order.
    /// </summary>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <param name="acceptFunc">
    ///     Determines whether a vertex index may be returned.
    /// </param>
    /// <returns>
    ///     The vertex's index, the nearest of all when none is allowed, or -1 on an empty mesh.
    /// </returns>
    public int FindNearestVertex(float x, float y, Func<int, bool> acceptFunc)
    {
        if (Vertices.Length == 0)
            return -1;

        Span<float> bestDistance = stackalloc float[CONSTANTS.NEAREST_VERTEX_CANDIDATES];
        Span<int> bestIndex = stackalloc int[CONSTANTS.NEAREST_VERTEX_CANDIDATES];
        var count = 0;

        //insertion into a sorted fixed buffer
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
            if (acceptFunc(bestIndex[i]))
                return bestIndex[i];

        return bestIndex[0];
    }

    /// <summary>Finds the triangle containing a point.</summary>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <returns>
    ///     The triangle's id, or -1. A point on a shared edge returns whichever triangle the grid lists first.
    /// </returns>
    public int FindTriangle(float x, float y)
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
    ///     Finds the corner slot a vertex occupies in a triangle.
    /// </summary>
    /// <param name="triangle">The triangle's id.</param>
    /// <param name="vertex">The vertex's index.</param>
    /// <returns>
    ///     The slot, 0 to 2, or -1 when the vertex is not a corner of the triangle.
    /// </returns>
    internal int FindVertexSlot(int triangle, int vertex)
    {
        for (var slot = 0; slot < 3; slot++)
            if (Corners[triangle * 3 + slot] == vertex)
                return slot;

        return -1;
    }

    /// <summary>
    ///     Builds a mesh from Poly2Tri's output, translating raster coordinates back into map coordinates.
    /// </summary>
    /// <param name="triangles">The walkable triangles.</param>
    /// <param name="xOffset">The raster's x offset from map coordinates.</param>
    /// <param name="yOffset">The raster's y offset from map coordinates.</param>
    /// <returns>The mesh in map coordinates.</returns>
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

                //a constrained edge is a wall, and a neighbour missing from the index lies outside the walkable set
                var across = triangle.Neighbors[i];

                neighbours[t * 3 + i]
                    = !triangle.EdgeIsConstrained[i] && across is not null && triangleIndex.TryGetValue(across, out var ni) ? ni : -1;
            }
        }

        return new TriangleMesh(vertices.ToArray(), corners, neighbours);
    }

    /// <summary>
    ///     Gets the vertices joined to a vertex by a triangle edge.
    /// </summary>
    /// <param name="vertex">The vertex's index.</param>
    /// <returns>The joined vertices' indices, ascending.</returns>
    internal ReadOnlySpan<int> GetVertexEdges(int vertex)
        => VertexEdgeTo.AsSpan(VertexEdgeStart[vertex], VertexEdgeStart[vertex + 1] - VertexEdgeStart[vertex]);

    /// <summary>Gets a triangle the vertex belongs to.</summary>
    /// <param name="vertex">The vertex's index.</param>
    /// <returns>The triangle's id.</returns>
    public int GetVertexTriangle(int vertex) => VertexTriangle[vertex];

    /// <summary>
    ///     Gets the neighbour across the edge opposite a corner.
    /// </summary>
    /// <param name="triangle">The triangle's id.</param>
    /// <param name="slot">The corner's slot, 0 to 2.</param>
    /// <returns>
    ///     The neighbour's id, or -1 where the edge is a wall.
    /// </returns>
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
    ///     Finds the nearest point inside the mesh: the point itself when it is inside, otherwise the nearest point on a
    ///     boundary edge stepped one unit into its triangle.
    /// </summary>
    /// <param name="x">The point's x.</param>
    /// <param name="y">The point's y.</param>
    /// <param name="maxDistance">
    ///     How far from the point to look for a boundary edge.
    /// </param>
    /// <param name="insideX">
    ///     The inside point's x, or the point's own when none is found.
    /// </param>
    /// <param name="insideY">
    ///     The inside point's y, or the point's own when none is found.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the point is inside or a boundary edge is within range; otherwise, <c>false</c> .
    /// </returns>
    public bool TryFindNearestInside(
        float x,
        float y,
        float maxDistance,
        out float insideX,
        out float insideY)
    {
        if (FindTriangle(x, y) >= 0)
        {
            insideX = x;
            insideY = y;

            return true;
        }

        var bestDistance = maxDistance * maxDistance;
        var bestTriangle = -1;
        var bestX = 0f;
        var bestY = 0f;

        CalculateCellRange(
            x - maxDistance,
            y - maxDistance,
            x + maxDistance,
            y + maxDistance,
            out var column0,
            out var row0,
            out var column1,
            out var row1);

        //a triangle sits in every cell its bounds touch, so one may be seen more than once
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

        //step one unit toward the centroid, or take the centroid when it is closer than that
        (var cx, var cy) = CalculateCentroid(bestTriangle);
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