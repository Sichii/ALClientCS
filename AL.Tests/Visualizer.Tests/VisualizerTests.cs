using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
using AL.Visualizer;
using AL.Visualizer.Extensions;
using Chaos.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;

namespace AL.Tests.Visualizer.Tests
{
    [TestClass]
    public class VisualizerTests : PathfindingTestBed
    {
        [TestMethod]
        public async Task DrawPathTest()
        {
            var start = new Location("main", -1582, 496);
            var endLoc = new Location("spookytown", 0, 0);
            var end = new Destination(endLoc, 0);

            //jit it!
            var path = await Pathfinder.FindPathAsync(start, new[] { end }).ToArrayAsync();

            var images = AL.Visualizer.Visualizer.DrawPath<DirectedGraph, NavMesh, GraphNode, GraphEdge>(Pathfinder.DirectedGraph,
                path.ToAsyncEnumerable());

            var imgPath = $@"{AssemblyInit.PATH_IMAGES_DIR}img";
            var counter = 1;

            await foreach (var image in images)
                await image.SaveAsync($"{imgPath}{counter++}.png");
        }

        [TestMethod]
        public async Task DumpMapImages()
        {
            foreach (var map in GameData.Maps.Values.DistinctBy(map => map.Accessor))
            {
                var navMesh = Pathfinder.GetNavMesh(map.Accessor);

                if (navMesh == null)
                    continue;

                var image = AL.Visualizer.Visualizer.CreateGridImage(navMesh).DrawEdges(navMesh);
                await image.SaveAsync($@"{AssemblyInit.IMAGE_DIR}\{map.Accessor}.png").ConfigureAwait(false);
            }
        }
    }
}