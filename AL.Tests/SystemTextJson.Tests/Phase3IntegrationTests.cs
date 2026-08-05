#region
using System.Text.Json;
using System.Text.Json.Nodes;
using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.Core.Json;
using AL.Data.Geometry;
using AL.Data.Items;
using AL.Data.Maps;
using AL.Data.Skills;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.SystemTextJson.Tests;

/// <summary>
///     STJ Phase 3 end-to-end check: real game-data models deserialize through the canonical <see cref="ALJson.Options" />
///     — proving the models' own System.Text.Json attributes and the type-matched converter factories are wired correctly
///     together.
///     <c>
///         [JsonPropertyName]
///     </c>
///     renames, the attributed-object harvest, scroll-stat recovery, the value-tuple factory (single and list), positional
///     arrays, map rectangles, and the tolerant-enum factory are all exercised end to end.
/// </summary>
public sealed class Phase3IntegrationTests
{
    [Test]
    public void ArrayToObject_PositionalType_BindsThroughSharedOptions()
    {
        // the shared options register ArrayToObjectConverterFactory; the recursion guard must neutralize the FACTORY
        // for the inner fill or the whole object binds to null (the Phase-3 gate's critical finding). GDoor is
        // [JsonArrayIndex]-mapped: 0=X 1=Y 2=Width 3=Height 4=DestinationMap 5=DestinationSpawnId.
        var door = JsonSerializer.Deserialize<GDoor>("""[10,20,30,40,"main",5,0,0,0]""", ALJson.Options)!;

        door.Should()
            .NotBeNull();

        door.X
            .Should()
            .Be(10);

        door.Y
            .Should()
            .Be(20);

        door.DestinationMap
            .Should()
            .Be("main");

        door.DestinationSpawnId
            .Should()
            .Be(5);
    }

    [Test]
    public void GGeometry_ObjectBinds_DespiteBeingIEnumerable()
    {
        // GGeometry : IRectangle : IEnumerable<IPoint>, so STJ classifies it as a collection and cannot bind its named
        // object form; IRectangle carries [JsonForcedObject], so ForcedObjectConverter binds it as a named object.
        var geometry = JsonSerializer.Deserialize<GGeometry>(
            """{"min_x":-100,"max_x":100,"min_y":-50,"max_y":50,"tiles":[],"x_lines":[],"y_lines":[]}""",
            ALJson.Options)!;

        geometry.MinX
                .Should()
                .Be(-100);

        geometry.MaxX
                .Should()
                .Be(100);

        geometry.MinY
                .Should()
                .Be(-50);

        geometry.MaxY
                .Should()
                .Be(50);

        geometry.Tiles
                .Should()
                .BeEmpty();
    }

    [Test]
    public void GItem_BindsRenames_HarvestsAttributes_RecoversScroll_ParsesGivesTuple()
    {
        const string WIRE = """{"attack":5,"g":100,"grade":3.6,"stat":"int","gives":[["str",10],["dex",5]]}""";

        var item = JsonSerializer.Deserialize<GItem>(WIRE, ALJson.Options)!;

        // [JsonPropertyName("g")] rename
        item.GoldValue
            .Should()
            .Be(100);

        // Grade is int?; the attributed inner path rounds the fractional wire value 3.6 -> 4 (banker's rounding)
        item.Grade
            .Should()
            .Be(4);

        // scroll-stat recovery: the non-numeric `stat` name is stripped before binding (float Stat stays 0) and recovered
        item.Stat
            .Should()
            .Be(0);

        item.ScrollStat
            .Should()
            .Be(ALAttribute.Int);

        // numeric ALAttribute key harvested into the dictionary behind IReadOnlyDictionary
        item.Attributes
            .Should()
            .ContainKey(ALAttribute.Attack)
            .WhoseValue
            .Should()
            .Be(5);

        // Gives carries no property-level converter; the global TupleConverterFactory fills each [name, amount] pair
        item.Gives
            .Should()
            .NotBeNull();

        item.Gives!.Should()
            .HaveCount(2);

        item.Gives[0]
            .Attribute
            .Should()
            .Be(ALAttribute.Str);

        item.Gives[0]
            .Amount
            .Should()
            .Be(10);

        item.Gives[1]
            .Attribute
            .Should()
            .Be(ALAttribute.Dex);

        item.Gives[1]
            .Amount
            .Should()
            .Be(5);
    }

    [Test]
    public void GItem_StackSize_FalsyValueDegradesToConverterDefault()
    {
        // [JsonConverter(FalsyStackSizeConverter)] on StackSize: a falsy `s` (JavaScript a&&a) must degrade to 1, not
        // throw binding a bool to int (the Phase-3 gate's minor finding). A real value still passes through.
        JsonSerializer.Deserialize<GItem>("""{"s":false}""", ALJson.Options)!.StackSize
                      .Should()
                      .Be(1);

        JsonSerializer.Deserialize<GItem>("""{"s":7}""", ALJson.Options)!.StackSize
                      .Should()
                      .Be(7);

        //an absent s never reaches the converter at all, so this is the property initializer rather than the
        //converter default - the same 1, by a different route, and worth pinning separately
        JsonSerializer.Deserialize<GItem>("{}", ALJson.Options)!.StackSize
                      .Should()
                      .Be(1);
    }

