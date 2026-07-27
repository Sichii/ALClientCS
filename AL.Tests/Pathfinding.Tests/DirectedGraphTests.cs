#region
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