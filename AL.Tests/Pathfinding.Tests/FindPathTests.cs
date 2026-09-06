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

public class FindPathTests : PathfindingTestBed
{
    /// <summary>
    ///     A start that already satisfies the end still yields one leg, so a caller that takes the last leg as its arrival
    ///     finds where it stands there, and the leg walks nowhere: exactly on the end, and inside its radius.
    /// </summary>
    [Test]
    [Arguments(0f, 0f)]
    [Arguments(10f, 0f)]
    public void AStartAlreadyInsideTheEndYieldsOneZeroCostLegThatStaysPut(float offsetX, float offsetY)
    {
        var start = new Location("main", offsetX, offsetY);
        var end = new Destination(new Location("main", 0, 0), 40);

        var path = Pathfinder.FindPath(start, [end]);

        path.Should()
            .ContainSingle();

        path[0]
            .Type
            .Should()
            .Be(EdgeType.Walk);

        path[0]
            .Cost
            .Should()
            .Be(0);

        path[0]
            .Start
            .Should()
            .Be(start);

        path[0]
            .End
            .Should()
            .Be(start);
    }

    /// <summary>
    ///     A destination the walk cannot end on still stops at the radius it was given. The search runs to the mesh node
    ///     beside a destination, so the radius has to survive as far as the shortcut pass or the legs leading up to that node
    ///     are never pruned and the walk arrives on the destination itself.
    /// </summary>
    /// <remarks>
    ///     Held against an NPC rather than a made-up point because the placements are what produce it: an NPC stands where the
    ///     flood fill never reached often enough to be ordinary, and the client asks to stop at counter range from every one
    ///     of them. The substitution that used to run here dropped the radius, and the symptom was a character walking on top
    ///     of the NPC and standing there for the whole errand.
    /// </remarks>
    [Test]
    public async Task AWalkToAnUnwalkableNpcStillStopsAtItsRadius()
    {
        const float RADIUS = 350f;

        //the two on main that the flood fill does not cover. Asserted rather than assumed - a mesh that came to
        //cover them would leave this test passing without exercising anything
        foreach (var npcKey in new[]
                 {
                     "basics",
                     "newupgrade"
                 })
        {
            var spot = GameData.NPCs[npcKey]!.Locations.First(location => location.Map == "main");

            Pathfinder.IsWalkable(spot)
                      .Should()
                      .BeFalse($"{npcKey} is the premise of this test, and a walkable one tests nothing");

            //far enough out that a walk is genuinely needed, and off the town spawn so no recall lands inside the
            //radius on its own
            var start = new Location("main", 1250, -100);

            var path = await Pathfinder.FindPathAsync(start, [new Destination(spot, RADIUS)])
                                       .ToArrayAsync();

            path.Should()
                .NotBeEmpty();

            var walked = path.Where(edge => edge.Type == EdgeType.Walk)
                             .ToArray();

            //no leg may end inside the radius, which is the statement that the pruning happened at all rather than
            //that the last vertex happens to sit far out
            walked.Should()
                  .NotContain(
                      edge => edge.End.Distance(spot) < (RADIUS - 1f),
                      $"a walk to {npcKey} that gets closer than {RADIUS} has spent legs nothing asked for");
        }
    }

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
            .Distance(endLoc)
            .Should()
            .BeApproximately(40f, 0.1f);
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
            .Should()
            .Be(start);

        path.Last()
            .End
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
            .Should()
            .Be(start);

        path.Last()
            .End
            .Should()
            .Be(end);
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
            .Should()
            .Be(start);

