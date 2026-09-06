#region
using AL.Core.Geometry;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class FunnelTests
{
    [Test]
    public void ASingleTriangleIsOneLeg()
    {
        var path = new List<Point>
        {
            new(-1, -1)
        };

        Funnel.Pull(
            TriangleMeshTests.Square(),
            [0],
            new Point(60, 10),
            new Point(90, 40),
            path);

        path.Should()
            .Equal(new Point(60, 10), new Point(90, 40));
    }

    [Test]
    public void ASingleTriangleWithCoincidentEndpointsIsOnePoint()
    {
        var path = new List<Point>();

        Funnel.Pull(
            TriangleMeshTests.Square(),
            [0],
            new Point(60, 10),
            new Point(60, 10),
            path);

        path.Should()
            .Equal(new Point(60, 10));
    }

    [Test]
    public void AStraightCorridorIsOneLeg()
    {
        var mesh = TriangleMeshTests.Square();
        var path = new List<Point>();

        Funnel.Pull(
            mesh,
            [
                1,
                0
            ],
            new Point(10, 50),
            new Point(90, 50),
            path);

        path.Should()
            .Equal(new Point(10, 50), new Point(90, 50));
    }

    [Test]
    public void AnLShapedCorridorBendsAtTheInnerCorner()
    {
        var path = new List<Point>();

        Funnel.Pull(
            LShape(),
            [
                0,
                1,
                2,
                3
            ],
            new Point(10, 10),
            new Point(45, 60),
            path);

        path.Should()
            .Equal(new Point(10, 10), new Point(20, 50), new Point(45, 60));
    }

    /// <summary>
    ///     An L: the vertical bar x 0..20, y 0..70 and the horizontal bar x 0..50, y 50..70. Four triangles in a chain, T0 at
    ///     the bottom of the bar, T3 at the end of the arm. The straight line from (10, 10) to (45, 60) leaves the L, so the
    ///     pull has to bend at the inner corner (20, 50).
    /// </summary>
    private static TriangleMesh LShape()
        => new(
            [
                new Point(0, 0),
                new Point(20, 0),
                new Point(20, 50),
                new Point(50, 50),
                new Point(50, 70),
                new Point(0, 70)
            ],
            [
                0,
                1,
                2,
                0,
                2,
                5,
                2,
                4,
                5,
                2,
                3,
                4
            ],
            [
                -1,
                1,
                -1,
                2,
                -1,
                0,
                -1,
                1,
                3,
                -1,
                2,
                -1
            ]);

    [Test]
    public void TheSameCorridorWalkedBackwardsBendsAtTheSameCorner()
    {
        var path = new List<Point>();

        Funnel.Pull(
            LShape(),
            [
                3,
                2,
                1,
                0
            ],
            new Point(45, 60),
            new Point(10, 10),
            path);

        path.Should()
            .Equal(new Point(45, 60), new Point(20, 50), new Point(10, 10));
    }
}