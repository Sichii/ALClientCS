using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AL.Core.Extensions;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
using AL.Pathfinding.Objects;
using Common.Logging;
using Priority_Queue;

namespace AL.Pathfinding.Abstractions
{
    public abstract class GraphBase<TNode, TEdge> : IEnumerable<TNode>
        where TNode: FastPriorityQueueNode, IGraphNode<TEdge>
    {
        protected abstract ILog Logger { get; init; }
        protected List<TNode> Nodes { get; }
        protected IConnector<TEdge>[,] Connectors { get; }
        protected FastPriorityQueue<TNode> Opened { get; }
        protected Func<TNode, TNode, float> DistanceFunc { get; }
        private readonly SemaphoreSlim Sync;

        protected GraphBase(
            List<TNode> nodes,
            Func<TNode, TNode, float> distanceFunc,
            Func<TNode, TNode, ConnectorType> typeFunc)
        {
            Nodes = nodes;
            Connectors = new IConnector<TEdge>[nodes.Count, nodes.Count];
            Opened = new FastPriorityQueue<TNode>(nodes.Count);
            DistanceFunc = distanceFunc;
            Sync = new SemaphoreSlim(1, 1);

            foreach (var node in Nodes)
                foreach (var neighbor in node.Neighbors)
                    Connect(node, (TNode) neighbor, typeFunc(node, (TNode) neighbor));
        }

        protected void Connect(TNode start, TNode end, ConnectorType type)
        {
            if (type == ConnectorType.Town)
                start.Neighbors.Add(end);

            Connectors[start.Index, end.Index] = new EdgeConnector<TEdge>
            {
                Start = start.Edge,
                End = end.Edge,
                Distance = type == ConnectorType.Town ? CONSTANTS.TOWN_AGGREGATE : DistanceFunc(start, end),
                Type = type
            };
        }

        protected void Disconnect(TNode start, TNode end)
        {
            Connectors[start.Index, end.Index] = null;
            start.Neighbors.Remove(end);
        }

        protected bool OpenNode(TNode node, float priority)
        {
            if (Opened.Contains(node))
            {
                if (node.Priority.SignificantlyGreaterThan(priority))
                {
                    Opened.UpdatePriority(node, priority);
                    return true;
                }
            } else
            {
                Opened.Enqueue(node, priority);
                return true;
            }

            return false;
        }

        protected void Reset()
        {
            Opened.Clear();

            foreach (var node in Nodes)
            {
                Opened.ResetNode(node);
                node.Reset();
            }
        }

        public async IAsyncEnumerable<IConnector<TEdge>> Navigate(
            int start,
            IEnumerable<int> ends,
            Action synchronizedSetup = default,
            Action synchronizedCleanup = default)
        {
            if (start < 0 || start >= Nodes.Count)
                throw new IndexOutOfRangeException("Invalid start index.");

            var endIndexes = ends.Select(index =>
                {
                    if (index < 0 || index >= Nodes.Count)
                        throw new IndexOutOfRangeException("Invalid end index.");

                    return index;
                })
                .ToArray();
            
            var path = new Stack<IConnector<TEdge>>();
            var startNode = Nodes[start];
            var endNodes = Nodes.ElementsAt(endIndexes).ToHashSet();
            var current = startNode;
            await Sync.WaitAsync();
            
            try
            {
                if (endNodes.Count == 0)
                    yield break;

                synchronizedSetup?.Invoke();
                Opened.Enqueue(current, 0);

                while (Opened.Count > 0)
                {
                    current = Opened.Dequeue();

                    if (current == null)
                        break;

                    //if we've reached any of the possible destinations... return that path
                    if (endNodes.Contains(current))
                        break;

                    for (var i = 0; i < current.Neighbors.Count; i++)
                    {
                        var neighbor = current.Neighbors[i];

                        if (neighbor.Closed)
                            continue;

                        if (OpenNode((TNode) neighbor,
                            current.Priority + Connectors[current.Index, neighbor.Index].Distance))
                            neighbor.Parent = current.Index;
                    }

                    current.Closed = true;
                }

                while (current?.Parent != null)
                {
                    var parent = Nodes[current.Parent.Value];
                    path.Push(Connectors[parent.Index, current.Index]);
                    current = parent;
                }

                synchronizedCleanup?.Invoke();
            } finally
            {
                Reset();
                Sync.Release();
            }

            while (path.Count > 0)
                yield return path.Pop();
        }

        public IEnumerator<TNode> GetEnumerator() => Nodes.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}