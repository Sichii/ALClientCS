#region
using System.Text.Json;
using AL.APIClient;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
#endregion

namespace AL.Benchmarks;

/// <summary>
///     Times <see cref="Pathfinder.FindPath{T}" /> over the <c>old-paths.json</c> routes at speed 60 and the
///     <c>blink-paths.json</c> trips under each recorded setting, hops off and on. Each benchmark searches every route
///     once, a route with no path included, so its mean is per search.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
[Config(typeof(Columns))]
public class PathfinderBenchmarks
{
    private const int OLD_PATH_COUNT = 1000;
    private const int BLINK_TRIP_COUNT = 8000;

    private List<(Location Start, Destination End, PathOptions Options)> OldPathsHopsOff = null!;
    private List<(Location Start, Destination End, PathOptions Options)> OldPathsHopsOn = null!;
    private List<(Location Start, Destination End, PathOptions Options)> BlinkHopsOff = null!;
    private List<(Location Start, Destination End, PathOptions Options)> BlinkHopsOn = null!;

    [GlobalSetup]
    public void Setup()
    {
        GameData.Populate(
            AlApiClient.GetGameDataAsync()
                       .GetAwaiter()
                       .GetResult());
        Pathfinder.Initialize();

        var fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures");
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixtures, "old-paths.json")));

        var oldRoutes = document.RootElement
                                .GetProperty("Paths")
                                .EnumerateArray()
                                .Select(ReadRoute)
                                .ToList();

        var corpus = JsonSerializer.Deserialize<Corpus>(File.ReadAllText(Path.Combine(fixtures, "blink-paths.json")))!;

        OldPathsHopsOff = CreateOldPaths(oldRoutes, false);
        OldPathsHopsOn = CreateOldPaths(oldRoutes, true);
        BlinkHopsOff = CreateBlinkTrips(corpus, false);
        BlinkHopsOn = CreateBlinkTrips(corpus, true);

        if ((OldPathsHopsOff.Count != OLD_PATH_COUNT) || (BlinkHopsOff.Count != BLINK_TRIP_COUNT))
            throw new InvalidOperationException(
                $"The fixtures hold {OldPathsHopsOff.Count} routes and {BlinkHopsOff.Count} trips; update the operation counts.");
    }

    [Benchmark(OperationsPerInvoke = OLD_PATH_COUNT)]
    public int OldPathsHopsOffSearch() => SearchAll(OldPathsHopsOff);

    [Benchmark(OperationsPerInvoke = OLD_PATH_COUNT)]
    public int OldPathsHopsOnSearch() => SearchAll(OldPathsHopsOn);

    [Benchmark(OperationsPerInvoke = BLINK_TRIP_COUNT)]
    public int BlinkCorpusHopsOffSearch() => SearchAll(BlinkHopsOff);

    [Benchmark(OperationsPerInvoke = BLINK_TRIP_COUNT)]
    public int BlinkCorpusHopsOnSearch() => SearchAll(BlinkHopsOn);

    private static List<(Location Start, Destination End, PathOptions Options)> CreateBlinkTrips(Corpus corpus, bool hops)
        => corpus.Settings
                 .Select(setting => new PathOptions
                 {
                     BlinkCost = corpus.Floor,
                     WalkSpeed = setting.Speed,
                     PenaltyMs = setting.PenaltyMs,
                     Mp = setting.Mp,
                     MaxMp = setting.MaxMp,
                     BlinkMpPerSecond = setting.Tracked ? setting.MpPerSecond : null,
                     BlinkMpReserve = setting.Reserve,
                     UseHops = hops
                 })
                 .SelectMany(options => corpus.Trips.Select(trip => (new Location(trip.Start.Map, trip.Start.X, trip.Start.Y),
                     new Destination(new Location(trip.End.Map, trip.End.X, trip.End.Y), 0f), options)))
                 .ToList();

    private static List<(Location Start, Destination End, PathOptions Options)> CreateOldPaths(
        List<(Location Start, Destination End)> routes,
        bool hops)
    {
        var options = new PathOptions
        {
            WalkSpeed = 60f,
            UseHops = hops
        };

        return routes.Select(route => (route.Start, route.End, options))
                     .ToList();
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

    private static int SearchAll(List<(Location Start, Destination End, PathOptions Options)> routes)
    {
        var found = 0;

        foreach (var route in routes)
            if (TryFind(route))
                found++;

        return found;
    }

    private static bool TryFind((Location Start, Destination End, PathOptions Options) route)
    {
        try
        {
            Pathfinder.FindPath(route.Start, [route.End], route.Options);

            return true;
        } catch (InvalidOperationException)
        {
            return false;
        }
    }

    private sealed class Columns : ManualConfig
    {
        /// <summary>
        ///     Logical processor 4 alone: one hyperthread of a performance core on this machine, away from core 0.
        /// </summary>
        private const nint AFFINITY_MASK = 0x10;

        public Columns()
        {
            AddJob(Job.Default.WithAffinity(AFFINITY_MASK));
            AddColumn(StatisticColumn.StdDev, StatisticColumn.Median, StatisticColumn.P95);
        }
    }

    private sealed record Corpus(float Floor, List<Setting> Settings, List<Trip> Trips);

    private sealed record Setting(
        float Speed,
        float PenaltyMs,
        bool Tracked,
        float Mp,
        float MaxMp,
        float MpPerSecond,
        float Reserve);

    private sealed record Spot(string Map, float X, float Y);

    private sealed record Trip(Spot Start, Spot End);
}