#region
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Data.Geometry;
using AL.Data.Maps;
using AL.Data.Monsters;
using FluentAssertions;
using System.Threading.Tasks;
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

    //pinned actuals for the committed snapshot. The plan (T6) guessed 7,283 / 12,830 / 96 / 146 / 116 /
    //450; where a number below differs from that guess it is reported as a finding — reality wins.
    private const int TILE_COUNT = 7283;

    private const int STRAIGHT_LINE_COUNT = 12830;

    private const int DOOR_COUNT = 96;

    private const int SPAWN_COUNT = 146;

    private const int ORIENTATION_COUNT = 116;

    private const int KILL_ACHIEVEMENT_COUNT = 450;

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

    #region Counts
    /// <summary>
    ///     Pins the exact per-type population sizes. These are the numbers verified against the plan's guess.
    /// </summary>
    [Test]
    public void T6_Counts_ArePinned()
    {
        Data.Value
            .Tiles
            .Count
            .Should()
            .Be(TILE_COUNT, "GTile count");

        Data.Value
            .StraightLines
            .Count
            .Should()
            .Be(STRAIGHT_LINE_COUNT, "StraightLine count");

        Data.Value
            .Doors
            .Count
            .Should()
            .Be(DOOR_COUNT, "GDoor count");

        Data.Value
            .Spawns
            .Count
            .Should()
            .Be(SPAWN_COUNT, "GSpawn count");

        Data.Value
            .Orientations
            .Count
            .Should()
            .Be(ORIENTATION_COUNT, "Orientation count");

        Data.Value
            .KillAchievements
            .Count
            .Should()
            .Be(KILL_ACHIEVEMENT_COUNT, "GKillAchievement count");
    }
    #endregion

    #region Enum-in-array mapping
    /// <summary>
    ///     Pins how string-valued enum members inside a positional array are read: a <see cref="GKillAchievement" /> array is
    ///     <c>
    ///         [points, "stat", "hp", amount]
    ///     </c>
    ///     , and the tolerant string-enum converter maps
    ///     <c>
    ///         "stat"
    ///     </c>
    ///     to <see cref="AchievementRewardType.Stat" /> and each attribute name to its <see cref="ALAttribute" /> member.
    /// </summary>
    [Test]
    public void T6_KillAchievements_MapStringEnumsInsideArray()
    {
        (Data.Value.KillAchievements.Count > 0).Should()
                                               .BeTrue("expected kill achievements in the snapshot");

        foreach (var parsed in Data.Value.KillAchievements)
        {
            //index 1 is a string reward type; only "stat" appears, and it maps to Stat (never the zero member)
            parsed.Value
                  .RewardType
                  .Should()
                  .Be(AchievementRewardType.Stat, $"reward type at index 1 ({parsed.Raw[1]}) should map to Stat");

            //index 2 is a string attribute; a real attribute name must not degrade to None
            parsed.Value
                  .Attribute
                  .Should()
                  .NotBe(ALAttribute.None, $"attribute at index 2 ({parsed.Raw[2]}) should map to a real ALAttribute member");
        }
    }
    #endregion

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

    /// <summary>
    ///     Every <see cref="Orientation" /> from a 2-element
    ///     <c>
    ///         position
    ///     </c>
    ///     /
    ///     <c>
    ///         positions
    ///     </c>
    ///     array is missing index 2, so <see cref="Orientation.Direction" /> falls to the enum's zero member (
    ///     <see cref="Direction.Down" />). A 3-element array carries the direction. 109 are 2-element and 7 are 3-element in
    ///     the committed snapshot.
    /// </summary>
    [Test]
    public async Task T6_Orientations_ShortArrays_DefaultDirectionToDown()
    {
        var twoElement = 0;
        var threeElement = 0;

        foreach (var parsed in Data.Value.Orientations)
            switch (parsed.Raw.Count)
            {
                case 2:
                    twoElement++;

                    parsed.Value
                          .Direction
                          .Should()
                          .Be(Direction.Down, "a 2-element orientation array has no index-2, so Direction defaults to the zero member");

                    break;
                case 3:
                    threeElement++;

                    //index 2 is an integer direction; the tolerant string-enum converter accepts integer wire forms
                    var expected = (Direction)parsed.Raw[2]!.GetValue<int>();

                    parsed.Value
                          .Direction
                          .Should()
                          .Be(expected, "a 3-element orientation array carries its direction at index 2");

                    break;
                default:
                    Assert.Fail($"unexpected orientation array length {parsed.Raw.Count}: {parsed.Raw.ToJsonString()}");

                    break;
            }

        twoElement.Should()
                  .Be(109, "2-element orientation arrays");

        threeElement.Should()
                    .Be(7, "3-element orientation arrays");
    }

    /// <summary>
    ///     A <see cref="GSpawn" /> from a short array defaults every missing trailing member: a 2-element
    ///     <c>
    ///         [x, y]
    ///     </c>
    ///     yields <see cref="Direction.Down" /> and
    ///     <c>
    ///         Distance == 0
    ///     </c>
    ///     . 88 spawns are 2-element in the committed snapshot — the plan (T6) guessed 87; reality is 88.
    /// </summary>
    [Test]
    public void T6_Spawns_ShortArrays_DefaultTrailingMembers()
    {
        var twoElement = 0;

        foreach (var parsed in Data.Value.Spawns)
        {
            if (parsed.Raw.Count == 2)
                twoElement++;

            if (parsed.Raw.Count < 4)
                parsed.Value
                      .Distance
                      .Should()
                      .Be(0f, "a spawn shorter than 4 elements has no index-3, so Distance defaults to 0");

            if (parsed.Raw.Count < 3)
                parsed.Value
                      .Direction
                      .Should()
                      .Be(Direction.Down, "a spawn shorter than 3 elements has no index-2, so Direction defaults to the zero member");
        }

        twoElement.Should()
                  .Be(88, "2-element spawn arrays (plan guessed 87)");
    }

    /// <summary>
    ///     A <see cref="GDoor" /> array runs 6..9 elements. <see cref="GDoor.LockType" /> (index 7) and
    ///     <see cref="GDoor.KeyType" /> (index 8) are the trailing members, so a door shorter than 8 defaults LockType to
    ///     <see cref="LockType.None" /> and one shorter than 9 defaults KeyType to <see cref="KeyType.None" />. 86 of 96 doors
    ///     are 7-element, so both default for the vast majority.
    /// </summary>
    [Test]
    public void T6_Doors_ShortArrays_DefaultTrailingEnums()
    {
        var lockDefaulted = 0;
        var keyDefaulted = 0;

        foreach (var parsed in Data.Value.Doors)
        {
            if (parsed.Raw.Count < 8)
            {
                lockDefaulted++;

                parsed.Value
                      .LockType
                      .Should()
                      .Be(LockType.None, "a door shorter than 8 elements has no index-7, so LockType defaults to None");
            }

            if (parsed.Raw.Count < 9)
            {
                keyDefaulted++;

                parsed.Value
                      .KeyType
                      .Should()
                      .Be(KeyType.None, "a door shorter than 9 elements has no index-8, so KeyType defaults to None");
            }
        }

        //6-element (1) + 7-element (86) lack LockType; those plus 8-element (4) lack KeyType
        lockDefaulted.Should()
                     .Be(87, "doors defaulting LockType");

        keyDefaulted.Should()
                    .Be(91, "doors defaulting KeyType");
    }

    /// <summary>
    ///     A <see cref="GTile" /> longer than its declared index set (0..4) ignores the extra trailing elements; the 4
    ///     six-element tiles all carry
    ///     <c>
    ///         null
    ///     </c>
    ///     at index 4, leaving <see cref="GTile.Unknown" /> null and dropping index 5 entirely.
    /// </summary>
    [Test]
    public void T6_Tiles_LongArrays_IgnoreTrailingElements_NullIndexFourStaysNull()
    {
        var longTiles = 0;

        foreach (var parsed in Data.Value.Tiles)
        {
            if (parsed.Raw.Count <= 5)
                continue;

            longTiles++;

            //every 6-element tile in the snapshot has null at index 4; a JSON null inside a JsonArray is a null
            //node reference, not a Null-kind node, so the element itself is what gets asserted null here
            parsed.Raw[4]
                  .Should()
                  .BeNull("the 6-element tiles carry null at index 4");

            parsed.Value
                  .Unknown
                  .Should()
                  .BeNull("a null at index 4 leaves Unknown null");

            //index 0..3 still bind from a long array; index 5 is never mapped
            parsed.Value
                  .TileSet
                  .Should()
                  .Be(parsed.Raw[0]!.GetValue<string>(), "TileSet still binds from index 0 of a long array");
        }

        longTiles.Should()
                 .Be(4, "6-element tile arrays");
    }

    /// <summary>
    ///     A 4-element tile (the common case, 6145 of them) has no index 4, so <see cref="GTile.Unknown" /> is null; a
    ///     5-element tile (1134 of them) binds a numeric <see cref="GTile.Unknown" /> from index 4.
    /// </summary>
    [Test]
    public void T6_Tiles_IndexFour_BindsOnlyWhenPresentAndNumeric()
    {
        var fourElement = 0;
        var fiveElement = 0;

        foreach (var parsed in Data.Value.Tiles)
            switch (parsed.Raw.Count)
            {
                case 4:
                    fourElement++;

                    parsed.Value
                          .Unknown
                          .Should()
                          .BeNull("a 4-element tile has no index-4, so Unknown is null");

                    break;
                case 5:
                    fiveElement++;

                    parsed.Value
                          .Unknown
                          .Should()
                          .Be(parsed.Raw[4]!.GetValue<float>(), "a 5-element tile binds Unknown from its numeric index-4");

                    break;
            }

        fourElement.Should()
                   .Be(6145, "4-element tile arrays");

        fiveElement.Should()
                   .Be(1134, "5-element tile arrays");
    }

    /// <summary>
    ///     Pins that <see cref="StraightLine.IsVertical" /> is always
    ///     <c>
    ///         false
    ///     </c>
    ///     for a parsed line: the wire array carries only indices 0..2 (On/Start/End) and
    ///     <c>
    ///         IsVertical
    ///     </c>
    ///     has no
    ///     <c>
    ///         [JsonArrayIndex]
    ///     </c>
    ///     , so it defaults. The x_line / y_line distinction is not recoverable from the converter — only
    ///     <c>
    ///         GGeometry
    ///     </c>
    ///     's property carries it.
    /// </summary>
    [Test]
    public void T6_StraightLines_IsVertical_AlwaysDefaultsFalse()
    {
        (Data.Value.StraightLines.Count > 0).Should()
                                            .BeTrue("expected straight lines in the snapshot");

        foreach (var parsed in Data.Value.StraightLines)
            parsed.Value
                  .IsVertical
                  .Should()
                  .BeFalse("IsVertical carries no array index, so the converter always leaves it false");
    }
    #endregion
}