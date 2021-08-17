using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
using AL.Pathfinding.Model;
using Chaos.Core.Extensions;
using Common.Logging;
using Priority_Queue;

#nullable enable

namespace AL.Pathfinding.Abstractions
{
    /// <summary>
    ///     Provides a basic implementation of djikstra search.
    /// </summary>
    /// <typeparam name="TNode">
    ///     An implementation of <see cref="IGraphNode{TEdge}" /> inheriting from
    ///     <see cref="FastPriorityQueueNode" />.
    /// </typeparam>
    /// <typeparam name="TEdge">
    ///     The underlying data type for a node, generally some sort of
    ///     <see cref="AL.Core.Interfaces.IPoint" />.
    /// </typeparam>
    /// <seealso cref="IEnumerable{T}" />
    public abstract class GraphBase<TNode, TEdge> : IEnumerable<TNode> where TNode: FastPriorityQueueNode, IGraphNode<TEdge>
                                                                       where TEdge: IEquatable<TEdge>
    {
        private readonly Func<TNode, TNode, float> HeuristicFunc;
        private readonly FastPriorityQueue<TNode> Opened;
        private readonly SemaphoreSlim Sync;
        private readonly Func<TNode, TNode, ConnectorType> TypeFunc;
        /// <summary>
        ///     A <see cref="Common.Logging">Common.Logging</see> logger.
        /// </summary>
        protected abstract ILog Logger { get; init; }

        /// <summary>
        ///     The connections between the edges.
        /// </summary>
        protected internal IConnector<TEdge>?[,] Connectors { get; }

        /// <summary>
        ///     A list of nodes, index is important.
        /// </summary>
        protected internal List<TNode> Nodes { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GraphBase{TNode,TEdge}" /> class.
        /// </summary>
        /// <param name="nodes">
        ///     The nodes to populate the graph with. <see cref="Connect" /> will be called for every node =>
        ///     neighbor.
        /// </param>
        /// <param name="heuristicFunc">The heuristic to use when connecting nodes.</param>
        /// <param name="typeFunc">A function to determine the type of connection.</param>
        /// <exception cref="ArgumentNullException">nodes</exception>
        /// <exception cref="ArgumentNullException">heuristicFunc</exception>
        /// <exception cref="ArgumentNullException">typeFunc</exception>
        protected GraphBase(List<TNode> nodes, Func<TNode, TNode, float> heuristicFunc, Func<TNode, TNode, ConnectorType> typeFunc)
        {

            Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            Connectors = new IConnector<TEdge>[nodes.Count, nodes.Count];
            Opened = new FastPriorityQueue<TNode>(nodes.Count);
            HeuristicFunc = heuristicFunc ?? throw new ArgumentNullException(nameof(heuristicFunc));
            TypeFunc = typeFunc ?? throw new ArgumentNullException(nameof(typeFunc));
            Sync = new SemaphoreSlim(1, 1);

            foreach (var node in Nodes)
                foreach (var neighbor in node.Neighbors)
                    Connect(node, (TNode)neighbor, typeFunc(node, (TNode)neighbor));
        }

        /// <summary>
        ///     Connects a start node to an end node. The connection is 1 way.
        /// </summary>
        /// <param name="start">The start of the connection/</param>
        /// <param name="end">The end of the connection.</param>
        /// <param name="type">If type is specified, the type func will not be called.</param>
        /// <exception cref="ArgumentNullException">start</exception>
        /// <exception cref="ArgumentNullException">end</exception>
        protected void Connect(TNode start, TNode end, ConnectorType? type = default)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (end == null)
                throw new ArgumentNullException(nameof(end));

            var finalType = type ?? TypeFunc(start, end);

            if (finalType == ConnectorType.Town)
                start.Neighbors.Add(end);

            Connectors[start.Index, end.Index] = new EdgeConnector<TEdge>
            {
                Start = start.Edge,
                End = end.Edge,
                Heuristic = finalType == ConnectorType.Town ? CONSTANTS.TOWN_HEURISTIC : HeuristicFunc(start, end),
                Type = finalType
            };
        }

        /// <summary>
        ///     Disconnects a start node and end node. (Order matters, connections are 1 way)
        /// </summary>
        /// <param name="start">The start of the connection/</param>
        /// <param name="end">The end of the connection.</param>
        /// <exception cref="ArgumentNullException">start</exception>
        /// <exception cref="ArgumentNullException">end</exception>
        protected void Disconnect(TNode start, TNode end)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (end == null)
                throw new ArgumentNullException(nameof(end));

            Connectors[start.Index, end.Index] = null;
            start.Neighbors.Remove(end);
        }

