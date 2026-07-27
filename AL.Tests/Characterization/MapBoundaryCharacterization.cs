#region
using System.Globalization;
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
        => Fixture.ShouldMatchCommittedSnapshot(Render(CollectRectangles()), COMMITTED_NAME);

    /// <summary>
    ///     Documents the converter's two array shapes, both built from Point(x1, y1) and Point(x2, y2). In the 5-element form
    ///     the leading element fails
    ///     <c>
    ///         float.TryParse
    ///     </c>
    ///     and is taken as the map name rather than a coordinate; the 4-element form carries no map name. All four
    ///     coordinates have been
    ///     <c>
    ///         float
    ///     </c>
    ///     since the Phase 1 widening, so no fraction is lost either way: center X = (100.7 + 300.4) / 2 = 200.55, center
    ///     Y = (200.9 + 400.6) / 2 = 300.75, and both extents are 199.7. This is the mechanism the snapshot captures in
    ///     aggregate.
    /// </summary>
    [Test]
    [Arguments("[100.7, 200.9, 300.4, 400.6]", "")]
    [Arguments("[\"main\", 100.7, 200.9, 300.4, 400.6]", "main")]
    public void T5_CoordinateArray_KeepsEveryFraction(string payload, string expectedMap)
    {
        var rectangle = TestJson.Data<MapRectangle>(payload)!;

        rectangle.Map
                 .Should()
                 .Be(expectedMap, "only a non-numeric leading element is taken as a map name");

        rectangle.X
                 .Should()
                 .BeApproximately(200.55f, 0.001f, "center X keeps the first coordinate's fraction");

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
    ///     One captured rectangle: a stable location key plus the final <see cref="MapRectangle" /> fields the migration can
    ///     move. <paramref name="Field" /> is rendered into the fixture, which is what pins the per-category counts.
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