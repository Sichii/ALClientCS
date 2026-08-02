#region
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.SocketClient.Definitions;
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

    /// <summary>
    ///     The three edges the stale-frame position repair leans on. It reckons where a leg has got to as
    ///     <c>start.OffsetTowards(destination, speed * elapsed)</c>, so an overshoot would put the character past the
    ///     corner and a divide-by-nothing would put it at NaN - both of which are read as truth by every range check
    ///     until the leg ends.
    /// </summary>
    [Test]
    public void OffsetTowardsClampsAtTheDestination()
    {
        var start = new Point(100, -1000);
        var destination = new Point(176.54f, -1000);

        //further than the leg is long - a repair running late must land on the corner, never past it
        var overshot = start.OffsetTowards(destination, 500f);

        overshot.X
                .IsNear(destination.X, CONSTANTS.EPSILON)
                .Should()
                .BeTrue();

        //a speed of zero is a character that has not moved, not one that has arrived
        var stationary = start.OffsetTowards(destination, 0f);

        stationary.X
                  .IsNear(start.X, CONSTANTS.EPSILON)
                  .Should()
                  .BeTrue();

        //a zero length leg normalises by zero if it is not special cased
        var degenerate = start.OffsetTowards(start, 25f);

        float.IsNaN(degenerate.X)
             .Should()
             .BeFalse();

        degenerate.X
                  .IsNear(start.X, CONSTANTS.EPSILON)
                  .Should()
                  .BeTrue();
    }

    /// <summary>
    ///     The repair hands <see cref="EntityBase" /> a position the 30Hz simulation did not produce, so the closed
    ///     form has to agree with the stepwise one. A disagreement here is the repair teleporting the character
    ///     somewhere the client would never have walked, which reads as a desync rather than as a fix.
    /// </summary>
    [Test]
    public void DeadReckonAgreesWithTheSimulatedWalk()
    {
        const float SPEED = 69f;
        const float TOLERANCE = 0.1f;

        //the circle component's real leg: a 76.5px chord, radius 100 over eight sides
        var start = new Point(100, -1000);
        var destination = new Point(176.54f, -1000);

        var character = new Character();
        character.BackfillSoftDefault(EntityUpdateField.Speed, SPEED);
        character.UpdateLocation(start);
        character.SetMoving(destination);

        //ALClientSettings.PositionPollingRate - the loop the repair has to agree with
        var step = TimeSpan.FromMilliseconds(1000d / 30d);
        var elapsed = TimeSpan.Zero;

        //short of the 33 steps the leg takes, so the comparison never straddles Update's arrival snap
        for (var tick = 0; tick < 20; tick++)
        {
            character.Update(step);
            elapsed += step;

            var reckoned = start.OffsetTowards(destination, (float)(SPEED / 1000d * elapsed.TotalMilliseconds));

            //looser than EPSILON on purpose: the simulation re-offsets from its own last position every tick, so it
            //accumulates float error the closed form does not. A tenth of a unit is well inside anything that reads
            //this position and far tighter than the drift the repair exists to undo
            character.X
                     .IsNear(reckoned.X, TOLERANCE)
                     .Should()
                     .BeTrue();

            character.Y
                     .IsNear(reckoned.Y, TOLERANCE)
                     .Should()
                     .BeTrue();
        }
    }

    /// <summary>
    ///     The server resolves every attack, skill and aggro check with the gap on each axis independently, each
    ///     clamped at zero. An axis the two boxes already overlap on contributes nothing, so two entities standing
    ///     alongside each other are exactly their horizontal gap apart however tall either of them is.
    /// </summary>
    [Test]
    public void EdgeToEdgeDistanceIsPerAxisSeparation()
    {
        //two boxes of different heights, side by side, overlapping vertically. Ten units of clear air between them
        var left = new BoundingRectangle(0, 0, 5, 35, 0);
        var right = new BoundingRectangle(20, 0, 5, 20, 0);

        left.EdgeToEdgeDistance(right)
            .Should()
            .BeApproximately(10f, 0.001f);

        //and it does not care which way round it is asked
        right.EdgeToEdgeDistance(left)
             .Should()
             .BeApproximately(10f, 0.001f);

        //pulling one box vertically while the two still overlap on that axis changes nothing at all - the reading
        //stays the horizontal gap rather than growing toward the nearest corner
        var raised = new BoundingRectangle(20, -10, 5, 20, 0);

        left.EdgeToEdgeDistance(raised)
            .Should()
            .BeApproximately(10f, 0.001f);
    }

    /// <summary>
    ///     Separated on both axes it is the diagonal of the two gaps, and any overlap at all reads as zero rather
    ///     than going negative.
    /// </summary>
    [Test]
    public void EdgeToEdgeDistanceIsDiagonalWhenApartOnBothAxes()
    {
        //spans x -5..5 and y -10..0
        var lower = new BoundingRectangle(0, 0, 5, 10, 0);

        //spans x 9..19 and y -23..-13, so four units clear across and three clear up
        var upper = new BoundingRectangle(14, -13, 5, 10, 0);

        lower.EdgeToEdgeDistance(upper)
             .Should()
             .BeApproximately(5f, 0.001f);

        var overlapping = new BoundingRectangle(3, 0, 5, 10, 0);

        lower.EdgeToEdgeDistance(overlapping)
             .Should()
             .Be(0f);
    }

    /// <summary>
    ///     The reading a melee character actually depends on. The boxes are bottom-anchored and rise from the
    ///     entity's feet, so alongside each other the half-widths are the whole of the difference: a character and a
    ///     mole standing 43 apart are 15 apart by the measure the server grants the mole its attack with.
    /// </summary>
    [Test]
    public void EdgeToEdgeDistanceMatchesTheServerForACharacterAndAMonster()
    {
        //a player is 26 wide and 36 tall; a mole is 30 by 20
        var character = new BoundingRectangle(0, 0, 13, 36, 0);
        var mole = new BoundingRectangle(43, 0, 15, 20, 0);

        character.EdgeToEdgeDistance(mole)
                 .Should()
                 .BeApproximately(15f, 0.001f);

        //so the 28 units of hit box between them is exactly what a centre-to-centre reading would have charged the
        //character for, and is the whole reason a warrior reaching 18 can hit something standing 43 away
        character.Distance(mole)
                 .Should()
                 .BeApproximately(43f, 0.001f);
    }

    /// <summary>
    ///     The point-to-box reading follows the same rule: an axis the point already lies within is not part of the
    ///     distance.
    /// </summary>
    [Test]
    public void EdgeToCenterDistanceIsPerAxisSeparation()
    {
        var rect = new BoundingRectangle(0, 0, 10, 20, 0);

        //directly alongside, inside the box's vertical span
        rect.EdgeToCenterDistance(new Point(25, -5))
            .Should()
            .BeApproximately(15f, 0.001f);

        //inside it entirely
        rect.EdgeToCenterDistance(new Point(0, -10))
            .Should()
            .Be(0f);

        //clear of both axes, so the diagonal of the two gaps
        rect.EdgeToCenterDistance(new Point(13, 4))
            .Should()
            .BeApproximately(5f, 0.001f);
    }
}
