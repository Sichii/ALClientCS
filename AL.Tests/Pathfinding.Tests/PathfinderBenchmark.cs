#region
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AL.Core.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Model;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Writes a benchmark of the pathfinder on the <c>old-paths.json</c> routes at speed 60 and the
///     <c>blink-paths.json</c> trips under every recorded setting, hops off and on: search time, allocation per search,
///     build time and memory held. Run in Release with <c>PATHFINDER_BENCHMARK_OUT</c> naming the file; without it the
///     test does nothing.
/// </summary>
public class PathfinderBenchmark : PathfindingTestBed
{
    [Test]
    [Explicit]
    [NotInParallel]
    public async Task WriteBenchmarkAsync()
    {
        var outPath = Environment.GetEnvironmentVariable("PATHFINDER_BENCHMARK_OUT");

        if (string.IsNullOrEmpty(outPath))
            return;

        var oldJson = await File.ReadAllTextAsync(Path.Combine("Fixtures", "pathfinding", "old-paths.json"));
        using var document = JsonDocument.Parse(oldJson);

        var oldRoutes = document.RootElement
                                .GetProperty("Paths")
                                .EnumerateArray()
                                .Select(ReadRoute)
                                .ToList();

        var blinkJson = await File.ReadAllTextAsync(Path.Combine("Fixtures", "pathfinding", "blink-paths.json"));
        var corpus = JsonSerializer.Deserialize<BlinkCorpus>(blinkJson)!;

        var report = new StringBuilder();

        report.AppendLine("# Pathfinder benchmark")
              .AppendLine()
              .AppendLine($"`old-paths.json`: {oldRoutes.Count} routes at speed 60, blink off.")
              .AppendLine(
                  $"`blink-paths.json`: {corpus.Trips.Count} trips under each of its {corpus.Settings.Count} settings, "
                  + $"blink on at the corpus floor of {corpus.Floor:F0}.")
              .AppendLine()
              .AppendLine("Each route is timed as its best of three after one untimed pass over the row. The spread is p5–p95.")
              .AppendLine()
              .AppendLine("| Routes | Hops | Found | Mean µs | Spread µs | KB per search |")
              .AppendLine("|---|---|---:|---:|---:|---:|");

        foreach (var hops in new[]
                 {
                     false,
                     true
                 })
            AppendRow(
                report,
                "old-paths",
                hops,
                oldRoutes.Select(route => (route.Start, route.End, new PathOptions
                         {
                             WalkSpeed = 60f,
                             UseHops = hops
                         }))
                         .ToList());

        foreach (var hops in new[]
                 {
                     false,
                     true
                 })
            AppendRow(
                report,
                "blink corpus",
                hops,
                corpus.Settings
                      .Select(setting => CreateBlinkOptions(setting, corpus.Floor, hops))
                      .SelectMany(options => corpus.Trips.Select(trip => ReadTrip(trip, options)))
                      .ToList());

        (var graphMs, var graphBytes) = Enumerable.Range(0, 3)
                                                  .Select(_ => MeasureGraphBuild())
                                                  .MinBy(build => build.Ms);

        var initStarted = Stopwatch.GetTimestamp();
        Pathfinder.Initialize();

        var initMs = Stopwatch.GetElapsedTime(initStarted)
                              .TotalMilliseconds;

        report.AppendLine()
              .AppendLine("| Build | ms | Memory held MB |")
              .AppendLine("|---|---:|---:|")
              .AppendLine(FormattableString.Invariant($"| world graph, fastest of three | {graphMs:F0} | {graphBytes / 1048576.0:F1} |"))
              .AppendLine(FormattableString.Invariant($"| `Pathfinder.Initialize` | {initMs:F0} | |"));

        await File.WriteAllTextAsync(outPath, report.ToString(), new UTF8Encoding(false));
    }

    private static void AppendRow(
        StringBuilder report,
        string name,
        bool hops,
        List<(Location Start, Destination End, PathOptions Options)> routes)
    {
        var found = routes.Where(route => TryFind(route.Start, route.End, route.Options))
                          .ToList();
        var times = new List<double>();
        long bytes = 0;

        foreach ((var start, var end, var options) in found)
        {
            var best = double.MaxValue;

            for (var i = 0; i < 3; i++)
            {
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                var started = Stopwatch.GetTimestamp();
                TryFind(start, end, options);

                best = Math.Min(
                    best,
                    Stopwatch.GetElapsedTime(started)
                             .TotalMicroseconds);
                bytes += GC.GetAllocatedBytesForCurrentThread() - allocated;
            }

            times.Add(best);
        }

        times.Sort();

        var p5 = times[(int)(times.Count * 0.05)];
        var p95 = times[(int)(times.Count * 0.95)];
        var kilobytes = bytes / 3.0 / times.Count / 1024.0;

        var hopsName = hops ? "on" : "off";

        report.AppendLine(
            FormattableString.Invariant(
                $"| {name} | {hopsName} | {found.Count}/{routes.Count} | {times.Average():F0} | {p5:F0}–{p95:F0} | {kilobytes:F1} |"));
    }

    private static PathOptions CreateBlinkOptions(BlinkSetting setting, float floor, bool hops)
        => new()
        {
            BlinkCost = floor,
            WalkSpeed = setting.Speed,
            PenaltyMs = setting.PenaltyMs,
            Mp = setting.Mp,
            MaxMp = setting.MaxMp,
            BlinkMpPerSecond = setting.Tracked ? setting.MpPerSecond : null,
            BlinkMpReserve = setting.Reserve,
            UseHops = hops
        };

    private static (double Ms, long Bytes) MeasureGraphBuild()
    {
        var before = GC.GetTotalMemory(true);
        var started = Stopwatch.GetTimestamp();
        var graph = new DirectedGraph(Pathfinder.Meshes, true);

        var ms = Stopwatch.GetElapsedTime(started)
                          .TotalMilliseconds;
        var after = GC.GetTotalMemory(true);
        GC.KeepAlive(graph);

        return (ms, after - before);
    }

    private static Location ReadLocation(JsonElement spot)
        => new(
            spot.GetProperty("Map")
                .GetString()!,
            spot.GetProperty("X")
                .GetSingle(),
            spot.GetProperty("Y")
                .GetSingle());

    private static (Location Start, Destination End) ReadRoute(JsonElement path)
    {
        var end = new Destination(
            ReadLocation(path.GetProperty("End")),
            path.GetProperty("Radius")
                .GetSingle());

        return (ReadLocation(path.GetProperty("Start")), end);
    }

    private static (Location Start, Destination End, PathOptions Options) ReadTrip(BlinkTrip trip, PathOptions options)
        => (new Location(trip.Start.Map, trip.Start.X, trip.Start.Y),
            new Destination(new Location(trip.End.Map, trip.End.X, trip.End.Y), 0f), options);

    private static bool TryFind(Location start, Destination end, PathOptions options)
    {
        try
        {
            Pathfinder.FindPath(start, [end], options);

            return true;
        } catch (InvalidOperationException)
        {
            return false;
        }
    }
}