#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class DirectedGraphTests : PathfindingTestBed
{
    [Test]
    public async Task FindPathDirectWalkTest()
    {
        //200 units of open ground north of main's spawn, so the search should be skipped entirely for one walk
        //that stops at the near edge of the destination's radius rather than at its centre
        var start = new Location("main", 0, 0);
        var endLoc = new Location("main", 0, 200);
        var end = new Destination(endLoc, 40);

        var path = await Pathfinder.FindPathAsync(start, [end])
                                   .ToArrayAsync();

        path.Should()
            .ContainSingle();

        path[0]
            .Type
            .Should()
            .Be(EdgeType.Walk);

        path[0]
            .End
            .Vertex
            .Distance(endLoc)
            .Should()
            .BeApproximately(40f, 0.1f);
    }

    /// <summary>
    ///     The half of the fast path that matters more than the hit: a destination it must not answer still reaches
    ///     the search. A shortcut that swallowed one would walk the character into whatever is between the two, and
    ///     the symptom - a walk that goes nowhere - reads identically to a path the search failed to find.
    /// </summary>
    [Test]
    public async Task FindPathDefersToTheSearchWhereTheShortcutDoesNotApply()
    {
        //100 units apart with a wall between them, which is what makes the pair worth hardcoding
        var blocked = await Pathfinder.FindPathAsync(new Location("main", -1582, 496), [new Destination(new Location("main", -1582, 396), 0)])
                                      .ToArrayAsync();

        blocked.Length
               .Should()
               .BeGreaterThan(1, "a blocked straight line has to be routed around rather than walked through");

        //and the bound on the other guard: a clear line long enough that towning could have beaten it is the search's
        //call to make, which is what turns TOWN_HEURISTIC into a decision rather than an incidental constant
        var far = await Pathfinder.FindPathAsync(new Location("main", 0, 0), [new Destination(new Location("main", 0, 1400), 0)])
                                  .ToArrayAsync();

        far.Length
           .Should()
           .BeGreaterThan(1, "a walk past the town heuristic is one the search has to price against towning");
    }

    [Test]
    public async Task FindPathFromTownNodeTest()
    {
        var start = new Location("bank", 0, -37);
        var endLoc = new Location("spookytown", 0, 0);
        var end = new Destination(endLoc, 0);

        var path = await Pathfinder.FindPathAsync(start, [end])
                                   .ToArrayAsync();

        //the start is bank's spawn, so the start-side town connector would teleport us where we already
        //stand. Town elsewhere on the route is fair game - it beats walking most maps end to end.
        path.First()
            .Type
            .Should()
            .NotBe(EdgeType.Town);
    }

    [Test]
    public async Task FindPathMultiMapAcrossDoorsTest()
    {
        //a second cross-map destination, so a regression that only breaks one cluster of maps still fails a test
        var start = new Location("main", -1582, 496);
        var endLoc = new Location("spookytown", 0, 0);
        var end = new Destination(endLoc, 0);

        var path = await Pathfinder.FindPathAsync(start, [end])
                                   .ToArrayAsync();

        path.Should()
            .Contain(p => p.Type == EdgeType.Door);

        path.Should()
            .NotContain(p => p.Type == EdgeType.Leave);

        path.First()
            .Start
            .Vertex
            .Should()
            .Be(start);

        path.Last()
            .End
            .Vertex
            .Should()
            .Be(end);
    }

    [Test]
    public async Task FindPathMultiMapTest()
    {
        var start = new Location("main", -1582, 496);
        var endLoc = new Location("winter_cave", -84, 0);
        var end = new Destination(endLoc, 0);

        var path = await Pathfinder.FindPathAsync(start, [end])
                                   .ToArrayAsync();

        path.Should()
            .ContainSingle(p => p.Type == EdgeType.Town);

        path.Should()
            .ContainSingle(p => p.Type == EdgeType.Transport);

        path.Should()
            .ContainSingle(p => p.Type == EdgeType.Door);

        path.Should()
            .Contain(p => p.Type == EdgeType.Door);

        path.Should()
            .NotContain(p => p.Type == EdgeType.Leave);

        path.First()
            .Start
            .Vertex
            .Should()
            .Be(start);

        path.Last()
            .End
            .Vertex
            .Should()
            .Be(end);
    }

    /// <summary>
    ///     The three things smoothing owes its caller, written as properties rather than as an expected edge count -
    ///     a count would only pin whatever the smoother happens to do today. A scan that stops too early leaves behind
    ///     a pair a straight line could still have merged; one that reaches too far emits a walk through a wall.
    /// </summary>
    [Test]
    public async Task SmoothedPathIsConnectedWalkableAndFullyCollapsed()
    {
        var start = new Location("main", -1582, 496);

        //one route per shape the partitioning has to handle: same map, across a door, and across a town teleport
        Location[] ends =
        [
            new("main", 1891, -47),
            new("spookytown", 0, 0),
            new("winter_cave", -84, 0)
        ];

        foreach (var endLoc in ends)
        {
            var path = await Pathfinder.FindPathAsync(start, [new Destination(endLoc, 0)])
                                       .ToArrayAsync();

            path.Length
                .Should()
                .BeGreaterThan(1, $"the route to {endLoc} is not one straight walk");

            for (var i = 0; i < path.Length; i++)
            {
                var edge = path[i];

                //a collapsed walk still has to be one the character can make in a straight line
                if (edge.Type == EdgeType.Walk)
                    Pathfinder.CanMove(edge.Start.Vertex, edge.End.Vertex)
                              .Should()
                              .BeTrue($"edge {i} of the route to {endLoc} is a walk the smoother emitted");
            }

            //walk pairs only. the walk into a door is retargeted to stop where the door opens rather than at the
            //door itself, so that handoff is a real gap by design
            for (var i = 0; (i + 1) < path.Length; i++)
            {
                if ((path[i].Type != EdgeType.Walk) || (path[i + 1].Type != EdgeType.Walk))
                    continue;

                path[i]
                    .End
                    .Vertex
                    .Distance(path[i + 1].Start.Vertex)
                    .Should()
                    .BeApproximately(0f, 0.01f, $"edge {i} of the route to {endLoc} has to hand off to edge {i + 1}");

                //greedy string pulling leaves nothing a further straight line could have merged. the last walk
                //before a non-walk edge is exempt, because FindDistanceShortcuts pulls its end in toward the
                //door's circle and can leave a pair looking mergeable that the smoother was never offered
                if (((i + 2) < path.Length) && (path[i + 2].Type == EdgeType.Walk))
                    Pathfinder.CanMove(path[i].Start.Vertex, path[i + 1].End.Vertex)
                              .Should()
                              .BeFalse($"edges {i} and {i + 1} of the route to {endLoc} would have collapsed into one");
            }
        }
    }

    [Test]
    public async Task FindPathSingleMapTest()
    {
        var start = new Location("main", -1582, 496);
        var endLoc = new Location("main", 1891, -47);
        var end = new Destination(endLoc, 0);

        var path = await Pathfinder.FindPathAsync(start, [end])
                                   .ToArrayAsync();

        path.Should()
            .ContainSingle(p => p.Type == EdgeType.Town);

        path.Should()
            .Contain(p => p.Type == EdgeType.Door);

        path.Should()
            .Contain(p => p.Type == EdgeType.Walk);

        path.Should()
            .NotContain(p => (p.Type == EdgeType.Transport) || (p.Type == EdgeType.Leave));

        path.First()
            .Start
            .Vertex
            .Should()
            .Be(start);

        path.Last()
            .End
            .Vertex
            .Should()
            .Be(end);
    }
}