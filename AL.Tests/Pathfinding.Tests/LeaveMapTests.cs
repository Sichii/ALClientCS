#region
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Holds the ways off an irregular map to what the server actually accepts, restated here from its own source rather
///     than read from the graph the tests are checking.
/// </summary>
/// <remarks>
///     Two rules, and the code used to have neither. The leave command is taken from jail and cyberland only - the server
///     checks the map by name, plus solo instances, which only a gm can create - and it hands the character to the start
///     map's first spawn whatever doors the map also has. A door is a separate way out, walked to and used with transport,
///     and being irregular has nothing to do with whether a map has one.
/// </remarks>
public class LeaveMapTests : PathfindingTestBed
{
    //the far right of main, where a farm spot sits - far enough that a free recall would be worth planning around
    private static readonly Destination FAR_RIGHT_OF_MAIN = new(new Location("main", 1937f, -12f), 0f);

    private static readonly Location INSIDE_CYBERLAND = new("cyberland", 0f, 0f);

    //main's ninth spawn, where cyberland's door lands, and the transporter that goes back in stands beside it
    private static readonly Destination TRANSPORTER_ON_MAIN = new(new Location("main", -85f, -389f), 0f);

    private static readonly Location INSIDE_DUELLAND = new("duelland", 0f, 0f);
    private static readonly Location INSIDE_RESORT = new("resort", -8f, 91f);
    private static readonly Destination TAVERN_DOOR_LANDING = new(new Location("tavern", 272f, -200f), 0f);

    /// <summary>
    ///     Cyberland keeps both ways out, and the door is the shorter one when the trip ends near where it lands - a leave
    ///     costs the same from anywhere but arrives 400 units further from the transporter.
    /// </summary>
    [Test]
    public async Task CyberlandsDoorIsTakenWhenItLandsNearerThanTheLeave()
    {
        var path = await Pathfinder.FindPathAsync(INSIDE_CYBERLAND, [TRANSPORTER_ON_MAIN])
                                   .ToArrayAsync();

        path.Should()
            .Contain(edge => edge.Type == EdgeType.Door);

        path.Should()
            .NotContain(edge => edge.Type == EdgeType.Leave);
    }

    /// <summary>
    ///     Duelland is an instance but not a solo one, so the server refuses a leave from it the same way.
    /// </summary>
    [Test]
    public async Task DuellandLeavesThroughItsDoor()
    {
        var path = await Pathfinder.FindPathAsync(INSIDE_DUELLAND, [FAR_RIGHT_OF_MAIN])
                                   .ToArrayAsync();

        path.Should()
            .Contain(edge => edge.Type == EdgeType.Door);

        path.Should()
            .NotContain(edge => edge.Type == EdgeType.Leave);
    }

    /// <summary>
    ///     The character arrives on the town spawn already, so a recall there moves it nowhere and costs it three seconds of
    ///     channel it can be knocked out of.
    /// </summary>
    [Test]
    public async Task LeavingCyberlandForMainNeverRecalls()
    {
        var path = await Pathfinder.FindPathAsync(INSIDE_CYBERLAND, [FAR_RIGHT_OF_MAIN])
                                   .ToArrayAsync();

        path.Should()
            .NotContain(edge => edge.Type == EdgeType.Town);
    }

    [Test]
    public async Task LeavingCyberlandLandsOnMainsFirstSpawn()
    {
        var path = await Pathfinder.FindPathAsync(INSIDE_CYBERLAND, [FAR_RIGHT_OF_MAIN])
                                   .ToArrayAsync();

        var leaveEdge = path.Should()
                            .ContainSingle(edge => edge.Type == EdgeType.Leave)
                            .Subject;

        var mainSpawn = GameData.Maps.Main.Spawns[0];

        leaveEdge.End
                 .X
                 .Should()
                 .Be(mainSpawn.X);

        leaveEdge.End
                 .Y
                 .Should()
                 .Be(mainSpawn.Y);
    }

    /// <summary>
    ///     The server's list, written out from its handler rather than read from the code under test. Being irregular is not
    ///     what decides it - duelland and resort are irregular and refuse the command.
    /// </summary>
    [Test]
    [Arguments("jail", true)]
    [Arguments("cyberland", true)]
    [Arguments("duelland", false)]
    [Arguments("resort", false)]
    [Arguments("test", false)]
    [Arguments("main", false)]
    public void OnlyJailAndCyberlandTakeALeave(string map, bool accepted)
        => CONSTANTS.AcceptsLeave(map)
                    .Should()
                    .Be(accepted);

    /// <summary>
    ///     Resort is irregular and the server refuses a leave from it, so its door to the tavern is the only way out it has.
    ///     Skipping the doors over irregular maps left it with none at all.
    /// </summary>
    [Test]
    public async Task ResortLeavesThroughItsDoor()
    {
        var path = await Pathfinder.FindPathAsync(INSIDE_RESORT, [TAVERN_DOOR_LANDING])
                                   .ToArrayAsync();

        path.Should()
            .Contain(edge => edge.Type == EdgeType.Door);

        path.Should()
            .NotContain(edge => edge.Type == EdgeType.Leave);
    }

    /// <summary>
    ///     With recall off the table entirely there has to still be a path, which is what fails when the arrival node has no
    ///     walk out of it. A recall that gets interrupted is retried exactly this way.
    /// </summary>
    [Test]
    public async Task TheTripOffCyberlandStandsWithoutRecallAtAll()
    {
        var path = await Pathfinder.FindPathAsync(INSIDE_CYBERLAND, [FAR_RIGHT_OF_MAIN], false)
                                   .ToArrayAsync();

        path.Should()
            .NotBeEmpty();

        path.Should()
            .NotContain(edge => edge.Type == EdgeType.Town, "recall off suppresses every recall on the route");

        path.Last()
            .End
            .Map
            .Should()
            .Be("main");
    }
}