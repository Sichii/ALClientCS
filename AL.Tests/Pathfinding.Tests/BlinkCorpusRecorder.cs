#region
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AL.Core.Definitions;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Model;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Records <c>blink-paths.json</c>: the current search's routes for 1,000 seeded random trips under every
///     <see cref="BlinkSetting" />, best-of-three timed. Run in Release with <c>BLINK_CORPUS_OUT</c> naming the file and
///     <c>BLINK_CORPUS_TREE</c> the commit it was recorded on; without the first it does nothing.
/// </summary>
public class BlinkCorpusRecorder : PathfindingTestBed
{
    private const int SEED = 20260925;
    private const int TRIPS = 1000;
    private const float FLOOR = 400f;

    /// <summary>
    ///     Maps no walk can reach: every door into one is key-locked. Collected from the game data rather than named.
    /// </summary>
    private static readonly HashSet<string> BehindAKey = GameData.Maps
                                                                 .Values
                                                                 .SelectMany(map => map.Doors)
                                                                 .Where(door => door.LockType == DoorLockType.Key)
                                                                 .Select(door => door.DestinationMap)
                                                                 .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static bool IsOptimized()
    {
        var attribute = typeof(Pathfinder).Assembly
                                          .GetCustomAttributes(typeof(DebuggableAttribute), false)
                                          .OfType<DebuggableAttribute>()
                                          .FirstOrDefault();

        return attribute is null || !attribute.IsJITOptimizerDisabled;
    }

    private static ILocation? RandomWalkable(Random rng, string map, GGeometry geo)
    {
        for (var attempt = 0; attempt < 400; attempt++)
        {
            var x = geo.MinX + (float)(rng.NextDouble() * (geo.MaxX - geo.MinX));
            var y = geo.MinY + (float)(rng.NextDouble() * (geo.MaxY - geo.MinY));
            var loc = new Location(map, x, y);

            if (Pathfinder.IsWalkable(loc))
                return loc;
        }

        return null;
    }

    [Test]
    [Explicit]
    public async Task RecordAsync()
    {
        var outPath = Environment.GetEnvironmentVariable("BLINK_CORPUS_OUT");

        if (string.IsNullOrEmpty(outPath))
            return;

        var tree = Environment.GetEnvironmentVariable("BLINK_CORPUS_TREE") ?? "unknown";

        var maps = GameData.Geometry
                           .Keys
                           .Where(map => Pathfinder.GetNavMesh(map) is not null)
                           .Where(map => GameData.Maps[map]?.World != WorldType.Dungeon)
                           .Where(map => !BehindAKey.Contains(map))
                           .Order(StringComparer.Ordinal)
                           .ToList();

        var rng = new Random(SEED);
        var kept = new List<(ILocation Start, ILocation End)>();

        while (kept.Count < TRIPS)
        {
            var startMap = maps[rng.Next(maps.Count)];
            var start = RandomWalkable(rng, startMap, GameData.Geometry[startMap]!);
            var endMap = maps[rng.Next(maps.Count)];
            var end = RandomWalkable(rng, endMap, GameData.Geometry[endMap]!);

            if (start is null || end is null)
                continue;

            if (TryFind(
                    start,
                    end,
                    PathOptions.Default,
                    out _))
                kept.Add((start, end));
        }

        var settings = BlinkSetting.All();

        //steady state before timing
        foreach ((var start, var end) in kept.Take(20))
            foreach (var setting in settings)
                TryFind(
                    start,
                    end,
                    OptionsFor(setting),
                    out _);

        var trips = new List<BlinkTrip>(kept.Count);

        for (var id = 0; id < kept.Count; id++)
        {
            (var start, var end) = kept[id];
            var results = new List<BlinkResult>(settings.Count);

            foreach (var setting in settings)
            {
                var options = OptionsFor(setting);
                var bestMicros = double.MaxValue;
                var found = false;
                IReadOnlyList<PathEdge> path = [];

                for (var run = 0; run < 3; run++)
                {
                    var started = Stopwatch.GetTimestamp();

                    found = TryFind(
                        start,
                        end,
                        options,
                        out path);

                    bestMicros = Math.Min(
                        bestMicros,
                        Stopwatch.GetElapsedTime(started)
                                 .TotalMilliseconds
                        * 1000.0);
                }

                var legs = found
                    ? path.Select(ToLeg)
                          .ToList()
                    : [];
                var cost = found ? path.Sum(leg => leg.Cost) : 0f;

                results.Add(
                    new BlinkResult(
                        setting.Name,
                        found,
                        cost,
                        bestMicros,
                        legs));
            }

            trips.Add(
                new BlinkTrip(
                    id,
                    ToPoint(start),
                    ToPoint(end),
                    results));
        }

        var corpus = new BlinkCorpus(
            SEED,
            $"Release, optimized={IsOptimized()}, tree={tree}",
            FLOOR,
            settings,
            trips);

        var json = JsonSerializer.Serialize(corpus);

        await File.WriteAllTextAsync(outPath, json, new UTF8Encoding(false));
    }

    private static PathOptions OptionsFor(BlinkSetting setting)
        => new()
        {
            BlinkCost = FLOOR,
            WalkSpeed = setting.Speed
        };

    private static BlinkLeg ToLeg(PathEdge edge)
        => new(
            edge.Type.ToString(),
            ToPoint(edge.Start),
            ToPoint(edge.End),
            edge.Cost);

    private static BlinkPoint ToPoint(ILocation location) => new(location.Map, location.X, location.Y);

    /// <summary>
    ///     A search that throws is an unreachable pair, which is an ordinary answer for a random draw.
    /// </summary>
    private static bool TryFind(
        ILocation start,
        ILocation end,
        PathOptions options,
        out IReadOnlyList<PathEdge> path)
    {
        try
        {
            path = Pathfinder.FindPath(start, [new Destination(end, 0f)], options);
        } catch (InvalidOperationException)
        {
            path = [];

            return false;
        }

        var last = path[^1].End;

        return (last.Map == end.Map) && (last.Distance(end) <= 0.01f);
    }
}