        public IEnumerator<TNode> GetEnumerator() => Nodes.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Performs a djikstra search, given a start node index, and any number of end node indexes.
        /// </summary>
        /// <param name="start">The index of a node to start searching from.</param>
        /// <param name="ends">
        ///     Any number of indexes of nodes that can be the end of the path. Upon reaching any of the end nodes,
        ///     that path will be returned.
        /// </param>
        /// <param name="synchronizedSetup">
        ///     A function that can be before the djikstra search begins, but still inside the internal
        ///     synchronization.
        /// </param>
        /// <param name="synchronizedCleanup">
        ///     A function that can be run after the djikstra search finishes, but still inside the
        ///     internal synchronization.
        /// </param>
        /// <returns>
        ///     <see cref="IAsyncEnumerable{T}" /> of <see cref="IConnector{TEdge}" /> <br />
        ///     A lazy enumeration of the first path found that starts with the specified starting node, and ends with any of the
        ///     specified end nodes.
        /// </returns>
        /// <exception cref="ArgumentNullException">ends</exception>
        /// <exception cref="IndexOutOfRangeException">Invalid start index.</exception>
        /// <exception cref="IndexOutOfRangeException">Invalid end index.</exception>
        protected async IAsyncEnumerable<IConnector<TEdge>> Navigate(
            int start,
            IEnumerable<int> ends,
            Action? synchronizedSetup = default,
            Action? synchronizedCleanup = default)
        {
            if (ends == null)
                throw new ArgumentNullException(nameof(ends));

            if ((start < 0) || (start >= Nodes.Count))
                throw new IndexOutOfRangeException("Invalid start index.");

            var endIndexes = ends.Select(index =>
                {
                    if ((index < 0) || (index >= Nodes.Count))
                        throw new IndexOutOfRangeException("Invalid end index.");

                    return index;
                })
                .ToArray();

            var path = new Stack<IConnector<TEdge>>();
            var startNode = Nodes[start];
            var endNodes = Nodes.ElementsAt(endIndexes).ToHashSet();

            //if we're standing on an end point... yield nothing
            if (endNodes.Any(endNode => startNode == endNode))
                yield break;

            var current = startNode;
            await Sync.WaitAsync().ConfigureAwait(false);

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

                        if (OpenNode((TNode)neighbor, current.Priority + Connectors[current.Index, neighbor.Index]!.Heuristic))
                            neighbor.Parent = current.Index;
                    }

                    current.Closed = true;
                }

                while (current?.Parent != null)
                {
                    var parent = Nodes[current.Parent.Value];
                    path.Push(Connectors[parent.Index, current.Index]!);
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

        private bool OpenNode(TNode node, float priority)
        {
            if (Opened.Contains(node))
            {
                if (node.Priority.SignificantlyGreaterThan(priority, Core.Definitions.CONSTANTS.EPSILON))
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

        /// <summary>
        ///     Clears the opened priority queue, and resets all nodes in the node array.
        /// </summary>
        protected void Reset()
        {
            Opened.Clear();

            foreach (var node in Nodes)
            {
                Opened.ResetNode(node);
                node.Reset();
            }
        }
    }
}