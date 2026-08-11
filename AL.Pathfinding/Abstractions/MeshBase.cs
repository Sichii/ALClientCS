#region
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Extensions;
using AL.Pathfinding.Interfaces;
using Chaos.Extensions.Common;
using Priority_Queue;
#endregion

namespace AL.Pathfinding.Abstractions;

/// <summary>
///     Represents a triangulated mesh of graph nodes.
/// </summary>
/// <typeparam name="TNode">
///     An implementation of <see cref="IGraphNode{TEdge}" />.
/// </typeparam>
/// <typeparam name="TEdge">
///     An implementation of <see cref="IGraphEdge{TNode}" />.
/// </typeparam>
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public abstract class MeshBase<TNode, TEdge> : IEnumerable<TNode> where TNode: FastPriorityQueueNode, IGraphNode<TEdge>
                                                                  where TEdge: IGraphEdge<TNode>
{
    protected internal readonly TNode? TownNode;
    protected internal string Map { get; set; }
    protected internal PointType[,] PointMap { get; set; }
    protected internal ICollection<IGenericTriangle<TNode>> Triangles { get; set; }
    protected internal int XOffset { get; }
    protected internal int YOffset { get; }

    protected MeshBase(
        string map,
        IEnumerable<IGenericTriangle<ILocation>> triangles,
        PointType[,] pointMap,
        int xOffset,
        int yOffset)
    {
        Map = map;
        PointMap = pointMap;
        XOffset = xOffset;
        YOffset = yOffset;
        Triangles = BuildConnections(triangles);
        TownNode = CreateTownNode();
    }

    public IEnumerator<TNode> GetEnumerator()
        => Triangles.SelectMany(t => t)
                    .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     The pointmap can only have positive indexes. This method applies an offset to a triangulated point to make it
    ///     equivalent to it's pointmap index.
    /// </summary>
    /// <param name="point">
    ///     The point to offset.
    /// </param>
    /// <returns>
    ///     <see cref="IPoint" />
    ///     <br />
    ///     A new point with it's coordinate offset to the positive quadrant.
    /// </returns>
    public virtual IPoint ApplyOffset(IPoint point) => new Point(point.X + XOffset, point.Y + YOffset);

    private ICollection<IGenericTriangle<TNode>> BuildConnections(IEnumerable<IGenericTriangle<ILocation>> triangles)
    {
        var nodeDic = new Dictionary<ILocation, TNode>();
        var nodeTriangles = new HashSet<IGenericTriangle<TNode>>();

        foreach (var triangle in triangles)
        {
            var loc1 = triangle.Vertices[0];
            var loc2 = triangle.Vertices[1];
            var loc3 = triangle.Vertices[2];
            var offsetPoint1 = RemoveOffset(loc1);
            var offsetPoint2 = RemoveOffset(loc2);
            var offsetPoint3 = RemoveOffset(loc3);
            var vertex1 = ConstructVertex(new Location(loc1.Map, offsetPoint1));
            var vertex2 = ConstructVertex(new Location(loc2.Map, offsetPoint2));
            var vertex3 = ConstructVertex(new Location(loc3.Map, offsetPoint3));

            if (!nodeDic.TryGetValue(vertex1, out var node1))
            {
                node1 = ConstructNode(vertex1);
                nodeDic[vertex1] = node1;
            }

            if (!nodeDic.TryGetValue(vertex2, out var node2))
            {
                node2 = ConstructNode(vertex2);
                nodeDic[vertex2] = node2;
            }

            if (!nodeDic.TryGetValue(vertex3, out var node3))
            {
                node3 = ConstructNode(vertex3);
                nodeDic[vertex3] = node3;
            }

            node1.Edges.Add(ConstructEdge(node1, node2));
            node1.Edges.Add(ConstructEdge(node1, node3));
            node2.Edges.Add(ConstructEdge(node2, node1));
            node2.Edges.Add(ConstructEdge(node2, node3));
            node3.Edges.Add(ConstructEdge(node3, node1));
            node3.Edges.Add(ConstructEdge(node3, node2));

            nodeTriangles.Add(ConstructTriangle(node1, node2, node3));
        }

        return nodeTriangles;
    }

    protected internal virtual float CalculateHeuristic(ILocation start, ILocation end) => start.Distance(end);

    /// <summary>
    ///     Determines whether or not it's possible to move from one point to another.
    /// </summary>
    /// <param name="start">
    ///     The starting point.
    /// </param>
    /// <param name="end">
    ///     The ending point.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if you can move from <paramref name="start" /> to <paramref name="end" />, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public virtual bool CanMove(IPoint start, IPoint end)
    {
        var startOffset = ApplyOffset(start);
        var endOffset = ApplyOffset(end);
        var width = PointMap.GetLength(0);
        var height = PointMap.GetLength(1);

        //(int) rather than Convert.ToInt32, which rounds and range checks - RayTraceTo only ever
        //yields whole numbers, and this runs once per grid cell the line crosses
        foreach (var point in startOffset.RayTraceTo(endOffset))
        {
            var x = (int)point.X;
            var y = (int)point.Y;

            //same reckoning as IsWalkable - the point map is indexed directly, so a line leaving the map's extents
            //would throw rather than answer, and nothing out there was walkable anyway
            if ((x < 0) || (y < 0) || (x >= width) || (y >= height))
                return false;

            if (PointMap[x, y]
                .HasFlag(PointType.Wall))
                return false;
        }

        return true;
    }

    protected internal virtual EdgeType ConnectorTypeSelector(ILocation start, ILocation end)
    {
        var gMap1 = GameData.Maps[start.Map]!;

        if (!start.OnSameMapAs(end))
        {
            if (gMap1 is { Irregular: true })
                return EdgeType.Leave;

            if (gMap1.Doors.Any(door => door.Equals(start)))
                return EdgeType.Door;
        }

        //a map can reference an npc id that G.npcs has no entry for, which leaves Data null
        //after enrichment - one unknown npc must not abort the whole nav mesh build
        var transport = gMap1.NPCs.FirstOrDefault(npc => (npc.Data?.Role == NPCRole.Transport) && npc.Locations.Contains(start));

        if (transport != null)
            return EdgeType.Transport;

        if ((TownNode != null) && TownNode.Vertex.Equals(end))
            return EdgeType.Town;

        return EdgeType.Walk;
    }

    protected internal abstract TEdge ConstructEdge(TNode start, TNode end, EdgeType? typeOverride = null);

    protected internal abstract TNode ConstructNode(ILocation vertex);

    protected internal abstract IGenericTriangle<TNode> ConstructTriangle(TNode node1, TNode node2, TNode node3);

    protected internal abstract ILocation ConstructVertex(ILocation location);

    private TNode? CreateTownNode()
    {
        var gMap = GameData.Maps[Map];

        if (gMap == null)
            throw new InvalidOperationException($"Missing map metadata for {Map}");

        if (gMap.Boundless)
            return null;

        var spawn = gMap.Spawns.Count > 0 ? gMap.Spawns[0] : default;

        if (spawn == null)
            return null;

        var spawnLocation = ConstructVertex(new Location(Map, spawn.X, spawn.Y));
        var node = ConstructNode(spawnLocation);

        var containingTriangle = GetContainingTriangle(spawnLocation);

        if (containingTriangle == null)
            throw new InvalidOperationException("Spawn located out of bounds");

        foreach (var nodeVertex in containingTriangle)
            node.Edges.Add(ConstructEdge(node, nodeVertex));

        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected internal virtual TNode FindBestNode(ILocation vertex)
    {
        //inside a triangle every vertex of it is reachable in a straight line, so the nearest one needs no checking
        if (GetContainingTriangle(vertex) is { } containing)
            return containing.Where(n => n.Edges.Count >= 2)
                             .MinBy(n => n.Vertex.FastDistance(vertex))!;

        //no triangle holds it. Containment is a strict barycentric test, so this is not only a point off the mesh -
        //a point sitting on a triangle's own edge fails it too, which is where a character parked against a wall
        //stands. Nearest by raw distance is what this used to answer, and the leg to it is the one leg the search
        //never validates, so across a line is exactly what it picked
        var nearest = this.Where(n => n.Edges.Count >= 2)
                          .OrderBy(n => n.Vertex.FastDistance(vertex))
                          .Take(Definitions.CONSTANTS.REACHABLE_NODE_CANDIDATES)
                          .ToArray();

        foreach (var candidate in nearest)
            if (CanMove(vertex, candidate.Vertex))
                return candidate;

        //nothing near is reachable, which a point genuinely inside a wall answers this way for every node. The
        //nearest one keeps the search able to run; the caller is what has to decide whether the leg may be walked
        return nearest.FirstOrDefault()
               ?? this.Where(n => n.Edges.Count >= 2)
                      .MinBy(n => n.Vertex.FastDistance(vertex))!;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected internal virtual IGenericTriangle<TNode>? GetContainingTriangle(ILocation vertex)
        => Triangles.FirstOrDefault(t => t.Contains<TNode, TEdge>(vertex));

    /// <summary>
    ///     The point map cell a point falls in. Flooring rather than rounding, because that is what
    ///     <c>RayTraceTo</c> does and therefore what <see cref="CanMove" /> measures a walk against - a cell holds the
    ///     unit square that starts at it. Rounding here instead put the two a cell apart for any coordinate past the
    ///     half, so a point this called walkable was one <see cref="CanMove" /> refused to set out from, and every
    ///     candidate a search offered it was rejected on the first traced cell.
    /// </summary>
    /// <param name="point">
    ///     The point to locate.
    /// </param>
    /// <returns>
    ///     The point map indices for the point, which may be outside the map's own extents.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected internal (int X, int Y) ToCell(IPoint point)
    {
        var offsetLoc = ApplyOffset(point);

        return ((int)MathF.Floor(offsetLoc.X), (int)MathF.Floor(offsetLoc.Y));
    }

    /// <summary>
    ///     Checks if a point is a wall.
    /// </summary>
    /// <param name="point">
    ///     The point to check.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if the location is a wall, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public virtual bool IsWall(IPoint point)
    {
        (var x, var y) = ToCell(point);

        return PointMap[x, y]
            .HasFlag(PointType.Wall);
    }

    /// <summary>
    ///     Checks if a point is somewhere a character could actually stand, which is a stronger question than
    ///     <see cref="IsWall" /> answers. The mesh is built by flooding out from the map's own spawn points, so a
    ///     point the flood never reached is one no walk can end on - open water and the void outside an arena are both
    ///     wall-free and both unreachable. The server decides the same way and defeats a character that lands off it.
    /// </summary>
    /// <param name="point">
    ///     The point to check.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if the flood fill reached the point, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     - including for a point outside the map's own extents.
    /// </returns>
    public virtual bool IsWalkable(IPoint point)
    {
        (var x, var y) = ToCell(point);

        //the point map is sized to the map's extents and indexed directly, so an out of bounds point throws rather
        //than answering - and nothing outside the map was ever standable anyway
        if ((x < 0) || (y < 0) || (x >= PointMap.GetLength(0)) || (y >= PointMap.GetLength(1)))
            return false;

        //every value the fill writes carries the Walkable bit; None and Wall are the two that do not
        return PointMap[x, y]
            .HasFlag(PointType.Walkable);
    }

    /// <summary>
    ///     This method reverses the offset applied in <see cref="ApplyOffset" />.
    /// </summary>
    /// <param name="point">
    ///     The point to reverse the offset of.
    /// </param>
    /// <returns>
    ///     <see cref="IPoint" />
    ///     <br />
    ///     A new point with it's coordinate offset back to it's original coordinates.
    /// </returns>
    public virtual IPoint RemoveOffset(IPoint point) => new Point(point.X - XOffset, point.Y - YOffset);

    /// <summary>
    ///     Finds the closest point the flood fill reached, for a point it did not. The wall raster is padded by the
    ///     character's own collision base, which is the same limit the server clamps a character to when it slides
    ///     along a line - so the two boundaries coincide and an ordinary graze puts a legal position inside the
    ///     padding. This is what turns that back into somewhere a walk can start or end.
    /// </summary>
    /// <param name="point">
    ///     The point to search around.
    /// </param>
    /// <param name="walkable">
    ///     The closest walkable point, when one was found within
    ///     <see cref="Definitions.CONSTANTS.MAX_UNSTICK_DISTANCE" />.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if a walkable point was found, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    /// <remarks>
    ///     Nearest wins with no regard for which side of the padding it lies on, so a point sitting past the middle of
    ///     a band can be answered with the far side of it. The answer is never further than the point is deep, though,
    ///     since it reached the padding from ground that close: measured at 0% for a point one unit in and 1-2% for
    ///     two to five, which is the whole range a clamp against a line or a lattice-snapped arrival produces. Closing
    ///     it needs the side the character came from, which is a caller's to know and not in this signature.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected internal virtual bool TryFindNearestWalkable(IPoint point, out IPoint walkable)
    {
        walkable = Point.None;

        (var centerX, var centerY) = ToCell(point);
        var width = PointMap.GetLength(0);
        var height = PointMap.GetLength(1);

        for (var ring = 0; ring <= Definitions.CONSTANTS.MAX_UNSTICK_DISTANCE; ring++)
        {
            var bestDistance = int.MaxValue;

            for (var dx = -ring; dx <= ring; dx++)
                for (var dy = -ring; dy <= ring; dy++)
                {
                    //only the ring's own edge - everything inside it was covered by a smaller ring
                    if ((Math.Abs(dx) != ring) && (Math.Abs(dy) != ring))
                        continue;

                    var x = centerX + dx;
                    var y = centerY + dy;

                    if ((x < 0) || (y < 0) || (x >= width) || (y >= height))
                        continue;

                    if (!PointMap[x, y]
                            .HasFlag(PointType.Walkable))
                        continue;

                    var distance = (dx * dx) + (dy * dy);

                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    walkable = RemoveOffset(new Point(x, y));
                }

            //the ring is walked whole before answering, since a corner of it is further away than a side
            if (bestDistance < int.MaxValue)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Edges can be added apart from the triangles. This method will traverse all unique edges and return them.
    ///     (processing intensive)
    /// </summary>
    /// <returns>
    ///     <see cref="ICollection{T}" /> of <typeparamref name="TEdge" />
    ///     <br />
    ///     A collection of all edges contained within this mesh. If an edge would lead to another mesh, it is ignored.
    /// </returns>
    public virtual ICollection<TEdge> TraverseEdges()
    {
        var edges = new HashSet<TEdge>();

        if (Triangles.Count == 0)
            return edges;

        var opened = new HashSet<TNode>();

        //add all spawns to opened list
        foreach (var spawn in GameData.Maps[Map]!.Spawns)
        {
            var node = FindBestNode(new Location(Map, spawn));
            opened.Add(node);
        }

        while (opened.Count > 0)
        {
            var node = opened.First();

            foreach (var edge in node.Edges)
            {
                if (!Map.EqualsI(edge.End.Vertex.Map))
                    continue;

                edges.Add(edge);

                if (!edge.End.Closed)
                    opened.Add(edge.End);
            }

            opened.Remove(node);
            node.Closed = true;
        }

        foreach (var edge in edges)
        {
            edge.Start.Closed = false;
            edge.End.Closed = false;
        }

        return edges;
    }

    /// <summary>
    ///     Edges can be added apart from the triangles. These new edges can lead to new nodes not attached to any triangle.
    ///     This method will traverse all unique edges and return all unique nodes. (processing intensive)
    /// </summary>
    /// <returns>
    ///     <see cref="ICollection{T}" /> of <typeparamref name="TNode" />
    ///     <br />
    ///     A collection of all nodes contained within this mesh.
    /// </returns>
    public virtual ICollection<TNode> TraverseNodes()
    {
        var nodes = new HashSet<TNode>();

        if (Triangles.Count == 0)
            return nodes;

        var opened = new HashSet<TNode>();

        //add all spawns to opened list
        foreach (var spawn in GameData.Maps[Map]!.Spawns)
        {
            var node = FindBestNode(new Location(Map, spawn));
            opened.Add(node);
        }

        while (opened.Count > 0)
        {
            var node = opened.First();
            nodes.Add(node);

            foreach (var edge in node.Edges)
            {
                if (!Map.EqualsI(edge.End.Vertex.Map))
                    continue;

                if (!edge.End.Closed)
                    opened.Add(edge.End);
            }

            opened.Remove(node);
            node.Closed = true;
        }

        foreach (var discoveredNode in nodes)
            discoveredNode.Closed = false;

        return nodes;
    }
}