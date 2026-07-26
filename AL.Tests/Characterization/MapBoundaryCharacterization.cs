#region
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AL.Core.Geometry;
using AL.Data.Maps;
using FluentAssertions;
#endregion

namespace AL.Tests.Characterization;

/// <summary>
///     Pins the exact coordinates
///     <c>
///         MapRectangleConverter
///     </c>
///     produces for every map rectangle in the committed snapshot. The committed fixture is the oracle: it was frozen
///     while the Newtonsoft converter still existed, so reproducing it through the System.Text.Json path proves what the
///     old two-engine differential proved, with no Newtonsoft left in the loop (plan T5 / S12 / S17).
/// </summary>
public class MapBoundaryCharacterization
{
    /// <summary>
    ///     Committed fixture, copied into the build output by the
    ///     <c>
    ///         Fixtures\**\*
    ///     </c>
    ///     glob in
    ///     <c>
    ///         AL.Tests.csproj
    ///     </c>
    ///     . Read back through <see cref="Fixture.ReadCommittedSnapshot" />.
    /// </summary>
    private const string COMMITTED_NAME = "map-boundaries.json";

    /// <summary>
    ///     A distinct name for the freshly generated copy, so regenerating it never overwrites the committed fixture's output
    ///     path and turns the regression check into a tautology.
    /// </summary>
    private const string GENERATED_NAME = "map-boundaries.generated.json";

    //matches JsonConvert.ToString's escaping - quotes, backslash and control characters only, with < > & +
    //left alone - so the rendered strings stay byte-identical to the frozen fixture
    private static readonly JsonSerializerOptions StringOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static void Add(
        List<RectRecord> records,
        string loc,
        string field,
        MapRectangle? rectangle)
    {
        if (rectangle == null)
            return;

        records.Add(
            new RectRecord(
                loc,
                field,
                rectangle.Map,
                rectangle.X,
                rectangle.Y,
                rectangle.Width,
                rectangle.Height));
    }

    /// <summary>
    ///     Deserializes every <see cref="MapRectangle" /> the converter is wired to — a monster's
    ///     <c>
    ///         boundary
    ///     </c>
    ///     /
    ///     <c>
    ///         boundaries
    ///     </c>
    ///     /
    ///     <c>
    ///         rage
    ///     </c>
    ///     and an NPC's
    ///     <c>
    ///         boundary
    ///     </c>
    ///     — from the committed game-data snapshot, through the production System.Text.Json options, in a deterministic order.
    /// </summary>
    private static List<RectRecord> CollectRectangles()
    {
        var maps = Fixture.Section("maps")
                          .AsObject();
        var records = new List<RectRecord>();

        //ordinal map order keeps the fixture stable regardless of document order
        foreach ((var mapKey, var mapValue) in maps.OrderBy(property => property.Key, StringComparer.Ordinal))
        {
            var map = TestJson.Data<GMap>(mapValue!.ToJsonString())!;

            for (var monsterIndex = 0; monsterIndex < map.Monsters.Count; monsterIndex++)
            {
                var monster = map.Monsters[monsterIndex];
                var owner = $"{mapKey}|monster[{monsterIndex}]:{monster.Name}";

                Add(
                    records,
                    $"{owner}|boundary",
                    "boundary",
                    monster._boundary);

                if (monster._boundaries != null)
                    for (var i = 0; i < monster._boundaries.Count; i++)
                        Add(
                            records,
                            $"{owner}|boundaries[{i}]",
                            "boundaries",
                            monster._boundaries[i]);

                Add(
                    records,
                    $"{owner}|rage",
                    "rage",
                    monster.RageRect);
            }

            for (var npcIndex = 0; npcIndex < map.NPCs.Count; npcIndex++)
            {
                var npc = map.NPCs[npcIndex];

                Add(
                    records,
                    $"{mapKey}|npc[{npcIndex}]:{npc.Id}|boundary",
                    "npc",
                    npc.Boundary);
            }
        }

        return records;
    }

    /// <summary>
    ///     Line-ending agnostic comparison, as the sibling census suites already do. <see cref="Render" /> emits
    ///     <c>
    ///         \n
    ///     </c>
    ///     , but the repo has
    ///     <c>
    ///         core.autocrlf=true
    ///     </c>
    ///     and no
    ///     <c>
    ///         .gitattributes
    ///     </c>
    ///     , so a checkout rewrites the committed fixture to CRLF and a raw string compare would fail on line 2 of a fresh
    ///     clone.
    /// </summary>
    private static string Normalize(string text) => text.Replace("\r\n", "\n");

    /// <summary>
    ///     Renders the records as deterministic, diff-friendly JSON. Floats use round-trip formatting so the fixture preserves
    ///     the exact bits, and Phase 1's diff shows precisely which coordinates recovered a fraction.
    /// </summary>
    private static string Render(List<RectRecord> records)
    {
        var builder = new StringBuilder();

        builder.Append("{\n  \"count\": ")
               .Append(records.Count)
               .Append(",\n  \"rectangles\": [\n");

        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];

