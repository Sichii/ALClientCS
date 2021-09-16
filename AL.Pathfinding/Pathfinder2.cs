using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Model;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Chaos.Core.Extensions;
using Common.Logging;

namespace AL.Pathfinding
{
    public static class Pathfinder2
    {
        private static readonly string[] IGNORED_MAPS =
        {
            "A/B Testing",
            "New Town!",
            "Test"
        };
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Pathfinder2).FullName);
        internal static DirectedGraph DirectedGraph;

        public static bool CanMove(string mapAccessor, IPoint start, IPoint end)
        {
            if (string.IsNullOrEmpty(mapAccessor))
                throw new ArgumentNullException(nameof(mapAccessor));

            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (end == null)
                throw new ArgumentNullException(nameof(end));

            return DirectedGraph.CanMove(mapAccessor, start, end);
        }

        public static bool CanMove(ILocation start, ILocation end) => DirectedGraph.CanMove(start, end);

        public static IAsyncEnumerable<GraphEdge> FindPathAsync(
            ILocation start,
            IEnumerable<Destination> ends,
            bool useTownIfOptimal = true) =>
            //do other things
            DirectedGraph.FindPathAsync(start, ends, useTownIfOptimal);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static async Task InitializeAsync()
        {
            var navMeshes = new AwaitableDictionary<GMap, NavMesh2>();

            var maps = GameData.Maps.DistinctBy(kvp => kvp.Value.Accessor)
                .Where(kvp => kvp.Value.Ignore == false)
                .Where(kvp => !IGNORED_MAPS.ContainsI(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var timer = Stopwatch.StartNew();
            Logger.Info("Preparing map navigation");

            await maps.ToAsyncEnumerable()
                .AsyncParallelForEach(async kvp =>
                {
                    (var name, var map) = kvp;
                    var navMesh = TryBuildNavMesh(name, map);

                    if (navMesh != null)
                        await navMeshes.AddAsync(map, navMesh).ConfigureAwait(false);
                }, Convert.ToInt32(Environment.ProcessorCount * 1.5))
                .ConfigureAwait(false);

            timer.Stop();
            Logger.Info($"Prepared maps in {timer.ElapsedMilliseconds}ms");

            var navs = await navMeshes.ToDictionaryAsync(kvp => kvp.Key.Accessor, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)
                .ConfigureAwait(false);

            DirectedGraph = new DirectedGraph(navs);
        }
        
        private static NavMesh2? TryBuildNavMesh(string name, GMap map)
        {
            var geometry = GameData.Geometry[name];

            if ((geometry == null) || (geometry.VerticalLines.Count == 0) || (geometry.HorizontalLines.Count == 0))
            {
                Logger.Debug($"Ignored {name}");

                return null;
            }

            var generator = new NavMeshBuilder2(map, geometry);
            var navMesh = generator.Build();
            Logger.Debug($"Prepared {name}");

            return navMesh;
        }
    }
}