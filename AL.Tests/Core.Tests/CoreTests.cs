using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using Chaos.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Core.Tests
{
    [TestClass]
    public class CoreTests
    {
        [TestMethod]
        public void OffsetTest()
        {
            var point1 = new Point(500, 250);
            var point2 = new Point(-500, -250);
            var distance = point1.Distance(point2);

            var relation = point1.AngularRelationTo(point2);
            var offset = point2.AngularOffset(relation, distance / 2);

            Assert.IsTrue(offset.X.NearlyEquals(0f, CONSTANTS.EPSILON));
            Assert.IsTrue(offset.Y.NearlyEquals(0f, CONSTANTS.EPSILON));

            relation = point2.AngularRelationTo(point1);
            offset = point1.AngularOffset(relation, distance / 2);

            Assert.IsTrue(offset.X.NearlyEquals(0f, CONSTANTS.EPSILON));
            Assert.IsTrue(offset.Y.NearlyEquals(0f, CONSTANTS.EPSILON));
        }

        [TestMethod]
        public void PointComparerTest()
        {
            var circle = new Circle(10, 15, 3);
            var location = new Location("foo", 10, 15);

            Assert.IsTrue(circle.Equals(location));
        }
    }
}