namespace AL.Pathfinding.Model;

/// <summary>
///     Represents everything a search over one map's <see cref="MeshGraph" /> writes to, held per thread so searches never
///     share state and never allocate once warm.
/// </summary>
internal sealed class MeshScratch
{
    [ThreadStatic]
    private static MeshScratch? Current;

    public readonly PriorityQueue<int, float> Queue = new();

    /// <summary>
    ///     The cost per vertex; read through <see cref="GetCost" />, since a stale stamp means unreached.
    /// </summary>
    public float[] Cost = [];

    /// <summary>
    ///     The hop each vertex was reached by, or -1 for a walk or a seed.
    /// </summary>
    public int[] ParentHop = [];

    /// <summary>
    ///     The vertex each vertex was reached from, or -1 for a seed. In a backward search it is the next vertex towards the
    ///     source.
    /// </summary>
    public int[] Parent = [];

    /// <summary>
    ///     The in-motion penalty score on reaching each vertex in a forward search.
    /// </summary>
    public float[] Score = [];

    /// <summary>
    ///     The <see cref="Stamp" /> each vertex was last reached in; another value means unreached this search.
    /// </summary>
    private int[] Reached = [];

    /// <summary>
    ///     The <see cref="Stamp" /> each vertex was last settled in.
    /// </summary>
    private int[] Settled = [];

    private int Stamp;

    /// <summary>
    ///     The <see cref="Stamp" /> each vertex was last marked a target in.
    /// </summary>
    private int[] Targets = [];

    /// <summary>Gets a vertex's cost this search.</summary>
    /// <param name="vertex">The vertex's id.</param>
    /// <returns>
    ///     The cost, or <see cref="float.MaxValue" /> when the vertex was not reached.
    /// </returns>
    public float GetCost(int vertex) => Reached[vertex] == Stamp ? Cost[vertex] : float.MaxValue;

    public bool IsTarget(int vertex) => Targets[vertex] == Stamp;

    /// <summary>Marks a vertex a target of this search.</summary>
    /// <param name="vertex">The vertex's id.</param>
    /// <returns>
    ///     <c>true</c> if the vertex was not already a target; otherwise, <c>false</c>.
    /// </returns>
    public bool MarkTarget(int vertex)
    {
        if (Targets[vertex] == Stamp)
            return false;

        Targets[vertex] = Stamp;

        return true;
    }

    /// <summary>Records a vertex as reached at a cost.</summary>
    /// <param name="vertex">The vertex's id.</param>
    /// <param name="cost">The cost.</param>
    /// <param name="parent">The vertex it was reached from, or -1.</param>
    /// <param name="parentHop">The hop it was reached by, or -1.</param>
    /// <param name="score">The penalty score on reaching it.</param>
    public void MarkReached(
        int vertex,
        float cost,
        int parent,
        int parentHop,
        float score)
    {
        Reached[vertex] = Stamp;
        Cost[vertex] = cost;
        Parent[vertex] = parent;
        ParentHop[vertex] = parentHop;
        Score[vertex] = score;
        Queue.Enqueue(vertex, cost);
    }

    /// <summary>
    ///     Gets the calling thread's scratch, created on first use.
    /// </summary>
    /// <returns>The calling thread's scratch.</returns>
    public static MeshScratch Rent() => Current ??= new MeshScratch();

    /// <summary>
    ///     Starts a new search over a graph of a number of vertices: every vertex reads as unreached, unsettled and no target.
    /// </summary>
    /// <param name="vertices">The graph's vertex count.</param>
    public void Reset(int vertices)
    {
        if (Cost.Length < vertices)
        {
            Cost = new float[vertices];
            Parent = new int[vertices];
            ParentHop = new int[vertices];
            Score = new float[vertices];
            Reached = new int[vertices];
            Settled = new int[vertices];
            Targets = new int[vertices];
        }

        //a wrapped stamp would read old marks as this search's
        if (++Stamp == int.MaxValue)
        {
            Array.Clear(Reached);
            Array.Clear(Settled);
            Array.Clear(Targets);
            Stamp = 1;
        }

        Queue.Clear();
    }

    /// <summary>Settles a vertex.</summary>
    /// <param name="vertex">The vertex's id.</param>
    /// <returns>
    ///     <c>true</c> if the vertex was not already settled; otherwise, <c>false</c>.
    /// </returns>
    public bool TrySettle(int vertex)
    {
        if (Settled[vertex] == Stamp)
            return false;

        Settled[vertex] = Stamp;

        return true;
    }
}