#region
using AL.Core.Geometry;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     A mesh built by hand: the unit square from (0, 0) to (100, 100) split along its diagonal into T0 below the
///     diagonal (v0, v1, v2) and T1 above it (v0, v2, v3). Neighbour slot i is across the edge opposite corner i.
/// </summary>
public class TriangleMeshTests
{
    internal static TriangleMesh Square()
        => new(
            [
                new Point(0, 0),
                new Point(100, 0),
                new Point(100, 100),
                new Point(0, 100)
            ],
            [0, 1, 2, 0, 2, 3],
            [-1, 1, -1, -1, -1, 0]);

    [Test]
    public void TriangleAtAnswersTheContainingTriangle()
    {
        var mesh = Square();

        mesh.TriangleAt(90, 10)
            .Should()
            .Be(0);

        mesh.TriangleAt(10, 90)
            .Should()
            .Be(1);

        mesh.TriangleAt(-5, 50)
            .Should()
            .Be(-1);

        mesh.TriangleAt(50, 50)
            .Should()
            .BeOneOf(0, 1);
    }

    [Test]
    public void NeighboursAreReadBySlot()
    {
        var mesh = Square();

        mesh.Neighbour(0, 1)
            .Should()
            .Be(1);

        mesh.Neighbour(1, 2)
            .Should()
            .Be(0);

        mesh.Neighbour(0, 0)
            .Should()
            .Be(-1);
    }

    [Test]
    public void CentroidIsTheMeanOfTheCorners()
    {
        (var x, var y) = Square()
            .Centroid(0);

        x.Should()
         .BeApproximately(200f / 3f, 0.001f);

        y.Should()
         .BeApproximately(100f / 3f, 0.001f);
    }

    [Test]
    public void NearestVertexPrefersOneThePredicateAccepts()
    {
        var mesh = Square();

        mesh.NearestVertex(10, 10, _ => true)
            .Should()
            .Be(0);

        //v0 is nearest but refused, so v1 at 90 units beats v3 at 90 units only by index order; refuse it too
        mesh.NearestVertex(10, 10, index => index is not 0 and not 1)
            .Should()
            .Be(3);

        //nothing accepted: the nearest of all
        mesh.NearestVertex(10, 10, _ => false)
            .Should()
            .Be(0);
    }

    [Test]
    public void EveryVertexKnowsATriangleItBelongsTo()
    {
        var mesh = Square();

        for (var vertex = 0; vertex < mesh.Vertices.Length; vertex++)
        {
            var triangle = mesh.TriangleOfVertex(vertex);

            (mesh.Corners[triangle * 3] == vertex || mesh.Corners[triangle * 3 + 1] == vertex || mesh.Corners[triangle * 3 + 2] == vertex)
                .Should()
                .BeTrue();
        }
    }

    [Test]
    public void NearestInsideStepsAcrossTheNearestBoundaryEdge()
    {
        var mesh = Square();

        //already inside: the point itself
        mesh.TryNearestInside(50, 50, 24, out var x, out var y)
            .Should()
            .BeTrue();

        x.Should()
         .Be(50);

        y.Should()
         .Be(50);

        //five left of the left edge: just inside it, in the triangle that owns that edge
        mesh.TryNearestInside(-5, 50, 24, out x, out y)
            .Should()
            .BeTrue();

        x.Should()
         .BeInRange(0, 1);

        y.Should()
         .BeApproximately(50, 1);

        mesh.TriangleAt(x, y)
            .Should()
            .Be(1);

        //past the range: nothing
        mesh.TryNearestInside(-30, 50, 24, out _, out _)
            .Should()
            .BeFalse();
    }

    /// <summary>
    ///     Two triangles touching only at v2 (50, 50): T0 (v0, v1, v2) below it and T1 (v2, v4, v3) above it, with no
    ///     shared edge, so the fan round v2 is two sectors.
    /// </summary>
    private static TriangleMesh BowTie()
        => new(
            [
                new Point(0, 0),
                new Point(100, 0),
                new Point(50, 50),
                new Point(0, 100),
                new Point(100, 100)
            ],
            [0, 1, 2, 2, 4, 3],
            [-1, -1, -1, -1, -1, -1]);

    [Test]
    public void PinchVertexGetsNoEdges()
    {
        var mesh = BowTie();

        mesh.EdgesFrom(2)
            .ToArray()
            .Should()
            .BeEmpty();

        //the other vertices keep the edges between themselves, none towards the pinch
        mesh.EdgesFrom(0)
            .ToArray()
            .Should()
            .Equal(1);

        mesh.EdgesFrom(1)
            .ToArray()
            .Should()
            .Equal(0);

        mesh.EdgesFrom(3)
            .ToArray()
            .Should()
            .Equal(4);

        mesh.EdgesFrom(4)
            .ToArray()
            .Should()
            .Equal(3);
    }

    [Test]
    public void EdgesAreListedInAscendingVertexOrder()
    {
        var mesh = Square();

        mesh.EdgesFrom(0)
            .ToArray()
            .Should()
            .Equal(1, 2, 3);

        mesh.EdgesFrom(2)
            .ToArray()
            .Should()
            .Equal(0, 1, 3);
    }
}
