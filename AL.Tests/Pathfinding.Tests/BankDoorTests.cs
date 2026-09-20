#region
using AL.Core.Geometry;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     The bank door on main opens from far enough along the street that a round trip through it, at the flat door price,
///     undercut walking past. These hold the price that stops that without stopping a real trip into the bank.
/// </summary>
public class BankDoorTests : PathfindingTestBed
{
    [Test]
    public async Task ATripIntoTheBankStillTakesTheDoor()
    {
        var spawn = BankDoorSpawn();
        var start = new Location("main", spawn.X + 150f, spawn.Y);

        var path = await Pathfinder.FindPathAsync(start, [new Destination(new Location("bank", 0, -100), 0)], PathOptions.NoTown)
                                   .ToArrayAsync();

        path.Should()
            .ContainSingle(edge => edge.Type == EdgeType.Door)
            .Which
            .Cost
            .Should()
            .Be(CONSTANTS.BANK_DOOR_COST);
    }

    private static GSpawn BankDoorSpawn()
    {
        var main = GameData.Maps["main"]!;
        var door = main.Doors.First(d => d.DestinationMap.Equals("bank", StringComparison.OrdinalIgnoreCase));

        return main.Spawns[(int)door.CurrentMapSpawnId];
    }

    /// <summary>
    ///     Starts and ends on opposite sides of the door, along the street and from below it - the placements that bounced at
    ///     the flat price. None of them should use a door at all.
    /// </summary>
    [Test]
    [Arguments(1f, 0f, 150f)]
    [Arguments(-1f, 0f, 150f)]
    [Arguments(1f, 0.5f, 150f)]
    [Arguments(-1f, 0.5f, 150f)]
    [Arguments(1f, 0.5f, 200f)]
    [Arguments(-1f, 0.5f, 200f)]
    public async Task WalkingPastTheBankStaysOnTheStreet(float dx, float dy, float reach)
    {
        var spawn = BankDoorSpawn();
        var start = new Location("main", spawn.X + dx * reach, spawn.Y + dy * reach);
        var end = new Location("main", spawn.X - dx * reach, spawn.Y + dy * reach);

        var path = await Pathfinder.FindPathAsync(start, [new Destination(end, 0)], PathOptions.NoTown)
                                   .ToArrayAsync();

        path.Should()
            .OnlyContain(edge => edge.Type == EdgeType.Walk, string.Join(" | ", path));
    }
}