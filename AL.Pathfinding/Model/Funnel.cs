#region
using System.Buffers;
using System.Runtime.CompilerServices;
using AL.Core.Geometry;
#endregion

namespace AL.Pathfinding.Model;

/// <summary>
///     The simple stupid funnel: string-pulls a corridor of triangles into the shortest polyline through the
///     portals between them. Every corner it emits is a mesh vertex, and mesh vertices are the centres of walkable
///     raster cells a unit clear of the padded walls, which is what lets every leg pass the exact line test.
/// </summary>
public static class Funnel
{
    /// <summary>
    ///     Pulls the corridor into <paramref name="path" />, which is cleared first. The corridor lists triangle ids
    ///     from the one containing <paramref name="start" /> to the one containing <paramref name="end" />, each
    ///     adjacent to the next.
    /// </summary>
    public static void Pull(
        TriangleMesh mesh,
        ReadOnlySpan<int> corridor,
        Point start,
        Point end,
        List<Point> path)
    {
        path.Clear();
        Append(path, start);

        if (corridor.Length <= 1)
        {
            Append(path, end);

            return;
        }

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
                var to = corridor[i + 1];
                var slot = SlotOf(mesh, from, to);
                var a = mesh.Vertices[mesh.Corners[from * 3 + (slot + 1) % 3]];
                var b = mesh.Vertices[mesh.Corners[from * 3 + (slot + 2) % 3]];
                (var cx, var cy) = mesh.Centroid(from);

                //portals: one per triangle boundary, plus the end as a zero-width portal. left and right are chosen
                //so that TriArea2(previous centroid, left, right) > 0, which is the orientation the pull below assumes
                //a zero-area result (a sliver so thin the centroid is collinear with the portal) falls to the else and picks a side arbitrarily
                if (TriArea2(cx, cy, a, b) > 0f)
                {
                    lefts[i] = a;
                    rights[i] = b;
                } else
                {
                    lefts[i] = b;
                    rights[i] = a;
                }
            }

            lefts[portalCount - 1] = end;
            rights[portalCount - 1] = end;

            var apex = start;
            var left = lefts[0];
            var right = rights[0];
            var apexIndex = 0;
            var leftIndex = 0;
            var rightIndex = 0;

            for (var i = 1; i < portalCount; i++)
            {
                var nextLeft = lefts[i];
                var nextRight = rights[i];

                //tighten the right side
                if (TriArea2(apex, right, nextRight) <= 0f)
                {
                    if (Same(apex, right) || (TriArea2(apex, left, nextRight) > 0f))
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
                    if (Same(apex, left) || (TriArea2(apex, right, nextLeft) < 0f))
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

                        continue;
                    }
                }
            }

            Append(path, end);
        } finally
        {
            if (leftsArray is not null)
                ArrayPool<Point>.Shared.Return(leftsArray);

            if (rightsArray is not null)
                ArrayPool<Point>.Shared.Return(rightsArray);
        }
    }

    //appends only if it would not duplicate the last point already in path
    private static void Append(List<Point> path, Point point)
    {
        if ((path.Count == 0) || !Same(path[^1], point))
            path.Add(point);
    }

    //which slot of 'from' faces 'to'
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SlotOf(TriangleMesh mesh, int from, int to)
    {
        for (var slot = 0; slot < 3; slot++)
            if (mesh.Neighbour(from, slot) == to)
                return slot;

        throw new InvalidOperationException($"Triangles {from} and {to} are not adjacent.");
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Same(Point a, Point b) => (MathF.Abs(a.X - b.X) < 0.001f) && (MathF.Abs(a.Y - b.Y) < 0.001f);
}