#region
using System.Runtime.CompilerServices;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     <inheritdoc cref="GraphBase{TMesh,TNode,TEdge}" />
/// </summary>
public sealed class DirectedGraph : GraphBase<NavMesh, GraphNode, GraphEdge>
{
    public DirectedGraph(Dictionary<string, NavMesh> navMeshes)
        : base(navMeshes) { }

    private async IAsyncEnumerable<GraphEdge> EnhancePathAsync(IAsyncEnumerable<GraphEdge> edges)
    {
        var partitionedPath = new List<GraphEdge>();

        //for each edge in the path
        await foreach (var edge in edges)
            if (edge.Type != EdgeType.Walk) //flush the path (with smoothing) if we come across a non walk node
            {
                partitionedPath.Add(edge);

                foreach (var partitionedEdge in SmoothPath(partitionedPath))
                    yield return partitionedEdge;

                partitionedPath.Clear();
            } else if (edge.End.Radius > 0) //find distance shortcuts up to this point if we come across a node with a radius
            {
                partitionedPath.Add(edge);

                var smoothPath = SmoothPath(partitionedPath);

                foreach (var partitionedEdge in FindDistanceShortcuts(smoothPath))
                    yield return partitionedEdge;

                partitionedPath.Clear(); //discard everything up to this point
            } else
                partitionedPath.Add(edge);

        //yield anything left over (with smoothing)
        if (partitionedPath.Count > 0)
            foreach (var partitionedEdge in SmoothPath(partitionedPath))
                yield return partitionedEdge;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private IEnumerable<GraphEdge> FindDistanceShortcuts(IEnumerable<GraphEdge> edges)
    {
        var edgesArr = edges.ToArray();
        var endEdge = edgesArr.Last();
        var end = endEdge.End;

        //the end will have a radius, create a destination from it
        var destination = new Destination(end.Vertex, end.Radius);

        foreach (var edge in edgesArr)
        {
            var current = edge.Start.Vertex;

            //if the node isnt on the same map as the current node, there are no shortcuts to find
            if (!current.OnSameMapAs(destination))
            {
                yield return edge;

                continue;
            }

            //if the destination already contains this point, we can stop here
            if (destination.Contains(current))
                yield break;

            //create a new destination by offsetting the end of the path towards the current node by the end's radius
            var navMesh = NavMeshes[current.Map];
            var newDestination = new Location(current.Map, destination.OffsetTowards(current, destination.Radius));

            //if we can move to this point, return a new connection and break the path
            if (navMesh.CanMove(current, newDestination))
            {
                var endNode = navMesh.ConstructNode(newDestination);

                yield return navMesh.ConstructEdge(edge.Start, endNode, EdgeType.Walk);

                yield break;
            }

            //if that didnt work, check if this edge is intersected by the end circle
            var intersectionPoint = destination.CalculateIntersectionEntryPoint(edge);

            if (intersectionPoint != null)
            {
                var endLoc = new Location(navMesh.Map, intersectionPoint);
                var endNode = navMesh.ConstructNode(endLoc);

                yield return navMesh.ConstructEdge(edge.Start, endNode, EdgeType.Walk);

                yield break;
            }

            yield return edge;
        }
    }

    /// <inheritdoc cref="GraphBase{TMesh,TNode,TEdge}.FindPathAsync" />
    public async IAsyncEnumerable<GraphEdge> FindPathAsync<T>(ILocation start, IEnumerable<T> ends, bool useTownIfOptimal = true)
        where T: ILocation, ICircle
    {
        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(ends);

        var endsArr = ends.ToArray();

        if (endsArr.Length == 0)
            yield break;

        //a short hop with a clear line needs no search. The dijkstra below is serialized process-wide, so every
        //character waits on every other one's path, and short hops are most of what gets asked for.
        //
        //Bounded by the town heuristic because towning is the only same-map alternative a clear straight line can
        //lose to, and any path using it costs at least that. Doors and transports are cheaper by heuristic - 50
        //against 500 - but a cross-map exit is filtered out by the same-map test below, and a same-map one lands
        //back at its own door, so neither competes with a walk that is already the shortest one there is.
        //
        //One destination only, which is what keeps that argument sound. Against a set, the nearest same-map
        //candidate is not the cheapest answer - a destination one door away costs the transport heuristic of 50 and
        //beats a clear 490-unit walk - so a shortcut that committed to the near one would quietly return a worse
        //path than the search. It would also widen two smaller holes: CanMove indexes the point map unchecked, so
        //every extra candidate is another chance to throw on an out of bounds point, and skipping the search skips
        //the throw for a destination whose map has no mesh
        if ((endsArr.Length == 1) && NavMeshes.TryGetValue(start.Map, out var navMesh))
            foreach (var end in endsArr.Where(e => e.OnSameMapAs(start) && (start.Distance(e) < CONSTANTS.TOWN_HEURISTIC)))
            {
                //stop at the near edge of the destination rather than its centre, the same shortcut
                //FindDistanceShortcuts applies to a searched path
                var target = end.OffsetTowards(start, end.Radius);

                //the offset clamps to the start when already inside the radius, so there is nothing to walk
                if (IPoint.Comparer.Equals(target, start))
                    yield break;

                if (!navMesh.CanMove(start, target))
                    continue;

                var startNode = navMesh.ConstructNode(start);
                var endNode = navMesh.ConstructNode(new Location(start.Map, target));

                yield return navMesh.ConstructEdge(startNode, endNode, EdgeType.Walk);

                yield break;
            }

        var path = base.FindPathAsync(start, endsArr.Cast<ILocation>(), useTownIfOptimal);

        await foreach (var edge in EnhancePathAsync(path))
            yield return edge;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private IEnumerable<GraphEdge> SmoothPath(IReadOnlyList<GraphEdge> connectors)
    {
        if (connectors.Count == 0)
            yield break;

        for (var i = 0; i < connectors.Count; i++)
        {
            var current = connectors[i];

            for (var e = i + 1; e < connectors.Count; e++)
            {
                var next = connectors[e];

                //can town from anywhere
                if (next.Type == EdgeType.Town)
                {
                    i = e;

                    break;
                }

                //if you can move to this node, it's better
                if (CanMove(current.Start.Vertex, next.End.Vertex))
                    i = e;
            }

            var bestNext = connectors[i];

            if (current == bestNext)
                yield return current;
            else
            {
                var navMesh = NavMeshes[current.Start.Vertex.Map];

                //keep the collapsed edge's own type - re-deriving it relabels a walk that lands on the
                //map's spawn as a town teleport
                yield return navMesh.ConstructEdge(current.Start, bestNext.End, bestNext.Type);
            }
        }
    }
}