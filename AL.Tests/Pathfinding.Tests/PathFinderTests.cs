using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding;
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
        private static readonly ILog Logger = LogManager.GetLogger<PathFinderTests>();
        private static readonly Point StartPoint = new(-1582, 496);
        private readonly List<Point> EndPoints = new() { new Point(1891, -47) };
        private static readonly Map StartMap = GameData.Maps["main"];
        private static readonly Map EndMap = GameData.Maps["winter_cave"];
        private static NavMesh NavMesh;
        private static WorldMesh WorldMesh;
        private static readonly SemaphoreSlim Sync = new(1, 1);

        [ClassInitialize]
        public static async Task Init(TestContext context)
        {
            await PathFinder.InitializeAsync();
            NavMesh = await PathFinder.NavMeshes[GameData.Maps["main"].Key];
            WorldMesh = PathFinder.WorldMesh;
        }
        
        [TestInitialize]
        public async Task TestSetup() => await Sync.WaitAsync().ConfigureAwait(false);

        [TestCleanup]
        public void TestCleanup() => Sync.Release();

        [TestMethod]
        public async Task FindRouteTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var route = await WorldMesh.FindRoute(StartMap, new[] { EndMap }).ToArrayAsync();
            
            timer.Stop();
            Logger.Debug($"Route found from main to winter_cave in {timer.ElapsedMilliseconds}");
            Logger.Trace(string.Join(Environment.NewLine, route.Select(connector => connector.End.Key)));
            
            Assert.IsTrue(route.Any());
        }
        
        [TestMethod]
        public async Task FindPathTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var path = await NavMesh.FindPath(StartPoint, EndPoints).ToArrayAsync();

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindAnyPathTest()
        {
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var path = await NavMesh.FindPath(EndPoints.First(), possibleEnds).ToArrayAsync();

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");
            
            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindAnyPathWithDistanceTest()
        {
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var path = await NavMesh.FindPath(EndPoints.First(), possibleEnds, 200).ToArrayAsync();

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");
            
            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindPathWithDistanceTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var path = await NavMesh.FindPath(StartPoint, EndPoints, 200).ToArrayAsync();

            timer.Stop();
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");
            
            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task FindPathWithDistanceBenchTest()
        {
            var count = 1000;
            var timer = new Stopwatch();
            timer.Start();

            var pathGenerator = Enumerable.Range(0, count)
                .ToAsyncEnumerable()
                .SelectMany(_ => NavMesh.FindPath(StartPoint, EndPoints, 200));

            var i = 0;
            await foreach (var _ in pathGenerator)
                i++;

            timer.Stop();
            Logger.Debug($"{count} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");
            
            Assert.IsTrue(i > count);
        }

        [TestMethod]
        public async Task FindPathBenchTest()
        {
            var count = 10000;
            var timer = new Stopwatch();
            timer.Start();

            var pathGenerator = Enumerable.Range(0, count)
                .ToAsyncEnumerable()
                .SelectMany(_ => NavMesh.FindPath(StartPoint, EndPoints));

            var i = 0;
            await foreach (var _ in pathGenerator)
                i++;

            timer.Stop();
            Logger.Debug($"{count} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");
            
            Assert.IsTrue(i > count);
        }

        [TestMethod]
        public async Task FindAnyPathBenchTest()
        {
            var count = 1000;
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var pathGenerator = Enumerable.Range(0, count)
                .ToAsyncEnumerable()
                .SelectMany(_ => NavMesh.FindPath(EndPoints.First(), possibleEnds));

            var i = 0;
            await foreach (var _ in pathGenerator)
                i++;

            timer.Stop();
            Logger.Debug($"{count} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");
            
            Assert.IsTrue(i > count);
        }

        [TestMethod]
        public async Task FindAnyPathWithDistanceBenchTest()
        {
            var count = 1000;
            var timer = new Stopwatch();
            var possibleEnds = new[] { new Point(-1250f, -163f), StartPoint };

            timer.Start();

            var pathGenerator = Enumerable.Range(0, count)
                .ToAsyncEnumerable()
                .SelectMany(_ => NavMesh.FindPath(EndPoints.First(), possibleEnds, 200));

            var i = 0;
            await foreach (var _ in pathGenerator)
                i++;

            timer.Stop();
            Logger.Debug($"{count} Paths found in {timer.ElapsedMilliseconds}ms");
            Logger.Debug($"{i} nodes traversed");
            
            Assert.IsTrue(i > count);
        }

        [TestMethod]
        public async Task VisualizePathTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var path = await NavMesh.FindPath(StartPoint, EndPoints, smoothPath: true, useTownIfOptimal: true, distance: 0)
                .ToArrayAsync();

            timer.Stop();
            var current = new Point(StartPoint.X + NavMesh.XOffset, StartPoint.Y + NavMesh.YOffset);

            var image = Visualizer.Visualizer.CreateGridImage(NavMesh.PointMap)
                .DrawConnections(NavMesh)
                .DrawPath(new[] { current }.Concat(path.Select(connector => connector.End)));

            await image.SaveAsync(@"images\singlePath.png");
            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");

            Assert.IsTrue(path.Any());
        }

        [TestMethod]
        public async Task VisualizePathWithDistanceTest()
        {
            var timer = new Stopwatch();
            timer.Start();

            var path = await NavMesh.FindPath(StartPoint, EndPoints, 500).ToArrayAsync();

            timer.Stop();
            var current = new Point(StartPoint.X + NavMesh.XOffset, StartPoint.Y + NavMesh.YOffset);

            var image = Visualizer.Visualizer.CreateGridImage(NavMesh.PointMap)
                .DrawConnections(NavMesh)
                .DrawPath(new[] { current }.Concat(path.Select(connector => connector.End)));

            Logger.Debug($"Path found from {StartPoint} to {path.Last().End} in {timer.ElapsedMilliseconds}ms");
            await image.SaveAsync(@"images\singlePathDistance.png");
            
            Assert.IsTrue(path.Any());
        }
    }
}