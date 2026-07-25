#region
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AL.Core.Geometry;
using AL.Core.Json.Converters;
using AL.Data.Maps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the exact coordinates <see cref="MapRectangleConverter" /> produces today, under Newtonsoft,
///     for every map rectangle in the committed snapshot. The converter composes
///     <c>ArrayToTupleConverter&lt;object,int,int,int,int&gt;</c>, so three of the four coordinates come back
///     through <c>int</c> and lose their fraction; Phase 1 widens them to <c>float</c> and the recovered
///     fractions are the migration diff. This is the fixed target that diff is measured against (plan
///     T5 / S12 / S17).
/// </summary>
[TestClass]
public class MapBoundaryCharacterization
{
    /// <summary>
    ///     Committed fixture, copied into the build output by the <c>Fixtures\**\*</c> glob in
    ///     <c>AL.Tests.csproj</c>. Read back through <see cref="Fixture.ReadCommittedSnapshot" />.
    /// </summary>
    private const string COMMITTED_NAME = "map-boundaries.json";

    /// <summary>
    ///     A distinct name for the freshly generated copy, so regenerating it never overwrites the committed
    ///     fixture's output path and turns the regression check into a tautology.
    /// </summary>
    private const string GENERATED_NAME = "map-boundaries.generated.json";

    /// <summary>The System.Text.Json regeneration, kept beside the Newtonsoft one for diffing.</summary>
    private const string STJ_GENERATED_NAME = "map-boundaries.stj-generated.json";

    /// <summary>
    ///     One captured rectangle: a stable location key plus the final <see cref="MapRectangle" /> fields the
    ///     migration can move. <paramref name="Field" /> is retained so counts can be reported per category.
    /// </summary>
    private readonly record struct RectRecord(string Loc, string Field, string RectMap, float X, float Y, float W, float H);

    /// <summary>
    ///     Deserializes every <see cref="MapRectangle" /> the converter is wired to — a monster's
    ///     <c>boundary</c>/<c>boundaries</c>/<c>rage</c> and an NPC's <c>boundary</c> — from the committed
    ///     game-data snapshot, in a deterministic order.
    /// </summary>
    private static List<RectRecord> CollectRectangles(Func<JToken, GMap> deserialize)
    {
        var maps = (JObject)Fixture.Section("maps");
        var records = new List<RectRecord>();

        //ordinal map order keeps the fixture stable regardless of document order
        foreach (var mapProperty in maps.Properties()
                                        .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            var mapKey = mapProperty.Name;
            var map = deserialize(mapProperty.Value);

            for (var monsterIndex = 0; monsterIndex < map.Monsters.Count; monsterIndex++)
            {
                var monster = map.Monsters[monsterIndex];
                var owner = $"{mapKey}|monster[{monsterIndex}]:{monster.Name}";

                Add(records, $"{owner}|boundary", "boundary", monster._boundary);

                if (monster._boundaries != null)
                    for (var i = 0; i < monster._boundaries.Count; i++)
                        Add(records, $"{owner}|boundaries[{i}]", "boundaries", monster._boundaries[i]);

                Add(records, $"{owner}|rage", "rage", monster.RageRect);
            }

            for (var npcIndex = 0; npcIndex < map.NPCs.Count; npcIndex++)
            {
                var npc = map.NPCs[npcIndex];

                Add(records, $"{mapKey}|npc[{npcIndex}]:{npc.Id}|boundary", "npc", npc.Boundary);
            }
        }

        return records;
    }

    /// <summary>The frozen Newtonsoft oracle.</summary>
    private static GMap FromNewtonsoft(JToken token) => token.ToObject<GMap>()!;

    /// <summary>The production System.Text.Json path, through <c>MapRectangleConverter</c>.</summary>
    private static GMap FromStj(JToken token) => TestJson.Data<GMap>(token.ToString(Formatting.None))!;

    private static void Add(List<RectRecord> records, string loc, string field, MapRectangle? rectangle)
    {
        if (rectangle == null)
            return;

        records.Add(new RectRecord(loc, field, rectangle.Map, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
    }

    /// <summary>
    ///     Renders the records as deterministic, diff-friendly JSON. Floats use round-trip formatting so the
    ///     fixture preserves the exact bits, and Phase 1's diff shows precisely which coordinates recovered a
    ///     fraction.
    /// </summary>
    private static string Render(List<RectRecord> records)
    {
        var builder = new StringBuilder();
        builder.Append("{\n  \"count\": ").Append(records.Count).Append(",\n  \"rectangles\": [\n");

        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];

            builder.Append(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"    {{\"loc\": {JsonConvert.ToString(record.Loc)}, \"field\": {JsonConvert.ToString(record.Field)}, \"rectMap\": {JsonConvert.ToString(record.RectMap)}, \"x\": {record.X:R}, \"y\": {record.Y:R}, \"w\": {record.W:R}, \"h\": {record.H:R}}}"));

            builder.Append(i < records.Count - 1 ? ",\n" : "\n");
        }