    [Test]
    public void GItem_StackSize_EveryWireSpellingIsNumeric()
    {
        //the design tables spell most stackables "s":true, and the converter above would read a boolean as its
        //falsy default of 1 - so every stackable would report a cap of one and read as not stackable at all. That
        //never happens, because design/items.js:7441-7443 rewrites true to 9999 before the server serialises G, and
        //the wire this snapshot captured is what the client actually fetches. Pinned because the boolean spelling in
        //the design tables is convincing enough to have been mistaken for a live defect: if a host ever serves the
        //raw spelling this fails here rather than silently disabling every stack in the client
        Fixture.Section("items")
               .AsObject()
               .Where(entry => entry.Value is JsonObject wire && wire.ContainsKey("s"))
               .Select(entry => entry.Value!["s"]!.GetValueKind())
               .Distinct()
               .Should()
               .ContainSingle()
               .Which.Should()
               .Be(JsonValueKind.Number);
    }

    [Test]
    public void GMap_ParsesOnDeathTuple_AndRename()
    {
        const string WIRE = """{"on_death":["main",7],"safe_pvp":true}""";

        var map = JsonSerializer.Deserialize<GMap>(WIRE, ALJson.Options)!;

        // single-tuple property: [JsonPropertyName("on_death")] on (string Map, float Spawn), served by the tuple factory
        map.OnDeath
           .Map
           .Should()
           .Be("main");

        map.OnDeath
           .Spawn
           .Should()
           .Be(7);

        // [JsonPropertyName("safe_pvp")] rename on a non-attributed object
        map.SafePvP
           .Should()
           .BeTrue();
    }

    [Test]
    public void GSkill_ParsesLevelModsTuple_AndRename()
    {
        const string WIRE = """{"range_bonus":5,"levels":[[1,2],[3,4]]}""";

        var skill = JsonSerializer.Deserialize<GSkill>(WIRE, ALJson.Options)!;

        // [JsonPropertyName("range_bonus")] rename
        skill.RangeBonus
             .Should()
             .Be(5);

        // [JsonPropertyName("levels")] on a list of tuples -> the global tuple factory fills each [level, value] pair
        skill.LevelMods
             .Should()
             .NotBeNull();

        skill.LevelMods!.Should()
             .HaveCount(2);

        skill.LevelMods[0]
             .Should()
             .Be((1f, 2f));

        skill.LevelMods[1]
             .Should()
             .Be((3f, 4f));
    }

    [Test]
    public void GSkill_WeaponTypes_AcceptsSingleStringAndArray()
    {
        // [JsonConverter(ArrayOrSingleConverter<WeaponType>)] on IReadOnlyList<WeaponType>: the server sends `wtype`
        // as either a single string (stomp='basher') or an array; both must bind (the Phase-3 gate's major finding).
        var single = JsonSerializer.Deserialize<GSkill>("""{"wtype":"basher"}""", ALJson.Options)!;

        single.WeaponTypes
              .Should()
              .NotBeNull();

        single.WeaponTypes!.Should()
              .HaveCount(1);

        var many = JsonSerializer.Deserialize<GSkill>("""{"wtype":["basher","fist"]}""", ALJson.Options)!;

        many.WeaponTypes
            .Should()
            .NotBeNull();

        many.WeaponTypes!.Should()
            .HaveCount(2);
    }

    [Test]
    public void GTrap_Polygon_And_Position_Bind()
    {
        var trap = JsonSerializer.Deserialize<GTrap>("""{"polygon":[[10,20],[30,40]],"position":[5,6]}""", ALJson.Options)!;

        trap.Polygon
            .Should()
            .NotBeNull();

        trap.Polygon!.Vertices
            .Should()
            .HaveCount(2);

        // a Point member needs no property-level attribute; the globally registered ArrayToPointConverter claims it
        trap.Position
            .X
            .Should()
            .Be(5);

        trap.Position
            .Y
            .Should()
            .Be(6);
    }

    [Test]
    public void GZone_PolygonVertices_BindFromNestedArrays()
    {
        // Vertices is a Polygon (IEnumerable<IPoint> with no add-path), so no per-element converter can apply under
        // STJ; PolygonConverter claims the whole [[x,y],...] shape instead (fix-verify residual).
        var zone = JsonSerializer.Deserialize<GZone>("""{"polygon":[[10,20],[30,40],[50,60]]}""", ALJson.Options)!;

        zone.Vertices
            .Should()
            .NotBeNull();

        zone.Vertices
            .Vertices
            .Should()
            .HaveCount(3);

        zone.Vertices
            .Vertices[0]
            .X
            .Should()
            .Be(10);

        zone.Vertices
            .Vertices[0]
            .Y
            .Should()
            .Be(20);
    }

    [Test]
    public void MapRectangle_DeserializesFromNumericArray()
    {
        var rectangle = JsonSerializer.Deserialize<MapRectangle>("[0,0,100,100]", ALJson.Options)!;

        // a 4-length numeric array carries no map name
        rectangle.Map
                 .Should()
                 .BeEmpty();
    }

    [Test]
    public void Point_DeserializesFromArray()
    {
        var point = JsonSerializer.Deserialize<Point>("[10,20]", ALJson.Options);

        point.X
             .Should()
             .Be(10);

        point.Y
             .Should()
             .Be(20);
    }

    [Test]
    public void TolerantEnum_DegradesUnknown_AndAcceptsNumeric()
    {
        // unknown wire value -> the enum's zero member instead of throwing, which is what the tolerant factory is for
        JsonSerializer.Deserialize<DamageType>("\"totally_bogus\"", ALJson.Options)
                      .Should()
                      .Be(default);

        // a string-enum member also accepts the raw integer form; the factory allows both
        JsonSerializer.Deserialize<DamageType>("1", ALJson.Options)
                      .Should()
                      .Be((DamageType)1);
    }
}