#region
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using AL.Data.Geometry;
using AL.Data.Maps;
using AL.Data.Monsters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins what <see cref="ArrayToObjectConverter{T}" /> produces today, under Newtonsoft, for every
///     positional array in the committed game-data snapshot: <see cref="GTile" />,
///     <see cref="StraightLine" />, <see cref="GDoor" />, <see cref="GSpawn" />, <see cref="Orientation" />
///     and <see cref="GKillAchievement" />. Phase 4 rewrites the converter with direct construction and is
///     differentially tested against the canonical fixture this suite commits (plan T6 / decision 8).
/// </summary>
/// <remarks>
///     Every array is fed through the real converter exactly as production does. The four
///     <c>ItemConverterType</c>/<c>JsonConverter</c> sites this covers all resolve the inner element with
///     <c>ArrayToObjectConverter&lt;T&gt;.Singleton</c>, whose <c>ReadJson</c> builds a <see cref="JObject" />
///     from the declared indices and calls <c>JToken.ToObject&lt;T&gt;()</c> with a default serializer — so
///     driving each array through <see cref="JsonConvert.DeserializeObject{T}(string, JsonConverter[])" /> is
///     byte-identical to the game-data load path.
/// </remarks>
[TestClass]
public class PositionalArrayCharacterization
{
    private const string CANONICAL_FIXTURE = "positional-arrays.canonical.json";

    //written beside the binary on every run for diffing; never read back, so it can never satisfy the
    //committed-fixture guard the way writing CANONICAL_FIXTURE itself would
    private const string GENERATED_SIDECAR = "positional-arrays.canonical.generated.json";

    //the same, regenerated through the production System.Text.Json options
    private const string STJ_GENERATED_SIDECAR = "positional-arrays.canonical.stj-generated.json";

    //pinned actuals for the committed snapshot. The plan (T6) guessed 7,283 / 12,830 / 96 / 146 / 116 /
    //450; where a number below differs from that guess it is reported as a finding — reality wins.
    private const int TILE_COUNT = 7283;

    private const int STRAIGHT_LINE_COUNT = 12830;

    private const int DOOR_COUNT = 96;

    private const int SPAWN_COUNT = 146;

    private const int ORIENTATION_COUNT = 116;

    private const int KILL_ACHIEVEMENT_COUNT = 450;

    private static readonly Lazy<Population> Data = new(() => Collect(useStj: false));

    /// <summary>
    ///     The same population parsed through the production System.Text.Json options. Kept separate and lazy so
    ///     the Newtonsoft oracle above stays untouched and neither engine pays for the other's 20k-line parse.
    /// </summary>
    private static readonly Lazy<Population> StjData = new(() => Collect(useStj: true));

    #region Collection

