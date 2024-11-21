#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
using Chaos.Extensions.Common;
using Priority_Queue;
using CONSTANTS = AL.Core.Definitions.CONSTANTS;
#endregion

namespace AL.Pathfinding.Abstractions;

/// <summary>
///     Represents a Directed Graph.
/// </summary>
/// <typeparam name="TMesh">
///     An implementation of <see cref="MeshBase{TNode,TEdge}" />.
/// </typeparam>
/// <typeparam name="TNode">
///     An implementation of <see cref="IGraphNode{TEdge}" /> and <see cref="FastPriorityQueueNode" />.
/// </typeparam>
/// <typeparam name="TEdge">
///     An implementation of <see cref="IGraphEdge{TNode}" />.
/// </typeparam>
public abstract class GraphBase<TMesh, TNode, TEdge> where TMesh: MeshBase<TNode, TEdge>
                                                     where TNode: FastPriorityQueueNode, IGraphNode<TEdge>
                                                     where TEdge: IGraphEdge<TNode>, new()
{
    protected internal Dictionary<string, TMesh> NavMeshes { get; }
    protected FastPriorityQueue<TNode> Opened { get; }
    private List<TNode> OpenedNodes { get; }
    protected SemaphoreSlim Sync { get; }

    protected GraphBase(Dictionary<string, TMesh> navMeshes)
    {
        NavMeshes = navMeshes;

        //guestimating max pq length based on average number of nodes in the 5 most populous meshes
        var meshes = NavMeshes.Values
                              .DistinctBy(n => n.Map)
                              .ToArray();

        var averageOfTop5 = meshes.Select(m => m.Count())
                                  .OrderByDescending(c => c)
                                  .Take(5)
                                  .Sum()
                            / 5;

        Opened = new FastPriorityQueue<TNode>(averageOfTop5);
        OpenedNodes = new List<TNode>(averageOfTop5);
        Sync = new SemaphoreSlim(1, 1);
        BuildConnections();
    }

    protected void BuildConnections()
    {
        //connect all the meshes
        foreach ((var map, var navMesh) in NavMeshes)
        {
            var gMap = GameData.Maps[map]!;

            if (gMap.Irregular)
            {
                var leaveTo = gMap.Exits.FirstOrDefault(e => e.Type == ExitType.Door)
                                  ?.ToLocation
                              ?? new Location("main", GameData.Maps.Main.Spawns[0]);

                //construct a leave connector
                var toNavMesh = NavMeshes[leaveTo.Map];
                var leavetoVertex = toNavMesh.ConstructVertex(leaveTo);
                var endNode = toNavMesh.ConstructNode(leavetoVertex);

                //add the leave connector to every node on the map
                foreach (var node in navMesh)
                {
                    var leaveEdge = navMesh.ConstructEdge(node, endNode, EdgeType.Leave);
                    node.Edges.Add(leaveEdge);
                }
            } else
                foreach (var exit in gMap.Exits)
                {
                    if (exit.Type == ExitType.Door)
                    {
                        var door = gMap.Doors.FirstOrDefault(d => IPoint.Comparer.Equals(d, exit));

                        //dont add locked doors
                        //TODO: change in the future, when we add support for entering instances with keys
                        if (door is { LockType: LockType.Locked })
                            continue;
                    }

                    //the navmesh for the map the exit leads to
                    var toNavMesh = navMesh;

                    if (!exit.Map.EqualsI(exit.ToLocation.Map))
                        if (!NavMeshes.TryGetValue(exit.ToLocation.Map, out toNavMesh))
                            toNavMesh = null;

                    if (toNavMesh == null)
                        continue;

                    //construct an edge from the exit to it's destination
                    var exitVertex = navMesh.ConstructVertex(exit);
                    var exitToVertex = toNavMesh.ConstructVertex(exit.ToLocation);
                    var startNode = navMesh.ConstructNode(exitVertex);
                    var endNode = toNavMesh.ConstructNode(exitToVertex);

                    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                    var edge = navMesh.ConstructEdge(
                        startNode,
                        endNode,
                        exit.Type switch
                        {
                            ExitType.Door        => EdgeType.Door,
                            ExitType.Transporter => EdgeType.Transport,
                            _                    => throw new ArgumentOutOfRangeException()
                        });

                    //add the edge to the start node
                    startNode.Edges.Add(edge);

                    //find the closest node to the start node in the current map
                    var startNodeForStartNode = navMesh.FindBestNode(exitVertex);

                    //find the closest node to the end node in the current map
                    var endNodeForEndNode = toNavMesh.FindBestNode(exitToVertex);

                    var toStartEdge = navMesh.ConstructEdge(startNodeForStartNode, startNode, EdgeType.Walk);
                    startNodeForStartNode.Edges.Add(toStartEdge);
                    var fromEndEdge = toNavMesh.ConstructEdge(endNode, endNodeForEndNode, EdgeType.Walk);
                    endNode.Edges.Add(fromEndEdge);
                }
        }
    }

    /// <summary>
    ///     Determines whether or not it's possible to move from one location to another.
    /// </summary>
    /// <param name="map">
    ///     The map to check against.
    /// </param>
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
    public virtual bool CanMove(string map, IPoint start, IPoint end)
    {
        var mesh = NavMeshes[map];

        return mesh.CanMove(start, end);
    }

    /// <summary>
    ///     Determines whether or not it's possible to move from one location to another.
    /// </summary>
    /// <param name="start">
    ///     The starting location.
    /// </param>
    /// <param name="end">
    ///     The ending location.
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
    public virtual bool CanMove(ILocation start, ILocation end) => start.OnSameMapAs(end) && CanMove(start.Map, start, end);

    /// <summary>
    ///     Performs a dijkstra search, returning when reaching any end location.
    /// </summary>
    /// <param name="start">
    ///     The starting location
    /// </param>
    /// <param name="ends">
    ///     Any number of ending locations
    /// </param>
    /// <param name="useTownIfOptimal">
    ///     Whether or not to consider using the town skill
    /// </param>
    /// <returns>
    ///     <see cref="IAsyncEnumerable{T}" /> of <see cref="TEdge" />
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     start
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     ends
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     No mesh found for map {start.Map}
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     No mesh found for map {end.Map}
    /// </exception>
    /// <remarks>
    ///     If towning is interrupted, this will automatically retry without towning enabled.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public virtual async IAsyncEnumerable<TEdge> FindPathAsync(ILocation start, IEnumerable<ILocation> ends, bool useTownIfOptimal = true)
    {
        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(ends);

        var endsArr = ends.ToArray();

        if (endsArr.Length == 0)
            yield break;

        if (!NavMeshes.TryGetValue(start.Map, out var startNavMesh))
            throw new InvalidOperationException($"No mesh found for map {start.Map}");

        var startNode = startNavMesh.ConstructNode(start);
        var bestStartNode = startNavMesh.FindBestNode(start);
        var startEdge = startNavMesh.ConstructEdge(startNode, bestStartNode, EdgeType.Walk);
        bestStartNode.Parent = startEdge;

        var endNodeLookup = endsArr.ToDictionary(
            end =>
            {
                if (!NavMeshes.TryGetValue(end.Map, out var endNavMesh))
                    throw new InvalidOperationException($"No mesh found for map {end.Map}");

                return endNavMesh.FindBestNode(end);
            });

        var path = new Stack<TEdge>();
        var current = bestStartNode;

        await Sync.WaitAsync();
        var townConnectors = new List<TEdge>();

        try
        {
            if (useTownIfOptimal && (startNavMesh.TownNode != null))
            {
                var townConnector = startNavMesh.ConstructEdge(bestStartNode, startNavMesh.TownNode);
                bestStartNode.Edges.Add(townConnector);
                townConnectors.Add(townConnector);
            }

            OpenNode(current, 0);

            while (Opened.Count > 0)
            {
                current = Opened.Dequeue();

                if ((current == null) || endNodeLookup.Keys.Contains(current)) //ienumerable contains is faster here
                    break;

                foreach (var edge in current.Edges)
                {
                    var neighbor = edge.End;

                    if (neighbor.Closed)
                        continue;

                    if (OpenNode(neighbor, current.Priority + edge.Heuristic))
                    {
                        neighbor.Parent = edge;

                        //this should failfast on type 99% of the time
                        if (edge.Type is EdgeType.Transport or EdgeType.Leave or EdgeType.Door
                            && useTownIfOptimal
                            && !edge.Start.Vertex.OnSameMapAs(edge.End.Vertex)
                            && NavMeshes.TryGetValue(edge.End.Vertex.Map, out var navMesh)
                            && (navMesh.TownNode != null))
                        {
                            var townConnector = navMesh.ConstructEdge(edge.End, navMesh.TownNode, EdgeType.Town);
                            edge.End.Edges.Add(townConnector);
                            townConnectors.Add(townConnector);
                        }
                    }
                }

                current.Closed = true;
            }

            //get the true end node from the lookup, create a node and edge from it and add it to the path
            var endNav = NavMeshes[current!.Vertex.Map];
            var endPoint = endNodeLookup[current];
            var endNode = endNav.ConstructNode(endPoint);
            var endEdge = endNav.ConstructEdge(current, endNode, EdgeType.Walk);
            path.Push(endEdge);

            while (current is { Parent: not null })
            {
                var edge = current.Parent;
                var parentNode = edge.Start;

                path.Push(edge);
                current = parentNode;
            }
        } finally
        {
            foreach (var townConnector in townConnectors)
                townConnector.Start.Edges.Remove(townConnector);

            Reset();
            Sync.Release();
        }

        while (path.Count > 0)
            yield return path.Pop();
    }

    /// <summary>
    ///     Checks if a location is a wall.
    /// </summary>
    /// <param name="location">
    ///     The location to check.
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
    public virtual bool IsWall(ILocation location)
    {
        var mesh = NavMeshes[location.Map];

        return mesh.IsWall(location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected virtual bool OpenNode(TNode node, float priority)
    {
        if (Opened.Contains(node))
        {
            if (node.Priority.IsGreater(priority, CONSTANTS.EPSILON))
            {
                Opened.UpdatePriority(node, priority);

                return true;
            }
        } else
        {
            Opened.Enqueue(node, priority);
            OpenedNodes.Add(node);

            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected virtual void Reset()
    {
        foreach (var node in Opened)
            Opened.ResetNode(node);

        Opened.Clear();

        foreach (var node in OpenedNodes)
            node.Reset();

        OpenedNodes.Clear();
    }
}