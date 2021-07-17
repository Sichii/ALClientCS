using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.Core.Interfaces;
using AL.Pathfinding;
using AL.Pathfinding.Interfaces;
using AL.Visualizer.Extensions;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using Point = AL.Core.Geometry.Point;

namespace AL.Tests.Pathfinding.Tests
{
    [TestClass]
    public class PathFinderTests
    {
        private const int COUNT = 1000;
        private static readonly ILog Logger = LogManager.GetLogger<PathFinderTests>();
        private static readonly IPoint StartPoint = new Point(-1582, 496);
        private static readonly SemaphoreSlim Sync = new(1, 1);
        private readonly List<IPoint> EndPoints = new() { new Point(1891, -47) };

        [TestMethod]
        public async Task FindAnyPathBenchTest()
        {
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var pathGenerator = Enumerable.Range(0, COUNT)
                .ToAsyncEnumerable()
                .SelectMany(_ => PathFinder.FindPath("main", EndPoints.First(), possibleEnds));

            var i = 0;
            await foreach (var _ in pathGenerator)
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

            var path = await PathFinder.FindPath("main", EndPoints.First(), possibleEnds).ToArrayAsync();

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
                .SelectMany(_ => PathFinder.FindPath("main", EndPoints.First(), possibleEnds, 200));

            var i = 0;
            await foreach (var _ in pathGenerator)
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

            var path = await PathFinder.FindPath("main", EndPoints.First(), possibleEnds, 200).ToArrayAsync();

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindPathBenchTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var pathGenerator = Enumerable.Range(0, COUNT)
                .ToAsyncEnumerable()
                .SelectMany(_ => PathFinder.FindPath("main", StartPoint, EndPoints));

            var i = 0;
            await foreach (var _ in pathGenerator)
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

            var path = await PathFinder.FindPath("main", StartPoint, EndPoints).ToArrayAsync();

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
                .SelectMany(_ => PathFinder.FindPath("main", StartPoint, EndPoints, 200));

            var i = 0;
            await foreach (var _ in pathGenerator)
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

            var path = await PathFinder.FindPath("main", StartPoint, EndPoints, 200).ToArrayAsync();

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindRouteTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var route = await PathFinder.FindRoute("main", new[] { "winter_cave" }).ToArrayAsync();

            timer.Stop();
            Logger.Debug($"Route found from main to winter_cave in {timer.ElapsedMilliseconds}");
            Logger.Trace(string.Join(Environment.NewLine, route.Select(connector => connector.End.Key)));

            Assert.IsTrue(route.Any());
        }

        [ClassInitialize]
        public static async Task Init(TestContext context) => await PathFinder.InitializeAsync();

        [TestCleanup]
        public void TestCleanup() => Sync.Release();

        [TestInitialize]
        public async Task TestSetup() => await Sync.WaitAsync().ConfigureAwait(false);

        [TestMethod]
        public async Task VisualizePathTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var path = await PathFinder
                .FindPath("main", StartPoint, EndPoints, smoothPath: true, useTownIfOptimal: true, distance: 0)
                .ToArrayAsync();

            timer.Stop();
            var navMesh = PathFinder.GetNavMesh("main")!;

            var image = Visualizer.Visualizer.CreateGridImage(navMesh)
                .DrawConnections(navMesh)
                .DrawPath<IConnector<Point>, Point>(path);

            await image.SaveAsync(@"images\singlePath.png");
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task VisualizePathWithDistanceTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var navMesh = PathFinder.GetNavMesh("main")!;
            var path = await PathFinder.FindPath("main", StartPoint, EndPoints, 500).ToArrayAsync();

            timer.Stop();
            var image = Visualizer.Visualizer.CreateGridImage(navMesh)
                .DrawConnections(navMesh)
                .DrawPath<IConnector<Point>, Point>(path);

            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");
            await image.SaveAsync(@"images\singlePathDistance.png");

            Assert.IsTrue(path.Any());
        }
    }
}