#region
using System.Text.Json;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using Chaos.Extensions.Common;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Blink priced inside the search at the time it takes: a walk at least the floor long may come back as one teleport,
///     a pair no walk joins is bridged by one, and the cooldown, the penalty and a tracked bar decide which is faster.
/// </summary>
public class BlinkLegTests : PathfindingTestBed
{
    private static readonly PathOptions BLINK_AT_400 = new()
    {
        BlinkCost = 400f
    };

    private static readonly Destination MAIN_600 = new(new Location("main", 0, 600), 0);
    private static readonly Location MAIN_ORIGIN = new("main", 0, 0);

    /// <summary>
    ///     A character's speed at the slow end of what moves in game, with blink off.
    /// </summary>
    private static readonly PathOptions SPEED_60 = new()
    {
        WalkSpeed = 60f
    };

    private static readonly PathOptions SPEED_60_BLINK = SPEED_60 with
    {
        BlinkCost = 400f
    };

    /// <summary>
    ///     Asynchronously reads one trip's start and end out of <c>blink-paths.json</c>.
    /// </summary>
    private static async Task<(Location Start, Destination End)> LoadTripAsync(int id)
    {
        var json = await File.ReadAllTextAsync(Path.Combine("Fixtures", "pathfinding", "blink-paths.json"));
        var corpus = JsonSerializer.Deserialize<BlinkCorpus>(json)!;
        var trip = corpus.Trips.Single(candidate => candidate.Id == id);

        return (new Location(trip.Start.Map, trip.Start.X, trip.Start.Y),
            new Destination(new Location(trip.End.Map, trip.End.X, trip.End.Y), 0f));
    }

    /// <summary>
    ///     A long walk on one map is one blink to the destination object itself, carrying the walked length it replaces.
    /// </summary>
    [Test]
    public void ALongWalkOnOneMapIsOneBlinkToTheDestination()
    {
        var start = new Location("main", 0, 0);
        var end = new Destination(new Location("main", 0, 1400), 0);

        var path = Pathfinder.FindPath(start, [end], BLINK_AT_400);

        var blink = path.Should()
                        .ContainSingle()
                        .Which;

        blink.Type
             .Should()
             .Be(EdgeType.Blink);

        blink.Start
             .Should()
             .Be(start);

        blink.End
             .Should()
             .Be(end);

        blink.Cost
             .Should()
             .BeGreaterThan(400f);
    }

    /// <summary>
    ///     Over the recorded corpus with blink on: a blink stays on its map, carries the distance it covers, and is followed
    ///     by a door or transporter or is the last leg, since nothing walks out of a landing. Every blink replaces a walk at
    ///     least the floor long, or bridges a pair no walk joins.
    /// </summary>
    [Test]
    [Arguments(400f)]
    [Arguments(1000f)]
    public async Task EveryRouteOnTheCorpusIsShapedForTheCast(float blinkCost)
    {
        var json = await File.ReadAllTextAsync(Path.Combine("Fixtures", "pathfinding", "old-paths.json"));
        var corpus = JsonSerializer.Deserialize<Corpus>(json)!;

        var options = new PathOptions
        {
            BlinkCost = blinkCost
        };

        var blinks = 0;

        foreach (var recorded in corpus.Paths)
        {
            var start = new Location(recorded.Start.Map, recorded.Start.X, recorded.Start.Y);
            var end = new Destination(new Location(recorded.End.Map, recorded.End.X, recorded.End.Y), recorded.Radius);
            IReadOnlyList<PathEdge> path;

            try
            {
                path = Pathfinder.FindPath(start, [end], options);
            } catch (InvalidOperationException)
            {
                //no route is the search's contract for a pair between two instances, so the trip is simply not in the sample
                continue;
            }

            for (var index = 0; index < path.Count; index++)
            {
                var edge = path[index];

                if (edge.Type != EdgeType.Blink)
                    continue;

                //a cast that replaced a walk carries that walk's length; one under the floor must be a bridge, whose
                //length is the ruler, and then no walk on the map may join its two ends
                if (edge.Cost < blinkCost)
                    NoWalkJoins(edge)
                        .Should()
                        .BeTrue($"route #{recorded.Id} at {blinkCost}: the blink at leg {index} replaced a walk under the floor");

                blinks++;

                edge.Start
                    .Map
                    .Should()
                    .Be(edge.End.Map, $"route #{recorded.Id} at {blinkCost}: a blink stays on its map");

                edge.Cost
                    .Should()
                    .BeGreaterThan(0f, $"route #{recorded.Id} at {blinkCost}: a blink carries the distance it covers");

                if (index < (path.Count - 1))
                    path[index + 1]
                        .Type
                        .Should()
                        .BeOneOf(
                            [
                                EdgeType.Door,
                                EdgeType.Transport
                            ],
                            $"route #{recorded.Id} at {blinkCost}: nothing walks out of a landing");
            }
        }

        blinks.Should()
              .BeGreaterThan(0, "the corpus holds trips long enough to cast on");
    }

