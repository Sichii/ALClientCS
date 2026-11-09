#region
using System.Diagnostics;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
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

    private static Graph BuildGraph(string map, bool buildHops)
        => new(
            new DirectedGraph(
                new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase)
                {
                    [map] = Pathfinder.GetNavMesh(map)!
                },
                buildHops));

    private static IReadOnlyList<PathEdge> FindGolemPath(bool useHops, float? speed)
        => BuildGraph("winterland", true)
            .FindPath(
                TOWN,
                [new Destination(GOLEM_SPAWN, 0f)],
                PathOptions.NoTown with
                {
                    WalkSpeed = speed,
                    UseHops = useHops
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
    public void AHopIsTakenOnlyWhenAsked()
    {
        FluentActions.Invoking(() => FindGolemPath(false, 30f))
                     .Should()
                     .Throw<InvalidOperationException>();

        FindGolemPath(true, 30f)
            .Should()
            .Contain(edge => edge.Type == EdgeType.Hop);
    }

    [Test]
    public void TheWorldSearchTakesHopsOnlyWhenAsked()
    {
        var options = PathOptions.NoTown with
        {
            WalkSpeed = 30f
        };

        FluentActions.Invoking(() => Pathfinder.FindPath(TOWN, [new Destination(GOLEM_SPAWN, 0f)], options))
                     .Should()
                     .Throw<InvalidOperationException>();

        Pathfinder.FindPath(
                      TOWN,
                      [new Destination(GOLEM_SPAWN, 0f)],
                      options with
                      {
                          UseHops = true
                      })
                  .Should()
                  .Contain(edge => edge.Type == EdgeType.Hop);
    }

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
                HopBuilder.AllowsHops(map)
                          .Should()
                          .BeFalse(name);

        HopBuilder.AllowsHops(GameData.Maps["winterland"]!)
                  .Should()
                  .BeTrue();
    }

    [Test]
    public void AScoreCarriedIntoTheSearchDelaysTheHop()
    {
        var graph = BuildGraph("winterland", true);

        var options = PathOptions.NoTown with
        {
            WalkSpeed = 60f,
            UseHops = true
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
    public void AScoreCarriedThroughARealDoorRefusesTheHopBeyondIt()
    {
        var inn = new Location("winter_inn", 0f, 49f);

        var options = PathOptions.NoTown with
        {
            WalkSpeed = 30f,
            UseHops = true
        };

        var fresh = Pathfinder.FindPath(inn, [new Destination(GOLEM_SPAWN, 0f)], options);

        var carried = Pathfinder.FindPath(
            inn,
            [new Destination(GOLEM_SPAWN, 0f)],
            options with
            {
                HopScore = 50f
            });

        //map changes before the hop: the door out of the inn, then any detour taken to walk the score off
        static int CountMapChangesBeforeTheHop(IReadOnlyList<PathEdge> path)
            => path.TakeWhile(edge => edge.Type != EdgeType.Hop)
                   .Count(edge => edge.Type is EdgeType.Door or EdgeType.Transport);

        fresh.Should()
             .Contain(edge => edge.Type == EdgeType.Hop);

        CountMapChangesBeforeTheHop(fresh)
            .Should()
            .Be(1);

        //the score kept through the inn's door refuses the walk onto the island from that arrival
        CountMapChangesBeforeTheHop(carried)
            .Should()
            .BeGreaterThan(1);

        carried.Sum(edge => edge.Cost)
               .Should()
               .BeGreaterThan(fresh.Sum(edge => edge.Cost));
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
            WalkSpeed = 50f,
            UseHops = true
        };

        //at 50 each hop passes alone
        graph.FindPath(start, [new Destination(firstLanding, 0f)], slow)
             .Should()
             .Contain(edge => (edge.Type == EdgeType.Hop) && edge.End.Equals(firstLanding));

        graph.FindPath(secondTakeoff, [new Destination(end, 0f)], slow)
             .Should()
             .Contain(edge => (edge.Type == EdgeType.Hop) && edge.Start.Equals(secondTakeoff));

        //at 50 the second hop is safe only once the first one's score decays, which takes a loop between the two; the mesh
        //search keeps one cost per vertex, so it never walks one and the pair is refused
        FluentActions.Invoking(() => graph.FindPath(start, [new Destination(end, 0f)], slow))
                     .Should()
                     .Throw<InvalidOperationException>();

        var fastPath = graph.FindPath(
            start,
            [new Destination(end, 0f)],
            slow with
            {
                WalkSpeed = 60f
            });

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
                                    WalkSpeed = 80f,
                                    UseHops = true
                                }))
                        .Should()
                        .Throw<InvalidOperationException>();

    [Test]
    public void ASearchWithHopsAndBlinkEndsAcrossMaps()
    {
        var started = Stopwatch.GetTimestamp();

        Pathfinder.FindPath(
                      new Location("uhills", -17f, -515f),
                      [new Destination(new Location("level2e", 121f, 493f), 0f)],
                      PathOptions.NoTown with
                      {
                          WalkSpeed = 143.7f,
                          HopScore = 25.84f,
                          BlinkCost = 500f,
                          Mp = 5000f,
                          MaxMp = 5000f,
                          UseHops = true
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
            .FindPath(
                WALL_WEST,
                [new Destination(WALL_EAST, 0f)],
                options with
                {
                    UseHops = true
                });

        Pathfinder.GetNavMesh("winterland")!.CanMove(WALL_WEST, WALL_EAST)
                  .Should()
                  .BeFalse();

        shortcut.Should()
                .Contain(edge => edge.Type == EdgeType.Hop);

        shortcut.Sum(edge => edge.Cost)
                .Should()
                .BeLessThan(walk.Sum(edge => edge.Cost));
    }

    [Test]
    public void AScoreCarriedThroughADoorRefusesTheNextMapsHop()
    {
        //an exit on one map, then the arrival its door lands on; past them the route's start and its end
        const int EXIT = 0;
        const int ARRIVAL = 1;
        const int START = 2;
        const int END = 3;

        var doors = new DoorSearch(
            [
                new DoorSearch.Edge(
                    EXIT,
                    ARRIVAL,
                    EdgeType.Door,
                    50f)
            ],
            2);

        var options = PathOptions.NoTown with
        {
            WalkSpeed = 30f,
            UseHops = true
        };

        //the walk out of the start lands a hop at 50, and the door keeps that score
        var hopped = FindEndWalk(
            new DoorSearch.Edge(
                START,
                EXIT,
                EdgeType.Walk,
                10f,
                EndScore: 50f));

        //a walk out of the start without a hop arrives with no score, so the next map's hop is taken
        var walked = FindEndWalk(
            new DoorSearch.Edge(
                START,
                EXIT,
                EdgeType.Walk,
                10f));

        hopped.MinSpeed
              .Should()
              .Be(0f);

        walked.MinSpeed
              .Should()
              .BePositive();

        return;

        DoorSearch.Edge FindEndWalk(DoorSearch.Edge startWalk)
        {
            var scratch = SearchScratch.Rent();
            scratch.ResetNodes(END + 1);
            scratch.SearchEdges.Add(startWalk);

            //the next map's walk to the end: a hop checked from a score of 0, and the long way round without one
            scratch.SearchEdges.Add(
                new DoorSearch.Edge(
                    ARRIVAL,
                    END,
                    EdgeType.Walk,
                    10f,
                    MinSpeed: 30f,
                    EndScore: 20f));

            scratch.SearchEdges.Add(
                new DoorSearch.Edge(
                    ARRIVAL,
                    END,
                    EdgeType.Walk,
                    500f));
            scratch.IndexSearchEdges(END + 1);

            var winner = doors.Search(
                scratch,
                options,
                CONSTANTS.CalculateTownCost(30f),
                START,
                END,
                END + 1);

            winner.Should()
                  .BeGreaterThanOrEqualTo(0);

            return doors.GetEdge(scratch, scratch.Arrivals[winner].EdgeIndex);
        }
    }

    [Test]
    public void HopsOffEqualsANoHopBuild()
    {
        const int ROUTES = 300;

        var noHops = new DirectedGraph(Pathfinder.Meshes, false);
        var rng = new Random(20261109);
        var withHopsDiffer = 0;

        var options = PathOptions.Default with
        {
            WalkSpeed = 30f
        };

        foreach ((var start, var end) in SamplePairs(rng, ROUTES))
        {
            var destination = new Destination(end, (float)(rng.NextDouble() * 40));

            var expected = TryFind(
                noHops,
                start,
                destination,
                options);

            var actual = TryFind(
                Pathfinder.Graph!,
                start,
                destination,
                options);

            actual.Error
                  .Should()
                  .Be(expected.Error, $"{start} -> {destination}");

            actual.Legs
                  .Should()
                  .HaveCount(expected.Legs.Count, $"{start} -> {destination}");

            for (var i = 0; i < expected.Legs.Count; i++)
            {
                var a = actual.Legs[i];
                var e = expected.Legs[i];

                a.Type
                 .Should()
                 .Be(e.Type, $"leg {i} of {start} -> {destination}");

                ILocation.Comparer
                         .Equals(a.Start, e.Start)
                         .Should()
                         .BeTrue($"leg {i} start of {start} -> {destination}: {a.Start} vs {e.Start}");

                ILocation.Comparer
                         .Equals(a.End, e.End)
                         .Should()
                         .BeTrue($"leg {i} end of {start} -> {destination}: {a.End} vs {e.End}");

                a.Cost
                 .Should()
                 .BeApproximately(e.Cost, 0.001f, $"leg {i} of {start} -> {destination}");
            }

            var hopped = TryFind(
                Pathfinder.Graph!,
                start,
                destination,
                options with
                {
                    UseHops = true
                });

            if (hopped.Legs.Any(leg => leg.Type == EdgeType.Hop))
                withHopsDiffer++;
        }

        Console.WriteLine($"HOPS OFF routes={ROUTES} takeAHopWhenOn={withHopsDiffer}");
    }

    private static (string? Error, List<PathEdge> Legs) TryFind(
        DirectedGraph graph,
        ILocation start,
        Destination end,
        PathOptions options)
    {
        try
        {
            return (null, graph.FindPath(start, [end], options)
                               .ConvertAll(edge => edge.ToPathEdge()));
        } catch (InvalidOperationException e)
        {
            return (e.GetType()
                     .Name, []);
        }
    }

    /// <summary>
    ///     Represents a graph whose routes come back as the legs <see cref="Pathfinder" /> hands out.
    /// </summary>
    /// <param name="Directed">The graph.</param>
    private sealed record Graph(DirectedGraph Directed)
    {
        public List<PathEdge> FindPath(ILocation start, Destination[] ends, PathOptions options)
            => Directed.FindPath(start, ends, options)
                       .ConvertAll(edge => edge.ToPathEdge());
    }
}
