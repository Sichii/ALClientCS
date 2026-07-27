#region
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AL.Core.Geometry;
using AL.Data.Geometry;
using AL.Data.Maps;
using AL.Data.Monsters;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins what the positional-array converter produces for every positional array in the committed game-data snapshot:
///     <see cref="GTile" />, <see cref="StraightLine" />, <see cref="GDoor" />, <see cref="GSpawn" />,
///     <see cref="Orientation" /> and <see cref="GKillAchievement" />. The committed canonical fixture is frozen text
///     generated before the System.Text.Json port, so reproducing it line-for-line is the port's proof (plan T6 / decision
///     8).
/// </summary>
/// <remarks>
///     Every array is fed through the production System.Text.Json options via <see cref="TestJson.Data{T}" />, which
///     resolve the inner element with
///     <c>
///         ArrayToObjectConverter&lt;T&gt;
///     </c>
///     exactly as the game-data load path does — so deserializing each array on its own is byte-identical to loading it in
///     place.
/// </remarks>
public class PositionalArrayCharacterization
{
    private const string CANONICAL_FIXTURE = "positional-arrays.canonical.json";

    //written beside the binary on every run for diffing; never read back, so it can never satisfy the
    //committed-fixture guard the way writing CANONICAL_FIXTURE itself would
    private const string STJ_GENERATED_SIDECAR = "positional-arrays.canonical.stj-generated.json";

    //parsed once through the production System.Text.Json options; lazy so a 20k-line parse only happens if a
    //test in this class actually runs
    private static readonly Lazy<Population> Data = new(Collect);

    //renders the sidecar close to the frozen fixture so the two stay diffable: 2-space indent, and < > & +
    //left unescaped rather than \uXXXX. Newlines are LF here and CRLF in the fixture, so diff -w it.
    private static readonly JsonSerializerOptions SidecarOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private sealed record Parsed<T>(JsonArray Raw, T Value);

    private sealed record Population(
        IReadOnlyList<Parsed<GTile>> Tiles,
        IReadOnlyList<Parsed<StraightLine>> StraightLines,
        IReadOnlyList<Parsed<GDoor>> Doors,
        IReadOnlyList<Parsed<GSpawn>> Spawns,
        IReadOnlyList<Parsed<Orientation>> Orientations,
        IReadOnlyList<Parsed<GKillAchievement>> KillAchievements);

    #region Collection
    private static Population Collect()
    {
        var tiles = new List<Parsed<GTile>>();
        var straightLines = new List<Parsed<StraightLine>>();
        var doors = new List<Parsed<GDoor>>();
        var spawns = new List<Parsed<GSpawn>>();
        var orientations = new List<Parsed<Orientation>>();
        var killAchievements = new List<Parsed<GKillAchievement>>();

        //geometry.<map>.tiles / x_lines / y_lines
        var geometry = Fixture.Section("geometry")
                              .AsObject();

        foreach (var mapProperty in geometry.OrderBy(property => property.Key, StringComparer.Ordinal))
        {
            if (mapProperty.Value is not JsonObject map)
                continue;

            foreach (var tile in InnerArrays(map, "tiles"))
                tiles.Add(ParseOne<GTile>(tile));

            //VerticalLines binds x_lines, HorizontalLines binds y_lines; both parse to StraightLine
            foreach (var line in InnerArrays(map, "x_lines"))
                straightLines.Add(ParseOne<StraightLine>(line));

            foreach (var line in InnerArrays(map, "y_lines"))
                straightLines.Add(ParseOne<StraightLine>(line));
        }

        //maps.<map>.doors / spawns / npcs[].position / npcs[].positions
        var maps = Fixture.Section("maps")
                          .AsObject();

        foreach (var mapProperty in maps.OrderBy(property => property.Key, StringComparer.Ordinal))
        {
            if (mapProperty.Value is not JsonObject map)
                continue;

            foreach (var door in InnerArrays(map, "doors"))
                doors.Add(ParseOne<GDoor>(door));

            foreach (var spawn in InnerArrays(map, "spawns"))
                spawns.Add(ParseOne<GSpawn>(spawn));

            if (map["npcs"] is JsonArray npcs)
                foreach (var npc in npcs.OfType<JsonObject>())
                {
                    //_position is a single positional array; _positions is an array of them
                    if (npc["position"] is JsonArray position)
                        orientations.Add(ParseOne<Orientation>(position));

                    if (npc["positions"] is JsonArray positions)
                        foreach (var inner in positions.OfType<JsonArray>())
                            orientations.Add(ParseOne<Orientation>(inner));
                }
        }

        //monsters.<monster>.achievements
        var monsters = Fixture.Section("monsters")
                              .AsObject();

        foreach (var monsterProperty in monsters.OrderBy(property => property.Key, StringComparer.Ordinal))
        {
            if (monsterProperty.Value is not JsonObject monster)
                continue;

            foreach (var achievement in InnerArrays(monster, "achievements"))
                killAchievements.Add(ParseOne<GKillAchievement>(achievement));
        }

        return new Population(
            tiles,
            straightLines,
            doors,
            spawns,
            orientations,
            killAchievements);
    }