    /// <summary>
    ///     Whether no walk on the map joins a blink leg's two ends: routed with blink and recall off, the trip either has no
    ///     route or needs a map change.
    /// </summary>
    private static bool NoWalkJoins(PathEdge blink)
    {
        IReadOnlyList<PathEdge> walk;

        try
        {
            walk = Pathfinder.FindPath(
                blink.Start,
                [new Destination(new Location(blink.End.Map, blink.End.X, blink.End.Y), 0)],
                PathOptions.NoTown);
        } catch (InvalidOperationException)
        {
            //no route at all is the search's answer for ground nothing reaches
            return true;
        }

        return walk.Any(leg => leg.Type != EdgeType.Walk);
    }

    /// <summary>
    ///     Town and blink compete on price. At speed 60 the recall to spawn 0 of the current map is 216; a rested cast under
    ///     a 100 floor is 12 + 100 and wins, and the same cast 5s off is 312 + 100 and loses.
    /// </summary>
    [Test]
    public void TownAndBlinkArePricedAgainstEachOther()
    {
        var spawn = GameData.Maps["main"]!.Spawns[0];
        var home = new Destination(new Location("main", spawn), 40);
        var start = new Location("main", 1891, -47);

        var floorAt100 = SPEED_60 with
        {
            BlinkCost = 100f
        };

        var blinked = Pathfinder.FindPath(start, [home], floorAt100);

        blinked.Should()
               .Contain(edge => edge.Type == EdgeType.Blink);

        blinked.Should()
               .NotContain(edge => edge.Type == EdgeType.Town);

        var recalled = Pathfinder.FindPath(
            start,
            [home],
            floorAt100 with
            {
                BlinkReadyInMs = 5000f
            });

        recalled.Should()
                .Contain(edge => edge.Type == EdgeType.Town);

        recalled.Should()
                .NotContain(edge => edge.Type == EdgeType.Blink);
    }

    /// <summary>
    ///     389 units of walking from (1891, -47) on main is under the 400 floor. A recall to main's spawn and a cast back from
    ///     there would take 3.2s against 6.5s on foot, and the cast replaces a walk far over the floor, but it lands 389 units
    ///     from where the route stood on main, so it is refused and the trip is walked.
    /// </summary>
    [Test]
    public void ARecallToSpawnCannotSplitAShortWalkIntoACast()
    {
        var path = Pathfinder.FindPath(
            new Location("main", 1891, -47),
            [new Destination(new Location("main", 1591, -47), 0)],
            SPEED_60_BLINK);

        path.Should()
            .OnlyContain(edge => edge.Type == EdgeType.Walk, string.Join(", ", path));
    }