            builder.Append(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"    {{\"loc\": {JsonSerializer.Serialize(record.Loc, StringOptions)}, \"field\": {JsonSerializer.Serialize(record.Field, StringOptions)}, \"rectMap\": {JsonSerializer.Serialize(record.RectMap, StringOptions)}, \"x\": {record.X:R}, \"y\": {record.Y:R}, \"w\": {record.W:R}, \"h\": {record.H:R}}}"));

            builder.Append(i < (records.Count - 1) ? ",\n" : "\n");
        }

        builder.Append("  ]\n}\n");

        return builder.ToString();
    }

    /// <summary>
    ///     The migration's proof for T5: all 160-plus rectangles re-collected through the production System.Text.Json options
    ///     must render to the
    ///     <b>
    ///         same
    ///     </b>
    ///     committed fixture, which is frozen text from the Newtonsoft era. <see cref="Render" /> is engine-neutral, so any
    ///     diff is a real coordinate difference.
    /// </summary>
    [Test]
    public void T5_AllMapRectangles_StjPath_ReproducesCommittedSnapshot()
    {
        var generated = Render(CollectRectangles());

        //written beside the binary under a distinct name so it can be diffed / re-committed, and so it never
        //clobbers the committed fixture's own output path
        Fixture.WriteSnapshot(GENERATED_NAME, generated);

        var committed = Fixture.ReadCommittedSnapshot(COMMITTED_NAME);

        committed.Should()
                 .NotBeNull(
                     $"Committed fixture '{COMMITTED_NAME}' is missing. A freshly generated copy was written to the "
                     + $"test binary's Fixtures\\snapshots\\{GENERATED_NAME}. Copy it into "
                     + $"AL.Tests/Fixtures/snapshots/{COMMITTED_NAME} and commit it.");

        Normalize(generated)
            .Should()
            .Be(Normalize(committed), "the System.Text.Json map-rectangle path does not reproduce the pinned Newtonsoft coordinates");
    }

    /// <summary>
    ///     Documents the 5-element form
    ///     <c>
    ///         [map, x1, y1, x2, y2]
    ///     </c>
    ///     : the first element is a non-numeric string, so
    ///     <c>
    ///         float.TryParse
    ///     </c>
    ///     fails and it is taken as the map name rather than a coordinate.
    /// </summary>
    /// <remarks>
    ///     The name predates the widening — nothing rounds any more; the assertions below are the truth.
    /// </remarks>
    [Test]
    public void T5_FiveElementArray_WithMapName_RoundsAllFourCoordinates()
    {
        const string PAYLOAD = "[\"main\", 100.7, 200.9, 300.4, 400.6]";

        var rectangle = TestJson.Data<MapRectangle>(PAYLOAD)!;

        //after the Phase 1 widening all four coordinates are float: 100.7, 200.9, 300.4, 400.6
        //  center X = (100.7 + 300.4) / 2 = 200.55
        //  center Y = (200.9 + 400.6) / 2 = 300.75
        //  width    = |100.7 - 300.4| = 199.7
        //  height   = |200.9 - 400.6| = 199.7
        rectangle.Map
                 .Should()
                 .Be("main");

        rectangle.X
                 .Should()
                 .BeApproximately(200.55f, 0.001f, "coordinates are float now, so the fraction survives even with a map name");

        rectangle.Y
                 .Should()
                 .BeApproximately(300.75f, 0.001f);

        rectangle.Width
                 .Should()
                 .BeApproximately(199.7f, 0.001f);

        rectangle.Height
                 .Should()
                 .BeApproximately(199.7f, 0.001f);
    }

    /// <summary>
    ///     Documents how the converter builds a rectangle from a 4-element numeric array. All four coordinates have been
    ///     <c>
    ///         float
    ///     </c>
    ///     since the Phase 1 widening, so no fraction is lost. This is the mechanism the snapshot captures in aggregate.
    /// </summary>
    /// <remarks>
    ///     The name predates the widening — nothing rounds any more; the assertions below are the truth.
    /// </remarks>
    [Test]
    public void T5_FourElementNumericArray_KeepsFirstCoordinateFractional_RoundsTheRest()
    {
        //[x1, y1, x2, y2] — after the Phase 1 widening ALL four coordinates are float; none rounds
        const string PAYLOAD = "[100.7, 200.9, 300.4, 400.6]";

        var rectangle = TestJson.Data<MapRectangle>(PAYLOAD)!;

        //built from Point(100.7, 200.9) and Point(300.4, 400.6) — every fraction now survives:
        //  center X = (100.7 + 300.4) / 2 = 200.55
        //  center Y = (200.9 + 400.6) / 2 = 300.75
        //  width    = |100.7 - 300.4| = 199.7
        //  height   = |200.9 - 400.6| = 199.7
        rectangle.X
                 .Should()
                 .BeApproximately(200.55f, 0.001f, "center X keeps the first coordinate's fraction");

        rectangle.Y
                 .Should()
                 .BeApproximately(300.75f, 0.001f, "center Y now keeps its fraction (float, not rounded int)");

        rectangle.Width
                 .Should()
                 .BeApproximately(199.7f, 0.001f);

        rectangle.Height
                 .Should()
                 .BeApproximately(199.7f, 0.001f);

        rectangle.Map
                 .Should()
                 .Be(string.Empty, "a 4-element numeric array carries no map name");
    }

    /// <summary>
    ///     Pins the converter's culture behaviour — the hazard S17 flagged: a coordinate parsed under
    ///     <c>
    ///         de-DE
    ///     </c>
    ///     , where
    ///     <c>
    ///         ,
    ///     </c>
    ///     is the decimal separator, must not come back 100x off. This test drives the real converter under
    ///     <c>
    ///         de-DE
    ///     </c>
    ///     and asserts what actually happens, restoring the culture afterwards.
    /// </summary>
    [Test]
    public void T5_MapRectangleConverter_CultureSensitivity_DeDE()
    {
        //a 4-element numeric array whose first coordinate has a fraction — the value a decimal-comma culture would maul
        const string PAYLOAD = "[123.45, 10, 20, 30]";

        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var invariant = TestJson.Data<MapRectangle>(PAYLOAD)!;

            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var german = TestJson.Data<MapRectangle>(PAYLOAD)!;

            //invariant baseline: x1=123.45, y1/x2/y2 = 10/20/30
            //  center X = (123.45 + 20) / 2 = 71.725   (first coordinate float-parsed, fraction survives)
            invariant.X
                     .Should()
                     .BeApproximately(71.725f, 0.001f, "invariant baseline keeps the 0.45 fraction");

            invariant.Y
                     .Should()
                     .BeApproximately(20.0f, 0.001f);

            //no 100x corruption: the JSON number never round-trips through a culture-sensitive string. Utf8JsonReader
            //parses it straight to float, and the one string path left (a numeric map-name element) pins
            //CultureInfo.InvariantCulture on float.TryParse explicitly, so CurrentCulture cannot reach either.
            german.X
                  .Should()
                  .Be(invariant.X, "de-DE center X must match invariant");

            german.Y
                  .Should()
                  .Be(invariant.Y, "de-DE center Y must match invariant");

            german.Width
                  .Should()
                  .Be(invariant.Width);

            german.Height
                  .Should()
                  .Be(invariant.Height);
        } finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    ///     Guards the count the plan asserts (T5 claims 160). Boundaries = monster
    ///     <c>
    ///         boundary
    ///     </c>
    ///     +
    ///     <c>
    ///         boundaries
    ///     </c>
    ///     + NPC
    ///     <c>
    ///         boundary
    ///     </c>
    ///     .
    ///     <c>
    ///         rage
    ///     </c>
    ///     is the same converter but a separate field, so it is counted apart. The exact numbers are verified here rather than
    ///     trusted.
    /// </summary>
    [Test]
    public void T5_RectangleCounts_ArePinned()
    {
        //these counts are a property of the data, not of the engine that read it
        var records = CollectRectangles();

        var monsterBoundary = records.Count(record => record.Field == "boundary");
        var monsterBoundaries = records.Count(record => record.Field == "boundaries");
        var npcBoundary = records.Count(record => record.Field == "npc");
        var rage = records.Count(record => record.Field == "rage");
        var boundaries = monsterBoundary + monsterBoundaries + npcBoundary;

        monsterBoundary.Should()
                       .Be(146, "monster single boundary count");

        monsterBoundaries.Should()
                         .Be(7, "monster boundaries-list element count");

        npcBoundary.Should()
                   .Be(3, "npc boundary count");

        rage.Should()
            .Be(4, "rage rectangle count");

        boundaries.Should()
                  .Be(156, "boundary rectangles (monster boundary + boundaries + npc)");

        //the plan's "160" is the whole converter population INCLUDING the 4 rage rectangles, not just boundaries
        records.Count
               .Should()
               .Be(160, "total rectangles through MapRectangleConverter (plan T5's 160)");
    }

    /// <summary>
    ///     One captured rectangle: a stable location key plus the final <see cref="MapRectangle" /> fields the migration can
    ///     move. <paramref name="Field" /> is retained so counts can be reported per category.
    /// </summary>
    private readonly record struct RectRecord(
        string Loc,
        string Field,
        string RectMap,
        float X,
        float Y,
        float W,
        float H);
}