    //the elements of obj[key], each of which is itself a positional array
    private static IEnumerable<JsonArray> InnerArrays(JsonObject obj, string key)
    {
        if (obj[key] is not JsonArray outer)
            yield break;

        foreach (var element in outer)
            if (element is JsonArray inner)
                yield return inner;
    }

    private static Parsed<T> ParseOne<T>(JsonArray array)
    {
        //ToJsonString() is the compact form; JsonNode.ToString() would indent
        var json = array.ToJsonString();

        return new Parsed<T>(array, TestJson.Data<T>(json)!);
    }
    #endregion

    #region Canonicalization
    private static IReadOnlyList<string> CanonicalLines(Population population)
    {
        var lines = new List<string>();

        Emit(
            lines,
            "GTile",
            population.Tiles,
            parsed => Canon(parsed.Value));

        Emit(
            lines,
            "StraightLine",
            population.StraightLines,
            parsed => Canon(parsed.Value));

        Emit(
            lines,
            "GDoor",
            population.Doors,
            parsed => Canon(parsed.Value));

        Emit(
            lines,
            "GSpawn",
            population.Spawns,
            parsed => Canon(parsed.Value));

        Emit(
            lines,
            "Orientation",
            population.Orientations,
            parsed => Canon(parsed.Value));

        Emit(
            lines,
            "GKillAchievement",
            population.KillAchievements,
            parsed => Canon(parsed.Value));

        return lines;
    }

    private static void Emit<T>(
        List<string> lines,
        string name,
        IReadOnlyList<Parsed<T>> items,
        Func<Parsed<T>, string> canon)
    {
        lines.Add($"## {name} count={items.Count}");

        foreach (var item in items)
            lines.Add(canon(item));
    }

    private static string Canon(GTile tile)
        => $"GTile TileSet={S(tile.TileSet)} X={F(tile.X)} Y={F(tile.Y)} Size={F(tile.Size)} Unknown={NF(tile.Unknown)}";

    private static string Canon(StraightLine line)
        => $"StraightLine On={line.On} Start={line.Start} End={line.End} IsVertical={line.IsVertical}";

    private static string Canon(GDoor door)
        => $"GDoor X={F(door.X)} Y={F(door.Y)} Width={F(door.Width)} Height={F(door.Height)} DestinationMap={S(door.DestinationMap)} "
           + $"DestinationSpawnId={door.DestinationSpawnId} LockType={E(door.LockType)} KeyType={E(door.KeyType)} CurrentMapSpawnId={F(door.CurrentMapSpawnId)}";

    private static string Canon(GSpawn spawn)
        => $"GSpawn X={F(spawn.X)} Y={F(spawn.Y)} Direction={E(spawn.Direction)} Distance={F(spawn.Distance)}";

