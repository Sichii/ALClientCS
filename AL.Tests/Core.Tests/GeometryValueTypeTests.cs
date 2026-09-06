#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using FluentAssertions;
#endregion

namespace AL.Tests.Core.Tests;

public class GeometryValueTypeTests
{
    [Test]
    public void ValuePointRoundTripsThroughPoint()
    {
        var point = new Point(1, 2);
        ValuePoint value = point;
        Point back = value;

        back.Should().Be(point);
        value.X.Should().Be(1);
        value.Y.Should().Be(2);
        value.Equals((IPoint)point).Should().BeTrue();
    }

    [Test]
    public void ValueLocationKeepsMapAndComparesMapCaseInsensitively()
    {
        var location = new Location("main", 3, 4);
        ValueLocation value = location;

        value.Map.Should().Be("main");
        value.X.Should().Be(3);
        value.Y.Should().Be(4);
        value.Equals((ILocation)new Location("MAIN", 3, 4)).Should().BeTrue();
        value.Equals((ILocation)new Location("cave", 3, 4)).Should().BeFalse();
    }

    [Test]
    public void ValueCircleCarriesRadius()
    {
        var circle = new Circle(1, 1, 7);
        ValueCircle value = circle;

        value.Radius.Should().Be(7);
        value.Equals((ICircle)circle).Should().BeTrue();
        value.Equals((ICircle)new Circle(1, 1, 8)).Should().BeFalse();
    }

    [Test]
    public void ValueRectangleMatchesRectangleEdgesAndVertices()
    {
        var rectangle = new Rectangle(0, 0, 10, 20);
        ValueRectangle value = rectangle;

        value.Left.Should().Be(-5);
        value.Right.Should().Be(5);
        value.Top.Should().Be(-10);
        value.Bottom.Should().Be(10);
        value.Width.Should().Be(10);
        value.Height.Should().Be(20);

        var vertices = value.Vertices;
        vertices.Should().HaveCount(4);
        vertices[0].Should().Be(new Point(-5, -10));
        vertices[1].Should().Be(new Point(5, -10));
        vertices[2].Should().Be(new Point(5, 10));
        vertices[3].Should().Be(new Point(-5, 10));
    }

    [Test]
    public void HypotIsTheHypotenuse()
    {
        MathEx.Hypot(3, 4).Should().Be(5);
        MathEx.Hypot(0, 0).Should().Be(0);
    }

    [Test]
    public void ValueLocationWithNullMapDoesNotThrow()
    {
        var value = default(ValueLocation);

        value.Equals((ILocation)new Location("main", 0, 0)).Should().BeFalse();

        //a ref struct local cannot be captured in a lambda, so this calls ToString directly - an unhandled throw fails the test just the same
        value.ToString().Should().NotBeNull();
    }

    [Test]
    public void GenericDistanceWorksOnRefStructs()
    {
        var a = new ValuePoint(0, 0);
        var b = new ValuePoint(3, 4);

        a.Distance(b).Should().Be(5);
        a.FastDistance(b).Should().Be(25);
        a.MidPoint(b).Should().Be(new Point(1.5f, 2));
        new Point(0, 0).Distance(new Location("main", 6, 8)).Should().Be(10);
    }

    [Test]
    public void RayTraceVisitsEveryCellTheLineCrosses()
    {
        new Point(0.5f, 0.5f).RayTraceTo(new Point(2.5f, 0.5f))
                             .ToArray()
                             .Should()
                             .Equal(new Point(0, 0), new Point(1, 0), new Point(2, 0));

        new Point(0.5f, 0.5f).RayTraceTo(new Point(0.5f, -1.5f))
                             .ToArray()
                             .Should()
                             .Equal(new Point(0, 0), new Point(0, -1), new Point(0, -2));

        new Point(0.5f, 0.5f).RayTraceTo(new Point(0.5f, 0.5f))
                             .ToArray()
                             .Should()
                             .Equal(new Point(0, 0));

        //a diagonal visits one cell per unit step on each axis, and never skips a corner
        var diagonal = new Point(0.5f, 0.5f).RayTraceTo(new Point(2.5f, 2.5f))
                                            .ToArray();

        diagonal.First().Should().Be(new Point(0, 0));
        diagonal.Last().Should().Be(new Point(2, 2));
        diagonal.Length.Should().Be(5);

        for (var i = 1; i < diagonal.Length; i++)
            (Math.Abs(diagonal[i].X - diagonal[i - 1].X) + Math.Abs(diagonal[i].Y - diagonal[i - 1].Y)).Should().Be(1);
    }

    [Test]
    public void RayTraceEnumeratesWithoutAllocating()
    {
        var start = new Point(-3.2f, 7.7f);
        var end = new Point(40.1f, -12.4f);
        var count = 0;

        var before = GC.GetAllocatedBytesForCurrentThread();

        foreach (var _ in start.RayTraceTo(end))
            count++;

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        count.Should().BeGreaterThan(40);
        allocated.Should().Be(0);
    }

    [Test]
    public void GenericLocationChecksWorkOnRefStructs()
    {
        var value = new ValueLocation("main", 0, 0);

        value.OnSameMapAs(new Location("MAIN", 5, 5)).Should().BeTrue();
        value.OnSameMapAs(new ValueLocation("cave", 5, 5)).Should().BeFalse();
        value.DistanceWithMapCheck(new Location("main", 3, 4)).Should().Be(5);
        value.DistanceWithMapCheck(new Location("cave", 3, 4)).Should().Be(float.MaxValue);
        value.ToLocation().Should().Be(new Location("main", 0, 0));
    }

    [Test]
    public void GenericEdgeDistancesWorkOnRefStructs()
    {
        var rectangle = new ValueRectangle(0, 0, 10, 20);

        rectangle.EdgeToCenterDistance(new ValuePoint(8, 0)).Should().Be(3);
        rectangle.EdgeToCenterDistance(new ValuePoint(0, 0)).Should().Be(0);
        rectangle.EdgeToCenterDistance(new Point(8, 14)).Should().Be(5);
        rectangle.EdgeToEdgeDistance(new ValueRectangle(20, 0, 10, 10)).Should().Be(10);
        rectangle.Intersects(new Rectangle(4, 0, 10, 10)).Should().BeTrue();
        rectangle.Intersects(new ValueRectangle(0, 0, 10, 20)).Should().BeTrue();
        rectangle.Intersects(new Rectangle(20, 0, 10, 10)).Should().BeFalse();
        rectangle.Intersects(new Rectangle(0, 30, 10, 10)).Should().BeFalse();

        var circle = new ValueCircle(0, 0, 5);

        circle.EdgeToCenterDistance(new ValuePoint(8, 0)).Should().Be(3);
        circle.EdgeToEdgeDistance(new Circle(12, 0, 2)).Should().Be(5);
        circle.Intersects(new ValueCircle(9, 0, 4)).Should().BeTrue();
        circle.Points(4).Should().NotBeEmpty();
        circle.GenerateCircumferencePoints(4).Should().HaveCount(4);
    }

    [Test]
    public void MapCheckOverloadsStillWinForClassReceivers()
    {
        //the design's one rule: OffsetTowards, AngularRelationTo and DirectionalRelationTo stay on the interfaces so a
        //class-typed receiver keeps its map check. moving them into the generic block would make these return a point
        new Location("main", 0, 0).OffsetTowards(new Location("cave", 10, 0), 5f)
                                  .Should()
                                  .Be(Point.None);

        new Location("main", 0, 0).OffsetTowards(new Location("main", 10, 0), 5f)
                                  .Should()
                                  .NotBe(Point.None);
    }
}