    /// <summary>
    ///     Over the corpus at both speeds, rested and just through a door: once a route casts, no door, transporter or
    ///     <c>leave</c> takes it back onto a map it had been on before that cast.
    /// </summary>
    [Test]
    [Arguments(60f, 0f)]
    [Arguments(60f, 3200f)]
    [Arguments(120f, 0f)]
    [Arguments(120f, 3200f)]
    public async Task AfterABlinkTheRouteNeverReentersAnEarlierMap(float speed, float penaltyMs)
    {
        var json = await File.ReadAllTextAsync(Path.Combine("Fixtures", "pathfinding", "blink-paths.json"));
        var corpus = JsonSerializer.Deserialize<BlinkCorpus>(json)!;

        var options = new PathOptions
        {
            BlinkCost = 400f,
            WalkSpeed = speed,
            PenaltyMs = penaltyMs
        };

        var casts = 0;

        foreach (var trip in corpus.Trips)
        {
            var path = Pathfinder.FindPath(
                new Location(trip.Start.Map, trip.Start.X, trip.Start.Y),
                [new Destination(new Location(trip.End.Map, trip.End.X, trip.End.Y), 0f)],
                options);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                trip.Start.Map
            };

            var closed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var edge in path)
            {
                if (edge.Type == EdgeType.Blink)
                {
                    casts++;
                    closed = new HashSet<string>(visited, StringComparer.OrdinalIgnoreCase);

                    continue;
                }

                if (edge.Type is not (EdgeType.Door or EdgeType.Transport or EdgeType.Leave))
                    continue;

                //a door between two floors of one map does not leave it
                if (!edge.End.Map.EqualsI(edge.Start.Map))
                    closed.Should()
                          .NotContain(edge.End.Map, $"trip #{trip.Id}: {string.Join(", ", path)}");

                visited.Add(edge.End.Map);
            }
        }

        casts.Should()
             .BeGreaterThan(0, "the corpus holds trips long enough to cast on");
    }

    /// <summary>
    ///     Trip 100 leaves duelland by a door 444 units away and blinks twice beyond it. Rested, the first cast goes straight
    ///     to the door. With 10s of penalty pending, a cast there would leave the next one 11.2s off; walking the 7.4s lets
    ///     the penalty run down first, and the route is faster for it.
    /// </summary>
    [Test]
    public async Task WalkingToADoorBeatsBlinkingWhenItLeavesTheNextCastReadySooner()
    {
        var trip = await LoadTripAsync(100);

        var rested = Pathfinder.FindPath(trip.Start, [trip.End], SPEED_60_BLINK);

        rested[0]
            .Type
            .Should()
            .Be(EdgeType.Blink);

        rested[1]
            .Type
            .Should()
            .Be(EdgeType.Door);

        var pending = Pathfinder.FindPath(
            trip.Start,
            [trip.End],
            SPEED_60_BLINK with
            {
                PenaltyMs = 10000f
            });

        var door = pending.ToList()
                          .FindIndex(edge => edge.Type == EdgeType.Door);

        door.Should()
            .BeGreaterThan(0);

        pending.Take(door)
               .Should()
               .OnlyContain(edge => edge.Type == EdgeType.Walk);

        pending.Skip(door)
               .Should()
               .Contain(edge => edge.Type == EdgeType.Blink);

        //the same route with the walk to the door cast instead
        var walked = pending.Take(door)
                            .Sum(edge => edge.Cost);

        List<PathEdge> castToTheDoor =
        [
            new(
                EdgeType.Blink,
                pending[0].Start,
                pending[door - 1].End,
                walked),
            .. pending.Skip(door)
        ];

        var clock = new ClockSettings(
            60f,
            10000f,
            0f,
            null,
            0f,
            0f,
            0f);

        RouteClock.Measure(pending, clock)
                  .Seconds
                  .Should()
                  .BeLessThan(
                      RouteClock.Measure(castToTheDoor, clock)
                                .Seconds);
    }

    /// <summary>
    ///     A 300 unit walk under a 400 floor is walked, though casts would land in a fraction of the time. Blinking to the
    ///     tavern door, through it and back, and on to the target would replace two walks over the floor; the first cast
    ///     closes main, so the door back is refused.
    /// </summary>
    [Test]
    public void AWalkShorterThanTheFloorIsWalked()
    {
        var path = Pathfinder.FindPath(new Location("main", 0, 0), [new Destination(new Location("main", 0, 300), 0)], SPEED_60_BLINK);

        path.Should()
            .OnlyContain(edge => edge.Type == EdgeType.Walk, string.Join(", ", path));
    }

    /// <summary>
    ///     A trip that blinks across three maps or more with doors between costs more starting just through a door: the 3200ms
    ///     of penalty is charged to the first cast, and what is left of it to the next.
    /// </summary>
    [Test]
    public async Task ADoorBlinkChainIsChargedTheWaitBeforeTheNextCast()
    {
        var trip = await LoadTripAsync(16);

        var rested = SPEED_60 with
        {
            BlinkCost = 400f
        };

        var throughADoor = rested with
        {
            PenaltyMs = 3200f
        };

        var restedPath = Pathfinder.FindPath(trip.Start, [trip.End], rested);
        var doorPath = Pathfinder.FindPath(trip.Start, [trip.End], throughADoor);

        restedPath.Count(edge => edge.Type == EdgeType.Blink)
                  .Should()
                  .BeGreaterThanOrEqualTo(3, "the trip is picked for a chain of casts");

        var restedPrice = RouteClock.Measure(
                                        restedPath,
                                        new ClockSettings(
                                            60f,
                                            0f,
                                            0f,
                                            null,
                                            0f,
                                            0f,
                                            0f))
                                    .Price;

        var doorPrice = RouteClock.Measure(
                                      doorPath,
                                      new ClockSettings(
                                          60f,
                                          3200f,
                                          0f,
                                          null,
                                          0f,
                                          0f,
                                          0f))
                                  .Price;

        doorPrice.Should()
                 .BeGreaterThan(restedPrice);
    }

    /// <summary>
    ///     600 units at speed 60 is 10s on foot. Rested, one cast lands in 200ms and prices at 12; with the cast 12s off it
    ///     prices at 732, dearer than the walk, so the route walks.
    /// </summary>
    [Test]
    public void APendingPenaltyCanTurnABlinkIntoAWalk()
    {
        var restedPath = Pathfinder.FindPath(MAIN_ORIGIN, [MAIN_600], SPEED_60_BLINK);

        restedPath.Should()
                  .ContainSingle()
                  .Which
                  .Type
                  .Should()
                  .Be(EdgeType.Blink);

        var waitingPath = Pathfinder.FindPath(
            MAIN_ORIGIN,
            [MAIN_600],
            SPEED_60_BLINK with
            {
                BlinkReadyInMs = 12000f
            });

        waitingPath.Should()
                   .OnlyContain(edge => edge.Type == EdgeType.Walk);
    }

    /// <summary>
    ///     An empty tracked bar refilling at 50 a second needs 32s for blink's 1600 mana, far longer than the 10s walk.
    /// </summary>
    [Test]
    public void ALowTrackedBarWalksWhereAnUntrackedOneBlinks()
    {
        var path = Pathfinder.FindPath(
            MAIN_ORIGIN,
            [MAIN_600],
            SPEED_60_BLINK with
            {
                BlinkMpPerSecond = 50f,
                Mp = 0f,
                MaxMp = 4000f
            });

        path.Should()
            .OnlyContain(edge => edge.Type == EdgeType.Walk);
    }

    /// <summary>
    ///     A tracked bar short of a cast that never refills never blinks, even across the whole of main.
    /// </summary>
    [Test]
    public void ARateOfZeroNeverBlinks()
    {
        var end = new Destination(new Location("main", 0, 1400), 0);

        Pathfinder.FindPath(MAIN_ORIGIN, [end], SPEED_60_BLINK)
                  .Should()
                  .Contain(edge => edge.Type == EdgeType.Blink);

        Pathfinder.FindPath(
                      MAIN_ORIGIN,
                      [end],
                      SPEED_60_BLINK with
                      {
                          BlinkMpPerSecond = 0f,
                          Mp = 0f,
                          MaxMp = 4000f
                      })
                  .Should()
                  .NotContain(edge => edge.Type == EdgeType.Blink);
    }

    /// <summary>
    ///     With the bar untracked, an empty one and a full one route a multi-map trip the same way.
    /// </summary>
    [Test]
    public async Task TrackingOffIgnoresTheBar()
    {
        var trip = await LoadTripAsync(12);

        var empty = Pathfinder.FindPath(
            trip.Start,
            [trip.End],
            SPEED_60_BLINK with
            {
                Mp = 0f,
                MaxMp = 4000f
            });

        var full = Pathfinder.FindPath(
            trip.Start,
            [trip.End],
            SPEED_60_BLINK with
            {
                Mp = 4000f,
                MaxMp = 4000f
            });

        empty.Should()
             .Contain(edge => edge.Type == EdgeType.Blink);

        full.Should()
            .Equal(empty);
    }

    /// <summary>
    ///     Ground no walk leaves is bridged by a cast. The top-left of main is walled off from the rest: with blink off the
    ///     only way out is a recall, and with it on the trip is one blink.
    /// </summary>
    [Test]
    public void WalledOffGroundIsBridgedByABlinkAndRecalledOffWithoutOne()
    {
        var walled = new Location("main", -1086, -931);
        var across = new Destination(new Location("main", 1891, -47), 0);

        var recalled = Pathfinder.FindPath(walled, [across]);

        recalled.Should()
                .Contain(edge => edge.Type == EdgeType.Town);

        recalled.Should()
                .NotContain(edge => edge.Type == EdgeType.Blink);

        var bridged = Pathfinder.FindPath(walled, [across], BLINK_AT_400);

        bridged.Should()
               .ContainSingle()
               .Which
               .Type
               .Should()
               .Be(EdgeType.Blink);
    }

    /// <summary>
    ///     A blink at a door is aimed somewhere the server will land it. Its test is the ten-unit cell the landing rounds to
    ///     and the eight around it all being ground; a door's entry is on the edge of the ground, so Spooky Forest's door to
    ///     Spooky Town refused every cast aimed at it and the character bounced back to main instead. The aim still has to
    ///     open the door, which is the wider of the two windows at 112 units edge-to-edge.
    /// </summary>
    [Test]
    public void ABlinkAtADoorLandsWhereTheServerAcceptsOne()
    {
        var end = new Destination(new Location("spookytown", 265, -1360), 50);

        var path = Pathfinder.FindPath(new Location("main", 1600, -547), [end], BLINK_AT_400);

        var landing = path.Should()
                          .ContainSingle(edge => (edge.Type == EdgeType.Blink) && edge.End.Map.EqualsI("halloween"))
                          .Which
                          .End;

        var mesh = Pathfinder.GetNavMesh("halloween")!;

        for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
                mesh.IsWalkable(landing.X + dx * 10f, landing.Y + dy * 10f)
                    .Should()
                    .BeTrue($"the server checks ({dx}, {dy}) of the landing cell");
    }

    private sealed record Case(
        int Id,
        Spot Start,
        Spot End,
        float Radius);

    private sealed record Corpus(List<Case> Paths);

    private sealed record Spot(string Map, float X, float Y);
}