    private static string Canon(Orientation orientation)
        => $"Orientation X={F(orientation.X)} Y={F(orientation.Y)} Direction={E(orientation.Direction)}";

    private static string Canon(GKillAchievement achievement)
        => $"GKillAchievement RequiredPoints={F(achievement.RequiredPoints)} RewardType={E(achievement.RewardType)} "
           + $"Attribute={E(achievement.Attribute)} Amount={F(achievement.Amount)}";

    private static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);

    //nullable counterpart of F; the casing pairs it with F/S/E rather than following the method rule
    // ReSharper disable once InconsistentNaming
    private static string NF(float? value) => value.HasValue ? F(value.Value) : "null";

    private static string S(string? value) => value ?? "null";

    //name plus underlying integer, so an unmapped/degraded value is still unambiguous
    private static string E<TEnum>(TEnum value) where TEnum: struct, Enum
        => $"{value}(#{Convert.ToInt64(value, CultureInfo.InvariantCulture)})";

    private static string Sha256Hex(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));

        return Convert.ToHexString(bytes)
                      .ToLowerInvariant();
    }
    #endregion

    #region Whole-population pin
    /// <summary>
    ///     The migration's proof for T6: all 20,921 positional arrays parsed through the production System.Text.Json options
    ///     must canonicalize to the committed fixture — same per-type counts, same lines, same sha256. The fixture is frozen
    ///     text from before the port and the canonical rendering is engine-neutral, so any diff is a real parsed-value
    ///     difference.
    /// </summary>
    [Test]
    public void T6_AllPositionalArrays_ReproduceCommittedCanonicalFixture()
    {
        var lines = CanonicalLines(Data.Value);
        var body = string.Join('\n', lines);
        var hash = Sha256Hex(body);

        var population = Data.Value;

        var counts = new JsonObject
        {
            ["GTile"] = population.Tiles.Count,
            ["StraightLine"] = population.StraightLines.Count,
            ["GDoor"] = population.Doors.Count,
            ["GSpawn"] = population.Spawns.Count,
            ["Orientation"] = population.Orientations.Count,
            ["GKillAchievement"] = population.KillAchievements.Count
        };

        var generated = new JsonObject
        {
            ["description"] = "Canonical form of every positional array in the committed data.json snapshot, "
                              + "parsed through the positional-array converter under System.Text.Json. sha256 is over "
                              + "the newline-joined lines. Regenerated and diffed by PositionalArrayCharacterization.",
            ["sha256"] = hash,
            ["total"] = lines.Count,
            ["counts"] = counts,
            ["lines"] = new JsonArray(
                lines.Select(line => (JsonNode?)line)
                     .ToArray())
        };

        //read the committed fixture BEFORE writing anything, so the build-copied source stays authoritative;
        //the fresh copy goes to a distinct sidecar name that the test never reads back
        var committedRaw = Fixture.ReadCommittedSnapshot(CANONICAL_FIXTURE);
        Fixture.WriteSnapshot(STJ_GENERATED_SIDECAR, generated.ToJsonString(SidecarOptions));

        //the fixture is the frozen oracle: restore it from source control rather than regenerating it, since
        //regenerating to turn a red green would bless the very regression the comparison is there to catch
        committedRaw.Should()
                    .NotBeNull($"Committed fixture '{CANONICAL_FIXTURE}' is missing; restore it from source control.");

        var committed = JsonNode.Parse(committedRaw)!.AsObject();

        var committedCounts = committed["counts"]!.AsObject();

        foreach ((var type, var actual) in counts)
            actual!.GetValue<int>()
                   .Should()
                   .Be(committedCounts[type]!.GetValue<int>(), $"parsed count for {type} drifted from the committed fixture.");

        var committedLines = committed["lines"]!.AsArray()
                                                .Select(node => node!.GetValue<string>())
                                                .ToList();

        //find the first divergence for a legible failure before falling back to the hash
        var limit = Math.Min(committedLines.Count, lines.Count);

        for (var index = 0; index < limit; index++)
            lines[index]
                .Should()
                .Be(committedLines[index], $"canonical line {index} drifted from the committed baseline.");

        lines.Count
             .Should()
             .Be(committedLines.Count, "canonical line count drifted.");

        hash.Should()
            .Be(committed["sha256"]!.GetValue<string>(), "canonical sha256 drifted from the committed baseline.");
    }

    /// <summary>
    ///     Guards the committed fixture against hand-editing or corruption: its recorded hash must equal the hash of its own
    ///     recorded lines.
    /// </summary>
    [Test]
    public void T6_CommittedFixture_IsInternallyConsistent()
    {
        var committedRaw = Fixture.ReadCommittedSnapshot(CANONICAL_FIXTURE);

        committedRaw.Should()
                    .NotBeNull($"Committed fixture '{CANONICAL_FIXTURE}' is missing; restore it from source control.");

        var committed = JsonNode.Parse(committedRaw)!.AsObject();

        var committedLines = committed["lines"]!.AsArray()
                                                .Select(node => node!.GetValue<string>());
        var body = string.Join('\n', committedLines);

        Sha256Hex(body)
            .Should()
            .Be(committed["sha256"]!.GetValue<string>(), "Committed fixture's recorded sha256 does not match its recorded lines.");
    }
    #endregion

    #region Ragged arrays
    //pinned raw-length distributions of the committed snapshot (rawLength -> count)
    private static readonly IReadOnlyDictionary<int, int> TileLengths = new Dictionary<int, int>
    {
        [4] = 6145,
        [5] = 1134,
        [6] = 4
    };

    private static readonly IReadOnlyDictionary<int, int> StraightLineLengths = new Dictionary<int, int>
    {
        [3] = 12830
    };

    private static readonly IReadOnlyDictionary<int, int> DoorLengths = new Dictionary<int, int>
    {
        [6] = 1,
        [7] = 86,
        [8] = 4,
        [9] = 5
    };

    private static readonly IReadOnlyDictionary<int, int> SpawnLengths = new Dictionary<int, int>
    {
        [2] = 88,
        [3] = 53,
        [4] = 5
    };

    private static readonly IReadOnlyDictionary<int, int> OrientationLengths = new Dictionary<int, int>
    {
        [2] = 109,
        [3] = 7
    };

    private static readonly IReadOnlyDictionary<int, int> KillAchievementLengths = new Dictionary<int, int>
    {
        [4] = 450
    };

    private static IReadOnlyDictionary<int, int> Histogram<T>(IReadOnlyList<Parsed<T>> items)
        => items.GroupBy(parsed => parsed.Raw.Count)
                .ToDictionary(group => group.Key, group => group.Count());

    /// <summary>
    ///     Pins the exact raw-length distribution of every positional-array population. A future snapshot that added a ragged
    ///     shape would move these; the STJ port must handle every length present here.
    /// </summary>
    [Test]
    public void T6_RawLengthDistributions_ArePinned()
    {
        Histogram(Data.Value.Tiles)
            .ToList()
            .Should()
            .BeEquivalentTo(TileLengths.ToList(), "GTile lengths");

        Histogram(Data.Value.StraightLines)
            .ToList()
            .Should()
            .BeEquivalentTo(StraightLineLengths.ToList(), "StraightLine lengths");

        Histogram(Data.Value.Doors)
            .ToList()
            .Should()
            .BeEquivalentTo(DoorLengths.ToList(), "GDoor lengths");

        Histogram(Data.Value.Spawns)
            .ToList()
            .Should()
            .BeEquivalentTo(SpawnLengths.ToList(), "GSpawn lengths");

        Histogram(Data.Value.Orientations)
            .ToList()
            .Should()
            .BeEquivalentTo(OrientationLengths.ToList(), "Orientation lengths");

        Histogram(Data.Value.KillAchievements)
            .ToList()
            .Should()
            .BeEquivalentTo(KillAchievementLengths.ToList(), "GKillAchievement lengths");
    }
    #endregion
}