#region
using AL.Core.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Holds the four key-locked dungeon doors out of the portal graph. A dungeon is entered with the <c>enter</c> emit and
///     a key; a transport through one of these doors answers <c>cant_enter</c> (node/server.js:5642).
/// </summary>
public class KeyDoorTests : PathfindingTestBed
{
    /// <summary>
    ///     The walk to a dungeon door stays on the map the door stands on. Each case is that map, its first spawn, and the
    ///     entry spawn - which is also where the dungeon's own exit door lands, hence the round trip the graph preferred.
    /// </summary>
    [Test]
    [Arguments("cave", 0f, 0f, -193.41f, -1295.83f)]
    [Arguments("winterland", 0f, 0f, 1063f, -2007f)]
    [Arguments("gateway", 0f, 0f, -320f, -202f)]
    [Arguments("mansion", 0f, -21f, -0.18f, -481.98f)]
    public async Task WalkingToADungeonEntranceNeverCrossesItsDoor(
        string map,
        float fromX,
        float fromY,
        float toX,
        float toY)
    {
        var path = await Pathfinder.FindPathAsync(
                                       new Location(map, fromX, fromY),
                                       [new Destination(new Location(map, toX, toY), 0f)],
                                       false)
                                   .ToArrayAsync();

        //the other way this fails: with the edge gone the entry spawn still has to be walkable
        path.Should()
            .NotBeEmpty();

        path.Should()
            .NotContain(edge => edge.Type == EdgeType.Door);
    }
}
