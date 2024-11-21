#region
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AL.Core.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using Common.Logging;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.Pathfinding.Tests;

[TestClass]
public class DirectedGraphTests : PathfindingTestBed
{
    private readonly ILog Logger = LogManager.GetLogger<DirectedGraphTests>();

    [TestMethod]
    public async Task FindPathFromTownNodeTest()
    {
        var start = new Location("bank", 0, -37);
        var endLoc = new Location("spookytown", 0, 0);
        var end = new Destination(endLoc, 0);

        var path = await Pathfinder.FindPathAsync(start, [end])
                                   .ToArrayAsync();

        path.Should()
            .NotContain(p => p.Type == EdgeType.Town);
    }

    [TestMethod]
    public async Task FindPathMultiMapBenchTest()
    {
        var start = new Location("main", -1582, 496);
        var endLoc = new Location("spookytown", 0, 0);
        var end = new Destination(endLoc, 0);

        //jit it!
        // ReSharper disable once RedundantAssignment
        var path = await Pathfinder.FindPathAsync(start, [end])
                                   .ToArrayAsync();

        var timer = Stopwatch.StartNew();

        path = await Pathfinder.FindPathAsync(start, [end])
                               .ToArrayAsync();

        timer.Stop();
        var elapsed = timer.ElapsedMilliseconds;
        var builder = new StringBuilder();

        foreach (var edge in path)
            builder.AppendLine(edge.ToString());

        Logger.Info($"Finding path from {start} to {endLoc}");
        Logger.Info(builder);
        Logger.Info($"Found path to sppokytown in {elapsed}ms");

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

    [TestMethod]
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

    [TestMethod]
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