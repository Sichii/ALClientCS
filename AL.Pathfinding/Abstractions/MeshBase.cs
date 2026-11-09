#region
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Extensions;
using AL.Pathfinding.Interfaces;
using AL.Pathfinding.Model;
using CONSTANTS = AL.Pathfinding.Definitions.CONSTANTS;
using EdgeType = AL.Pathfinding.Definitions.EdgeType;
#endregion

namespace AL.Pathfinding.Abstractions;

/// <summary>Represents a triangulated mesh of graph nodes.</summary>
/// <typeparam name="TNode">
///     An implementation of <see cref="IGraphNode{TEdge}" />.
/// </typeparam>
/// <typeparam name="TEdge">
///     An implementation of <see cref="IGraphEdge{TNode}" />.
/// </typeparam>
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public abstract class MeshBase<TNode, TEdge> : IEnumerable<TNode> where TNode: class, IGraphNode<TEdge>
                                                                  where TEdge: IGraphEdge<TNode>
{
    /// <summary>
    ///     The side of one cell in the grid that <see cref="TryFindTriangle" /> searches.
    /// </summary>
    private const int GRID_CELL_SIZE = 64;

    /// <summary>
    ///     The triangles overlapping each grid cell, by bounding box.
    /// </summary>
    private readonly Dictionary<(int X, int Y), List<IGenericTriangle<TNode>>> TriangleGrid = new();

    /// <summary>The triangles touching each vertex node.</summary>
    private readonly Dictionary<TNode, List<IGenericTriangle<TNode>>> VertexTriangles = new();

    protected internal string Map { get; }

    /// <summary>
    ///     The map's walkable ground and wall lines, which answer every geometry question.
    /// </summary>
    protected internal NavMesh NavMesh { get; }

    protected internal ICollection<IGenericTriangle<TNode>> Triangles { get; }

    protected MeshBase(NavMesh navMesh)
    {
        Map = navMesh.Map;
        NavMesh = navMesh;
        Triangles = BuildConnections(navMesh.Mesh);
        IndexTriangles();
    }

    public IEnumerator<TNode> GetEnumerator()
        => Triangles.SelectMany(t => t)
                    .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private ICollection<IGenericTriangle<TNode>> BuildConnections(TriangleMesh mesh)
    {
        var nodeDic = new Dictionary<ILocation, TNode>();
        var nodeTriangles = new HashSet<IGenericTriangle<TNode>>();

        for (var t = 0; t < mesh.TriangleCount; t++)
        {
            var vertex1 = ConstructVertex(new Location(Map, mesh.Vertices[mesh.Corners[t * 3]]));
            var vertex2 = ConstructVertex(new Location(Map, mesh.Vertices[mesh.Corners[t * 3 + 1]]));
            var vertex3 = ConstructVertex(new Location(Map, mesh.Vertices[mesh.Corners[t * 3 + 2]]));

            if (!nodeDic.TryGetValue(vertex1, out var node1))
            {
                node1 = ConstructVertexNode(vertex1);
                nodeDic[vertex1] = node1;
            }

            if (!nodeDic.TryGetValue(vertex2, out var node2))
            {
                node2 = ConstructVertexNode(vertex2);
                nodeDic[vertex2] = node2;
            }

            if (!nodeDic.TryGetValue(vertex3, out var node3))
            {
                node3 = ConstructVertexNode(vertex3);
                nodeDic[vertex3] = node3;
            }

            node1.Edges.Add(ConstructEdge(node1, node2, EdgeType.Walk));
            node1.Edges.Add(ConstructEdge(node1, node3, EdgeType.Walk));
            node2.Edges.Add(ConstructEdge(node2, node1, EdgeType.Walk));
            node2.Edges.Add(ConstructEdge(node2, node3, EdgeType.Walk));
            node3.Edges.Add(ConstructEdge(node3, node1, EdgeType.Walk));
            node3.Edges.Add(ConstructEdge(node3, node2, EdgeType.Walk));

            nodeTriangles.Add(ConstructTriangle(node1, node2, node3));
        }

        return nodeTriangles;
    }

    protected internal virtual float CalculateHeuristic(ILocation start, ILocation end) => start.Distance(end);

    /// <summary>
    ///     Determines whether a character can move in a straight line from one point to another, by the server's own test.
    /// </summary>
    /// <param name="start">The starting point.</param>
    /// <param name="end">The ending point.</param>
    /// <returns>
    ///     <c>true</c> if the move crosses no wall; otherwise, <c>false</c> .
    /// </returns>
    public bool CanMove(IPoint start, IPoint end) => NavMesh.CanMove(start, end);

    /// <summary>Constructs an edge between two nodes.</summary>
    /// <param name="start">The node the edge leaves.</param>
    /// <param name="end">The node the edge reaches.</param>
    /// <param name="type">How the edge is travelled.</param>
    /// <param name="cost">
    ///     If populated, the price in place of the one the type gives, such as a recall's per-search price.
    /// </param>
    /// <returns>The edge.</returns>
    protected internal abstract TEdge ConstructEdge(
        TNode start,
        TNode end,
        EdgeType type,
        float? cost = null);

    /// <summary>
    ///     Constructs a node for a point that is not a mesh vertex, such as a route's start or a smoothed bend. It has no
    ///     edges and takes none.
    /// </summary>
    /// <param name="vertex">The point.</param>
    /// <returns>The node.</returns>
    protected internal abstract TNode ConstructNode(ILocation vertex);

    /// <summary>
    ///     Constructs a mesh vertex's node, whose edges the mesh fills.
    /// </summary>
    /// <param name="vertex">The vertex.</param>
    /// <returns>The node.</returns>
    protected internal abstract TNode ConstructVertexNode(ILocation vertex);

    protected internal abstract IGenericTriangle<TNode> ConstructTriangle(TNode node1, TNode node2, TNode node3);

    protected internal abstract ILocation ConstructVertex(ILocation location);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected internal virtual TNode FindBestNode(ILocation vertex)
    {
        //inside a triangle every vertex of it is reachable in a straight line, so the nearest one needs no checking
        if (GetContainingTriangle(vertex) is { } containing)
            return containing.Where(n => n.Edges.Count >= 2)
                             .MinBy(n => n.Location.FastDistance(vertex))!;

        //no triangle holds it. Containment is a strict barycentric test, so this is not only a point off the mesh - a
        //point sitting on a triangle's own edge fails it too, which is where a character parked against a wall
        //stands. Nearest by raw distance is what this used to answer, and that leg is the one never validated
        var nearest = this.Where(n => n.Edges.Count >= 2)
                          .OrderBy(n => n.Location.FastDistance(vertex))
                          .Take(CONSTANTS.NEAREST_VERTEX_CANDIDATES)
                          .ToArray();

        foreach (var candidate in nearest)
            if (CanMove(vertex, candidate.Location))
                return candidate;

        //nothing near is reachable, which a point genuinely inside a wall answers this way for every node. The
        //nearest one keeps the search able to run; the caller is what has to decide whether the leg may be walked
        return nearest.FirstOrDefault()
               ?? this.Where(n => n.Edges.Count >= 2)
                      .MinBy(n => n.Location.FastDistance(vertex))!;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected internal virtual IGenericTriangle<TNode>? GetContainingTriangle(ILocation vertex)
        => TriangleGrid.TryGetValue(ToGridCell(vertex.X, vertex.Y), out var candidates)
            ? candidates.FirstOrDefault(t => t.Contains<TNode, TEdge>(vertex))
            : null;

    /// <summary>
    ///     Finds the triangles that have a node as one of their corners.
    /// </summary>
    /// <param name="vertex">A vertex node of this mesh.</param>
    /// <returns>
    ///     The triangles touching the node, or none when it is not a triangle corner.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">vertex</exception>
    public IReadOnlyList<IGenericTriangle<TNode>> FindTrianglesAt(TNode vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        return VertexTriangles.TryGetValue(vertex, out var triangles) ? triangles : [];
    }

    /// <summary>
    ///     Finds the triangle containing a point, counting a point on a shared edge as inside both.
    /// </summary>
    /// <param name="point">The point to locate.</param>
    /// <param name="triangle">The first triangle found containing the point.</param>
    /// <returns>
    ///     <c>true</c> if a triangle contains the point; otherwise, <c>false</c> .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">point</exception>
    public bool TryFindTriangle(IPoint point, [MaybeNullWhen(false)] out IGenericTriangle<TNode> triangle)
    {
        ArgumentNullException.ThrowIfNull(point);

        triangle = null;

        if (!TriangleGrid.TryGetValue(ToGridCell(point.X, point.Y), out var candidates))
            return false;

        triangle = candidates.FirstOrDefault(candidate => ContainsPoint(candidate, point));

        return triangle is not null;
    }

    /// <summary>
    ///     Determines whether a triangle contains a point by barycentric weights, with a small tolerance so a point on an edge
    ///     or corner counts as inside.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>
    ///     <c>true</c> if the triangle contains the point; otherwise, <c>false</c> .
    /// </returns>
    internal static bool ContainsPoint(IGenericTriangle<TNode> triangle, IPoint point)
    {
        const double EPSILON = 1e-5;

        var p1 = triangle.Vertices[0].Location;
        var p2 = triangle.Vertices[1].Location;
        var p3 = triangle.Vertices[2].Location;

        var det = (double)(p2.Y - p3.Y) * (p1.X - p3.X) + (double)(p3.X - p2.X) * (p1.Y - p3.Y);
        var alpha = ((double)(p2.Y - p3.Y) * (point.X - p3.X) + (double)(p3.X - p2.X) * (point.Y - p3.Y)) / det;
        var beta = ((double)(p3.Y - p1.Y) * (point.X - p3.X) + (double)(p1.X - p3.X) * (point.Y - p3.Y)) / det;
        var gamma = 1.0 - alpha - beta;

        return (alpha >= -EPSILON) && (beta >= -EPSILON) && (gamma >= -EPSILON);
    }

    private void IndexTriangles()
    {
        foreach (var triangle in Triangles)
        {
            foreach (var node in triangle.Vertices)
            {
                if (!VertexTriangles.TryGetValue(node, out var touching))
                {
                    touching = [];
                    VertexTriangles[node] = touching;
                }

                touching.Add(triangle);
            }

            (var minCellX, var minCellY) = ToGridCell(
                triangle.Vertices.Min(node => node.Location.X),
                triangle.Vertices.Min(node => node.Location.Y));

            (var maxCellX, var maxCellY) = ToGridCell(
                triangle.Vertices.Max(node => node.Location.X),
                triangle.Vertices.Max(node => node.Location.Y));

            for (var x = minCellX; x <= maxCellX; x++)
                for (var y = minCellY; y <= maxCellY; y++)
                {
                    if (!TriangleGrid.TryGetValue((x, y), out var cell))
                    {
                        cell = [];
                        TriangleGrid[(x, y)] = cell;
                    }

                    cell.Add(triangle);
                }
        }
    }

    private static (int X, int Y) ToGridCell(float x, float y)
        => ((int)MathF.Floor(x / GRID_CELL_SIZE), (int)MathF.Floor(y / GRID_CELL_SIZE));

    /// <inheritdoc cref="Model.NavMesh.IsWalkable(IPoint)" />
    public bool IsWalkable(IPoint point) => NavMesh.IsWalkable(point);

    /// <inheritdoc cref="Model.NavMesh.TryFindNearestWalkable" />
    public bool TryFindNearestWalkable(IPoint point, out IPoint walkable) => NavMesh.TryFindNearestWalkable(point, out walkable);
}