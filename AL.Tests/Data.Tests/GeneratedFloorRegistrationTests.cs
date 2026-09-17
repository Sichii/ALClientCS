#region
using AL.Data;
using AL.Data.Maps;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     A dungeon run's floors are filed into the map and geometry tables while the run lasts, enriched the way G's own
///     maps are on load, and taken out again afterwards. The manifest files a floor's record before its geometry
///     arrives, so a stair on a delivered floor can already resolve the spawn it lands on.
/// </summary>
public class GeneratedFloorRegistrationTests : GameDataTestBed
{
    private const string RUN = "6f1e2d3c4b5a69788796a5b4";
    private static readonly string FLOOR_0 = GeneratedMapBundle.FloorKey(RUN, 0);
    private static readonly string FLOOR_1 = GeneratedMapBundle.FloorKey(RUN, 1);

    [Test]
    public void ADeliveredFloorReplacesItsManifestEntry()
    {
        try
        {
            GameData.RegisterGeneratedFloors(GeneratedMapBundle.Parse(Bundle(deliverFloor1: false)));

            GameData.Maps[FLOOR_1]!
                    .Geomertry
                    .Should()
                    .BeNull();

            GameData.RegisterGeneratedFloors(GeneratedMapBundle.Parse(Bundle(deliverFloor1: true)));

            var floor1 = GameData.Maps[FLOOR_1]!;

            floor1.Geomertry
                  .Should()
                  .NotBeNull();

            floor1.Exits
                  .Should()
                  .ContainSingle(exit => exit.ToLocation.Map == FLOOR_0);
        } finally
        {
            GameData.UnregisterGeneratedRun(RUN);
        }
    }

    [Test]
    public void RegisteringABundleFilesItsFloorsAndTakesThemOutAgain()
    {
        var mapsBefore = GameData.Maps.Entries.Count;
        var geometryBefore = GameData.Geometry.Entries.Count;

        try
        {
            GameData.RegisterGeneratedFloors(GeneratedMapBundle.Parse(Bundle(deliverFloor1: false)));

            var floor0 = GameData.Maps[FLOOR_0]!;

            floor0.Accessor
                  .Should()
                  .Be(FLOOR_0);

            floor0.Generated!
                  .Run
                  .Should()
                  .Be(RUN);

            var geometry = GameData.Geometry[FLOOR_0]!;

            floor0.Geomertry
                  .Should()
                  .BeSameAs(geometry);

            //the border walls every map gets on load, on top of the one line the floor itself carries
            geometry.VerticalLines
                    .Count
                    .Should()
                    .Be(3);

            geometry.HorizontalLines
                    .Count
                    .Should()
                    .Be(2);

            //both doors resolve: the stair through the manifest entry, the exit through main
            floor0.Exits
                  .Should()
                  .HaveCount(2);

            floor0.Exits
                  .Should()
                  .Contain(exit => exit.ToLocation.Map == FLOOR_1)
                  .And
                  .Contain(exit => exit.ToLocation.Map == "main");

            GameData.Maps[FLOOR_1]!
                    .Accessor
                    .Should()
                    .Be(FLOOR_1);
        } finally
        {
            GameData.UnregisterGeneratedRun(RUN);
        }

        GameData.Maps[FLOOR_0]
                .Should()
                .BeNull();

        GameData.Maps[FLOOR_1]
                .Should()
                .BeNull();

        GameData.Geometry[FLOOR_0]
                .Should()
                .BeNull();

        GameData.Maps
                .Entries
                .Count
                .Should()
                .Be(mapsBefore);

        GameData.Geometry
                .Entries
                .Count
                .Should()
                .Be(geometryBefore);
    }

    private static string Bundle(bool deliverFloor1)
    {
        var geometry1 = deliverFloor1
            ? """, "geometry": { "min_x": 0, "max_x": 400, "min_y": 0, "max_y": 400, "x_lines": [], "y_lines": [] }"""
            : "";

        var floor1 = $$"""
                       {
                         "key": "{{FLOOR_1}}",
                         "definition": {
                           "name": "Floor 2",
                           "key": "{{FLOOR_1}}",
                           "generated": { "run": "{{RUN}}", "floor": 1, "zone": "dreams" },
                           "spawns": [[20, 200], [200, 200]],
                           "doors": [[0, 200, 24, 32, "{{FLOOR_0}}", 1, 0]],
                           "monsters": [],
                           "npcs": []
                         }{{geometry1}}
                       }
                       """;

        var floor0 = $$"""
                       {
                         "key": "{{FLOOR_0}}",
                         "definition": {
                           "name": "Floor 1",
                           "key": "{{FLOOR_0}}",
                           "generated": { "run": "{{RUN}}", "floor": 0, "zone": "dreams" },
                           "spawns": [[200, 200], [360, 200]],
                           "doors": [[380, 200, 24, 32, "{{FLOOR_1}}", 0, 1], [200, 380, 24, 32, "main", 0, 0]],
                           "monsters": [],
                           "npcs": []
                         },
                         "geometry": { "min_x": 0, "max_x": 400, "min_y": 0, "max_y": 400, "x_lines": [[100, 0, 50]], "y_lines": [] }
                       }
                       """;

        return deliverFloor1
            ? $$"""{ "run": "{{RUN}}", "floors": [{{floor1}}], "manifest": [] }"""
            : $$"""{ "run": "{{RUN}}", "floors": [{{floor0}}], "manifest": [{{floor1}}] }""";
    }
}
