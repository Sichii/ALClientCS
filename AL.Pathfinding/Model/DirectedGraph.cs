using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;

namespace AL.Pathfinding.Model
{
    /// <summary>
    ///     <inheritdoc cref="GraphBase{TMesh,TNode,TEdge}" />
    /// </summary>
    public class DirectedGraph : GraphBase<NavMesh, GraphNode, GraphEdge>
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
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (ends == null)
                throw new ArgumentNullException(nameof(ends));

            var endsArr = ends.Cast<ILocation>().ToArray();

            if (!endsArr.Any())
                yield break;

            var path = base.FindPathAsync(start, endsArr, useTownIfOptimal);

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

                    yield return navMesh.ConstructEdge(current.Start, bestNext.End);
                }
            }
        }
    }
}