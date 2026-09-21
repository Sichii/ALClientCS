#region
using AL.Data;
using AL.Data.Maps;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     Scenery collision, which the game folds into a map's wall lines while it processes the map rather than shipping in
///     <c>G.geometry</c> . Nothing else in this client turns a box into lines, so an axis swapped here is a hole in the
///     nav mesh that only shows up as the server correcting a character mid-walk.
/// </summary>
public class AnimatableCollisionTests : GameDataTestBed
{
    /// <summary>
    ///     The table binds off the wire at all, which the boxes above cannot show on their own. The frozen snapshot carries
    ///     two pieces of scenery and neither blocks anything, so the fold is a no-op there - the binding is the half worth
    ///     pinning, since a key that stopped binding would look exactly like a map with no scenery.
    /// </summary>
    [Test]
    public void TheSceneryTableBindsFromTheWire()
    {
        GameData.Maps["main"]!.Animatables
                .Should()
                .ContainKey("the_door");

        GameData.Maps["main"]!.Animatables["the_door"]
                .X
                .Should()
                .Be(888);

        GameData.Maps["main"]!.Animatables["the_door"]
                .Collision
                .Should()
                .BeNull();
    }

    /// <summary>
    ///     The dungeon gate on <c>main</c> , the one piece of scenery in the live table that blocks anything. Its two pillars
    ///     are checked against coordinates worked from the wire values rather than from this code, so the two have to agree
    ///     independently.
    /// </summary>
    [Test]
    public void TheDungeonGateOnMainBecomesTwoPillars()
    {
        var gate = new GAnimatable
        {
            X = 816,
            Y = 1160,
            Collision =
            [
                [
                    -52,
                    8,
                    -28,
                    20
                ],
                [
                    28,
                    8,
                    52,
                    20
                ]
            ]
        };

        var lines = gate.CollisionLines()
                        .ToList();

        lines.Should()
             .HaveCount(8);

        //the left pillar spans x 764-788 and y 1168-1180
        lines.Where(line => line.IsVertical)
             .Select(line => (line.On, line.Start, line.End))
             .Should()
             .BeEquivalentTo(
                 new[]
                 {
                     (764, 1168, 1180),
                     (788, 1168, 1180),
                     (844, 1168, 1180),
                     (868, 1168, 1180)
                 });

        lines.Where(line => !line.IsVertical)
             .Select(line => (line.On, line.Start, line.End))
             .Should()
             .BeEquivalentTo(
                 new[]
                 {
                     (1168, 764, 788),
                     (1180, 764, 788),
                     (1168, 844, 868),
                     (1180, 844, 868)
                 });
    }

    /// <summary>
    ///     Scenery that blocks nothing is nearly all of it, and a box the server sent short is the shape a guess would turn
    ///     into a wall in the wrong place.
    /// </summary>
    [Test]
    public void SceneryWithoutAWholeBoxContributesNothing()
    {
        new GAnimatable
            {
                X = 100,
                Y = 100
            }.CollisionLines()
             .Should()
             .BeEmpty();

        new GAnimatable
            {
                X = 100,
                Y = 100,
                Collision =
                [
                    [
                        1,
                        2,
                        3
                    ]
                ]
            }.CollisionLines()
             .Should()
             .BeEmpty();
    }
}