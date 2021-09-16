using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AL.Core.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using Common.Logging;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AL.Tests.Pathfinding.Tests
{
    [TestClass]
    public class DirectedGraphTests : PathfindingTestBed
    {
        private ILog Logger = LogManager.GetLogger<DirectedGraphTests>();
        
        [TestMethod]
        public async Task FindPathSingleMapTest()
        {
            var start = new Location("main", -1582, 496);
            var endLoc = new Location("main", 1891, -47);
            var end = new Destination(endLoc, 0);

            var path = await Pathfinder2.FindPathAsync(start, new[] { end }).ToArrayAsync();

            path.Should().ContainSingle(p => p.Type == ConnectorType.Town);
            path.Should().Contain(p => p.Type == ConnectorType.Door);
            path.Should().Contain(p => p.Type == ConnectorType.Walk);
            path.Should().NotContain(p => (p.Type == ConnectorType.Transport) || (p.Type == ConnectorType.Leave));
            path.First().Start.Vertex.Should().Be(start);
            path.Last().End.Vertex.Should().Be(end);
        }

        [TestMethod]
        public async Task FindPathMultiMapTest()
        {
            var start = new Location("main", -1582, 496);
            var endLoc = new Location("winter_cave", -84, 0);
            var end = new Destination(endLoc, 0);
            
            var path = await Pathfinder2.FindPathAsync(start, new[] { end }).ToArrayAsync();

            path.Should().ContainSingle(p => p.Type == ConnectorType.Town);
            path.Should().ContainSingle(p => p.Type == ConnectorType.Transport);
            path.Should().ContainSingle(p => p.Type == ConnectorType.Door);
            path.Should().Contain(p => p.Type == ConnectorType.Door);
            path.Should().NotContain(p => p.Type == ConnectorType.Leave);
            path.First().Start.Vertex.Should().Be(start);
            path.Last().End.Vertex.Should().Be(end);
        }
        
        [TestMethod]
        public async Task FindPathMultiMapBenchTest()
        {
            var start = new Location("winter_cave", -84, 0);
            var endLoc = new Location("halloween", 170, -129);
            var end = new Destination(endLoc, 0);
            
            //jit it!
            var path = await Pathfinder2.FindPathAsync(start, new[] { end }).ToArrayAsync();

            var timer = Stopwatch.StartNew();

            for (var i = 0; i < 1000; i++)
                path = await Pathfinder2.FindPathAsync(start, new[] { end }).ToArrayAsync();

            timer.Stop();
            var elapsed = timer.ElapsedMilliseconds;
            Logger.Info($"Found 1000 paths to halloween in {elapsed}ms");

            path.Should().Contain(p => p.Type == ConnectorType.Door);
            path.Should().NotContain(p => p.Type == ConnectorType.Leave);
            path.First().Start.Vertex.Should().Be(start);
            path.Last().End.Vertex.Should().Be(end);
        }
    }
}