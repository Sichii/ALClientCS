#region
using System.Runtime.CompilerServices;
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

            //anywhere the server refuses the command, a leave connector is an edge that can only ever fail
            if (Definitions.CONSTANTS.AcceptsLeave(map))
            {
                //leave hands the character to the start map's first spawn wherever it is used from, whatever doors
                //the map also has - a door leads somewhere else entirely and is reached by walking to it
                var leaveTo = new Location("main", GameData.Maps.Main.Spawns[0]);
                var leaveToNavMesh = NavMeshes[leaveTo.Map];
                var leaveToVertex = leaveToNavMesh.ConstructVertex(leaveTo);
                var arrivalNode = leaveToNavMesh.ConstructNode(leaveToVertex);

                //the arrival is a node of its own and starts with no edges, so without this the only way out of it
                //is the town connector FindPathAsync grafts on after a map change - and every trip off the map
                //opened with a recall that moved the character nowhere
                var fromArrivalEdge = leaveToNavMesh.ConstructEdge(
                    arrivalNode,
                    leaveToNavMesh.FindBestNode(leaveToVertex),
                    EdgeType.Walk);

                arrivalNode.Edges.Add(fromArrivalEdge);

                //leave costs the same wherever it is used, so every node on the map carries the connector
                foreach (var node in navMesh)
                    node.Edges.Add(navMesh.ConstructEdge(node, arrivalNode, EdgeType.Leave));
            }

            //a map's doors are its own whether it is irregular or not. Irregular describes the shape of the walkable
            //area and says nothing about exits, and skipping them over it left duelland and resort with no way out
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
    ///     <see cref="IAsyncEnumerable{T}" /> of <typeparamref name="TEdge" />
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
    /// <param name="walkSpeed">
    ///     The character's own speed, which is what a town channel is priced against - the channel is a duration and
    ///     every other edge is a distance. Null prices it at the nominal speed.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public virtual async IAsyncEnumerable<TEdge> FindPathAsync(
        ILocation start,
        IEnumerable<ILocation> ends,
        bool useTownIfOptimal = true,
        float? walkSpeed = null)
    {
        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(ends);

        var endsArr = ends.ToArray();

        if (endsArr.Length == 0)
            yield break;

        if (!NavMeshes.TryGetValue(start.Map, out var startNavMesh))
            throw new InvalidOperationException($"No mesh found for map {start.Map}");

        //the first leg is stitched straight from wherever the character is to a mesh node, and nothing validates it.
        //From a point the flood fill never reached that node is chosen out of the whole mesh by raw distance, so it
        //is routinely on the far side of the line - and the server walks it, hashing only the two endpoints
        TEdge? unstickEdge = default;

        if (!startNavMesh.IsWalkable(start) && startNavMesh.TryFindNearestWalkable(start, out var steppedOut))
        {
            var stuckLoc = new Location(start.Map, start);
            var freeLoc = new Location(start.Map, steppedOut);

            unstickEdge = startNavMesh.ConstructEdge(
                startNavMesh.ConstructNode(stuckLoc),
                startNavMesh.ConstructNode(freeLoc),
                EdgeType.Walk);

            start = freeLoc;
        }

        var startNode = startNavMesh.ConstructNode(start);
        var bestStartNode = startNavMesh.FindBestNode(start);
        var startEdge = startNavMesh.ConstructEdge(startNode, bestStartNode, EdgeType.Walk);
        bestStartNode.Parent = startEdge;

        //several ends routinely collapse onto one mesh node, so this cannot be a key selector handed to ToDictionary:
        //the second of them throws, and callers read that throw as "no path". On a tie the end nearest the shared
        //node wins, which is the shortest closing leg of the ends that ranked equal
        var endNodeLookup = new Dictionary<TNode, ILocation>();

        foreach (var end in endsArr)
        {
            if (!NavMeshes.TryGetValue(end.Map, out var endNavMesh))
                throw new InvalidOperationException($"No mesh found for map {end.Map}");

            var endNode = endNavMesh.FindBestNode(end);

            if (!endNodeLookup.TryGetValue(endNode, out var claimed)
                || (end.Distance(endNode.Vertex) < claimed.Distance(endNode.Vertex)))
                endNodeLookup[endNode] = end;
        }

        var path = new Stack<TEdge>();
        var current = bestStartNode;

        await Sync.WaitAsync();
        var townConnectors = new List<TEdge>();

        //once per search rather than per edge: every town connector in one search is the same character's channel.
        //Qualified, because this file aliases CONSTANTS to AL.Core's for EPSILON
        var townCost = walkSpeed is { } speed
            ? Definitions.CONSTANTS.TownCost(speed)
            : Definitions.CONSTANTS.NOMINAL_TOWN_COST;

        try
        {
            if (useTownIfOptimal && (startNavMesh.TownNode != null))
            {
                var townConnector = startNavMesh.ConstructEdge(bestStartNode, startNavMesh.TownNode, townCost: townCost);
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
                            var townConnector = navMesh.ConstructEdge(edge.End, navMesh.TownNode, EdgeType.Town, townCost);
                            edge.End.Edges.Add(townConnector);
                            townConnectors.Add(townConnector);
                        }
                    }
                }

                current.Closed = true;
            }

            //the loop above only leaves early on an end node, so anything else means the queue ran dry: every node
            //reachable from the start was searched and none of the destinations was among them. An instance map is
            //the ordinary way here, and InvalidOperationException is what callers read as a walk that cannot be made
            if ((current == null) || !endNodeLookup.TryGetValue(current, out var endPoint))
                throw new InvalidOperationException(
                    $"No path from {start} to {string.Join(", ", endsArr.Select(end => end.ToString()))}");

            //get the true end node from the lookup, create a node and edge from it and add it to the path
            var endNav = NavMeshes[current.Vertex.Map];

            //the closing leg is the start leg's mirror and unvalidated for the same reason. The first branch takes a
            //destination it cannot walk to because the radius is only spent downstream, where DirectedGraph's
            //shortcut pass reads it off this node - substituting a standable point hands that pass nothing to prune
            if (endNav.CanMove(current.Vertex, endPoint) || HasReachableEdge(endNav, current.Vertex, endPoint))
                path.Push(endNav.ConstructEdge(current, endNav.ConstructNode(endPoint), EdgeType.Walk));
            else if (endNav.TryFindNearestWalkable(endPoint, out var nearestStandable)
                     && endNav.CanMove(current.Vertex, nearestStandable))
            {
                //the radius does not survive this, so the walk runs to the point rather than stopping at the near
                //edge of the destination. What is left for it is a destination carrying no radius at all, and one
                //whose near edge is walled off from the last mesh node as thoroughly as its centre is
                var standableLoc = new Location(endPoint.Map, nearestStandable);

                path.Push(endNav.ConstructEdge(current, endNav.ConstructNode(standableLoc), EdgeType.Walk));
            }

            //and where neither is reachable the path simply stops at the last mesh node, which is as close as the
            //character can legitimately get. Callers already treat a walk that does not arrive as ordinary

            while (current is { Parent: not null })
            {
                var edge = current.Parent;
                var parentNode = edge.Start;

                path.Push(edge);
                current = parentNode;
            }

            //last onto the stack is first off it, so the step out of the wall leads the path it was made for
            if (unstickEdge is not null)
                path.Push(unstickEdge);
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
    ///     Whether a walk may legitimately stop on the near edge of a destination's radius, coming from
    ///     <paramref name="from" />. False for a destination carrying no radius.
    /// </summary>
    /// <remarks>
    ///     What a caller means by a radius is "stop once you are this close", and that stays answerable when the
    ///     centre itself is not: an NPC recorded off the flood fill still has ordinary ground all around it. So a
    ///     centre nothing can walk to is not on its own a reason to discard the radius and aim at the placement.
    ///     <br />
    ///     True where <paramref name="from" /> is already inside, since the shortcut pass ends the path there rather
    ///     than walking any closing leg at all.
    /// </remarks>
    private static bool HasReachableEdge(TMesh mesh, ILocation from, ILocation endPoint)
    {
        if (endPoint is not ICircle { Radius: > 0 } circle)
            return false;

        var nearEdge = new Location(endPoint.Map, circle.OffsetTowards(from, circle.Radius));

        return mesh.IsWalkable(nearEdge) && mesh.CanMove(from, nearEdge);
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