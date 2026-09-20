#region
using System.Text.Json;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Blink priced inside the search: a walk dearer than the cast comes back as one teleport, a pair no walk joins is
///     bridged by one, and town competes with it on cost alone.
/// </summary>
public class BlinkLegTests : PathfindingTestBed
{
    private static readonly PathOptions BLINK_AT_400 = new()
    {
        BlinkCost = 400f
    };

    /// <summary>
    ///     A long walk on one map is one blink to the destination object itself, priced at the walk it replaces.
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
    ///     by a door or transporter or is the last leg, since nothing walks out of a landing. No run of walks between two
    ///     other legs is dearer than the cast, because such a run is one walk edge of the search and the clamp would have
    ///     charged the cast for it.
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

            var walked = 0f;

            for (var index = 0; index < path.Count; index++)
            {
                var edge = path[index];

                if (edge.Type == EdgeType.Walk)
                {
                    walked += edge.Cost;

                    //a unit of slack: the run is read off the funnelled polyline, the clamp off the search's own cost
                    walked.Should()
                          .BeLessThanOrEqualTo(
                              blinkCost + 1f,
                              $"route #{recorded.Id} at {blinkCost}: a walk run at leg {index} outgrew the cast");

                    continue;
                }

                walked = 0f;

                if (edge.Type != EdgeType.Blink)
                    continue;

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
    ///     Town and blink compete on cost alone. To spawn 0 of the current map a recall at about 180 beats a cast at 400, and
    ///     a cast priced under the recall wins instead.
    /// </summary>
    [Test]
    public void TownAndBlinkArePricedAgainstEachOther()
    {
        var spawn = GameData.Maps["main"]!.Spawns[0];
        var home = new Destination(new Location("main", spawn), 40);
        var start = new Location("main", 1891, -47);

        var recalled = Pathfinder.FindPath(start, [home], BLINK_AT_400);

        recalled.Should()
                .Contain(edge => edge.Type == EdgeType.Town);

        recalled.Should()
                .NotContain(edge => edge.Type == EdgeType.Blink);

        var blinked = Pathfinder.FindPath(
            start,
            [home],
            new PathOptions
            {
                BlinkCost = 100f
            });

        blinked.Should()
               .Contain(edge => edge.Type == EdgeType.Blink);

        blinked.Should()
               .NotContain(edge => edge.Type == EdgeType.Town);
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

    private sealed record Case(
        int Id,
        Spot Start,
        Spot End,
        float Radius);

    private sealed record Corpus(List<Case> Paths);

    private sealed record Spot(string Map, float X, float Y);
}