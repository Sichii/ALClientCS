using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
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
            : base(context.Nodes, (current, neighbor) => current.Edge.Distance(neighbor.Edge),
                (_, _) => ConnectorType.Walk)
        {
            Logger = LogManager.GetLogger<NavMesh>();
            XOffset = context.XOffset;
            YOffset = context.YOffset;
            PointMap = context.PointMap;
            TownNode = context.TownNode;
            Reset();
        }

        internal bool CanMove(IPoint start, IPoint end)
        {
            foreach ((var x, var y) in new Line(start, end).Points())
                if (PointMap[(int) x, (int) y].HasFlag(PointType.Wall))
                    return false;

            return true;
        }

        private static void FindDistanceShortcut(List<IConnector<Point>> connectors, float distance = 0)
        {
            var end = connectors[^1].End;
            var circle = new Circle(end, distance * 0.98f);

            for (var i = 0; i < connectors.Count; i++)
            {
                var connector = connectors[i];

                if (connector.Type != ConnectorType.Walk)
                    continue;

                var intersection = circle.Intersects(connector.ToLine());

                if (intersection != null)
                {
                    var chopIndex = i - 1;
                    connectors.RemoveRange(chopIndex, connectors.Count - chopIndex);
                    connectors.Add(new EdgeConnector<Point>
                        { Start = connector.Start, End = intersection.GetPoint(), Type = ConnectorType.Walk });

                    break;
                }
            }
        }

        /// <summary>
        ///     Finds the shortest path between a start point and any number of end points.
        /// </summary>
        /// <param name="start">A starting point.</param>
        /// <param name="ends">Any number of end points. Upon reaching any of the end point, that path will be returned.</param>
        /// <param name="distance">
        ///     An acceptable distance the first end node found to walk to. <br />
        ///     Distance checking involves circle intersections which can be costly.
        ///     For this reason distance checking is only done after the most optimal path is found.
        /// </param>
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
            IEnumerable<IPoint> ends,
            float distance = 0,
            bool smoothPath = true,
            bool useTownIfOptimal = true)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (ends == null)
                throw new ArgumentNullException(nameof(ends));

            //initialization / offset start and end points
            start = new Point(start.X + XOffset, start.Y + YOffset);
            var endsArr = ends.Select(end => new Point(end.X + XOffset, end.Y + YOffset)).ToArray();

            //if we're standing on an end point... yield nothing
            if (endsArr.Any(end => (Point) start == end))
                yield break;

            //get closest to start
            var startNode = Nodes.OrderBy(node => start.FastDistance(node.Edge))
                .FirstOrDefault(node => CanMove(start, node.Edge));

            if (startNode == null)
                throw new InvalidOperationException($"Unable to locate a start node for the given point. {start}");

            var endPointLookup = new Dictionary<GraphNode<Point>, Point>();

            //for each possible end
            //add a lookup as vertex : end
            //so we can figure out which end was discovered
            foreach (var end in endsArr)
            {
                var endNode = Nodes.OrderBy(node => end.FastDistance(node.Edge))
                    .FirstOrDefault(node => CanMove(end, node.Edge));

                if (endNode == null)
                    continue;

                endPointLookup[endNode] = end;
            }

            if (endPointLookup.Count == 0)
                throw new InvalidOperationException(
                    $"Unable to locate any end nodes for the given points. {string.Join(',', endsArr)}");

            var setup = useTownIfOptimal && (TownNode != null)
                ? () => Connect(startNode, TownNode, ConnectorType.Town)
                : default(Action);

            var cleanup = useTownIfOptimal && (TownNode != null)
                ? () => Disconnect(startNode, TownNode)
                : default(Action);

            //get the path (prepend the real start of the path
            var path = await Navigate(startNode.Index, endPointLookup.Keys.Select(node => node.Index), setup, cleanup)
                .Prepend(new EdgeConnector<Point> { Start = (Point) start, End = startNode.Edge })
                .ToListAsync();

            //add the real end of the path based on the last node found
            var indexedLast = endPointLookup.FirstOrDefault(kvp => kvp.Key.Edge == path.Last().End).Value;
            path.Add(new EdgeConnector<Point> { Start = path.Last().End, End = indexedLast });

            var finalPath = smoothPath ? SmoothPath(path, distance) : path;

            foreach (var connector in finalPath)
                yield return connector;
        }

        private IEnumerable<IConnector<Point>> SmoothPath(List<IConnector<Point>> connectors, float distance = 0)
        {
            if (connectors.Count == 0)
                yield break;

            var bestIndex = 1;
            var current = connectors[0];

            if (distance > 0)
                FindDistanceShortcut(connectors, distance);

            //while the next "best" node isnt the end node
            while (bestIndex < connectors.Count)
            {
                //iterate all connectors after the current node
                for (var i = bestIndex; i < connectors.Count; i++)
                {
                    var next = connectors[i];

                    if (next.Type == ConnectorType.Town)
                    {
                        bestIndex = i;
                        break;
                    }

                    //if we can move to this node from the current node
                    //we can also town from anywhere
                    if (CanMove(current.End, next.End))
                        bestIndex = i;
                }

                //yield the best node
                current = connectors[bestIndex];
                yield return current;

                bestIndex++;
            }
        }
    }
}