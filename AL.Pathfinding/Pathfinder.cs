using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Model;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Chaos.Core.Extensions;
using Common.Logging;

namespace AL.Pathfinding
{
    /// <summary>
    ///     Provides static access to pathfinding methods in <see cref="DirectedGraph" />.
    /// </summary>
    public static class Pathfinder
    {
        internal static DirectedGraph DirectedGraph = null!;
        private static readonly string[] IGNORED_MAPS =
        {
            "A/B Testing",
            "New Town!",
            "Test"
        };

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Pathfinder).FullName);

        /// <summary>
        ///     Determines whether or not it's possible to move from one location to another.
        /// </summary>
        /// <param name="mapAccessor">The map to check against.</param>
        /// <param name="start">The starting point.</param>
        /// <param name="end">The ending point.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if you can move from <paramref name="start" /> to <paramref name="end" />, otherwise <c>false</c>.
        /// </returns>
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

        /// <summary>
        ///     Determines whether or not it's possible to move from one location to another.
        /// </summary>
        /// <param name="start">The starting location.</param>
        /// <param name="end">The ending location.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if you can move from <paramref name="start" /> to <paramref name="end" />, otherwise <c>false</c>.
        /// </returns>
        public static bool CanMove(ILocation start, ILocation end) => DirectedGraph.CanMove(start, end);

        /// <inheritdoc cref="GraphBase{TMesh,TNode,TEdge}.FindPathAsync" />
        public static IAsyncEnumerable<GraphEdge> FindPathAsync<T>(ILocation start, IEnumerable<T> ends, bool useTownIfOptimal = true)
            where T: ILocation, ICircle =>
            //do other things
            DirectedGraph.FindPathAsync(start, ends, useTownIfOptimal);

        /// <summary>
        ///     Retreives the navmesh associated with a given map.
        /// </summary>
        /// <param name="name">The map whose navmesh to retreive.</param>
        /// <returns>
        ///     <see cref="NavMesh" /> <br />
        ///     The navmesh for the given map.
        /// </returns>
        public static NavMesh? GetNavMesh(string name) => DirectedGraph.NavMeshes.TryGetValue(name, out var mesh) ? mesh : null;

        /// <summary>
        ///     Initializes pathfinding, building navmeshes for all maps and constructing a directed graph from them
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static async Task InitializeAsync()
        {
            var navMeshes = new AwaitableDictionary<GMap, NavMesh>();

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

        /// <summary>
        ///     Checks if a location is a wall.
        /// </summary>
        /// <param name="location">The location to check.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the location is a wall, otherwise <c>false</c>.
        /// </returns>
        public static bool IsWall(ILocation location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            var mesh = GetNavMesh(location.Map) ?? throw new InvalidOperationException($"No mesh found for the map \"{location.Map}\"");

            return mesh.IsWall(location);
        }

        private static NavMesh? TryBuildNavMesh(string name, GMap map)
        {
            var geometry = GameData.Geometry[name];

            if ((geometry == null) || (geometry.VerticalLines.Count == 0) || (geometry.HorizontalLines.Count == 0))
            {
                Logger.Debug($"Ignored {name}");

                return null;
            }

            var generator = new NavMeshBuilder(map, geometry);
            var navMesh = generator.Build();
            Logger.Debug($"Prepared {name}");

            return navMesh;
        }
    }
}