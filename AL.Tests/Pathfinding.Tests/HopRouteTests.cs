#region
using System.Diagnostics;
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
    private static readonly Location COVE_START = new("winter_cove", -57f, -1275f);
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
        var path = FindGolemPath(true, 30f);

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
    public void AHopLegCarriesTheScoreItLandsWith()
    {
        var leg = FindGolemPath(true, 30f)
            .Single(edge => edge.Type == EdgeType.Hop);

        var hop = HopBuilder.FindHops(Pathfinder.GetNavMesh("winterland")!)
                            .Single(hop => (hop.Takeoff.Distance(leg.Start) < 1f) && (hop.Landing.Distance(leg.End) < 1f));

        (_, var end) = HopRisk.Replay(
            hop.Penalties,
            hop.Length,
            30f,
            0f);

        leg.HopScore
           .Should()
           .BePositive()
           .And
           .BeApproximately(end, 0.001f);
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
    public void AScoreCarriedIntoTheSearchDelaysTheHop()
    {
        var graph = BuildGraph("winterland", true);

        var options = PathOptions.NoTown with
        {
            WalkSpeed = 60f
        };

        var fresh = graph.FindPath(LAKE_TAKEOFF, [new Destination(GOLEM_SPAWN, 0f)], options);

        //from a carried 40 the lake hop would pass the limit, so the route walks off first to let the score decay
        var carried = graph.FindPath(
            LAKE_TAKEOFF,
            [new Destination(GOLEM_SPAWN, 0f)],
            options with
            {
                HopScore = 40f
            });

        fresh.Sum(edge => edge.Cost)
             .Should()
             .BeLessThan(carried.Sum(edge => edge.Cost));
    }

    [Test]
    public void TwoHopsInARowCarryTheScoreBetweenThem()
    {
        var hops = HopBuilder.FindHops(Pathfinder.GetNavMesh("winter_cove")!);

        var second = hops.Where(hop => (hop.ToPiece != hop.FromPiece) && (hop.Landing.Distance(COVE_END) < 5f))
                         .MinBy(hop => hop.Takeoff.Distance(new Point(23f, -1283f)));

        //the hop onto the second's island from the start's, landing nearest the second's takeoff
        var first = hops.Where(hop => (hop.ToPiece == second.FromPiece)
                                      && (hop.FromPiece != hop.ToPiece)
                                      && (hop.FromPiece != second.ToPiece)
                                      && (hop.Takeoff.Distance(COVE_START) < 100f))
                        .MinBy(hop => hop.Landing.Distance(second.Takeoff));
        var start = new Location("winter_cove", first.Takeoff);
        var end = new Location("winter_cove", second.Landing);
        var firstLanding = new Location("winter_cove", first.Landing);
        var secondTakeoff = new Location("winter_cove", second.Takeoff);
        var graph = BuildGraph("winter_cove", true);

        var slow = PathOptions.NoTown with
        {
            WalkSpeed = 50f
        };

        //at 50 each hop passes alone
        graph.FindPath(start, [new Destination(firstLanding, 0f)], slow)
             .Should()
             .Contain(edge => (edge.Type == EdgeType.Hop) && edge.End.Equals(firstLanding));

        graph.FindPath(secondTakeoff, [new Destination(end, 0f)], slow)
             .Should()
             .Contain(edge => (edge.Type == EdgeType.Hop) && edge.Start.Equals(secondTakeoff));

        //at 50 the second hop waits for the first one's score to decay, so the route walks further than at 60
        var slowPath = graph.FindPath(start, [new Destination(end, 0f)], slow);

        var fastPath = graph.FindPath(
            start,
            [new Destination(end, 0f)],
            slow with
            {
                WalkSpeed = 60f
            });

        slowPath.Sum(edge => edge.Cost)
                .Should()
                .BeGreaterThan(fastPath.Sum(edge => edge.Cost));

        var hopLegs = fastPath.Where(edge => edge.Type == EdgeType.Hop)
                              .ToList();

        hopLegs.Should()
               .HaveCount(2);

        hopLegs[1]
            .Start
            .Should()
            .Be(secondTakeoff);
    }

    [Test]
    public void ASearchWithHopsEndsWhenNoRouteExists()
        => FluentActions.Invoking(() => BuildGraph("winter_cove", true)
                            .FindPath(
                                new Location("winter_cove", -143f, -1456f),
                                [new Destination(new Location("winter_cove", -889f, -395f), 0f)],
                                PathOptions.NoTown with
                                {
                                    WalkSpeed = 80f
                                }))
                        .Should()
                        .Throw<InvalidOperationException>();

    [Test]
    public void ASearchWithHopsAndBlinkEndsAcrossMaps()
    {
        var meshes = new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase);

        foreach ((var name, _) in GameData.Maps.Entries)
            if (Pathfinder.GetNavMesh(name) is { } mesh)
                meshes[name] = mesh;

        var graph = new PortalGraph(meshes, true);
        var started = Stopwatch.GetTimestamp();

        graph.FindPath(
                 new Location("uhills", -17f, -515f),
                 [new Destination(new Location("level2e", 121f, 493f), 0f)],
                 PathOptions.NoTown with
                 {
                     WalkSpeed = 143.7f,
                     HopScore = 25.84f,
                     BlinkCost = 500f,
                     Mp = 5000f,
                     MaxMp = 5000f
                 })
             .Should()
             .NotBeEmpty();

        Stopwatch.GetElapsedTime(started)
                 .Should()
                 .BeLessThan(TimeSpan.FromSeconds(10));
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