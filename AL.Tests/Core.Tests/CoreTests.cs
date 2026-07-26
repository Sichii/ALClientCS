#region
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.SocketClient.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Core.Tests;

public class CoreTests
{
    [Test]
    public void InstancedDistanceTest()
    {
        var instancedLoc1 = new PartyMember
        {
            X = 5,
            Y = 5,
            In = "beep"
        };

        var instancedLoc2 = new PartyMember
        {
            X = 10,
            Y = 10
        };

        var distance = instancedLoc1.DistanceWithInstanceCheck(instancedLoc2);

        (distance < 10).Should()
                       .BeTrue();
    }

    [Test]
    public void OffsetTest()
    {
        var point1 = new Point(500, 250);
        var point2 = new Point(-500, -250);
        var distance = point1.Distance(point2);

        var relation = point1.AngularRelationTo(point2);
        var offset = point2.AngularOffset(relation, distance / 2);

        offset.X
              .IsNear(0f, CONSTANTS.EPSILON)
              .Should()
              .BeTrue();

        offset.Y
              .IsNear(0f, CONSTANTS.EPSILON)
              .Should()
              .BeTrue();

        relation = point2.AngularRelationTo(point1);
        offset = point1.AngularOffset(relation, distance / 2);

        offset.X
              .IsNear(0f, CONSTANTS.EPSILON)
              .Should()
              .BeTrue();

        offset.Y
              .IsNear(0f, CONSTANTS.EPSILON)
              .Should()
              .BeTrue();
    }

    [Test]
    public void PointComparerTest()
    {
        var circle = new Circle(10, 15, 3);
        var location = new Location("foo", 10, 15);

        circle.Equals(location)
              .Should()
              .BeTrue();
    }
}