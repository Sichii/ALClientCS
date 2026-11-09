#region
using AL.Core.Geometry;
using AL.Pathfinding.Interfaces;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Represents everything smoothing a walk writes to, held per thread so a search's many candidate walks reuse it.
/// </summary>
internal sealed class SmoothScratch
{
    [ThreadStatic]
    private static SmoothScratch? Current;

    /// <summary>
    ///     The triangles a corridor rotation found its way through, from the last back to the first.
    /// </summary>
    public readonly List<IGenericTriangle<GraphNode>> Chain = [];

    /// <summary>
    ///     The node at each corner of the corridor, so a bend the funnel pulls onto one reuses its node.
    /// </summary>
    public readonly Dictionary<(float X, float Y), GraphNode> Corners = [];

    /// <summary>
    ///     A copy of a stretch's points, cut at the reach it ends in.
    /// </summary>
    public readonly List<GraphNode> CutPoints = [];

    /// <summary>The corridor being pulled.</summary>
    public List<IGenericTriangle<GraphNode>> Corridor = [];

    /// <summary>
    ///     The copy a corridor is extended on, swapped with <see cref="Corridor" /> when the extension holds.
    /// </summary>
    public List<IGenericTriangle<GraphNode>> Extended = [];

    /// <summary>
    ///     The triangle each triangle of a fan was reached from, while a corridor rotates round a vertex.
    /// </summary>
    public readonly Dictionary<IGenericTriangle<GraphNode>, IGenericTriangle<GraphNode>> Parents = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    ///     The points of the stretch being smoothed, in order.
    /// </summary>
    public readonly List<GraphNode> Points = [];

    /// <summary>The line the funnel pulls through the corridor.</summary>
    public readonly List<Point> Pulled = [];

    /// <summary>
    ///     The run of walk legs a route is smoothed in, between two legs that are not walks.
    /// </summary>
    public readonly List<GraphEdge> Stretch = [];

    /// <summary>
    ///     The triangles of a fan still to visit, while a corridor rotates round a vertex.
    /// </summary>
    public readonly Queue<IGenericTriangle<GraphNode>> Queue = new();

    /// <summary>The left point of each portal of the corridor.</summary>
    public Point[] Lefts = [];

    /// <summary>The right point of each portal of the corridor.</summary>
    public Point[] Rights = [];

    /// <summary>Makes room for a corridor's portals.</summary>
    /// <param name="count">The corridor's triangle count.</param>
    public void EnsurePortals(int count)
    {
        if (Lefts.Length >= count)
            return;

        Lefts = new Point[count * 2];
        Rights = new Point[count * 2];
    }

    /// <summary>Gets the calling thread's scratch.</summary>
    public static SmoothScratch Rent() => Current ??= new SmoothScratch();
}