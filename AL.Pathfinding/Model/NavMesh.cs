using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Extensions;
using AL.Pathfinding.Interfaces;
using Common.Logging;

namespace AL.Pathfinding.Model
{
    /// <summary>
    ///     <inheritdoc /> <br />
    ///     Represents a delauney triangulated mesh generated from a set of nodes. <br />
    ///     A <see cref="NavMesh" /> is generated for a specific map for pathfinding purposes.
    /// </summary>
    /// <seealso cref="GraphBase{TNode,TEdge}" />
    public class NavMesh : GraphBase<GraphNode<Point>, Point>
    {
        protected sealed override ILog Logger { get; init; }
        internal PointType[,] PointMap { get; }
        internal GraphNode<Point>? TownNode { get; }
        internal int XOffset { get; }
        internal int YOffset { get; }

        internal NavMesh(NavMeshBuilderContext context)
            : base(context.Nodes, (current, neighbor) => current.Edge.Distance(neighbor.Edge), (_, _) => ConnectorType.Walk)
        {
            Logger = LogManager.GetLogger<NavMesh>();
            XOffset = context.XOffset;
            YOffset = context.YOffset;
            PointMap = context.PointMap;
            TownNode = context.TownNode;
            Reset();
        }

        internal Point ApplyOffset(IPoint point) => new(point.X + XOffset, point.Y + YOffset);

        internal bool CanMove(IPoint start, IPoint end)
        {
            foreach ((var x, var y) in new Line(start, end).Points())
                if (PointMap[Convert.ToInt32(x), Convert.ToInt32(y)].HasFlag(PointType.Wall))
                    return false;

            return true;
        }

        private static void FindDistanceShortcut(List<IConnector<Point>> connectors, float distance = 0)
        {
            var end = connectors.Last().End;
            var circle = new Circle(end, distance * 0.98f);

            for (var i = 0; i < connectors.Count; i++)
            {
                var connector = connectors[i];

                if (connector.Type != ConnectorType.Walk)
                    continue;

                var intersection = circle.CalculateIntersectionEntryPoint(connector.ToLine());

                if (intersection != null)
                {
                    connectors.RemoveRange(i, connectors.Count - i);

                    connectors.Add((EdgeConnector<Point>)connector with
                    {
                        End = intersection.Value, Heuristic = connector.Start.Distance(intersection.Value)
                    });

                    break;
                }
            }
        }

        /// <summary>
        ///     Finds the shortest path between a start point and any number of end points.
        /// </summary>
        /// <param name="start">A starting point.</param>
        /// <param name="ends">Any number of end points. Upon reaching any of the end points, that path will be returned.</param>
        /// <param name="smoothPath">
        ///     Whether or not to smooth the path before returning it. <br />
        ///     Path smoothing is lazy, so execution cost of raytracing is spread out.
        /// </param>
        /// <param name="useTownIfOptimal">Whether or not to consider using the <c>Town</c> skill.</param>
        /// <returns>
        ///     <see cref="IAsyncEnumerable{T}" /> of <see cref="IConnector{TEdge}" /> of <see cref="Point" /> <br />
        ///     A lazy enumeration of points along the most optimal path between the start node and the first end node a path is
        ///     found for.
        /// </returns>
        /// <exception cref="ArgumentNullException">start</exception>
        /// <exception cref="ArgumentNullException">ends</exception>
        /// <exception cref="InvalidOperationException">
        ///     Unable to locate a start node for the given point. {
        ///     <paramref name="start" />}
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Unable to locate any end nodes for the given points. {string.Join(',',
        ///     <paramref name="ends" />)}
        /// </exception>
        public async IAsyncEnumerable<IConnector<Point>> FindPath(
            IPoint start,
            IEnumerable<ICircle> ends,
            bool smoothPath = true,
            bool useTownIfOptimal = true)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (ends == null)
                throw new ArgumentNullException(nameof(ends));

            var endsArr = ends.ToArray();
            
            //initialization / offset start and end points
            var offsetStart = ApplyOffset(start);

            var offsetEnds = endsArr.Select(end =>
                {
                    if (end is Exit exit)
                        return exit with { X = exit.X + XOffset, Y = exit.Y + YOffset };

                    return (ICircle)new Circle(ApplyOffset(end), end.Radius);
                })
                .ToArray();

            //if we're standing on an end point... yield nothing
            if (offsetEnds.Any(end => end.Equals(offsetStart)))
                yield break;

            //get closest to start
            var startNode = Nodes.OrderBy(node => offsetStart.FastDistance(node.Edge)).FirstOrDefault(node => CanMove(offsetStart, node.Edge));

            if (startNode == null)
                throw new InvalidOperationException($"Unable to locate a start node for the given point. {start}");

            var endPointLookup = new Dictionary<GraphNode<Point>, ICircle>();

            //for each possible end
            //add a lookup as vertex : end
            //to figure out which end was discovered
            foreach (var end in offsetEnds)
            {
                var orderedNodes = Nodes.OrderBy(node => end.FastDistance(node.Edge));

                var endNode = end is Exit { Type: ExitType.Transporter or ExitType.Door }
                    ? orderedNodes.FirstOrDefault()
                    : orderedNodes.FirstOrDefault(node => CanMove(end, node.Edge));

                if (endNode == null)
                    continue;

                endPointLookup[endNode] = end;
            }

            if (endPointLookup.Count == 0)
                throw new InvalidOperationException(
                    $"Unable to locate any end nodes for the given points. {string.Join(',', endsArr.AsEnumerable())}");

            var setup = useTownIfOptimal && (TownNode != null) && !startNode.Equals(TownNode)
                ? () => Connect(startNode, TownNode, ConnectorType.Town)
                : default(Action);

            var cleanup = useTownIfOptimal && (TownNode != null) && !startNode.Equals(TownNode)
                ? () => Disconnect(startNode, TownNode)
                : default(Action);

            //get the path (prepend the real start of the path
            var path = await Navigate(startNode.Index, endPointLookup.Keys.Select(node => node.Index), setup, cleanup)
                .Prepend(new EdgeConnector<Point> { Start = offsetStart, End = startNode.Edge })
                .ToListAsync()
                .ConfigureAwait(false);

            //add the real end of the path based on the last node found
            var last = path.Last().End;
            var indexedLast = endPointLookup.FirstOrDefault(kvp => kvp.Key.Edge == last).Value;
            path.Add(new EdgeConnector<Point> { Start = last, End = indexedLast.ToPoint() });

            var finalPath = smoothPath ? SmoothPath(path, indexedLast.Radius) : path;

            foreach (var connector in finalPath)
                yield return connector;
        }

        internal bool IsWall(IPoint point) => PointMap[Convert.ToInt32(point.X), Convert.ToInt32(point.Y)].HasFlag(PointType.Wall);

        internal Point RemoveOffset(IPoint point) => new(point.X - XOffset, point.Y - YOffset);

        private IEnumerable<IConnector<Point>> SmoothPath(List<IConnector<Point>> connectors, float distance = 0)
        {
            if (connectors.Count == 0)
                yield break;

            if (distance > 0)
                FindDistanceShortcut(connectors, distance);

            for (var i = 0; i < connectors.Count; i++)
            {
                var current = connectors[i];

                if (i != connectors.Count)
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
                        if (CanMove(current.Start, next.End))
                            i = e;
                    }

                var bestNext = connectors[i];

                if (current == bestNext)
                    yield return current;
                else
                {
                    var combined = (EdgeConnector<Point>)bestNext with
                    {
                        Start = current.Start, Heuristic = current.Start.Distance(bestNext.End)
                    };

                    yield return combined;
                }
            }
        }
    }
}