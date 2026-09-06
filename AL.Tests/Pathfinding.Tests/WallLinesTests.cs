#region
using AL.Core.Geometry;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     The server's line test restated on a room the tests draw themselves, so a wrong port and a wrong oracle cannot
///     agree by accident. The room is a 200 by 200 box with a vertical wall from (100, 50) to (100, 150).
/// </summary>
public class WallLinesTests
{
    /// <summary>
    ///     The character test is four corner tracks plus the two fence tracks along the box's leading edges at the
    ///     destination, so a move whose centre track is clear is still refused when a corner clips the wall.
    /// </summary>
    [Test]
    public void ACornerClippingTheWallRefusesTheMove()
    {
        var room = Room();

        //centre track ends at x 95, but the right corner (x + 8) ends on 103, across the wall
        room.CanMoveLine(
                50,
                100,
                95,
                100)
            .Should()
            .BeTrue();

        room.CanMove(
                50,
                100,
                95,
                100,
                CONSTANTS.DEFAULT_BOUNDING_BASE)
            .Should()
            .BeFalse();

        room.CanMove(
                50,
                100,
                90,
                100,
                CONSTANTS.DEFAULT_BOUNDING_BASE)
            .Should()
            .BeTrue();
    }

    [Test]
    public void AHorizontalWallIsCheckedTheSameWay()
    {
        var walls = new WallLines(
            [],
            [
                new StraightLine(
                    100,
                    50,
                    150,
                    false)
            ]);

        walls.CanMoveLine(
                 100,
                 50,
                 100,
                 150)
             .Should()
             .BeFalse();

        walls.CanMoveLine(
                 25,
                 50,
                 25,
                 150)
             .Should()
             .BeTrue();
    }

    [Test]
    public void ATrackAcrossTheWallIsRefused()
        => Room()
           .CanMoveLine(
               50,
               100,
               150,
               100)
           .Should()
           .BeFalse();

    [Test]
    public void ATrackAlongTheColumnBelowTheWallIsAllowed()
        => Room()
           .CanMoveLine(
               100,
               10,
               100,
               40)
           .Should()
           .BeTrue();

    [Test]
    public void ATrackPastTheWallsEndIsAllowed()
        => Room()
           .CanMoveLine(
               50,
               25,
               150,
               25)
           .Should()
           .BeTrue();

    /// <summary>
    ///     The mirror of the clause above is not in the server, so a track sliding down the column across the whole wall is
    ///     allowed. Pinned so the port stays literal rather than sensible.
    /// </summary>
    [Test]
    public void ATrackSlidingDownTheColumnAcrossTheWholeWallIsAllowedAsTheServerAllowsIt()
        => Room()
           .CanMoveLine(
               100,
               160,
               100,
               40)
           .Should()
           .BeTrue();

    /// <summary>
    ///     The clause that has no crossing to compute: a track up the column from below the wall's start to past its end never
    ///     intersects it, and the server refuses it by name.
    /// </summary>
    [Test]
    public void ATrackSlidingUpTheColumnAcrossTheWholeWallIsRefused()
        => Room()
           .CanMoveLine(
               100,
               40,
               100,
               160)
           .Should()
           .BeFalse();

    /// <summary>
    ///     The server's second clause: a track that starts exactly on a line's column, below its start, and moves up past that
    ///     start is refused even though the crossing formula never fires for it.
    /// </summary>
    [Test]
    public void ATrackStartingOnTheColumnAndSlidingUpIntoTheWallIsRefused()
        => Room()
           .CanMoveLine(
               100,
               40,
               100,
               60)
           .Should()
           .BeFalse();

    [Test]
    public void ATrackThatStopsShortOfTheWallIsAllowed()
        => Room()
           .CanMoveLine(
               50,
               100,
               99,
               100)
           .Should()
           .BeTrue();

    [Test]
    public void BoxIntersectsIsTrueOnlyWhereALineCrossesTheBox()
    {
        var room = Room();

        //box spans x 92..108, y 93..102 around (100, 100): the wall column is inside it
        room.BoxIntersects(100, 100, CONSTANTS.DEFAULT_BOUNDING_BASE)
            .Should()
            .BeTrue();

        room.BoxIntersects(80, 100, CONSTANTS.DEFAULT_BOUNDING_BASE)
            .Should()
            .BeFalse();

        //below the wall's span
        room.BoxIntersects(100, 30, CONSTANTS.DEFAULT_BOUNDING_BASE)
            .Should()
            .BeFalse();
    }

    private static WallLines Room()
        => new(
            [
                new StraightLine(
                    0,
                    0,
                    200,
                    true),
                new StraightLine(
                    200,
                    0,
                    200,
                    true),
                new StraightLine(
                    100,
                    50,
                    150,
                    true)
            ],
            [
                new StraightLine(
                    0,
                    0,
                    200,
                    false),
                new StraightLine(
                    200,
                    0,
                    200,
                    false)
            ]);

    [Test]
    public void TheTestDoesNotAllocate()
    {
        var room = Room();

        //warm up
        room.CanMove(
            50,
            100,
            60,
            100,
            CONSTANTS.DEFAULT_BOUNDING_BASE);

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 10_000; i++)
            room.CanMove(
                50,
                100,
                60 + i % 30,
                100,
                CONSTANTS.DEFAULT_BOUNDING_BASE);

        (GC.GetAllocatedBytesForCurrentThread() - before).Should()
                                                         .Be(0);
    }
}