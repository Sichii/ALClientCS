#region
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Everything a search writes to, held per thread so searches never share state and never allocate once warm. The mesh
///     arrays are sized to the largest mesh searched so far, the node arrays to the portal graph plus the ends of the
///     current search.
/// </summary>
internal sealed class SearchScratch
{
    [ThreadStatic]
    private static SearchScratch? Current;

    /// <summary>
    ///     Every arrival the current search has made, in the order made; an arrival's index is its id, and its
    ///     <see cref="Arrival.Parent" /> is the id it came from.
    /// </summary>
    public readonly List<Arrival> Arrivals = [];

    public readonly PriorityQueue<int, float> ArrivalQueue = new();
    public readonly List<int> Chain = [];
    public readonly List<int> Corridor = [];
    public readonly List<Point> Polyline = [];
    public readonly List<PortalGraph.Edge> SearchEdges = [];
    public readonly List<int> VertexChain = [];
    public readonly PriorityQueue<int, float> VertexQueue = new();
    public Point[] EndEntry = [];

    /// <summary>
    ///     The current search's ends, resolved onto their meshes; a null mesh is an end that resolved nowhere.
    /// </summary>
    public NavMesh?[] EndMesh = [];

    public Reach[] EndReach = [];
    public int[] EndTriangle = [];

    /// <summary>
    ///     A lower bound per node on the price still to pay to reach an end; infinite where no end can be reached.
    /// </summary>
    public float[] LowerBound = [];

    public readonly PriorityQueue<int, float> LowerBoundQueue = new();

    /// <summary>
    ///     The ids of the arrivals at each node that no other arrival there beats.
    /// </summary>
    public List<int>[] NodeArrivals = [];

    /// <summary>
    ///     Each node's map, as the portal graph's map index.
    /// </summary>
    public int[] NodeMap = [];

    /// <summary>
    ///     Where each node's edges start in <see cref="SearchEdges" /> once <see cref="IndexSearchEdges" /> has sorted them.
    /// </summary>
    public int[] SearchEdgeStart = [];

    public int SearchTriangle = -1;
    private PortalGraph.Edge[] SearchEdgeBuffer = [];

    /// <summary>
    ///     The vertex search: cost and parent per mesh vertex, and the triangle and point it was seeded from.
    /// </summary>
    public float[] VertexCost = [];

    public int[] VertexParent = [];

    /// <summary>
    ///     The calling thread's scratch, created on first use.
    /// </summary>
    public static SearchScratch Rent() => Current ??= new SearchScratch();

    public void ResetEnds(int ends)
    {
        if (EndMesh.Length < ends)
        {
            EndMesh = new NavMesh?[ends];
            EndTriangle = new int[ends];
            EndEntry = new Point[ends];
            EndReach = new Reach[ends];
        }

        Array.Clear(EndMesh, 0, ends);
    }

    /// <summary>
    ///     Sorts <see cref="SearchEdges" /> by <see cref="PortalGraph.Edge.From" /> and fills <see cref="SearchEdgeStart" />.
    ///     The sort is stable, so the edges out of one node are relaxed in the order they were added.
    /// </summary>
    public void IndexSearchEdges(int nodes)
    {
        var count = SearchEdges.Count;

        if (SearchEdgeBuffer.Length < count)
            SearchEdgeBuffer = new PortalGraph.Edge[Math.Max(count, SearchEdgeBuffer.Length * 2)];

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
    ///     Empties every node's arrival list, the arrival pool and the queue, for a fresh search over the same edges.
    /// </summary>
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
            NodeArrivals = grown;
            SearchEdgeStart = new int[nodes + 1];
            NodeMap = new int[nodes];
            LowerBound = new float[nodes];
        }

        for (var i = 0; i < nodes; i++)
            NodeArrivals[i] ??= [];

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
    ///     One way of reaching a node: what it cost, the state it left the character in, and the move that made it.
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
        ///     The start or arrival node where the route last entered the current map, which the floor measures a cast's landing
        ///     from; -1 at a departure or an end, and whenever the floor rules are off.
        /// </summary>
        public int Anchor;

        /// <summary>
        ///     The maps the route has been on, one bit per map index; empty whenever the floor rules are off.
        /// </summary>
        public UInt128 Visited;

        /// <summary>
        ///     The maps visited before the route's most recent blink, which the route may not enter again.
        /// </summary>
        public UInt128 Closed;

        /// <summary>
        ///     Whether the current stay on this map is a return to it, which may not blink.
        /// </summary>
        public bool Revisit;

        /// <summary>
        ///     The arrival this one came from, or -1 for the start.
        /// </summary>
        public int Parent;

        /// <summary>
        ///     The edge that made it: a static edge's index, or <see cref="PortalGraph" />'s static count plus a search edge's.
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