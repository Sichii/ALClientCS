using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Extensions;
using AL.Pathfinding.Interfaces;
using AL.Pathfinding.Objects;
using Common.Logging;

namespace AL.Pathfinding
{
    public class NavMesh : GraphBase<GraphNode<Point>, Point>
    {
        protected sealed override ILog Logger { get; init; }
        internal PointType[,] PointMap { get; }
        internal int XOffset { get; }
        internal int YOffset { get; }
        internal GraphNode<Point> TownNode { get; }

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

        public async IAsyncEnumerable<IConnector<Point>> FindPath(
            Point start,
            IEnumerable<Point> ends,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            float distance = 0,
            bool smoothPath = true,
            bool useTownIfOptimal = true)
        {
            //initialization / offset start and end points
            start = new Point(start.X + XOffset, start.Y + YOffset);
            var endsArr = ends.Select(end => new Point(end.X + XOffset, end.Y + YOffset)).ToArray();

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

            var setup = useTownIfOptimal && TownNode != null
                ? () => Connect(startNode, TownNode, ConnectorType.Town)
                : default(Action);

            var cleanup = useTownIfOptimal && TownNode != null
                ? () => Disconnect(startNode, TownNode)
                : default(Action);

            //get the path (prepend the real start of the path
            var path = await Navigate(startNode.Index, endPointLookup.Keys.Select(node => node.Index), setup, cleanup)
                .Prepend(new EdgeConnector<Point> { Start = start, End = startNode.Edge })
                .ToListAsync();

            //add the real end of the path based on the last node found
            var indexedLast = endPointLookup.FirstOrDefault(kvp => kvp.Key.Edge == path.Last().End).Value;
            path.Add(new EdgeConnector<Point> { Start = path.Last().End, End = indexedLast });

            var finalPath = smoothPath ? SmoothPath(path, distance) : path;

            foreach (var connector in finalPath)
                yield return connector;
        }

        private static void FindDistanceShortcut(List<IConnector<Point>> connectors, float distance = 0)
        {
            var end = connectors[^1].End;
            var circle = new Circle(end, distance * 0.95f);

            for (var i = 0; i < connectors.Count; i++)
            {
                var connector = connectors[i];

                if (connector.Type != ConnectorType.Walk)
                    continue;

                var intersection = circle.LineIntersection(connector.ToLine());

                if (intersection != null)
                {
                    var chopIndex = i - 1;
                    connectors.RemoveRange(chopIndex, connectors.Count - chopIndex);
                    connectors.Add(new EdgeConnector<Point>
                        { Start = connector.Start, End = intersection.Point(), Type = ConnectorType.Walk });

                    break;
                }
            }
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

        public bool CanMove(Point start, Point end)
        {
            foreach (var point in new Line(start, end).Points())
                if (PointMap[(int) point.X, (int) point.Y].HasFlag(PointType.Wall))
                    return false;

            return true;
        }
    }
}