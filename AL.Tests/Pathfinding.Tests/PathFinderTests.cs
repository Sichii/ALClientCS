using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Visualizer.Extensions;
using Chaos.Core.Extensions;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using NavVisualizer = AL.Visualizer.Visualizer;
using Point = AL.Core.Geometry.Point;

namespace AL.Tests.Pathfinding.Tests
{
    [TestClass]
    public class PathFinderTests
    {
        private const int COUNT = 100;
        private static readonly ILog Logger = LogManager.GetLogger<PathFinderTests>();
        private static readonly IPoint StartPoint = new Point(-1582, 496);
        private static readonly SemaphoreSlim Sync = new(1, 1);
        private readonly List<IPoint> EndPoints = new() { new Point(1891, -47) };

        [TestMethod]
        public async Task PathfindToEnclosedPointTest()
        {
            var startLocation = new Location("main", 0, 0);
            var location = new Location("main", -35, -162);

            var path = await PathFinder.FindPath("main", startLocation, new[] { new MapCircle(location, 400) }, true).ToArrayAsync();

            Assert.IsFalse(path.Any());
        }
        
        [TestMethod]
        public async Task FindAnyPathBenchTest()
        {
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var pathGenerator = Enumerable.Range(0, COUNT)
                .ToAsyncEnumerable()
                .SelectMany(_ => PathFinder.FindPath("main", EndPoints.First(), possibleEnds.Select(end => new Circle(end, 0))));

            var i = 0;

            await foreach (var _ in pathGenerator.ConfigureAwait(false))
                i++;

            timer.Stop();
            Logger.Debug($"{COUNT} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");

            Assert.IsTrue(i > COUNT);
        }

        [TestMethod]
        public async Task FindAnyPathTest()
        {
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var path = await PathFinder.FindPath("main", EndPoints.First(), possibleEnds.Select(end => new Circle(end, 0)))
                .ToArrayAsync()
                .ConfigureAwait(false);

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindAnyPathWithDistanceBenchTest()
        {
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var pathGenerator = Enumerable.Range(0, COUNT)
                .ToAsyncEnumerable()
                .SelectMany(_ => PathFinder.FindPath("main", EndPoints.First(), possibleEnds.Select(end => new Circle(end, 200))));

            var i = 0;

            await foreach (var _ in pathGenerator.ConfigureAwait(false))
                i++;

            timer.Stop();
            Logger.Debug($"{COUNT} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");

            Assert.IsTrue(i > COUNT);
        }

        [TestMethod]
        public async Task FindAnyPathWithDistanceTest()
        {
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var path = await PathFinder.FindPath("main", EndPoints.First(), possibleEnds.Select(end => new Circle(end, 200)))
                .ToArrayAsync()
                .ConfigureAwait(false);

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindNonTownPathTest()
        {
            var point1 = new Point(-887, -80);
            var point2 = new Point(-681, 1553);
            var endCircle = new Circle(point2, 100);
            var path = await PathFinder.FindPath("main", point1, new[] { endCircle }).ToArrayAsync();

            Assert.IsTrue(path.All(c => c.Type != ConnectorType.Town));
        }

        [TestMethod]
        public async Task FindPathBenchTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var pathGenerator = Enumerable.Range(0, COUNT)
                .ToAsyncEnumerable()
                .SelectMany(_ => PathFinder.FindPath("main", StartPoint, EndPoints.Select(end => new Circle(end, 0))));

            var i = 0;

            await foreach (var _ in pathGenerator.ConfigureAwait(false))
                i++;

            timer.Stop();
            Logger.Debug($"{COUNT} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");

            Assert.IsTrue(i > COUNT);
        }

        [TestMethod]
        public async Task FindPathTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var path = await PathFinder.FindPath("main", StartPoint, EndPoints.Select(end => new Circle(end, 0)))
                .ToArrayAsync()
                .ConfigureAwait(false);

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindPathWithDistanceBenchTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var pathGenerator = Enumerable.Range(0, COUNT)
                .ToAsyncEnumerable()
                .SelectMany(_ => PathFinder.FindPath("main", StartPoint, EndPoints.Select(end => new Circle(end, 200))));

            var i = 0;

            await foreach (var _ in pathGenerator.ConfigureAwait(false))
                i++;

            timer.Stop();
            Logger.Debug($"{COUNT} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");

            Assert.IsTrue(i > COUNT);
        }

        [TestMethod]
        public async Task FindPathWithDistanceTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var path = await PathFinder.FindPath("main", StartPoint, EndPoints.Select(end => new Circle(end, 200)))
                .ToArrayAsync()
                .ConfigureAwait(false);

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindRouteTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var route = await PathFinder.FindRoute("jail", "winter_cave").ToArrayAsync().ConfigureAwait(false);

            timer.Stop();
            Logger.Debug($"Route found from main to winter_cave in {timer.ElapsedMilliseconds}");
            Logger.Debug(string.Join(Environment.NewLine, route.Select(connector => connector.End.Accessor)));

            Assert.IsTrue(route.Any());
        }

        [TestMethod]
        public async Task SmartMoveTest()
        {
            var route = PathFinder.FindRoute("main", "winterland");
            var startingPoint = new Point(-950, 52);
            var index = 0;

            await foreach (var map in route.ConfigureAwait(false))
            {
                var navMesh = PathFinder.GetNavMesh(map.Start.Accessor);
                var exits = map.Start.Exits.Where(exit => exit.ToLocation.Map.EqualsI(map.End.Accessor)).ToArray();
                var path = await PathFinder.FindPath(map.Start.Accessor, startingPoint, exits).ToArrayAsync().ConfigureAwait(false);

                await NavVisualizer.CreateGridImage(navMesh!)
                    .DrawConnections(navMesh!)
                    .DrawPath(navMesh!, path)
                    .SaveAsync($@"images\sMoveTest{index++}.png")
                    .ConfigureAwait(false);

                startingPoint = path.Last().End;

                var closestExit = exits.OrderBy(exit => exit.Distance(startingPoint)).First();
                Logger.Trace($"Distance from exit: {closestExit.Distance(startingPoint)}");
                startingPoint = closestExit.Center.ToPoint();
            }
        }

        [TestCleanup]
        public void TestCleanup() => Sync.Release();

        [TestInitialize]
        public async Task TestSetup() => await Sync.WaitAsync().ConfigureAwait(false);
    }
}