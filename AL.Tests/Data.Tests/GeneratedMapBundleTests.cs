#region
using AL.Data.Maps;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The bundle a run of map_chunk frames joins into. Each floor is an ordinary map record plus its geometry under a key
///     the run id and floor number spell out, and the manifest lists the floors the same run has not delivered yet. The
///     game's client checks the key against the run and floor before it trusts a floor; so does this.
/// </summary>
public class GeneratedMapBundleTests
{
    private const string RUN = "6f1e2d3c4b5a69788796a5b4";

    [Test]
    public void AFloorKeyThatDoesNotSpellItsRunAndFloorIsRefused()
    {
        var act = () => GeneratedMapBundle.Parse(Bundle($"zone_{RUN}_1", 0));

        act.Should()
           .Throw<InvalidOperationException>();
    }

    private static string Bundle(string floorKey, int floor)
        => $$"""
             {
               "run": "{{RUN}}",
               "floors": [
                 {
                   "key": "{{floorKey}}",
                   "definition": {
                     "name": "Floor 1",
                     "key": "{{floorKey}}",
                     "generated": { "run": "{{RUN}}", "floor": {{floor}}, "zone": "dreams" },
                     "spawns": [[200, 200]],
                     "doors": [[380, 200, 24, 32, "zone_{{RUN}}_1", 0, 0]],
                     "monsters": [],
                     "npcs": []
                   },
                   "geometry": {
                     "min_x": 0, "max_x": 400, "min_y": 0, "max_y": 400,
                     "x_lines": [[100, 0, 50]],
                     "y_lines": []
                   }
                 }
               ],
               "manifest": [
                 {
                   "key": "zone_{{RUN}}_1",
                   "definition": {
                     "name": "Floor 2",
                     "key": "zone_{{RUN}}_1",
                     "generated": { "run": "{{RUN}}", "floor": 1, "zone": "dreams" },
                     "spawns": [[20, 200]],
                     "doors": [],
                     "monsters": [],
                     "npcs": []
                   }
                 }
               ]
             }
             """;

    [Test]
    public void ParsesFloorsAndManifest()
    {
        var bundle = GeneratedMapBundle.Parse(Bundle($"zone_{RUN}_0", 0));

        bundle.Run
              .Should()
              .Be(RUN);

        var floor = bundle.Floors
                          .Should()
                          .ContainSingle()
                          .Subject;

        floor.Key
             .Should()
             .Be($"zone_{RUN}_0");

        floor.Definition
             .Generated
             .Should()
             .BeEquivalentTo(
                 new GGenerated
                 {
                     Run = RUN,
                     Floor = 0,
                     Zone = "dreams"
                 });

        floor.Definition
             .Doors
             .Should()
             .ContainSingle()
             .Which
             .DestinationMap
             .Should()
             .Be($"zone_{RUN}_1");

        floor.Geometry!.VerticalLines
             .Should()
             .ContainSingle();

        floor.Geometry!.MaxX
             .Should()
             .Be(400);

        bundle.Manifest
              .Should()
              .ContainSingle()
              .Which
              .Key
              .Should()
              .Be($"zone_{RUN}_1");
    }
}