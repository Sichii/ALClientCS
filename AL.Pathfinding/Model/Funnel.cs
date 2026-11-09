#region
using System.Buffers;
using System.Runtime.CompilerServices;
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     Provides the simple stupid funnel, which pulls a corridor of triangles into the shortest polyline through the
///     portals between them. Every corner it emits is a mesh vertex.
/// </summary>
public static class Funnel
{
    private static void Append(List<Point> path, Point point)
    {
        if ((path.Count == 0) || !IsSamePoint(path[^1], point))
            path.Add(point);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindPortalSlot(TriangleMesh mesh, int from, int to)
    {
        for (var slot = 0; slot < 3; slot++)
            if (mesh.Neighbour(from, slot) == to)
                return slot;

        throw new InvalidOperationException($"Triangles {from} and {to} are not adjacent.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSamePoint(Point a, Point b) => (MathF.Abs(a.X - b.X) < 0.001f) && (MathF.Abs(a.Y - b.Y) < 0.001f);

    /// <summary>
    ///     Pulls a corridor of triangles into the shortest polyline from <paramref name="start" /> to <paramref name="end" />.
    /// </summary>
    /// <param name="mesh">The mesh the corridor runs through.</param>
    /// <param name="corridor">
    ///     Triangle ids from the one containing the start to the one containing the end, each adjacent to the next.
    /// </param>
    /// <param name="start">The start of the path.</param>
    /// <param name="end">The end of the path.</param>
    /// <param name="path">
    ///     The list the polyline is written into, cleared first.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Two consecutive triangles in the corridor are not adjacent.
    /// </exception>
    public static void Pull(
        TriangleMesh mesh,
        ReadOnlySpan<int> corridor,
        Point start,
        Point end,
        List<Point> path)
    {
        var portalCount = corridor.Length;
        Point[]? leftsArray = null;
        Point[]? rightsArray = null;

        scoped Span<Point> lefts;
        scoped Span<Point> rights;

        if (portalCount <= 256)
        {
            lefts = stackalloc Point[portalCount];
            rights = stackalloc Point[portalCount];
        } else
        {
            leftsArray = ArrayPool<Point>.Shared.Rent(portalCount);
            rightsArray = ArrayPool<Point>.Shared.Rent(portalCount);
            lefts = leftsArray.AsSpan(0, portalCount);
            rights = rightsArray.AsSpan(0, portalCount);
        }

        try
        {
            for (var i = 0; i < (corridor.Length - 1); i++)
            {
                var from = corridor[i];
                var slot = FindPortalSlot(mesh, from, corridor[i + 1]);
                (var cx, var cy) = mesh.CalculateCentroid(from);

                SetPortal(
                    lefts,
                    rights,
                    i,
                    cx,
                    cy,
                    mesh.Vertices[mesh.Corners[from * 3 + (slot + 1) % 3]],
                    mesh.Vertices[mesh.Corners[from * 3 + (slot + 2) % 3]]);
            }

            PullThroughPortals(
                start,
                end,
                lefts,
                rights,
                path);
        } finally
        {
            if (leftsArray is not null)
                ArrayPool<Point>.Shared.Return(leftsArray);

            if (rightsArray is not null)
                ArrayPool<Point>.Shared.Return(rightsArray);
        }
    }

    /// <summary>
    ///     Sets the portal between a corridor's triangle and the next, oriented so
    ///     <c>TriArea2(centroid, left, right) &gt; 0</c> as the pull assumes.
    /// </summary>
    /// <param name="lefts">The portals' left points.</param>
    /// <param name="rights">The portals' right points.</param>
    /// <param name="index">The triangle's index in the corridor.</param>
    /// <param name="cx">The X coordinate of the triangle's centroid.</param>
    /// <param name="cy">The Y coordinate of the triangle's centroid.</param>
    /// <param name="a">One end of the edge shared with the next triangle.</param>
    /// <param name="b">The other end of that edge.</param>
    internal static void SetPortal(
        Span<Point> lefts,
        Span<Point> rights,
        int index,
        float cx,
        float cy,
        Point a,
        Point b)
    {
        //a sliver whose centroid is collinear with the portal picks a side arbitrarily
        if (TriArea2(
                cx,
                cy,
                a,
                b)
            > 0f)
        {
            lefts[index] = a;
            rights[index] = b;
        } else
        {
            lefts[index] = b;
            rights[index] = a;
        }
    }

    /// <summary>
    ///     Pulls the shortest polyline from <paramref name="start" /> to <paramref name="end" /> through a corridor's portals.
    /// </summary>
    /// <param name="start">The start of the path.</param>
    /// <param name="end">The end of the path.</param>
    /// <param name="lefts">
    ///     One slot per corridor triangle, each but the last holding the left point of the portal into the next; the last is
    ///     overwritten with the end.
    /// </param>
    /// <param name="rights">
    ///     The right points, laid out as <paramref name="lefts" />.
    /// </param>
    /// <param name="path">
    ///     The list the polyline is written into, cleared first.
    /// </param>
    internal static void PullThroughPortals(
        Point start,
        Point end,
        Span<Point> lefts,
        Span<Point> rights,
        List<Point> path)
    {
        path.Clear();
        Append(path, start);

        var portalCount = lefts.Length;

        if (portalCount <= 1)
        {
            Append(path, end);

            return;
        }

        lefts[portalCount - 1] = end;
        rights[portalCount - 1] = end;

        var apex = start;
        var left = lefts[0];
        var right = rights[0];
        int apexIndex;
        var leftIndex = 0;
        var rightIndex = 0;

        for (var i = 1; i < portalCount; i++)
        {
            var nextLeft = lefts[i];
            var nextRight = rights[i];

            //tighten the right side
            if (TriArea2(apex, right, nextRight) <= 0f)
            {
                if (IsSamePoint(apex, right) || (TriArea2(apex, left, nextRight) > 0f))
                {
                    right = nextRight;
                    rightIndex = i;
                } else
                {
                    //right crossed over left: left becomes the new apex, restart from it
                    apex = left;
                    apexIndex = leftIndex;
                    Append(path, apex);
                    left = apex;
                    right = apex;
                    leftIndex = apexIndex;
                    rightIndex = apexIndex;
                    i = apexIndex;

                    continue;
                }
            }

            //tighten the left side
            if (TriArea2(apex, left, nextLeft) >= 0f)
            {
                if (IsSamePoint(apex, left) || (TriArea2(apex, right, nextLeft) < 0f))
                {
                    left = nextLeft;
                    leftIndex = i;
                } else
                {
                    //left crossed over right: mirror of the right-crossing branch above
                    apex = right;
                    apexIndex = rightIndex;
                    Append(path, apex);
                    left = apex;
                    right = apex;
                    leftIndex = apexIndex;
                    rightIndex = apexIndex;
                    i = apexIndex;
                }
            }
        }

        Append(path, end);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float TriArea2(Point a, Point b, Point c) => (c.X - a.X) * (b.Y - a.Y) - (b.X - a.X) * (c.Y - a.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float TriArea2(
        float ax,
        float ay,
        Point b,
        Point c)
        => (c.X - ax) * (b.Y - ay) - (b.X - ax) * (c.Y - ay);
}