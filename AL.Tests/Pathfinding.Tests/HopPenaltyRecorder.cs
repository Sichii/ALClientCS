#region
using System.Globalization;
using System.Text;
using System.Text.Json;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Records, for every hop the builder makes, the lowest speed the bot's penalty replay calls it safe at beside the
///     lowest the server's own penalty grid does, and how many profile samples read under or over the server's penalty.
///     Run with <c>HOP_PENALTY_GRID</c> naming the server's <c>precomputed_map_data.js</c> and <c>HOP_PENALTY_OUT</c> the
///     tab-separated file to write; without both it does nothing.
/// </summary>
public class HopPenaltyRecorder : PathfindingTestBed
{
    private const int MAX_SPEED = 400;

    /// <summary>
    ///     The level the server gives a grid cell its penalty flood fill never reached.
    /// </summary>
    private const int UNREACHED_LEVEL = 8;

    private static int CalculateBotSpeed(Hop hop, float limit)
    {
        for (var speed = 1; speed <= MAX_SPEED; speed++)
            if (HopRisk.CalculatePeakScore(hop.Penalties, hop.Length, speed) < limit)
                return speed;

        return -1;
    }

    private static int CalculateServerSpeed(Hop hop, Dictionary<string, int> grid)
    {
        for (var speed = 1; speed <= MAX_SPEED; speed++)
            if (CalculateServerPeak(hop, grid, speed) <= HopRisk.SERVER_LIMIT)
                return speed;

        return -1;
    }

    private static float CalculateServerPeak(Hop hop, Dictionary<string, int> grid, float speed)
    {
        //ticks every 75 ms from a score of 0, as the bot's replay does
        var step = speed * 0.075f;
        var ticks = (int)MathF.Max(1f, MathF.Ceiling(hop.Length / step));
        var dx = (hop.Landing.X - hop.Takeoff.X) / hop.Length;
        var dy = (hop.Landing.Y - hop.Takeoff.Y) / hop.Length;
        var score = 0f;
        var peak = 0f;

        for (var tick = 1; tick <= ticks; tick++)
        {
            var at = MathF.Min(tick * step, hop.Length);
            var level = GetServerLevel(grid, hop.Takeoff.X + dx * at, hop.Takeoff.Y + dy * at);

            score = score * 0.99f + Math.Max(0, level - 1);
            peak = MathF.Max(peak, score);
        }

        return peak;
    }

    private static int GetServerLevel(Dictionary<string, int> grid, float x, float y)
    {
        //truncated toward zero onto the 10-unit grid, as the server's phash does
        var ix = (int)x;
        var iy = (int)y;

        return grid.TryGetValue($"{ix - ix % 10}|{iy - iy % 10}", out var level) ? level : UNREACHED_LEVEL;
    }

    private static (int Low, int High) CountPenaltyMismatches(Hop hop, Dictionary<string, int> grid)
    {
        var dx = (hop.Landing.X - hop.Takeoff.X) / hop.Length;
        var dy = (hop.Landing.Y - hop.Takeoff.Y) / hop.Length;
        var low = 0;
        var high = 0;

        for (var i = 0; i < hop.Penalties.Length; i++)
        {
            var at = i == (hop.Penalties.Length - 1) ? hop.Length : i * HopRisk.SAMPLE_STEP;
            var server = HopRisk.CalculatePenalty(GetServerLevel(grid, hop.Takeoff.X + dx * at, hop.Takeoff.Y + dy * at));

            if (hop.Penalties[i] < server)
                low++;
            else if (hop.Penalties[i] > server)
                high++;
        }

        return (low, high);
    }

    private static Dictionary<string, Dictionary<string, int>> LoadGrids(string path)
    {
        var text = File.ReadAllText(path);
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');

        using var document = JsonDocument.Parse(text.AsMemory(start, end - start + 1));

        var grids = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

        //a map without bounds is -1, and has no penalty
        foreach (var map in document.RootElement
                                    .GetProperty("smap_data")
                                    .EnumerateObject()
                                    .Where(map => map.Value.ValueKind == JsonValueKind.Object))
            grids[map.Name] = map.Value
                                 .EnumerateObject()
                                 .ToDictionary(cell => cell.Name, cell => cell.Value.GetInt32());

        return grids;
    }

    [Test]
    [Explicit]
    public async Task RecordAsync()
    {
        var gridPath = Environment.GetEnvironmentVariable("HOP_PENALTY_GRID");
        var outPath = Environment.GetEnvironmentVariable("HOP_PENALTY_OUT");

        if (string.IsNullOrEmpty(gridPath) || string.IsNullOrEmpty(outPath))
            return;

        var grids = LoadGrids(gridPath);
        var output = new StringBuilder();

        output.AppendLine(
            "map\ttakeoff_x\ttakeoff_y\tlanding_x\tlanding_y\tlength"
            + "\tbot_speed\tbot_speed_at_60\tserver_speed\tsamples_low\tsamples_high");

        foreach (var map in GameData.Maps.Keys.Order(StringComparer.Ordinal))
        {
            var mesh = Pathfinder.GetNavMesh(map);

            if (mesh is null
                || GameData.Maps[map] is not { } gMap
                || !PortalGraph.AllowsHops(gMap)
                || !grids.TryGetValue(map, out var grid))
                continue;

            foreach (var hop in HopBuilder.FindHops(mesh))
            {
                (var low, var high) = CountPenaltyMismatches(hop, grid);

                output.AppendLine(
                    string.Join(
                        '\t',
                        map,
                        hop.Takeoff.X.ToString(CultureInfo.InvariantCulture),
                        hop.Takeoff.Y.ToString(CultureInfo.InvariantCulture),
                        hop.Landing.X.ToString(CultureInfo.InvariantCulture),
                        hop.Landing.Y.ToString(CultureInfo.InvariantCulture),
                        hop.Length.ToString("F1", CultureInfo.InvariantCulture),
                        CalculateBotSpeed(hop, HopRisk.SAFE_SCORE),
                        CalculateBotSpeed(hop, HopRisk.SERVER_LIMIT),
                        CalculateServerSpeed(hop, grid),
                        low,
                        high));
            }
        }

        await File.WriteAllTextAsync(outPath, output.ToString(), new UTF8Encoding(false));
    }
}