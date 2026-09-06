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

    public readonly List<int> Chain = [];
    public readonly List<int> Corridor = [];
    public readonly PriorityQueue<int, float> NodeQueue = new();
    public readonly List<Point> Polyline = [];
    public readonly List<PortalGraph.Edge> SearchEdges = [];
    public readonly List<int> VertexChain = [];
    public readonly PriorityQueue<int, float> VertexQueue = new();
    public Point[] EndEntry = [];

    //the current search's ends, resolved onto their meshes; a null mesh is an end that resolved nowhere
    public NavMesh?[] EndMesh = [];
    public Reach[] EndReach = [];
    public int[] EndTriangle = [];

    public float[] NodeCost = [];
    public int[] NodeParent = [];
    public int[] NodeParentEdge = [];
    public int SearchTriangle = -1;

    //the vertex search: cost and parent per mesh vertex, and the triangle and point it was seeded from
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

    public void ResetNodes(int nodes)
    {
        if (NodeCost.Length < nodes)
        {
            NodeCost = new float[nodes];
            NodeParent = new int[nodes];
            NodeParentEdge = new int[nodes];
        }

        Array.Fill(
            NodeCost,
            float.MaxValue,
            0,
            nodes);

        Array.Fill(
            NodeParent,
            -1,
            0,
            nodes);

        Array.Fill(
            NodeParentEdge,
            -1,
            0,
            nodes);
        NodeQueue.Clear();
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
}