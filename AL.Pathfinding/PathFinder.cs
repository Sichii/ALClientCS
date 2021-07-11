using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Model;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Chaos.Core.Extensions;
using Common.Logging;

namespace AL.Pathfinding
{
    public static class PathFinder
    {
        private static readonly string[] IGNORED_MAPS =
        {
            "A/B Testing",
            "Jail",
            "Lair of the Dark Mage",
            "New Town!",
            "Test"
        };
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PathFinder).FullName);
        internal static WorldMesh WorldMesh { get; private set; }
        internal static AwaitableDictionary<string, NavMesh> NavMeshes { get; } = new();

        private static void BuildWorldMesh(Dictionary<string, Map> maps)
        {
            Logger.Info("Preparing world navigation");
            var nodeDic = new Dictionary<Map, GraphNode<Map>>();
            var index = 0;

            foreach ((_, var map) in maps)
                nodeDic.Add(map, new GraphNode<Map>(map, index++));

            foreach ((var map, var node) in nodeDic)
            {
                foreach (var exit in map.Exits.Value)
                {
                    var exitMap = GameData.Maps[exit.DestinationMap];

                    if (exitMap == null)
                    {
                        var mapName = maps.FirstOrDefault(kvp => kvp.Value == map).Key;
                        Logger.Warn($"{mapName} has an exit that points to an unknown map.{Environment.NewLine}{exit}");
                        continue;
                    }

                    if (nodeDic.TryGetValue(exitMap, out var neighbor))
                        node.Neighbors.Add(neighbor);
                    else
                    {
                        var mapName = maps.FirstOrDefault(kvp => kvp.Value == map).Key;
                        Logger.Warn($"{mapName} has an exit that points to an invalid map.{Environment.NewLine}{exit}");
                    }
                }

                node.Neighbors = node.Neighbors.DistinctBy(neighbor => neighbor.Edge.Key)
                    .Where(neighbor => !Equals(node, neighbor))
                    .ToList();
            }

            WorldMesh = new WorldMesh(nodeDic);
        }

        public static async Task InitializeAsync()
        {
            var maps = GameData.Maps.DistinctBy(kvp => kvp.Value.Key)
                .Where(kvp => kvp.Value.Ignore == false)
                .Where(kvp => !IGNORED_MAPS.ContainsI(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Logger.Info("Preparing map navigation");
            var timer = new Stopwatch();
            timer.Start();

            await Task.WhenAll(maps.AsParallel(new ParallelLinqOptions
                {
                    MaxDegreeOfParallelism = (int) (Environment.ProcessorCount * 1.5),
                    ExecutionMode = ParallelExecutionMode.ForceParallelism,
                    MergeOptions = ParallelMergeOptions.NotBuffered
                })
                .Select(async kvp =>
                {
                    (var name, var map) = kvp;
                    var navMesh = TryBuildNavMesh(name, map);

                    if (navMesh != null)
                        await NavMeshes.AddAsync(map.Key, navMesh);
                }));

            BuildWorldMesh(maps);

            timer.Stop();
            Logger.Info($"Prepared maps in {timer.ElapsedMilliseconds}ms");
        }

        private static NavMesh TryBuildNavMesh(string name, Map map)
        {
            var geometry = GameData.Geometry[name];

            if ((geometry?.XLines == null) || (geometry.YLines == null))
            {
                Logger.Info($"Ignored {name}");
                return null;
            }

            Logger.Info($"Preparing {name}");
            var generator = new NavMeshBuilder(map, geometry);
            var navMesh = generator.Build();
            return navMesh;
        }
    }
}