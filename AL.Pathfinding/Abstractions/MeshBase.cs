#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        return !startOffset.RayTraceTo(endOffset)
                           .Any(
                               p => PointMap[Convert.ToInt32(p.X), Convert.ToInt32(p.Y)]
                                   .HasFlag(PointType.Wall));
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

        var transport = gMap1.NPCs.FirstOrDefault(npc => (npc.Data!.Role == NPCRole.Transport) && npc.Locations.Contains(start));

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
        var possibleNodes = GetContainingTriangle(vertex) ?? this.AsEnumerable();

        return possibleNodes.Where(n => n.Edges.Count >= 2)
                            .MinBy(n => n.Vertex.FastDistance(vertex))!;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    protected internal virtual IGenericTriangle<TNode>? GetContainingTriangle(ILocation vertex)
        => Triangles.FirstOrDefault(t => t.Contains<TNode, TEdge>(vertex));

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
        var offsetLoc = ApplyOffset(point);

        return PointMap[Convert.ToInt32(offsetLoc.X), Convert.ToInt32(offsetLoc.Y)]
            .HasFlag(PointType.Wall);
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
    ///     Edges can be added apart from the triangles. This method will traverse all unique edges and return them.
    ///     (processing intensive)
    /// </summary>
    /// <returns>
    ///     <see cref="ICollection{T}" /> of <see cref="TEdge" />
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
    ///     <see cref="ICollection{T}" /> of <see cref="TEdge" />
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