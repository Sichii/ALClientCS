using System.Linq;
using System.Threading.Tasks;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Visualizer.Extensions;
using Chaos.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using NavVisualizer = AL.Visualizer.Visualizer;
using Point = AL.Core.Geometry.Point;

namespace AL.Tests.Visualizer.Tests
{
    [TestClass]
    public class VisualizerTests
    {
        [TestMethod]
        public async Task DumpMapImages()
        {
            foreach (var map in GameData.Maps.Values.DistinctBy(map => map.Accessor))
            {
                var navMesh = PathFinder.GetNavMesh(map.Accessor);

                if (navMesh == null)
                    continue;

                var image = NavVisualizer.CreateGridImage(navMesh).DrawConnections(navMesh);
                await image.SaveAsync($@"images\{map.Accessor}.png");
            }
        }

        //[TestMethod]
        public async Task VisualizePathTest()
        {
            var startPoint = new Point(0, 0);
            var endPoints = new[] { new Point(0, 0) };

            var path = await PathFinder.FindPath("main", startPoint, endPoints.Select(end => new Circle(end, 0))).ToArrayAsync();

            var navMesh = PathFinder.GetNavMesh("main")!;

            var image = NavVisualizer.CreateGridImage(navMesh).DrawConnections(navMesh).DrawPath(navMesh, path);

            await image.SaveAsync(@"images\singlePath.png");
        }
    }
}