#region
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Holds the door reach model against the server's own rule, restated here rather than shared with the code
///     under test - a probe that calls the thing it is checking agrees with itself while the character stands
///     outside the door wondering why it will not open.
/// </summary>
public class DoorReachTests : PathfindingTestBed
{
    //the server's own limit, spelled out rather than read from CONSTANTS.DOOR_RANGE, which carries a safety shave
    private const float SERVER_DOOR_DIST = 112f;

    /// <summary>
    ///     How far inside the limit a stop point has to be, rather than merely inside it.
    /// </summary>
    /// <remarks>
    ///     Being inside is not enough, and a suite that only checked that stayed green while every door in the game
    ///     refused to open. The walk stops on the edge of a reach circle, and the server has the character a few
    ///     units behind where the client reckons it by the time the emit lands, so a model that clears the limit by
    ///     nothing clears it by nothing in practice either. Just under the 5.6 units
    ///     <c>CONSTANTS.RANGE_SHAVE</c> leaves on a door, so returning that shave to the 2.5% it replaced
    ///     fails here.
    /// </remarks>
    private const float REQUIRED_MARGIN = 5f;

    //a character's sprite box on the server. restated here for the same reason as the limit above - a test that
    //reads CONSTANTS would agree with it whatever it said
    private const float SERVER_CHARACTER_WIDTH = 26f;
    private const float SERVER_CHARACTER_HEIGHT = 36f;

    /// <summary>
    ///     The server's door check, written out from its own source. A box the size of the door is placed on the
    ///     spawn the door's entry names - not on the door - and measured against the character's box. Both boxes
    ///     hang upward from their positions. The gap is taken per axis, clamped at zero, and combined.
    /// </summary>
    private static float ServerDoorDistance(GMap map, GDoor door, IPoint standing)
    {
        var spawn = map.Spawns[(int)door.CurrentMapSpawnId];

        var doorLeft = spawn.X - door.Width / 2;
        var doorRight = spawn.X + door.Width / 2;
        var doorTop = spawn.Y - door.Height;
        var doorBottom = spawn.Y;

        var standingLeft = standing.X - SERVER_CHARACTER_WIDTH / 2;
        var standingRight = standing.X + SERVER_CHARACTER_WIDTH / 2;
        var standingTop = standing.Y - SERVER_CHARACTER_HEIGHT;
        var standingBottom = standing.Y;

        var gapX = MathF.Max(MathF.Max(standingLeft - doorRight, doorLeft - standingRight), 0f);
        var gapY = MathF.Max(MathF.Max(standingTop - doorBottom, doorTop - standingBottom), 0f);

        return MathF.Sqrt(gapX * gapX + gapY * gapY);
    }

    private static bool SpawnIsResolvable(GMap map, GDoor door)
    {
        var spawnId = (int)door.CurrentMapSpawnId;

        return (spawnId >= 0) && (spawnId < map.Spawns.Count);
    }

    private static IEnumerable<(GMap Map, GDoor Door, Exit Exit)> EveryDoor()
    {
        foreach (var map in GameData.Maps.Values.DistinctBy(m => m.Accessor))
        {
            if (map.Ignore)
                continue;

            foreach (var door in map.Doors)
            {
                var exit = map.Exits.FirstOrDefault(e => (e.Type == ExitType.Door)
                                                         && IPoint.Comparer.Equals(e, door)
                                                         && e.ToLocation.Map.Equals(door.DestinationMap, StringComparison.OrdinalIgnoreCase));

                if (exit != null)
                    yield return (map, door, exit);
            }
        }
    }

    /// <summary>
    ///     Every circle we would stop on has to be a place the server accepts, everywhere on it - not just at its
    ///     centre. A circle that pokes outside the real region would let the pathfinder stand the character down
    ///     somewhere the door refuses to open, which reads as a hung path rather than an error.
    /// </summary>
    [Test]
    public async Task EveryReachCircleIsWhollyInsideTheServersDoorRange()
    {
        var checkedDoors = 0;

        foreach ((var map, var door, var exit) in EveryDoor())
        {
            checkedDoors++;

            //a door whose spawn cannot be resolved has no derived region, and must claim no reach at all - a
            //radius here turns EnhancePathAsync's shortcut back on against a region we could not work out
            if (!SpawnIsResolvable(map, door))
            {
                exit.Radius
                    .Should()
                    .Be(0f, $"the door {map.Accessor} => {door.DestinationMap} names a spawn that does not exist");

                continue;
            }

            foreach (var circle in exit.ReachableFrom.Append(new Circle(exit, exit.Radius)))
                for (var step = 0; step < 72; step++)
                {
                    var angle = step * MathF.Tau / 72;
                    var edgeOfCircle = new Point(circle.X + (circle.Radius * MathF.Cos(angle)), circle.Y + (circle.Radius * MathF.Sin(angle)));

                    ServerDoorDistance(map, door, edgeOfCircle)
                        .Should()
                        .BeLessThan(
                            SERVER_DOOR_DIST - REQUIRED_MARGIN,
                            $"the door {map.Accessor} => {door.DestinationMap} must open from every point of a circle we would stop on");
                }
        }

        checkedDoors.Should()
                    .BeGreaterThan(50, "the door table should not have emptied out");

        await Task.CompletedTask;
    }

    /// <summary>
    ///     The reach model is only worth having if the pathfinder actually uses it, so this walks a real route and
    ///     checks the point the character is left standing on before each door.
    /// </summary>
    [Test]
    public async Task APathStopsSomewhereTheDoorActuallyOpens()
    {
        var path = await Pathfinder.FindPathAsync(new Location("main", -1582, 496), [new Destination(new Location("main", 1891, -47), 0)])
                                   .ToArrayAsync();

        var doorsWalked = 0;

        for (var i = 1; i < path.Length; i++)
        {
            if (path[i].Type != EdgeType.Door)
                continue;

            var exit = (Exit)path[i].Start.Vertex;
            var standing = path[i - 1].End.Vertex;

            (var map, var door, _) = EveryDoor()
                .Single(d => SpawnIsResolvable(d.Map, d.Door)
                             && d.Map.Accessor.Equals(exit.Map, StringComparison.OrdinalIgnoreCase)
                             && IPoint.Comparer.Equals(d.Exit, exit)
                             && d.Door.DestinationMap.Equals(exit.ToLocation.Map, StringComparison.OrdinalIgnoreCase));

            doorsWalked++;

            ServerDoorDistance(map, door, standing)
                .Should()
                .BeLessThan(
                    SERVER_DOOR_DIST - REQUIRED_MARGIN,
                    $"the path stops at {standing} to use the door {map.Accessor} => {door.DestinationMap}");
        }

        doorsWalked.Should()
                   .BeGreaterThan(0, "this route is chosen because it goes through doors");
    }
}
