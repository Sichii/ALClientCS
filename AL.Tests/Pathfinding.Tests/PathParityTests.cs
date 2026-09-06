#region
using System.Diagnostics;
using System.Text.Json;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     The previous pathfinder's answers for 1,000 seeded random routes, recorded in Release before the rewrite (start,
///     end, radius, every leg, cost, best-of-three time, allocation). The new pathfinder must find every route the old one
///     found, may be materially longer on at most 1% of them, must emit only walk legs the wall test accepts, and in
///     Release must be faster and lighter in aggregate.
/// </summary>
public class PathParityTests : PathfindingTestBed
{
    private static bool IsOptimized()
    {
        var attribute = typeof(Pathfinder).Assembly
                                          .GetCustomAttributes(typeof(DebuggableAttribute), false)
                                          .OfType<DebuggableAttribute>()
                                          .FirstOrDefault();

        return attribute is null || !attribute.IsJITOptimizerDisabled;
    }

    [Test]
    public async Task TheNewPathfinderMatchesTheRecordedCorpus()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("Fixtures", "pathfinding", "old-paths.json"));
        var corpus = JsonSerializer.Deserialize<Corpus>(json)!;

        //steady state before timing
        foreach (var warm in corpus.Paths.Take(20))
            TryFind(warm, out _);

        var missing = new List<int>();
        var unexpected = new List<int>();
        var longer = new List<string>();
        var crossing = new List<string>();
        var newMicros = 0.0;
        var newBytes = 0L;
        var oldMicros = 0.0;
        var oldBytes = 0L;

        foreach (var recorded in corpus.Paths)
        {
            var before = GC.GetAllocatedBytesForCurrentThread();
            var started = Stopwatch.GetTimestamp();
            var found = TryFind(recorded, out var path);

            newMicros += Stopwatch.GetElapsedTime(started)
                                  .TotalMilliseconds
                         * 1000.0;
            newBytes += GC.GetAllocatedBytesForCurrentThread() - before;
            oldMicros += recorded.Micros;
            oldBytes += recorded.Bytes;

            if (recorded.Found && !found)
                missing.Add(recorded.Id);

            if (!recorded.Found && found)
                unexpected.Add(recorded.Id);

            if (!found)
                continue;

            var cost = path.Sum(edge => edge.Cost);

            if (recorded.Found && (cost > (recorded.Cost * 1.05f + 1f)))
                longer.Add($"#{recorded.Id} {recorded.Start.Map}->{recorded.End.Map} old {recorded.Cost:F0} new {cost:F0}");

            foreach (var edge in path)
                if ((edge.Type == EdgeType.Walk) && !Pathfinder.CanMove(edge.Start, edge.End))
                    crossing.Add($"#{recorded.Id} {ILocation.ToString(edge.Start)} -> {ILocation.ToString(edge.End)}");
        }

        Console.WriteLine(
            $"PARITY cases={corpus.Paths.Count} recordedFound={corpus.Found} missing={missing.Count} unexpected={unexpected.Count} longer={longer.Count} crossing={crossing.Count}");

        Console.WriteLine(
            $"PARITY time old {oldMicros / 1000:F0} ms new {newMicros / 1000:F0} ms | bytes old {oldBytes / 1024:F0} KB new {newBytes / 1024:F0} KB | optimized={IsOptimized()}");

        missing.Should()
               .BeEmpty("every route the old pathfinder found must still be found");

        crossing.Should()
                .BeEmpty("no walk leg may cross a wall line");

        longer.Should()
              .HaveCountLessThanOrEqualTo(
                  corpus.Found / 100,
                  "the funnel may only lose to the smoothed vertex path on rare corridor choices: {0}",
                  string.Join("; ", longer.Take(10)));

        //the corpus was recorded in Release; a Debug run says nothing about speed
        if (!IsOptimized())
            return;

        newMicros.Should()
                 .BeLessThan(oldMicros, "the rewrite exists to be faster");

        newBytes.Should()
                .BeLessThan(oldBytes, "the rewrite exists to allocate less");
    }

    private static bool TryFind(Case recorded, out IReadOnlyList<PathEdge> path)
    {
        var start = new Location(recorded.Start.Map, recorded.Start.X, recorded.Start.Y);
        var end = new Destination(new Location(recorded.End.Map, recorded.End.X, recorded.End.Y), recorded.Radius);

        try
        {
            path = Pathfinder.FindPath(start, [end]);
        } catch (InvalidOperationException)
        {
            path = [];

            return false;
        }

        var last = path[^1].End;

        return (last.Map == recorded.End.Map) && (last.Distance(end) <= (recorded.Radius + 0.01f));
    }

    //these mirror the recorded corpus file, so every member stays whether the assertions read it or not: dropping one
    //would make the record stop describing what old-paths.json holds
    // ReSharper disable NotAccessedPositionalProperty.Local
    private sealed record Case(
        int Id,
        Spot Start,
        Spot End,
        float Radius,
        bool Found,
        float Cost,
        double Micros,
        long Bytes,
        List<Leg> Legs);

    private sealed record Corpus(
        int Seed,
        string Config,
        int Cases,
        int Found,
        double MeanMicros,
        double MedianMicros,
        double MeanBytes,
        List<Case> Paths);

    private sealed record Leg(
        string Type,
        Spot Start,
        Spot End,
        float Cost);

    private sealed record Spot(string Map, float X, float Y);
}