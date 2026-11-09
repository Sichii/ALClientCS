#region
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents everything a door search and a mesh walk measurement write to, held per thread so searches never share
///     state and never allocate once warm.
/// </summary>
/// <remarks>
///     The vertex fields, <see cref="Corridor" /> and <see cref="Polyline" /> serve only <see cref="HopBuilder" />'s
///     funnel walk.
/// </remarks>
internal sealed class SearchScratch
{
    [ThreadStatic]
    private static SearchScratch? Current;

    public readonly PriorityQueue<int, float> ArrivalQueue = new();

    /// <summary>
    ///     Every arrival the current search has made, in the order made; an arrival's index is its id, and its
    ///     <see cref="Arrival.Parent" /> is the id it came from.
    /// </summary>
    public readonly List<Arrival> Arrivals = [];

    public readonly List<int> Chain = [];
    public readonly List<int> Corridor = [];

    public readonly PriorityQueue<int, float> LowerBoundQueue = new();
    public readonly List<Point> Polyline = [];
    public readonly List<DoorSearch.Edge> SearchEdges = [];
    public readonly List<int> VertexChain = [];
    public readonly PriorityQueue<int, float> VertexQueue = new();

    /// <summary>
    ///     A lower bound per node on the price still to pay to reach an end; infinite where no end can be reached.
    /// </summary>
    public float[] LowerBound = [];

    /// <summary>
    ///     The ids of the arrivals at each node that no other arrival there beats.
    /// </summary>
    public List<int>[] NodeArrivals = [];

    private DoorSearch.Edge[] SearchEdgeBuffer = [];

    /// <summary>
    ///     Where each node's edges start in <see cref="SearchEdges" /> once <see cref="IndexSearchEdges" /> has sorted them.
    /// </summary>
    public int[] SearchEdgeStart = [];

    public int SearchTriangle = -1;

    /// <summary>The vertex search's cost per mesh vertex.</summary>
    public float[] VertexCost = [];

    public int[] VertexParent = [];

    /// <summary>
    ///     Sorts <see cref="SearchEdges" /> by <see cref="DoorSearch.Edge.From" /> and fills <see cref="SearchEdgeStart" />.
    ///     The sort is stable, so the edges out of one node are relaxed in the order they were added.
    /// </summary>
    /// <param name="nodes">The number of nodes in the search.</param>
    public void IndexSearchEdges(int nodes)
    {
        var count = SearchEdges.Count;

        if (SearchEdgeBuffer.Length < count)
            SearchEdgeBuffer = new DoorSearch.Edge[Math.Max(count, SearchEdgeBuffer.Length * 2)];

        Array.Clear(SearchEdgeStart, 0, nodes + 1);

        foreach (var edge in SearchEdges)
            SearchEdgeStart[edge.From + 1]++;

        for (var i = 1; i <= nodes; i++)
            SearchEdgeStart[i] += SearchEdgeStart[i - 1];

        //placing each edge advances its node's start to the next node's, so shifting right by one restores them
        foreach (var edge in SearchEdges)
            SearchEdgeBuffer[SearchEdgeStart[edge.From]++] = edge;

        for (var i = nodes; i > 0; i--)
            SearchEdgeStart[i] = SearchEdgeStart[i - 1];

        SearchEdgeStart[0] = 0;

        for (var i = 0; i < count; i++)
            SearchEdges[i] = SearchEdgeBuffer[i];
    }

    /// <summary>
    ///     Gets the calling thread's scratch, created on first use.
    /// </summary>
    /// <returns>The calling thread's scratch.</returns>
    public static SearchScratch Rent() => Current ??= new SearchScratch();

    /// <summary>
    ///     Empties every node's arrival list, the arrival pool and the queue, for a fresh search over the same edges.
    /// </summary>
    /// <param name="nodes">The number of nodes in the search.</param>
    public void ResetArrivals(int nodes)
    {
        for (var i = 0; i < nodes; i++)
            NodeArrivals[i]
                .Clear();

        Arrivals.Clear();
        ArrivalQueue.Clear();
    }

    public void ResetNodes(int nodes)
    {
        if (NodeArrivals.Length < nodes)
        {
            var grown = new List<int>[nodes];
            Array.Copy(NodeArrivals, grown, NodeArrivals.Length);

            for (var i = NodeArrivals.Length; i < nodes; i++)
                grown[i] = [];

            NodeArrivals = grown;
            SearchEdgeStart = new int[nodes + 1];
            LowerBound = new float[nodes];
        }

        ResetArrivals(nodes);
        SearchEdges.Clear();
    }

    public void ResetVertices(int vertices)
    {
        if (VertexCost.Length < vertices)
        {
            VertexCost = new float[vertices];
            VertexParent = new int[vertices];
        }

        Array.Fill(
            VertexCost,
            float.MaxValue,
            0,
            vertices);

        Array.Fill(
            VertexParent,
            -1,
            0,
            vertices);
        VertexQueue.Clear();
    }

    /// <summary>
    ///     Represents one way of reaching a node: what it cost, the state it left the character in, and the move that made it.
    /// </summary>
    internal struct Arrival
    {
        /// <summary>The node reached.</summary>
        public int Node;

        /// <summary>The price so far, in walk units.</summary>
        public float Cost;

        /// <summary>
        ///     The blink cooldown, the pending penalty and the bar on arrival.
        /// </summary>
        public TravelState State;

        /// <summary>
        ///     The estimated in-motion penalty score on arrival.
        /// </summary>
        public float HopScore;

        /// <summary>
        ///     The arrival this one came from, or -1 for the start.
        /// </summary>
        public int Parent;

        /// <summary>
        ///     The edge that made it: a static edge's index, or the <see cref="DoorSearch" />'s static count plus a search edge's.
        /// </summary>
        public int EdgeIndex;

        /// <summary>
        ///     Whether the edge was cast rather than walked; the expansion reads it back.
        /// </summary>
        public bool Blinked;

        /// <summary>
        ///     Whether a later arrival at the same node beat this one, so taking it from the queue does nothing.
        /// </summary>
        public bool Dropped;
    }
}