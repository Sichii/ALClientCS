#region
using System.Diagnostics;
using System.Text.Json;
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using Chaos.Extensions.Common;
using FluentAssertions;
using CONSTANTS = AL.Pathfinding.Definitions.CONSTANTS;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Reruns the trips of <c>blink-paths.json</c> through the timed search and holds it to the gates: every recorded trip
///     is still found, no route breaks the floor, no route is dearer than an old one that kept the floor, and every short
///     cast bridges ground no walk joins. The same run at floor 1000 has no recording to compare against, so it is held to
///     the gates that need none.
/// </summary>
public class BlinkCorpusTests : PathfindingTestBed
{
    /// <summary>
    ///     A route is dearer than the recording only past half a walk unit per leg: each leg's length is a float sum, and the
    ///     old and new searches pull the same walk to slightly different lengths.
    /// </summary>
    private const double PRICE_SLACK_PER_LEG = 0.5;

    private const int RUNS = 3;
    private const double TIE_SECONDS = 0.001;
    private const float WIDER_FLOOR = 1000f;

    [Test]
    public async Task TheTimedSearchKeepsTheGatesOnTheRecordedCorpus()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("Fixtures", "pathfinding", "blink-paths.json"));
        var corpus = JsonSerializer.Deserialize<BlinkCorpus>(json)!;

        //steady state before timing: warmed on 20 or 200 trips, the first setting still timed two to three times the rest
        foreach (var warm in corpus.Trips)
            TimedFind(warm, OptionsFor(corpus.Settings[0], corpus.Floor));

        var missing = new List<string>();
        var floorBreaks = new List<string>();
        var dearer = new List<string>();
        var shortBlinks = new List<string>();

        foreach (var floor in new[]
                 {
                     corpus.Floor,
                     WIDER_FLOOR
                 })
            foreach (var setting in corpus.Settings)
            {
                var clock = setting.ToClock();
                var recordedFloor = floor == corpus.Floor;
                var rows = new List<Row>();

                foreach (var trip in corpus.Trips)
                {
                    var recorded = trip.Results.First(result => result.Setting == setting.Name);
                    (var found, var path, var micros) = TimedFind(trip, OptionsFor(setting, floor));
                    var label = $"#{trip.Id} floor {floor:F0} {setting.Name}";

                    if (!found)
                    {
                        if (recorded.Found)
                            missing.Add(label);

                        continue;
                    }

                    var measured = RouteClock.Measure(path, clock);
                    var newBreak = FloorBreak(path, floor);

                    if (newBreak is not null)
                        floorBreaks.Add($"{label} {newBreak}");

                    foreach (var blink in path.Where(edge => (edge.Type == EdgeType.Blink) && (edge.Cost < floor)))
                        if (!NoWalkJoins(blink))
                            shortBlinks.Add(
                                $"{label} {ILocation.ToString(blink.Start)} -> {ILocation.ToString(blink.End)} {blink.Cost:F0}");

                    var row = new Row(
                        trip.Id,
                        measured.Seconds,
                        path.Count(edge => edge.Type == EdgeType.Blink),
                        micros,
                        newBreak is not null);

                    if (recordedFloor && recorded.Found)
                    {
                        var oldPath = recorded.Legs
                                              .Select(leg => leg.ToPathEdge())
                                              .ToList();
                        var oldMeasured = RouteClock.Measure(oldPath, clock);
                        var oldBreaks = FloorBreak(oldPath, floor) is not null;
                        var slack = PRICE_SLACK_PER_LEG * Math.Max(oldPath.Count, path.Count);

                        if (!oldBreaks && (measured.Price > (oldMeasured.Price + slack)))
                            dearer.Add($"{label} old {oldMeasured.Price:F1} new {measured.Price:F1}");

                        row = row with
                        {
                            Old = new OldRoute(
                                oldMeasured.Seconds,
                                oldPath.Count(edge => edge.Type == EdgeType.Blink),
                                recorded.Micros,
                                oldBreaks)
                        };
                    }

                    rows.Add(row);
                }

                Console.WriteLine(recordedFloor ? ReportAgainstRecording(setting, floor, rows) : Report(setting, floor, rows));
            }

        Console.WriteLine($"BLINK optimized={IsOptimized()}");

        missing.Should()
               .BeEmpty("every trip the recorded search routed is still routed: {0}", string.Join("; ", missing.Take(10)));

        floorBreaks.Should()
                   .BeEmpty(
                       "a map's stretch from first arrival to last exit that holds a cast walks at least the floor: {0}",
                       string.Join("; ", floorBreaks.Take(10)));

        dearer.Should()
              .BeEmpty("no route is priced above a recorded one that keeps the floor: {0}", string.Join("; ", dearer.Take(10)));

        shortBlinks.Should()
                   .BeEmpty("a cast under the floor bridges a pair no walk joins: {0}", string.Join("; ", shortBlinks.Take(10)));
    }

    /// <summary>
    ///     Trip 368's detour: a 273 walk across the cave split into two casts by the door to main and straight back. Each cast
    ///     replaces a walk over 400, so only the check across the round trip catches it.
    /// </summary>
    [Test]
    public void TheFloorCheckCatchesADoorRoundTrip()
    {
        var start = new Location("cave", 43, -1165);
        var end = new Location("cave", -226, -1118);
        var toMain = GameData.Maps["cave"]!.Exits.First(exit => exit.ToLocation.Map == "main");
        var back = GameData.Maps["main"]!.Exits.First(exit => exit.ToLocation.Map == "cave");

        PathEdge[] path =
        [
            new(
                EdgeType.Blink,
                start,
                new Location("cave", toMain.X, toMain.Y),
                1340),
            new(
                EdgeType.Door,
                toMain,
                toMain.ToLocation,
                CONSTANTS.TRANSPORT_HEURISTIC),
            new(
                EdgeType.Door,
                back,
                back.ToLocation,
                CONSTANTS.TRANSPORT_HEURISTIC),
            new(
                EdgeType.Blink,
                back.ToLocation,
                end,
                1737)
        ];

        FloorBreak(path, 400f)
            .Should()
            .StartWith("cave walk");

        FloorBreak(path, 250f)
            .Should()
            .BeNull();
    }

    /// <summary>
    ///     Checks the floor across detours. For each map, the route's first stop on it and its last are taken; when they are
    ///     two or more hops apart and any hop between them is a blink, the walk on that one map between the two must be at
    ///     least the floor.
    /// </summary>
    /// <returns>
    ///     A description of the first stretch that breaks the floor, or null when none does.
    /// </returns>
    /// <remarks>
    ///     The walk is the one the graph prices, to the exit's reach, rather than a blink-off route between the two: that
    ///     route may itself leave through a door pair and come back, and cannot reach a door placed off the walkable ground. A
    ///     map entered and left in one hop needs no check, since its cast already replaced a walk of at least the floor.
    /// </remarks>
    private static string? FloorBreak(IReadOnlyList<PathEdge> path, float floor)
    {
        //the route as the graph's nodes: the start, then where each hop ends. A door hop starts on the door itself, the
        //node a walk to it arrives at, so the walk's stop in the door's reach is replaced by the door
        var stops = new List<Stop>
        {
            new(
                path[0].Start.Map,
                path[0].Start.X,
                path[0].Start.Y,
                false)
        };
        var blinked = new List<bool>();
        EdgeType? lastHop = null;

        foreach (var leg in path)
        {
            switch (leg.Type)
            {
                case EdgeType.Walk when lastHop == EdgeType.Walk:
                    stops[^1] = StopAt(leg.End, false);

                    break;
                case EdgeType.Door:
                case EdgeType.Transport:
                {
                    var door = StopAt(leg.Start, true);

                    if (lastHop is EdgeType.Walk or EdgeType.Blink)
                        stops[^1] = door;
                    else
                    {
                        stops.Add(door);
                        blinked.Add(false);
                    }

                    stops.Add(StopAt(leg.End, false));
                    blinked.Add(false);

                    break;
                }
                default:
                    stops.Add(StopAt(leg.End, false));
                    blinked.Add(leg.Type == EdgeType.Blink);

                    break;
            }

            lastHop = leg.Type;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var first = 0; first < stops.Count; first++)
        {
            var map = stops[first].Map;

            if (!seen.Add(map))
                continue;

            var last = stops.FindLastIndex(stop => stop.Map.EqualsI(map));

            if ((last - first) < 2)
                continue;

            if (!blinked.Skip(first)
                        .Take(last - first)
                        .Any(cast => cast))
                continue;

            var walk = WalkOnMap(stops[first], stops[last]);

            if (walk >= floor)
                continue;

            return $"{map} walk {walk:F0} ({stops[first].X:F0},{stops[first].Y:F0}) -> ({stops[last].X:F0},{stops[last].Y:F0})";
        }

        return null;
    }

    private static bool IsOptimized()
    {
        var attribute = typeof(Pathfinder).Assembly
                                          .GetCustomAttributes(typeof(DebuggableAttribute), false)
                                          .OfType<DebuggableAttribute>()
                                          .FirstOrDefault();

        return attribute is null || !attribute.IsJITOptimizerDisabled;
    }

    /// <summary>
    ///     Whether no walk on the blink's map joins its two ends. No route at all, or one that leaves the map, is ground
    ///     nothing walks.
    /// </summary>
    private static bool NoWalkJoins(PathEdge blink)
    {
        IReadOnlyList<PathEdge> walk;

        try
        {
            walk = Pathfinder.FindPath(
                blink.Start,
                [new Destination(new Location(blink.End.Map, blink.End.X, blink.End.Y), 0)],
                PathOptions.NoTown);
        } catch (InvalidOperationException)
        {
            return true;
        }

        return walk.Any(leg => leg.Type != EdgeType.Walk);
    }

    private static PathOptions OptionsFor(BlinkSetting setting, float floor)
        => new()
        {
            BlinkCost = floor,
            WalkSpeed = setting.Speed,
            PenaltyMs = setting.PenaltyMs,
            Mp = setting.Mp,
            MaxMp = setting.MaxMp,
            BlinkMpPerSecond = setting.Tracked ? setting.MpPerSecond : null,
            BlinkMpReserve = setting.Reserve
        };

    private static double Percentile(IReadOnlyList<double> sorted, double fraction)
        => sorted[Math.Max(0, (int)Math.Ceiling(fraction * sorted.Count) - 1)];

    private static string Report(BlinkSetting setting, float floor, List<Row> rows)
        => $"BLINK floor={floor:F0} {setting.Name} found={rows.Count} blinking={rows.Count(row => row.Blinks > 0)} "
           + $"mean_s new={rows.Average(row => row.Seconds):F2} blinks new={rows.Sum(row => row.Blinks)} "
           + $"floor_breaks new={rows.Count(row => row.BreaksFloor)} search_us new {Timing(rows.Select(row => row.Micros))}";

    private static string ReportAgainstRecording(BlinkSetting setting, float floor, List<Row> rows)
    {
        var compared = rows.Where(row => row.Old is not null)
                           .ToList();

        var faster = compared.Count(row => row.Seconds < (row.Old!.Seconds - TIE_SECONDS));
        var slower = compared.Count(row => row.Seconds > (row.Old!.Seconds + TIE_SECONDS));
        var worst = compared.MaxBy(row => row.Seconds - row.Old!.Seconds)!;

        return $"BLINK floor={floor:F0} {setting.Name} compared={compared.Count} "
               + $"blinking={compared.Count(row => (row.Blinks > 0) || (row.Old!.Blinks > 0))} "
               + $"faster={faster} tie={compared.Count - faster - slower} slower={slower} "
               + $"mean_s old={compared.Average(row => row.Old!.Seconds):F2} new={compared.Average(row => row.Seconds):F2} "
               + $"worst_regression={worst.Seconds - worst.Old!.Seconds:+0.000;-0.000}s(#{worst.Id}) "
               + $"blinks old={compared.Sum(row => row.Old!.Blinks)} new={compared.Sum(row => row.Blinks)} "
               + $"floor_breaks old={compared.Count(row => row.Old!.BreaksFloor)} new={rows.Count(row => row.BreaksFloor)} "
               + $"search_us old {Timing(compared.Select(row => row.Old!.Micros))} new {Timing(compared.Select(row => row.Micros))}";
    }

    private static Stop StopAt(ILocation location, bool isExit)
        => new(
            location.Map,
            location.X,
            location.Y,
            isExit);

    /// <summary>
    ///     Searches best of <see cref="RUNS" />, as the recording did, and reports the fastest time in microseconds.
    /// </summary>
    private static (bool Found, IReadOnlyList<PathEdge> Path, double Micros) TimedFind(BlinkTrip trip, PathOptions options)
    {
        var start = new Location(trip.Start.Map, trip.Start.X, trip.Start.Y);
        var end = new Location(trip.End.Map, trip.End.X, trip.End.Y);
        IReadOnlyList<PathEdge> path = [];
        var best = double.MaxValue;

        for (var run = 0; run < RUNS; run++)
        {
            var started = Stopwatch.GetTimestamp();

            try
            {
                path = Pathfinder.FindPath(start, [new Destination(end, 0f)], options);
            } catch (InvalidOperationException)
            {
                return (false, [], 0);
            }

            best = Math.Min(
                best,
                Stopwatch.GetElapsedTime(started)
                         .TotalMilliseconds
                * 1000.0);
        }

        var last = path[^1].End;

        return ((last.Map == end.Map) && (last.Distance(end) <= 0.01f), path, best);
    }

    private static string Timing(IEnumerable<double> micros)
    {
        var sorted = micros.Order()
                           .ToList();

        return $"median={Percentile(sorted, 0.5):F0} mean={sorted.Average():F0} p95={Percentile(sorted, 0.95):F0} max={sorted[^1]:F0}";
    }

    /// <summary>
    ///     The length of the walk on one map between two of a route's stops, priced the way the graph prices its walk edges:
    ///     to the exit's reach when <paramref name="to" /> is an exit.
    /// </summary>
    /// <param name="from">Where the walk starts; the map the walk is on.</param>
    /// <param name="to">Where the walk ends, on the same map.</param>
    /// <returns>
    ///     The walked length, or <see cref="float.MaxValue" /> when no walk joins the two.
    /// </returns>
    private static float WalkOnMap(Stop from, Stop to)
    {
        var mesh = Pathfinder.GetNavMesh(from.Map)!;
        var startTriangle = mesh.Locate(from.X, from.Y, out var startEntry);
        var endTriangle = mesh.Locate(to.X, to.Y, out var endEntry);

        if ((startTriangle < 0) || (endTriangle < 0))
            return float.MaxValue;

        var exit = to.IsExit
            ? GameData.Maps[to.Map]
                      ?.Exits
                      .FirstOrDefault(candidate => (candidate.X == to.X) && (candidate.Y == to.Y))
            : null;
        var reach = exit is null ? Reach.Circle(to.X, to.Y, 0f) : new Reach(exit.ReachBand, exit.ReachRange);

        var scratch = SearchScratch.Rent();
        mesh.Search(startTriangle, startEntry, scratch);

        var cost = mesh.WalkCost(
            startEntry,
            endTriangle,
            endEntry,
            reach,
            scratch);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (cost == float.MaxValue)
            return cost;

        return new Point(from.X, from.Y).Distance(startEntry) + cost;
    }

    /// <summary>
    ///     A recorded route's measurements, beside the new one's.
    /// </summary>
    private sealed record OldRoute(
        double Seconds,
        int Blinks,
        double Micros,
        bool BreaksFloor);

    /// <summary>
    ///     One trip's new route under one setting, and the recorded route when there is one at this floor.
    /// </summary>
    private sealed record Row(
        int Id,
        double Seconds,
        int Blinks,
        double Micros,
        bool BreaksFloor)
    {
        public OldRoute? Old { get; init; }
    }

    /// <summary>
    ///     A place the route stands: the start, where a hop ends, or an exit a door or transporter hop leaves from.
    /// </summary>
    private readonly record struct Stop(
        string Map,
        float X,
        float Y,
        bool IsExit);
}