        path.Last()
            .End
            .Should()
            .Be(end);
    }

    /// <summary>
    ///     Whether the straight line to the destination is blocked or clear, every leg is one the server accepts and ends
    ///     somewhere the flood fill reached. A leg walked through what lies between the two reads at runtime exactly like a
    ///     path the search failed to find: a walk that goes nowhere.
    /// </summary>
    [Test]
    public async Task NoLegCrossesAWallWhetherTheLineIsBlockedOrClear()
    {
        //100 units apart with a wall between them, which is what makes the pair worth hardcoding. The far side is
        //also off the flood fill, so nothing may end on it either - what is held is that no leg crosses the wall,
        //rather than a leg count, since a walk that stops short of an unreachable destination is one edge
        var blockedEnd = new Location("main", -1582, 396);

        var blocked = await Pathfinder.FindPathAsync(new Location("main", -1582, 496), [new Destination(blockedEnd, 0)])
                                      .ToArrayAsync();

        blocked.Should()
               .OnlyContain(
                   edge => Pathfinder.CanMove(edge.Start, edge.End),
                   "a blocked straight line has to be routed around rather than walked through");

        blocked.Should()
               .NotContain(edge => Pathfinder.IsWalkable(edge.End) == false, "no leg may end somewhere the flood fill never reached");

        //a long clear walk is one leg now; what has to hold is that every leg is one the server accepts
        var far = await Pathfinder.FindPathAsync(new Location("main", 0, 0), [new Destination(new Location("main", 0, 1400), 0)])
                                  .ToArrayAsync();

        far.Should()
           .NotBeEmpty();

        far.Where(edge => edge.Type == EdgeType.Walk)
           .Should()
           .OnlyContain(edge => Pathfinder.CanMove(edge.Start, edge.End) && Pathfinder.IsWalkable(edge.End));
    }

    /// <summary>
    ///     Recall off means no recall anywhere on the route rather than none out of the start. The recall grafted onto every
    ///     arrival node is the same move as the one out of the start, and a caller that cannot recall here cannot recall two
    ///     maps along either - so a route that leans on one mid-way has to be found without.
    /// </summary>
    [Test]
    public async Task RecallOffSuppressesEveryRecallOnTheRoute()
    {
        //a route whose cheapest shape recalls after a door rather than at the start, so the flag has something to
        //suppress that the start's own connector never covered
        var start = new Location("woffice", -73, -83);
        var end = new Destination(new Location("cyberland", 202, -157), 40);

        var withRecall = await Pathfinder.FindPathAsync(start, [end])
                                         .ToArrayAsync();

        withRecall.Where((edge, i) => (i > 0) && (edge.Type == EdgeType.Town) && (withRecall[i - 1].Type != EdgeType.Walk))
                  .Should()
                  .NotBeEmpty($"the premise is a recall taken mid-route: {string.Join(" ", withRecall.Select(edge => edge.Type))}");

        var withoutRecall = await Pathfinder.FindPathAsync(start, [end], false)
                                            .ToArrayAsync();

        withoutRecall.Should()
                     .NotContain(edge => edge.Type == EdgeType.Town);

        withoutRecall.Last()
                     .End
                     .Map
                     .Should()
                     .Be("cyberland");

        withoutRecall.Last()
                     .End
                     .Distance(end)
                     .Should()
                     .BeLessThanOrEqualTo(end.Radius + 0.01f);
    }

    /// <summary>
    ///     Two searches at once do not share state: scratch is per thread, and the graph is never written after Initialize.
    ///     Each search checks its own answer against the same search run alone.
    /// </summary>
    [Test]
    public async Task SearchesRunConcurrently()
    {
        var start = new Location("main", -1582, 496);

        Destination[] ends =
        [
            new(new Location("halloween", 0, 0), 0),
            new(new Location("winter_cave", -84, 0), 0)
        ];

        var alone = ends.Select(end => Pathfinder.FindPath(start, [end])
                                                 .Sum(edge => edge.Cost))
                        .ToArray();

        var together = await Task.WhenAll(
            Enumerable.Range(0, 16)
                      .Select(i => Task.Run(() => Pathfinder.FindPath(start, [ends[i % 2]])
                                                            .Sum(edge => edge.Cost))));

        for (var i = 0; i < together.Length; i++)
            together[i]
                .Should()
                .BeApproximately(alone[i % 2], 0.01f);
    }

    /// <summary>
    ///     Two ends landing in one triangle is an ordinary set rather than a caller's mistake: any two destinations closer
    ///     together than the triangulation is fine share a triangle, and a monster with overlapping spawn areas hands over
    ///     exactly that - fireroamer's two on desertland are the pair that found this.
    /// </summary>
    /// <remarks>
    ///     The lookup from the mesh back to the destination used to be built straight off a key selector, so the second end
    ///     sharing one threw a duplicate key. Callers cannot tell that throw from the one the search makes for a destination
    ///     nothing walks to, so a monster standing on open ground reported as unreachable.
    /// </remarks>
    [Test]
    public async Task SeveralEndsInOneTriangleIsAPathRatherThanAThrow()
    {
        var start = new Location("main", 0, 0);
        var endLoc = new Location("main", 0, 200);

        //the premise, asserted rather than assumed: a mesh that told these two apart would leave the test passing
        //for the wrong reason. A unit apart on open ground is far below what the triangulation resolves
        var mesh = Pathfinder.GetNavMesh("main")!;
        var neighbour = new Location("main", 1, 200);

        mesh.Mesh
            .TriangleAt(neighbour.X, neighbour.Y)
            .Should()
            .Be(mesh.Mesh.TriangleAt(endLoc.X, endLoc.Y));

        var path = await Pathfinder.FindPathAsync(
                                       start,
                                       [
                                           new Destination(endLoc, 0),
                                           new Destination(neighbour, 0)
                                       ])
                                   .ToArrayAsync();

        path.Should()
            .NotBeEmpty();

        //and the walk ends on one of the two rather than on whatever the search happened to close on last
        path.Last()
            .End
            .Distance(endLoc)
            .Should()
            .BeLessThan(2f);
    }

    /// <summary>
    ///     The corner half of the same debt: a bend the pull kept is a bend worth walking to. Restated independently of the
    ///     pull by stepping along every leg of a walk run - a cut from there straight to any later corner of the run that the
    ///     server would allow may not save more than a step over walking the corners.
    /// </summary>
    [Test]
    public async Task TightenedPathLeavesNoMidLegShortcutWorthTaking()
    {
        var start = new Location("main", -1582, 496);

        //the same three route shapes the connectedness test walks: same map, across a door, across a town recall
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

            for (var i = 0; (i + 1) < path.Length; i++)
            {
                if (path[i].Type != EdgeType.Walk)
                    continue;

                var runEnd = i;

                while (((runEnd + 1) < path.Length) && (path[runEnd + 1].Type == EdgeType.Walk))
                    runEnd++;

                if (runEnd == i)
                    continue;

                var legStart = path[i].Start;
                var bend = path[i].End;
                var legLength = legStart.Distance(bend);

                //the rule restated independently of the pull: stepping along the leg every 5 units, no standable
                //point with a clear line to any later corner of this walk run should save more than a step over
                //walking the corners
                for (var offset = 5f; offset < legLength; offset += 5f)
                {
                    var candidate = new Location(legStart.Map, legStart.OffsetTowards(bend, offset));

                    if (!Pathfinder.IsWalkable(candidate) || !Pathfinder.CanMove(legStart, candidate))
                        continue;

                    for (var j = runEnd; j > i; j--)
                    {
                        var target = path[j].End;

                        if (!Pathfinder.CanMove(candidate, target))
                            continue;

                        var currentCost = legLength - offset;

                        for (var k = i + 1; k <= j; k++)
                            currentCost += path[k]
                                           .Start
                                           .Distance(path[k].End);

                        var saving = currentCost - candidate.Distance(target);

                        saving.Should()
                              .BeLessThanOrEqualTo(
                                  1.1f,
                                  $"edge {i} of the route to {endLoc} left a {saving:F1} unit cut to corner {j}, {offset} units in");

                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     What the funnel owes its caller: every walk leg is one the server accepts, consecutive walks hand off exactly, and
    ///     no pair of them is left that a straight line could still have collapsed. No leg count is pinned - that would only
    ///     fix whatever the pull happens to do today.
    /// </summary>
    [Test]
    public async Task WalkLegsAreConnectedAndClear()
    {
        var start = new Location("main", -1582, 496);

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
                if (path[i].Type == EdgeType.Walk)
                    Pathfinder.CanMove(path[i].Start, path[i].End)
                              .Should()
                              .BeTrue($"edge {i} of the route to {endLoc} is a walk the funnel emitted");

                if ((i > 0) && (path[i].Type == EdgeType.Walk) && (path[i - 1].Type == EdgeType.Walk))
                    path[i - 1]
                        .End
                        .Distance(path[i].Start)
                        .Should()
                        .BeApproximately(0f, 0.01f, $"edge {i - 1} of the route to {endLoc} has to hand off to edge {i}");
            }

            //nothing a further straight line could have merged is left behind. The last walk before a non-walk edge
            //is exempt, which is why the pair has to be followed by a third walk: the walk into a door is retargeted
            //to stop where the door opens, so that pair can read as mergeable without the pull ever being offered it
            for (var i = 0; (i + 2) < path.Length; i++)
            {
                if ((path[i].Type != EdgeType.Walk) || (path[i + 1].Type != EdgeType.Walk) || (path[i + 2].Type != EdgeType.Walk))
                    continue;

                Pathfinder.CanMove(path[i].Start, path[i + 1].End)
                          .Should()
                          .BeFalse($"edges {i} and {i + 1} of the route to {endLoc} would have collapsed into one");
            }
        }
    }
}