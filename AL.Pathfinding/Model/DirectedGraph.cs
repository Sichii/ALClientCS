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
    public class DirectedGraph : GraphBase2<NavMesh2, GraphNode2, GraphEdge, ILocation, GraphTriangle>
    {
        public DirectedGraph(Dictionary<string, NavMesh2> navMeshes)
            : base(navMeshes)
        {
            /*
            //connect all the meshes
            foreach ((var map, var navMesh) in navMeshes)
            {
                var gMap = GameData.Maps[map]!;

                if (gMap.Irregular)
                {
                    ILocation leaveTo = gMap.Exits.FirstOrDefault(e => e.Type == ExitType.Door)?.ToLocation
                                        ?? new Location("main", GameData.Maps.Main.Spawns[0]);

                    var toNavMesh = navMeshes[leaveTo.Map];
                    var endNode = toNavMesh.ConstructNode(leaveTo);

                    foreach (var node in navMesh)
                    {
                        var leaveEdge = navMesh.ConstructEdge(node, endNode, ConnectorType.Leave);
                        node.Edges.Add(leaveEdge);
                    }
                } else
                    foreach (var exit in gMap.Exits)
                    {
                        NavMesh2 toNavMesh = exit.Map.EqualsI(map) ? navMesh : navMeshes[exit.ToLocation.Map]!;

                        var startNode = navMesh.ConstructNode(exit);
                        var endNode = toNavMesh.ConstructNode(exit.ToLocation);

                        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                        var edge = navMesh.ConstructEdge(startNode, endNode, exit.Type switch
                        {
                            ExitType.Door        => ConnectorType.Door,
                            ExitType.Transporter => ConnectorType.Transport,
                            _                    => throw new ArgumentOutOfRangeException()
                        });

                        startNode.Edges.Add(edge);

                        var startNodeForStartNode = navMesh.FindBestNode(exit);
                        var endNodeForEndNode = toNavMesh.FindBestNode(exit.ToLocation);

                        var toStartEdge = navMesh.ConstructEdge(startNodeForStartNode, startNode, ConnectorType.Walk);
                        startNodeForStartNode.Edges.Add(toStartEdge);
                        var fromEndEdge = toNavMesh.ConstructEdge(endNode, endNodeForEndNode, ConnectorType.Walk);
                        endNode.Edges.Add(fromEndEdge);
                    }
            } */
        }

        private async IAsyncEnumerable<GraphEdge> EnhancePathAsync(IAsyncEnumerable<GraphEdge> connectors)
        {
            var partitionedPath = new List<GraphEdge>();

            await foreach (var edge in connectors)
                if (edge.Type != ConnectorType.Walk) //flush the path if we come across a non walk node
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

            //yield anything left over
            if (partitionedPath.Count > 0)
                foreach (var partitionedEdge in SmoothPath(partitionedPath))
                    yield return partitionedEdge;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private IEnumerable<GraphEdge> FindDistanceShortcuts(IEnumerable<GraphEdge> connectors)
        {
            var connectorsArr = connectors.ToArray();
            var endConnector = connectorsArr.Last();
            var end = endConnector.End;

            //the end will have a radius, create a destination from it
            var destination = new Destination(end.Vertex, end.Radius);

            foreach (var connector in connectorsArr)
            {
                var current = connector.Start.Vertex;

                //if the connector type isnt a walk or isnt on the same map as the current node, there are no shortcuts to find
                if ((connector.Type != ConnectorType.Walk) || !current.OnSameMapAs(destination))
                {
                    yield return connector;

                    continue;
                }

                //if the destination already contains this point, we are already at the end of the path
                if (destination.Contains(current))
                    yield break;

                //create a new destination by offsetting the end of the path towards the current node by the end's radius
                var navMesh = NavMeshes[current.Map];
                var newDestination = new Location(current.Map, destination.OffsetTowards(current, destination.Radius));

                //if we can move to this point, return a new connection and break the path
                if (navMesh.CanMove(current, newDestination))
                {
                    var endNode = navMesh.ConstructNode(newDestination);

                    yield return navMesh.ConstructEdge(connector.Start, endNode, ConnectorType.Walk);

                    yield break;
                }

                yield return connector;
            }
        }

        public async IAsyncEnumerable<GraphEdge> FindPathAsync(ILocation start, IEnumerable<Destination> ends, bool useTownIfOptimal = true)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (ends == null)
                throw new ArgumentNullException(nameof(ends));

            var endsArr = ends.ToArray();

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
                    if (next.Type == ConnectorType.Town)
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