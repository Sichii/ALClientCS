#region
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
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
    ///     Several paths write an entity's position between ticks without re-deriving <see cref="EntityBase.Angle" />
    ///     - the server's correction handler, the in-leg repair, the character branch of an entities frame. A step
    ///     that steered by the stored heading would resume from the corrected position along the old line, walking
    ///     parallel to the one it should be on until the overshoot clamp finally tripped. Pinned here alongside the
    ///     two edges the derived heading still has to keep: a step short of the goal, and one that would pass it.
    /// </summary>
    [Test]
    public void AStepHeadsAtGoingFromWhereverThePositionWasWritten()
    {
        const float SPEED = 100f;
        const float TOLERANCE = 0.01f;

        //ten units of ground per step
        var step = TimeSpan.FromSeconds(0.1);

        var character = new Character();
        character.BackfillSoftDefault(EntityUpdateField.Speed, SPEED);
        character.UpdateLocation(new Point(200, -300));
        character.SetMoving(new Point(300, -300));

        //an ordinary step: due east, ten of the hundred units
        character.Update(step);

        character.X
                 .Should()
                 .BeApproximately(210f, TOLERANCE);

        character.Y
                 .Should()
                 .BeApproximately(-300f, TOLERANCE);

        //what a correction does - a position off the line the leg started on, with Angle left pointing due east
        character.UpdateLocation(new Point(210, -250));
        character.Update(step);

        //90 across and 50 up is a 102.96 unit leg, so ten units of it is 8.74 across and 4.86 up. Steering by the
        //stored heading would instead hold Y at -250 and put X at 220
        character.X
                 .Should()
                 .BeApproximately(218.74f, TOLERANCE);

        character.Y
                 .Should()
                 .BeApproximately(-254.86f, TOLERANCE);

        //arrival is unchanged: a step that would pass going snaps to it and stops
        character.UpdateLocation(new Point(295, -300));
        character.Update(step);

        character.X
                 .Should()
                 .BeApproximately(300f, TOLERANCE);

        character.Y
                 .Should()
                 .BeApproximately(-300f, TOLERANCE);

        character.Moving
                 .Should()
                 .BeFalse();
    }

    /// <summary>
    ///     An entity's movement is written by three threads that never coordinated: the 30Hz reckoning loop, the
    ///     socket's receive callbacks, and the consumer's own movement calls. Each write is a read-compute-write, so a
    ///     write landing inside another one used to be clobbered by a value derived from the state it replaced.
    /// </summary>
    /// <remarks>
    ///     The invariant is the group, not any single value: only two writers ever clear
    ///     <see cref="EntityBase.Moving" />, and both set the destination and the position together, so a stopped
    ///     entity standing anywhere other than its destination is a write that landed in halves. Run against the
    ///     unsynchronized version this fails within a few hundred rounds; run against the locked one the stopped state
    ///     is stable, because the reckoning loop does nothing at all while <see cref="EntityBase.Moving" /> is false.
    /// </remarks>
    [Test]
    public async Task AMovementWriteLandsAsAWholeOrNotAtAll()
    {
        const int ROUNDS = 20000;
        const float SPEED = 45f;

        var start = new Point(0, -1000);

        //far enough that the walk never arrives, so every round races a mid-leg step rather than an arrival
        var destination = new Point(100000, -1000);

        var character = new Character();
        character.BackfillSoftDefault(EntityUpdateField.Speed, SPEED);
        character.UpdateLocation(start);

        var step = TimeSpan.FromMilliseconds(1000d / 30d);
        var stopRequested = false;

        //held as text so a failure names the halves that disagreed rather than just the type
        string? violation = null;

        var reckoner = Task.Run(
            () =>
            {
                while (!Volatile.Read(ref stopRequested))
                    character.Update(step);
            });

        var mover = Task.Run(
            () =>
            {
                //seeded so a failure reproduces: the spin is what slides the stop across the step's compute window
                var random = new Random(20260809);

                //in a finally: the reckoner spins until this is set, so a throw anywhere above would hang
                //Task.WhenAll forever, and a hung suite is what gets a test deleted rather than fixed
                try
                {
                    for (var round = 0; round < ROUNDS; round++)
                    {
                        character.UpdateLocation(start);
                        character.SetMoving(destination);
                        Thread.SpinWait(random.Next(0, 400));
                        character.StopMoving();

                        var movement = character.Movement;

                        if (movement.Moving)
                            continue;

                        if (movement.X.IsNear(movement.GoingX, CONSTANTS.EPSILON)
                            && movement.Y.IsNear(movement.GoingY, CONSTANTS.EPSILON))
                            continue;

                        violation = $"round {round}: stopped at ({movement.X:N2}, {movement.Y:N2}) "
                                    + $"but going to ({movement.GoingX:N2}, {movement.GoingY:N2})";

                        break;
                    }
                } finally
                {
                    Volatile.Write(ref stopRequested, true);
                }
            });

        await Task.WhenAll(reckoner, mover);

        violation.Should()
                 .BeNull(
                     "a stopped entity stands on its destination - a position that disagrees with it is a step "
                     + "that was computed before a stop and written after it");
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

    /// <summary>
    ///     MeshBase.CanMove truncates each traced point to index the point map, which is only the same answer as
    ///     rounding while every point lands on a whole number. Nothing about the signature says so, and a fractional
    ///     point would move a wall by one cell rather than fail, so it is pinned here.
    /// </summary>
    [Test]
    public void RayTraceYieldsWholeNumbersFromFractionalEnds()
    {
        //fractional on both ends, both signs, and a few slopes - including the two degenerate axis-aligned cases
        IPoint[] starts = [new Point(0.4f, 0.6f), new Point(-7.25f, 3.75f), new Point(12.5f, -12.5f)];
        IPoint[] ends = [new Point(9.9f, -4.1f), new Point(-7.25f, 20.5f), new Point(30.5f, -12.5f)];

        foreach (var start in starts)
            foreach (var end in ends)
                foreach (var traced in start.RayTraceTo(end))
                {
                    traced.X
                          .Should()
                          .Be(MathF.Truncate(traced.X), $"the trace from {start} to {end} has to land on whole numbers");

                    traced.Y
                          .Should()
                          .Be(MathF.Truncate(traced.Y), $"the trace from {start} to {end} has to land on whole numbers");
                }
    }
}
