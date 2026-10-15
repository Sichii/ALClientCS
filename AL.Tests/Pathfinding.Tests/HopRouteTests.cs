#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class HopRouteTests : PathfindingTestBed
{
    private static readonly Location TOWN = new("winterland", 0f, 0f);
    private static readonly Location GOLEM_SPAWN = new("winterland", 864.5f, 429.5f);
    private static readonly Location LAKE_TAKEOFF = new("winterland", 721f, 261f);
    private static readonly Location COVE_START = new("winter_cove", -57f, -1317f);
    private static readonly Location COVE_END = new("winter_cove", 57f, -1184f);

    private static readonly Location WALL_WEST = new("winterland", 412f, -205f);
    private static readonly Location WALL_EAST = new("winterland", 497f, -155f);

    private static PortalGraph BuildGraph(string map, bool cheat)
        => new(
            new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase)
            {
                [map] = Pathfinder.GetNavMesh(map)!
            },
            cheat);

    private static IReadOnlyList<PathEdge> FindGolemPath(bool cheat, float? speed)
        => BuildGraph("winterland", cheat)
            .FindPath(
                TOWN,
                [new Destination(GOLEM_SPAWN, 0f)],
                PathOptions.NoTown with
                {
                    WalkSpeed = speed
                });

    [Test]
    public void ACharacterFastEnoughHopsOntoTheIceGolemIsland()
    {
        var path = FindGolemPath(true, 60f);

        path.Count(edge => edge.Type == EdgeType.Hop)
            .Should()
            .Be(1);

        var hopIndex = path.ToList()
                           .FindIndex(edge => edge.Type == EdgeType.Hop);

        hopIndex.Should()
                .BePositive();

        path[hopIndex]
            .Start
            .Should()
            .Be(path[hopIndex - 1].End);
    }

    [Test]
    public void WithoutHopsTheIslandCannotBeReached()
        => FluentActions.Invoking(() => FindGolemPath(false, 60f))
                        .Should()
                        .Throw<InvalidOperationException>();

    [Test]
    public void AnUnknownSpeedTakesNoHop()
        => FluentActions.Invoking(() => FindGolemPath(true, null))
                        .Should()
                        .Throw<InvalidOperationException>();

    [Test]
    public void ACrawlingCharacterTakesNoHop()
        => FluentActions.Invoking(() => FindGolemPath(true, 1f))
                        .Should()
                        .Throw<InvalidOperationException>();

    [Test]
    public void NoHopsOnInstancesGeneratedFloorsOrBoundlessMaps()
    {
        foreach ((var name, var map) in GameData.Maps.Entries)
            if (map.Instance || map.Boundless || map.Generated is not null)
                PortalGraph.AllowsHops(map)
                           .Should()
                           .BeFalse(name);

        PortalGraph.AllowsHops(GameData.Maps["winterland"]!)
                   .Should()
                   .BeTrue();
    }

    [Test]
    public void AScoreCarriedIntoTheSearchCanRefuseAHop()
    {
        var graph = BuildGraph("winterland", true);

        var options = PathOptions.NoTown with
        {
            WalkSpeed = 60f
        };

        graph.FindPath(LAKE_TAKEOFF, [new Destination(GOLEM_SPAWN, 0f)], options)
             .Should()
             .Contain(edge => edge.Type == EdgeType.Hop);

        FluentActions.Invoking(() => graph.FindPath(
                         LAKE_TAKEOFF,
                         [new Destination(GOLEM_SPAWN, 0f)],
                         options with
                         {
                             HopScore = 20f
                         }))
                     .Should()
                     .Throw<InvalidOperationException>();
    }

    [Test]
    public void TwoHopsInARowCarryTheScoreBetweenThem()
    {
        var hops = HopBuilder.FindHops(Pathfinder.GetNavMesh("winter_cove")!);
        var first = hops.Single(hop => (hop.Takeoff.Distance(COVE_START) < 5f) && (hop.Landing.Distance(new Point(-23f, -1315f)) < 5f));

        var second = hops.Single(hop => (hop.FromPiece == first.ToPiece) && (hop.Landing.Distance(COVE_END) < 5f));
        var start = new Location("winter_cove", first.Takeoff);
        var end = new Location("winter_cove", second.Landing);
        var firstLanding = new Location("winter_cove", first.Landing);
        var secondTakeoff = new Location("winter_cove", second.Takeoff);
        var graph = BuildGraph("winter_cove", true);

        var slow = PathOptions.NoTown with
        {
            WalkSpeed = 105f
        };

        //at 105 each hop passes alone, but the short walk between them sheds too little of the first one's score
        graph.FindPath(start, [new Destination(firstLanding, 0f)], slow)
             .Should()
             .Contain(edge => (edge.Type == EdgeType.Hop) && edge.End.Equals(firstLanding));

        graph.FindPath(secondTakeoff, [new Destination(end, 0f)], slow)
             .Should()
             .Contain(edge => (edge.Type == EdgeType.Hop) && edge.Start.Equals(secondTakeoff));

        FluentActions.Invoking(() => graph.FindPath(start, [new Destination(end, 0f)], slow))
                     .Should()
                     .Throw<InvalidOperationException>();

        var hopLegs = graph.FindPath(
                               start,
                               [new Destination(end, 0f)],
                               slow with
                               {
                                   WalkSpeed = 130f
                               })
                           .Where(edge => edge.Type == EdgeType.Hop)
                           .ToList();

        hopLegs.Should()
               .HaveCount(2);

        hopLegs[0]
            .End
            .Should()
            .Be(firstLanding);

        hopLegs[1]
            .Start
            .Should()
            .Be(secondTakeoff);
    }

    [Test]
    public void AShortcutInsideOnePieceBeatsTheWalkAround()
    {
        var options = PathOptions.NoTown with
        {
            WalkSpeed = 60f
        };

        var walk = BuildGraph("winterland", false)
            .FindPath(WALL_WEST, [new Destination(WALL_EAST, 0f)], options);

        var shortcut = BuildGraph("winterland", true)
            .FindPath(WALL_WEST, [new Destination(WALL_EAST, 0f)], options);

        Pathfinder.GetNavMesh("winterland")!.CanMove(WALL_WEST, WALL_EAST)
                  .Should()
                  .BeFalse();

        shortcut.Should()
                .Contain(edge => edge.Type == EdgeType.Hop);

        shortcut.Sum(edge => edge.Cost)
                .Should()
                .BeLessThan(walk.Sum(edge => edge.Cost));
    }
}