    private static Population Collect(bool useStj)
    {
        var tiles = new List<Parsed<GTile>>();
        var straightLines = new List<Parsed<StraightLine>>();
        var doors = new List<Parsed<GDoor>>();
        var spawns = new List<Parsed<GSpawn>>();
        var orientations = new List<Parsed<Orientation>>();
        var killAchievements = new List<Parsed<GKillAchievement>>();

        //geometry.<map>.tiles / x_lines / y_lines
        var geometry = (JObject)Fixture.Section("geometry");

        foreach (var mapProperty in geometry.Properties()
                                            .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            if (mapProperty.Value is not JObject map)
                continue;

            foreach (var tile in InnerArrays(map, "tiles"))
                tiles.Add(ParseOne<GTile>(tile, useStj));

            //VerticalLines binds x_lines, HorizontalLines binds y_lines; both parse to StraightLine
            foreach (var line in InnerArrays(map, "x_lines"))
                straightLines.Add(ParseOne<StraightLine>(line, useStj));

            foreach (var line in InnerArrays(map, "y_lines"))
                straightLines.Add(ParseOne<StraightLine>(line, useStj));
        }

        //maps.<map>.doors / spawns / npcs[].position / npcs[].positions
        var maps = (JObject)Fixture.Section("maps");

        foreach (var mapProperty in maps.Properties()
                                        .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            if (mapProperty.Value is not JObject map)
                continue;

            foreach (var door in InnerArrays(map, "doors"))
                doors.Add(ParseOne<GDoor>(door, useStj));

            foreach (var spawn in InnerArrays(map, "spawns"))
                spawns.Add(ParseOne<GSpawn>(spawn, useStj));

            if (map["npcs"] is JArray npcs)
                foreach (var npc in npcs.OfType<JObject>())
                {
                    //_position is a single positional array; _positions is an array of them
                    if (npc["position"] is JArray position)
                        orientations.Add(ParseOne<Orientation>(position, useStj));

                    if (npc["positions"] is JArray positions)
                        foreach (var inner in positions.OfType<JArray>())
                            orientations.Add(ParseOne<Orientation>(inner, useStj));
                }
        }

        //monsters.<monster>.achievements
        var monsters = (JObject)Fixture.Section("monsters");

        foreach (var monsterProperty in monsters.Properties()
                                                .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            if (monsterProperty.Value is not JObject monster)
                continue;

            foreach (var achievement in InnerArrays(monster, "achievements"))
                killAchievements.Add(ParseOne<GKillAchievement>(achievement, useStj));
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
    private static IEnumerable<JArray> InnerArrays(JObject obj, string key)
    {
        if (obj[key] is not JArray outer)
            yield break;

        foreach (var element in outer)
            if (element is JArray inner)
                yield return inner;
    }

    private static Parsed<T> ParseOne<T>(JArray array, bool useStj)
    {
        var json = array.ToString(Formatting.None);

        //a flag rather than a delegate: T varies per call site, and a delegate cannot stay generic
        var value = useStj
            ? TestJson.Data<T>(json)!
            : JsonConvert.DeserializeObject<T>(json, ArrayToObjectConverter<T>.Singleton)!;

        return new Parsed<T>(array, value);
    }

    #endregion

    #region Canonicalization

    private static IReadOnlyList<string> CanonicalLines(Population population)
    {
        var lines = new List<string>();

        Emit(lines, "GTile", population.Tiles, parsed => Canon(parsed.Value));
        Emit(lines, "StraightLine", population.StraightLines, parsed => Canon(parsed.Value));
        Emit(lines, "GDoor", population.Doors, parsed => Canon(parsed.Value));
        Emit(lines, "GSpawn", population.Spawns, parsed => Canon(parsed.Value));
        Emit(lines, "Orientation", population.Orientations, parsed => Canon(parsed.Value));
        Emit(lines, "GKillAchievement", population.KillAchievements, parsed => Canon(parsed.Value));

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

    private static string NF(float? value) => value.HasValue ? F(value.Value) : "null";

    private static string S(string? value) => value ?? "null";

    //name plus underlying integer, so an unmapped/degraded value is still unambiguous
    private static string E<TEnum>(TEnum value) where TEnum : struct, Enum
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
    ///     The differential target. Regenerates the canonical form of every parsed positional array and
    ///     asserts it matches the committed fixture — its hash, its per-type counts, and every line. The
    ///     Phase 4 STJ port must reproduce this fixture exactly.
    /// </summary>
    [TestMethod]
    public void T6_AllPositionalArrays_MatchCommittedCanonicalFixture()
        => AssertReproducesCommittedFixture(Data.Value, GENERATED_SIDECAR, "Newtonsoft");

    /// <summary>
    ///     The migration's proof for T6: all 20,921 positional arrays re-parsed through the production
    ///     System.Text.Json options must canonicalize to the <b>same</b> committed fixture — same per-type
    ///     counts, same lines, same sha256. Only the deserializer changes; the canonical rendering is
    ///     engine-neutral, so any diff is a real parsed-value difference.
    /// </summary>
    [TestMethod]
    public void T6_AllPositionalArrays_StjPath_ReproducesCommittedCanonicalFixture()
        => AssertReproducesCommittedFixture(StjData.Value, STJ_GENERATED_SIDECAR, "System.Text.Json");

    private static void AssertReproducesCommittedFixture(Population population, string sidecar, string engine)
    {
        var lines = CanonicalLines(population);
        var body = string.Join('\n', lines);
        var hash = Sha256Hex(body);

        var counts = new JObject
        {
            ["GTile"] = population.Tiles.Count,
            ["StraightLine"] = population.StraightLines.Count,
            ["GDoor"] = population.Doors.Count,
            ["GSpawn"] = population.Spawns.Count,
            ["Orientation"] = population.Orientations.Count,
            ["GKillAchievement"] = population.KillAchievements.Count
        };

        var generated = new JObject
        {
            ["description"] = "Canonical form of every positional array in the committed data.json snapshot, "
                              + $"parsed through the positional-array converter under {engine}. sha256 is over the "
                              + "newline-joined lines. Regenerated and diffed by PositionalArrayCharacterization.",
            ["sha256"] = hash,
            ["total"] = lines.Count,
            ["counts"] = counts,
            ["lines"] = new JArray(lines.Cast<object>().ToArray())
        };

        //read the committed fixture BEFORE writing anything, so the build-copied source stays authoritative;
        //the fresh copy goes to a distinct sidecar name that the test never reads back
        var committedRaw = Fixture.ReadCommittedSnapshot(CANONICAL_FIXTURE);
        var sidecarPath = Fixture.WriteSnapshot(sidecar, generated.ToString(Formatting.Indented));

        //the bootstrap instruction belongs to the Newtonsoft oracle only. On the STJ side the fixture already
        //exists by definition, and regenerating it to turn a red green would bless the very regression the
        //comparison is there to catch.
        Assert.IsNotNull(
            committedRaw,
            sidecar == STJ_GENERATED_SIDECAR
                ? $"Committed fixture '{CANONICAL_FIXTURE}' is missing; run the Newtonsoft pin first to bootstrap it."
                : $"Committed fixture '{CANONICAL_FIXTURE}' is missing. A freshly generated copy was written to "
                + $"'{sidecarPath}'. Rename it to '{CANONICAL_FIXTURE}', copy it into AL.Tests/Fixtures/snapshots/, and commit it.");

        var committed = JObject.Parse(committedRaw);

        var committedCounts = (JObject)committed["counts"]!;

        foreach (var (type, actual) in counts)
            Assert.AreEqual(
                (int)committedCounts[type]!,
                (int)actual!,
                $"{engine}: parsed count for {type} drifted from the committed fixture.");

        var committedLines = ((JArray)committed["lines"]!).Select(token => (string)token!)
                                                          .ToList();

        //find the first divergence for a legible failure before falling back to the hash
        var limit = Math.Min(committedLines.Count, lines.Count);

        for (var index = 0; index < limit; index++)
            Assert.AreEqual(
                committedLines[index],
                lines[index],
                $"{engine}: canonical line {index} drifted from the committed Newtonsoft baseline.");

        Assert.AreEqual(committedLines.Count, lines.Count, $"{engine}: canonical line count drifted.");

        Assert.AreEqual(
            (string)committed["sha256"]!,
            hash,
            $"{engine}: canonical sha256 drifted from the committed Newtonsoft baseline.");
    }

    /// <summary>
    ///     Guards the committed fixture against hand-editing or corruption: its recorded hash must equal the
    ///     hash of its own recorded lines.
    /// </summary>
    [TestMethod]
    public void T6_CommittedFixture_IsInternallyConsistent()
    {
        var committedRaw = Fixture.ReadCommittedSnapshot(CANONICAL_FIXTURE);

        Assert.IsNotNull(committedRaw, $"Committed fixture '{CANONICAL_FIXTURE}' is missing; run T6_AllPositionalArrays first.");

        var committed = JObject.Parse(committedRaw);
        var committedLines = ((JArray)committed["lines"]!).Select(token => (string)token!);
        var body = string.Join('\n', committedLines);

        Assert.AreEqual(
            (string)committed["sha256"]!,
            Sha256Hex(body),
            "Committed fixture's recorded sha256 does not match its recorded lines.");
    }

    #endregion

    #region Counts

    /// <summary>
    ///     Pins the exact per-type population sizes. These are the numbers verified against the plan's guess.
    /// </summary>
    [TestMethod]
    public void T6_Counts_ArePinned()
    {
        Assert.AreEqual(TILE_COUNT, Data.Value.Tiles.Count, "GTile count");
        Assert.AreEqual(STRAIGHT_LINE_COUNT, Data.Value.StraightLines.Count, "StraightLine count");
        Assert.AreEqual(DOOR_COUNT, Data.Value.Doors.Count, "GDoor count");
        Assert.AreEqual(SPAWN_COUNT, Data.Value.Spawns.Count, "GSpawn count");
        Assert.AreEqual(ORIENTATION_COUNT, Data.Value.Orientations.Count, "Orientation count");
        Assert.AreEqual(KILL_ACHIEVEMENT_COUNT, Data.Value.KillAchievements.Count, "GKillAchievement count");
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
    ///     Pins the exact raw-length distribution of every positional-array population. A future snapshot that
    ///     added a ragged shape would move these; the STJ port must handle every length present here.
    /// </summary>
    [TestMethod]
    public void T6_RawLengthDistributions_ArePinned()
    {
        CollectionAssert.AreEquivalent(TileLengths.ToList(), Histogram(Data.Value.Tiles).ToList(), "GTile lengths");
        CollectionAssert.AreEquivalent(StraightLineLengths.ToList(), Histogram(Data.Value.StraightLines).ToList(), "StraightLine lengths");
        CollectionAssert.AreEquivalent(DoorLengths.ToList(), Histogram(Data.Value.Doors).ToList(), "GDoor lengths");
        CollectionAssert.AreEquivalent(SpawnLengths.ToList(), Histogram(Data.Value.Spawns).ToList(), "GSpawn lengths");
        CollectionAssert.AreEquivalent(OrientationLengths.ToList(), Histogram(Data.Value.Orientations).ToList(), "Orientation lengths");
        CollectionAssert.AreEquivalent(KillAchievementLengths.ToList(), Histogram(Data.Value.KillAchievements).ToList(), "GKillAchievement lengths");
    }

    /// <summary>
    ///     Every <see cref="Orientation" /> from a 2-element <c>position</c>/<c>positions</c> array is missing
    ///     index 2, so <see cref="Orientation.Direction" /> falls to the enum's zero member
    ///     (<see cref="Direction.Down" />). A 3-element array carries the direction. 109 are 2-element and 7
    ///     are 3-element in the committed snapshot.
    /// </summary>
    [TestMethod]
    public void T6_Orientations_ShortArrays_DefaultDirectionToDown()
    {
        var twoElement = 0;
        var threeElement = 0;

        foreach (var parsed in Data.Value.Orientations)
            switch (parsed.Raw.Count)
            {
                case 2:
                    twoElement++;

                    Assert.AreEqual(
                        Direction.Down,
                        parsed.Value.Direction,
                        "a 2-element orientation array has no index-2, so Direction defaults to the zero member");

                    break;
                case 3:
                    threeElement++;

                    //index 2 is an integer direction; the tolerant string-enum converter reads it via AllowIntegerValues
                    var expected = (Direction)(int)parsed.Raw[2]!;

                    Assert.AreEqual(expected, parsed.Value.Direction, "a 3-element orientation array carries its direction at index 2");

                    break;
                default:
                    Assert.Fail($"unexpected orientation array length {parsed.Raw.Count}: {parsed.Raw.ToString(Formatting.None)}");

                    break;
            }

        Assert.AreEqual(109, twoElement, "2-element orientation arrays");
        Assert.AreEqual(7, threeElement, "3-element orientation arrays");
    }

    /// <summary>
    ///     A <see cref="GSpawn" /> from a short array defaults every missing trailing member: a 2-element
    ///     <c>[x, y]</c> yields <see cref="Direction.Down" /> and <c>Distance == 0</c>. 88 spawns are
    ///     2-element in the committed snapshot — the plan (T6) guessed 87; reality is 88.
    /// </summary>
    [TestMethod]
    public void T6_Spawns_ShortArrays_DefaultTrailingMembers()
    {
        var twoElement = 0;

        foreach (var parsed in Data.Value.Spawns)
        {
            if (parsed.Raw.Count == 2)
                twoElement++;

            if (parsed.Raw.Count < 4)
                Assert.AreEqual(0f, parsed.Value.Distance, "a spawn shorter than 4 elements has no index-3, so Distance defaults to 0");

            if (parsed.Raw.Count < 3)
                Assert.AreEqual(
                    Direction.Down,
                    parsed.Value.Direction,
                    "a spawn shorter than 3 elements has no index-2, so Direction defaults to the zero member");
        }

        Assert.AreEqual(88, twoElement, "2-element spawn arrays (plan guessed 87)");
    }

    /// <summary>
    ///     A <see cref="GDoor" /> array runs 6..9 elements. <see cref="GDoor.LockType" /> (index 7) and
    ///     <see cref="GDoor.KeyType" /> (index 8) are the trailing members, so a door shorter than 8 defaults
    ///     LockType to <see cref="LockType.None" /> and one shorter than 9 defaults KeyType to
    ///     <see cref="KeyType.None" />. 86 of 96 doors are 7-element, so both default for the vast majority.
    /// </summary>
    [TestMethod]
    public void T6_Doors_ShortArrays_DefaultTrailingEnums()
    {
        var lockDefaulted = 0;
        var keyDefaulted = 0;

        foreach (var parsed in Data.Value.Doors)
        {
            if (parsed.Raw.Count < 8)
            {
                lockDefaulted++;

                Assert.AreEqual(LockType.None, parsed.Value.LockType, "a door shorter than 8 elements has no index-7, so LockType defaults to None");
            }

            if (parsed.Raw.Count < 9)
            {
                keyDefaulted++;

                Assert.AreEqual(KeyType.None, parsed.Value.KeyType, "a door shorter than 9 elements has no index-8, so KeyType defaults to None");
            }
        }

        //6-element (1) + 7-element (86) lack LockType; those plus 8-element (4) lack KeyType
        Assert.AreEqual(87, lockDefaulted, "doors defaulting LockType");
        Assert.AreEqual(91, keyDefaulted, "doors defaulting KeyType");
    }

    /// <summary>
    ///     A <see cref="GTile" /> longer than its declared index set (0..4) ignores the extra trailing
    ///     elements; the 4 six-element tiles all carry <c>null</c> at index 4, leaving
    ///     <see cref="GTile.Unknown" /> null and dropping index 5 entirely.
    /// </summary>
    [TestMethod]
    public void T6_Tiles_LongArrays_IgnoreTrailingElements_NullIndexFourStaysNull()
    {
        var longTiles = 0;

        foreach (var parsed in Data.Value.Tiles)
        {
            if (parsed.Raw.Count <= 5)
                continue;

            longTiles++;

            //every 6-element tile in the snapshot has null at index 4
            Assert.AreEqual(JTokenType.Null, parsed.Raw[4]!.Type, "the 6-element tiles carry null at index 4");
            Assert.IsNull(parsed.Value.Unknown, "a null at index 4 leaves Unknown null");

            //index 0..3 still bind from a long array; index 5 is never mapped
            Assert.AreEqual((string)parsed.Raw[0]!, parsed.Value.TileSet, "TileSet still binds from index 0 of a long array");
        }

        Assert.AreEqual(4, longTiles, "6-element tile arrays");
    }

    /// <summary>
    ///     A 4-element tile (the common case, 6145 of them) has no index 4, so <see cref="GTile.Unknown" /> is
    ///     null; a 5-element tile (1134 of them) binds a numeric <see cref="GTile.Unknown" /> from index 4.
    /// </summary>
    [TestMethod]
    public void T6_Tiles_IndexFour_BindsOnlyWhenPresentAndNumeric()
    {
        var fourElement = 0;
        var fiveElement = 0;

        foreach (var parsed in Data.Value.Tiles)
            switch (parsed.Raw.Count)
            {
                case 4:
                    fourElement++;

                    Assert.IsNull(parsed.Value.Unknown, "a 4-element tile has no index-4, so Unknown is null");

                    break;
                case 5:
                    fiveElement++;

                    Assert.AreEqual((float)parsed.Raw[4]!, parsed.Value.Unknown, "a 5-element tile binds Unknown from its numeric index-4");

                    break;
            }

        Assert.AreEqual(6145, fourElement, "4-element tile arrays");
        Assert.AreEqual(1134, fiveElement, "5-element tile arrays");
    }

    /// <summary>
    ///     Pins that <see cref="StraightLine.IsVertical" /> is always <c>false</c> for a parsed line: the wire
    ///     array carries only indices 0..2 (On/Start/End) and <c>IsVertical</c> has no
    ///     <c>[JsonArrayIndex]</c>, so it defaults. The x_line / y_line distinction is not recoverable from the
    ///     converter — only <c>GGeometry</c>'s property carries it.
    /// </summary>
    [TestMethod]
    public void T6_StraightLines_IsVertical_AlwaysDefaultsFalse()
    {
        Assert.IsTrue(Data.Value.StraightLines.Count > 0, "expected straight lines in the snapshot");

        foreach (var parsed in Data.Value.StraightLines)
            Assert.IsFalse(
                parsed.Value.IsVertical,
                "IsVertical carries no array index, so the converter always leaves it false");
    }

    #endregion

    #region Enum-in-array mapping

    /// <summary>
    ///     Pins how string-valued enum members inside a positional array are read: a
    ///     <see cref="GKillAchievement" /> array is <c>[points, "stat", "hp", amount]</c>, and the tolerant
    ///     string-enum converter maps <c>"stat"</c> to <see cref="AchievementRewardType.Stat" /> and each
    ///     attribute name to its <see cref="ALAttribute" /> member.
    /// </summary>
    [TestMethod]
    public void T6_KillAchievements_MapStringEnumsInsideArray()
    {
        Assert.IsTrue(Data.Value.KillAchievements.Count > 0, "expected kill achievements in the snapshot");

        foreach (var parsed in Data.Value.KillAchievements)
        {
            //index 1 is a string reward type; only "stat" appears, and it maps to Stat (never the zero member)
            Assert.AreEqual(
                AchievementRewardType.Stat,
                parsed.Value.RewardType,
                $"reward type at index 1 ({parsed.Raw[1]}) should map to Stat");

            //index 2 is a string attribute; a real attribute name must not degrade to None
            Assert.AreNotEqual(
                ALAttribute.None,
                parsed.Value.Attribute,
                $"attribute at index 2 ({parsed.Raw[2]}) should map to a real ALAttribute member");
        }
    }

    #endregion

    private sealed record Parsed<T>(JArray Raw, T Value);

    private sealed record Population(
        IReadOnlyList<Parsed<GTile>> Tiles,
        IReadOnlyList<Parsed<StraightLine>> StraightLines,
        IReadOnlyList<Parsed<GDoor>> Doors,
        IReadOnlyList<Parsed<GSpawn>> Spawns,
        IReadOnlyList<Parsed<Orientation>> Orientations,
        IReadOnlyList<Parsed<GKillAchievement>> KillAchievements);
}
