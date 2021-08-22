using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Interfaces;
using AL.Pathfinding.Model;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Chaos.Core.Extensions;
using Common.Logging;

namespace AL.Pathfinding
{
    /// <summary>
    ///     Provides the ability to pathfind from/to anywhere in the game world.
    /// </summary>
    public static class PathFinder
    {
        private static readonly string[] IGNORED_MAPS =
        {
            "A/B Testing",
            "New Town!",
            "Test"
        };
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PathFinder).FullName);

        /// <summary>
        ///     A <see cref="Abstractions.GraphBase{TNode, TEdge}" /> implementation for the world.
        /// </summary>
        public static WorldMesh WorldMesh { get; private set; } = null!;
        private static Dictionary<string, NavMesh> NavMeshes { get; } = new(StringComparer.OrdinalIgnoreCase);

        private static void BuildWorldMesh(Dictionary<string, GMap> maps)
        {
            var nodeDic = new Dictionary<GMap, GraphNode<GMap>>();
            var index = 0;

            foreach ((_, var map) in maps)
                nodeDic.Add(map, new GraphNode<GMap>(map, index++));

            var main = GameData.Maps["main"];
            var mainNode = nodeDic[main!];

            foreach ((var map, var node) in nodeDic)
            {
                foreach (var exit in map.Exits)
                {
                    var exitMap = GameData.Maps[exit.ToLocation.Map];

                    if (exitMap == null)
                    {
                        var mapName = maps.FirstOrDefault(kvp => kvp.Value == map).Key;

                        Logger.Warn(
                            $"{mapName} has an exit that points to an unknown map.{Environment.NewLine}{exit}{Environment.NewLine}");

                        continue;
                    }

                    if (nodeDic.TryGetValue(exitMap, out var neighbor))
                        node.Neighbors.Add(neighbor);
                    else
                    {
                        var mapName = maps.FirstOrDefault(kvp => kvp.Value == map).Key;

                        Logger.Warn(
                            $"{mapName} has an exit that points to an invalid map.{Environment.NewLine}{exit}{Environment.NewLine}");
                    }
                }

                node.Neighbors = node.Neighbors.DistinctBy(neighbor => neighbor.Edge.Key)
                    .Where(neighbor => !node.Equals(neighbor))
                    .ToList();

                if (map.Irregular)
                    node.Neighbors.Add(mainNode);
            }

            WorldMesh = new WorldMesh(nodeDic);
        }

        /// <summary>
        ///     <inheritdoc cref="Model.NavMesh.CanMove" />
        /// </summary>
        /// <param name="mapAccessor">The accessor of the map to check these points against.</param>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if you can walk directly from <paramref name="start" /> to <paramref name="end" />, otherwise
        ///     <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">No map was found for the accessor "{mapAccessor}".</exception>
        public static bool CanMove(string mapAccessor, IPoint start, IPoint end)
        {
            var map = GameData.Maps[mapAccessor];

            if (map == null)
                throw new InvalidOperationException($"No map was found for the accessor \"{mapAccessor}\".");

            return !NavMeshes.TryGetValue(map.Key, out var navMesh)
                   || navMesh.CanMove(navMesh.ApplyOffset(start), navMesh.ApplyOffset(end));
        }

        /// <summary>
        ///     <inheritdoc cref="Model.NavMesh.CanMove" />
        /// </summary>
        /// <param name="start">The start location.</param>
        /// <param name="end">The end location.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if you can walk directly from <paramref name="start" /> to <paramref name="end" />, otherwise
        ///     <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">No map was found for the accessor "{mapAccessor}".</exception>
        public static bool CanMove(ILocation start, ILocation end) =>
            (start.Map == end.Map) && CanMove(start.Map, start, end);

        /// <param name="mapAccessor">The accessor of the map to find the map on.</param>
        /// <exception cref="ArgumentNullException">mapAccessor</exception>
        /// <exception cref="ArgumentNullException">start</exception>
        /// <exception cref="ArgumentNullException">ends</exception>
        /// <inheritdoc cref="NavMesh.FindPath" />
        /// <exception cref="InvalidOperationException">No map was found for the accessor "{<paramref name="mapAccessor" />}"</exception>
        public static async IAsyncEnumerable<IConnector<Point>> FindPath(
                string mapAccessor,
                // ReSharper disable InvalidXmlDocComment
                IPoint start,
                IEnumerable<ICircle> ends,
                bool smoothPath = true,
                bool useTownIfOptimal = true)
            // ReSharper restore InvalidXmlDocComment
        {
            if (string.IsNullOrEmpty(mapAccessor))
                throw new ArgumentNullException(nameof(mapAccessor));

            if (start == null)
                throw new ArgumentNullException(nameof(start));

            if (ends == null)
                throw new ArgumentNullException(nameof(ends));

            var endPoints = ends.ToArray();

            //if any of the end nodes are equal to the start node... dont need to move
            if ((endPoints.Length == 0) || endPoints.Any(end => end.Equals(start)))
                yield break;

            var map = GameData.Maps[mapAccessor];

            //failed to get map data
            if (map == null)
                throw new InvalidOperationException($"No map was found for the accessor \"{mapAccessor}\".");

            //failed to find a mesh (map is boundless?)
            if (!NavMeshes.TryGetValue(map.Key, out var navMesh))
            {
                //find the closest end from the start
                var bestDistance = float.MaxValue;
                ICircle closestEnd = default!;

                foreach (var endPoint in endPoints)
                {
                    var currentDistance = start.Distance(endPoint) - endPoint.Radius;

                    //if a distance was specified and we're already standing within that distance... yield break
                    if (currentDistance < 0)
                        yield break;

                    if (currentDistance < bestDistance)
                    {
                        bestDistance = currentDistance;
                        closestEnd = endPoint;
                    }
                }

                //if a distance was specified
                if (closestEnd.Radius > 0)
                {
                    //intersect a circle to get the best walkable point
                    var circle = new Circle(closestEnd, closestEnd.Radius);
                    var currentLine = new Line(start, closestEnd);
                    var newEnd = circle.CalculateIntersectionEntryPoint(currentLine)!;

                    yield return new EdgeConnector<Point>
                    {
                        Type = ConnectorType.Walk,
                        Start = start.GetPoint(),
                        End = newEnd.GetPoint(),
                        Heuristic = bestDistance
                    };
                } else
                    //create a connector from the start to the closest end (we're guaranteed to be able to walk on a boundless map)
                    yield return new EdgeConnector<Point>
                    {
                        Type = ConnectorType.Walk,
                        Start = start.GetPoint(),
                        End = closestEnd.GetPoint(),
                        Heuristic = bestDistance
                    };
            } else //navmesh found
            {
                //generate a path with the options specified and yield it
                var path = navMesh.FindPath(start, endPoints, smoothPath, useTownIfOptimal);

                await foreach (var connector in path.ConfigureAwait(false))
                {
                    var edgeConnector = (EdgeConnector<Point>)connector;

                    var unOffset = edgeConnector with
                    {
                        Start = navMesh.RemoveOffset(edgeConnector.Start),
                        End = navMesh.RemoveOffset(edgeConnector.End)
                    };

                    yield return unOffset;
                }
            }
        }

        /// <summary>
        ///     <inheritdoc cref="Model.WorldMesh.FindRoute" />
        /// </summary>
        /// <param name="fromMapAccessor">A starting map.</param>
        /// <param name="endMapAccessors">Any number of end maps. Upon reaching any of the end maps, that path will be returned.</param>
        /// <returns>
        ///     <see cref="IAsyncEnumerable{T}" /> of <see cref="IConnector{TEdge}" /> of <see cref="GMap" /> <br />
        ///     A lazy enumeration of maps along the most optimal path between the start node and the first end node a path is
        ///     found for.
        /// </returns>
        /// <exception cref="ArgumentNullException">fromMapAccessor</exception>
        /// <exception cref="ArgumentNullException">endMapAccessors</exception>
        /// <exception cref="InvalidOperationException">No map was found for the accessor "{fromMapAccessor}"</exception>
        /// <exception cref="InvalidOperationException">No map was found for the accessors {string.Join(", ", endAccessors)}</exception>
        public static async IAsyncEnumerable<IConnector<GMap>> FindRoute(string fromMapAccessor, params string[] endMapAccessors)
        {
            if (string.IsNullOrEmpty(fromMapAccessor))
                throw new ArgumentNullException(nameof(fromMapAccessor));

            if (endMapAccessors == null)
                throw new ArgumentNullException(nameof(endMapAccessors));

            var endAccessors = endMapAccessors.Where(end => !string.IsNullOrEmpty(end)).ToArray();

            if (string.IsNullOrEmpty(fromMapAccessor)
                || (endAccessors.Length == 0)
                || endAccessors.Any(end => end.EqualsI(fromMapAccessor)))
                yield break;

            var startMap = GameData.Maps[fromMapAccessor];
            var endMaps = endAccessors.Select(end => GameData.Maps[end]).Where(map => map != null).ToArray();

            if (startMap == null)
                throw new InvalidOperationException($"No map was found for the accessor \"{fromMapAccessor}\".");

            if (!endMaps.Any())
                throw new InvalidOperationException($"No map was found for the accessors {string.Join(", ", endAccessors)}");

            var route = WorldMesh.FindRoute(startMap, endMaps!);

            await foreach (var routeConnector in route.ConfigureAwait(false))
                yield return routeConnector;
        }

        /// <summary>
        ///     Gets a navmesh for a specific map.
        /// </summary>
        /// <param name="keyOrAccessor"></param>
        /// <returns></returns>
        public static NavMesh? GetNavMesh(string keyOrAccessor)
        {
            if (!NavMeshes.TryGetValue(keyOrAccessor, out var navMesh))
            {
                var map = GameData.Maps[keyOrAccessor];

                if (map == null)
                    return null;

                if (!NavMeshes.TryGetValue(map.Key, out navMesh))
                    return null;
            }

            return navMesh;
        }

        /// <summary>
        ///     Must be called after <see cref="GameData" /> is populated. <br />
        ///     Initializes the pathfinding system, generating and caching data needed to navigate the game world. <br />
        ///     This is multithreaded and very processing intensive.
        /// </summary>
        public static async Task InitializeAsync()
        {
            var navMeshes = new AwaitableDictionary<GMap, NavMesh>();

            var maps = GameData.Maps.DistinctBy(kvp => kvp.Value.Accessor)
                .Where(kvp => kvp.Value.Ignore == false)
                .Where(kvp => !IGNORED_MAPS.ContainsI(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var timer = Stopwatch.StartNew();
            Logger.Info("Preparing map navigation");

            await Task.WhenAll(maps.AsParallel(new ParallelLinqOptions
                    {
                        MaxDegreeOfParallelism = Convert.ToInt32(Environment.ProcessorCount * 1.5),
                        ExecutionMode = ParallelExecutionMode.ForceParallelism,
                        MergeOptions = ParallelMergeOptions.NotBuffered
                    })
                    .Select(async kvp =>
                    {
                        (var name, var map) = kvp;
                        var navMesh = TryBuildNavMesh(name, map);

                        if (navMesh != null)
                            await navMeshes.AddAsync(map, navMesh).ConfigureAwait(false);
                    }))
                .ConfigureAwait(false);

            await foreach ((var map, var value) in navMeshes.ConfigureAwait(false))
                NavMeshes.Add(map.Key, value);

            BuildWorldMesh(maps);
            Logger.Debug("Prepared worldmesh");

            timer.Stop();
            Logger.Info($"Prepared maps in {timer.ElapsedMilliseconds}ms");
        }

        /// <summary>
        ///     Checks if a given location is a wall.
        /// </summary>
        /// <param name="location">The location to check.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if the given location is part of a wall, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">No map was found for the accessor "{location.Map}".</exception>
        public static bool IsWall(ILocation location)
        {
            var map = GameData.Maps[location.Map];

            if (map == null)
                throw new InvalidOperationException($"No map was found for the accessor \"{location.Map}\".");

            return NavMeshes.TryGetValue(map.Key, out var navMesh) && navMesh.IsWall(navMesh.ApplyOffset(location));
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