        builder.Append("  ]\n}\n");

        return builder.ToString();
    }

    /// <summary>
    ///     The whole-population pin. Regenerates the snapshot from the committed data and asserts it equals the
    ///     committed fixture byte-for-byte. Phase 1 re-runs this: only the boundaries that recovered a
    ///     truncated fraction may differ, and the diff must be exactly those.
    /// </summary>
    [TestMethod]
    public void T5_AllMapRectangles_MatchCommittedSnapshot()
    {
        var generated = Render(CollectRectangles(FromNewtonsoft));

        //written beside the binary under a distinct name so it can be diffed / re-committed, and so it never
        //clobbers the committed fixture's own output path
        Fixture.WriteSnapshot(GENERATED_NAME, generated);

        var committed = Fixture.ReadCommittedSnapshot(COMMITTED_NAME);

        Assert.IsNotNull(
            committed,
            $"Committed fixture '{COMMITTED_NAME}' is missing. A freshly generated copy was written to the "
            + $"test binary's Fixtures\\snapshots\\{GENERATED_NAME}. Copy it into "
            + $"AL.Tests/Fixtures/snapshots/{COMMITTED_NAME} and commit it.");

        Assert.AreEqual(
            Normalize(committed),
            Normalize(generated),
            "Map rectangle coordinates drifted from the committed Newtonsoft baseline.");
    }

    /// <summary>
    ///     Line-ending agnostic comparison, as the sibling census suites already do. <see cref="Render" /> emits
    ///     <c>\n</c>, but the repo has <c>core.autocrlf=true</c> and no <c>.gitattributes</c>, so a checkout
    ///     rewrites the committed fixture to CRLF and a raw string compare would fail on line 2 of a fresh clone.
    /// </summary>
    private static string Normalize(string text) => text.Replace("\r\n", "\n");

    /// <summary>
    ///     The migration's proof for T5: all 160-plus rectangles re-collected through the production
    ///     System.Text.Json options must render to the <b>same</b> committed fixture. Only the deserializer
    ///     changes — <see cref="Render" /> is engine-neutral, so any diff is a real coordinate difference.
    /// </summary>
    [TestMethod]
    public void T5_AllMapRectangles_StjPath_ReproducesCommittedSnapshot()
    {
        var generated = Render(CollectRectangles(FromStj));

        Fixture.WriteSnapshot(STJ_GENERATED_NAME, generated);

        var committed = Fixture.ReadCommittedSnapshot(COMMITTED_NAME);

        Assert.IsNotNull(committed, $"Committed fixture '{COMMITTED_NAME}' is missing.");

        Assert.AreEqual(
            Normalize(committed),
            Normalize(generated),
            "the System.Text.Json map-rectangle path does not reproduce the pinned Newtonsoft coordinates");
    }

    /// <summary>
    ///     Guards the count the plan asserts (T5 claims 160). Boundaries = monster <c>boundary</c> +
    ///     <c>boundaries</c> + NPC <c>boundary</c>. <c>rage</c> is the same converter but a separate field, so
    ///     it is counted apart. The exact numbers are verified here rather than trusted.
    /// </summary>
    [TestMethod]
    public void T5_RectangleCounts_ArePinned()
    {
        var records = CollectRectangles(FromNewtonsoft);

        var monsterBoundary = records.Count(record => record.Field == "boundary");
        var monsterBoundaries = records.Count(record => record.Field == "boundaries");
        var npcBoundary = records.Count(record => record.Field == "npc");
        var rage = records.Count(record => record.Field == "rage");
        var boundaries = monsterBoundary + monsterBoundaries + npcBoundary;

        Assert.AreEqual(146, monsterBoundary, "monster single boundary count");
        Assert.AreEqual(7, monsterBoundaries, "monster boundaries-list element count");
        Assert.AreEqual(3, npcBoundary, "npc boundary count");
        Assert.AreEqual(4, rage, "rage rectangle count");
        Assert.AreEqual(156, boundaries, "boundary rectangles (monster boundary + boundaries + npc)");

        //the plan's "160" is the whole converter population INCLUDING the 4 rage rectangles, not just boundaries
        Assert.AreEqual(160, records.Count, "total rectangles through MapRectangleConverter (plan T5's 160)");
    }

    /// <summary>
    ///     Documents how the converter builds a rectangle from a 4-element numeric array. Only the first
    ///     coordinate is float-parsed; the remaining three go through <c>int</c> (which ROUNDS, it does not
    ///     truncate) and lose their fraction. This is the mechanism the snapshot captures in aggregate and
    ///     Phase 1 changes.
    /// </summary>
    [TestMethod]
    public void T5_FourElementNumericArray_KeepsFirstCoordinateFractional_RoundsTheRest()
    {
        //[x1, y1, x2, y2] — after the Phase 1 widening ALL four coordinates are float; none rounds
        const string PAYLOAD = "[100.7, 200.9, 300.4, 400.6]";

        var rectangle = JsonConvert.DeserializeObject<MapRectangle>(PAYLOAD, new MapRectangleConverter())!;

        //built from Point(100.7, 200.9) and Point(300.4, 400.6) — every fraction now survives:
        //  center X = (100.7 + 300.4) / 2 = 200.55
        //  center Y = (200.9 + 400.6) / 2 = 300.75
        //  width    = |100.7 - 300.4| = 199.7
        //  height   = |200.9 - 400.6| = 199.7
        Assert.AreEqual(200.55, rectangle.X, 0.001, "center X keeps the first coordinate's fraction");
        Assert.AreEqual(300.75, rectangle.Y, 0.001, "center Y now keeps its fraction (float, not rounded int)");
        Assert.AreEqual(199.7, rectangle.Width, 0.001);
        Assert.AreEqual(199.7, rectangle.Height, 0.001);
        Assert.AreEqual(string.Empty, rectangle.Map, "a 4-element numeric array carries no map name");
    }

    /// <summary>
    ///     Documents the 5-element form <c>[map, x1, y1, x2, y2]</c>: the first element is a non-numeric
    ///     string, so <c>float.TryParse</c> fails and ALL four coordinates go through <c>int</c>.
    /// </summary>
    [TestMethod]
    public void T5_FiveElementArray_WithMapName_RoundsAllFourCoordinates()
    {
        const string PAYLOAD = "[\"main\", 100.7, 200.9, 300.4, 400.6]";

        var rectangle = JsonConvert.DeserializeObject<MapRectangle>(PAYLOAD, new MapRectangleConverter())!;

        //after the Phase 1 widening all four coordinates are float: 100.7, 200.9, 300.4, 400.6
        //  center X = (100.7 + 300.4) / 2 = 200.55
        //  center Y = (200.9 + 400.6) / 2 = 300.75
        //  width    = |100.7 - 300.4| = 199.7
        //  height   = |200.9 - 400.6| = 199.7
        Assert.AreEqual("main", rectangle.Map);
        Assert.AreEqual(200.55, rectangle.X, 0.001, "coordinates are float now, so the fraction survives even with a map name");
        Assert.AreEqual(300.75, rectangle.Y, 0.001);
        Assert.AreEqual(199.7, rectangle.Width, 0.001);
        Assert.AreEqual(199.7, rectangle.Height, 0.001);
    }

    /// <summary>
    ///     Pins the converter's culture behaviour. The plan (S17) predicts 100x coordinate corruption under
    ///     <c>de-DE</c> because <c>float.TryParse</c> is called with no <see cref="IFormatProvider" />. This
    ///     test drives the real converter under <c>de-DE</c> and asserts what actually happens, restoring the
    ///     culture afterwards.
    /// </summary>
    [TestMethod]
    public void T5_MapRectangleConverter_CultureSensitivity_DeDE()
    {
        //a 4-element numeric array whose first coordinate has a fraction — the only value float-parsed
        const string PAYLOAD = "[123.45, 10, 20, 30]";

        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var invariant = JsonConvert.DeserializeObject<MapRectangle>(PAYLOAD, new MapRectangleConverter())!;

            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var german = JsonConvert.DeserializeObject<MapRectangle>(PAYLOAD, new MapRectangleConverter())!;

            //invariant baseline: obj=123.45(double), y1/x2/y2 = 10/20/30
            //  center X = (123.45 + 20) / 2 = 71.725   (first coordinate float-parsed, fraction survives)
            Assert.AreEqual(71.725, invariant.X, 0.001, "invariant baseline keeps the 0.45 fraction");
            Assert.AreEqual(20.0, invariant.Y, 0.001);

            //REFUTES the plan's "100x corruption" for the CURRENT converter: Newtonsoft parses the JSON
            //number to a double, then obj.ToString() re-stringifies it through de-DE ("123,45"), and
            //float.TryParse reads it back through the same de-DE culture -> 123.45, unchanged. The stringify
            //and the parse cancel because both honour CurrentCulture, so there is no corruption here. The
            //100x risk is real only for the Phase 1 converter that float-parses the raw JSON substring.
            Assert.AreEqual(invariant.X, german.X, 0.0, "de-DE center X must match invariant");
            Assert.AreEqual(invariant.Y, german.Y, 0.0, "de-DE center Y must match invariant");
            Assert.AreEqual(invariant.Width, german.Width, 0.0);
            Assert.AreEqual(invariant.Height, german.Height, 0.0);
        } finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
