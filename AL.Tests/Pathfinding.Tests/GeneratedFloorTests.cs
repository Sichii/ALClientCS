#region
using AL.Core.Geometry;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     A dungeon run's floors get meshes of their own the moment they arrive, and a portal graph of their own that joins
///     them through the stairs. The world graph never learns about a run: nothing walks into one, the party is pulled in
///     by the keeper, so a route only ever starts on a floor. Taking the run out drops its meshes, its graph and its map
///     records together.
/// </summary>
public class GeneratedFloorTests : PathfindingTestBed
{
    private const string RUN = "6f1e2d3c4b5a69788796a5b4";
    private static readonly string FLOOR_0 = GeneratedMapBundle.FloorKey(RUN, 0);
    private static readonly string FLOOR_1 = GeneratedMapBundle.FloorKey(RUN, 1);

    [Test]
    public void ARegisteredRunRoutesBetweenItsFloorsThroughTheStairs()
    {
        try
        {
            Pathfinder.RegisterGeneratedRun(GeneratedMapBundle.Parse(Bundle()));

            Pathfinder.GetNavMesh(FLOOR_0)
                      .Should()
                      .NotBeNull();

            Pathfinder.IsWall(new Location(FLOOR_0, 50, 200))
                      .Should()
                      .BeFalse();

            var path = Pathfinder.FindPath(new Location(FLOOR_0, 50, 200), [new Destination(new Location(FLOOR_1, 300, 200), 20)]);

            path.Should()
                .Contain(edge => edge.Type == EdgeType.Door);

            path[^1]
                .End
                .Map
                .Should()
                .Be(FLOOR_1);
        } finally
        {
            Pathfinder.UnregisterGeneratedRun(RUN);
        }

        Pathfinder.GetNavMesh(FLOOR_0)
                  .Should()
                  .BeNull();

        GameData.Maps[FLOOR_0]
                .Should()
                .BeNull();
    }

    /// <summary>
    ///     The server sends only the floor a character stands on, and builds the next one when somebody first takes the
    ///     stairs (generated_maps.js, send_generated_maps and ensure_generated_floor). So the floor below is a manifest
    ///     record with spawns and no geometry until the stairs have been used, and the route down has to end by stepping
    ///     through the door onto the spawn it lands on.
    /// </summary>
    [Test]
    public void ARouteDownEndsThroughTheStairsOntoAFloorNotYetSent()
    {
        try
        {
            Pathfinder.RegisterGeneratedRun(GeneratedMapBundle.Parse(LiveBundle()));

            Pathfinder.GetNavMesh(LIVE_FLOOR_1)
                      .Should()
                      .BeNull();

            //every spawn on the floor below, the way the cave errand asks for the stairs
            var ends = new[]
            {
                new Destination(new Location(LIVE_FLOOR_1, 20, 200), 20),
                new Destination(new Location(LIVE_FLOOR_1, 200, 200), 20)
            };

            var path = Pathfinder.FindPath(new Location(LIVE_FLOOR_0, 50, 200), ends);

            var last = path[^1];

            last.Type
                .Should()
                .Be(EdgeType.Door);

            //the door lands on the floor's first spawn, and the leg ends on the caller's own end object
            last.End
                .Should()
                .Be(ends[0]);

            last.Start
                .Should()
                .BeOfType<Exit>()
                .Which
                .ToSpawnIndex
                .Should()
                .Be(0);

            path.Take(path.Count - 1)
                .Should()
                .OnlyContain(edge => (edge.Type == EdgeType.Walk) && edge.End.Map == LIVE_FLOOR_0);
        } finally
        {
            Pathfinder.UnregisterGeneratedRun(LIVE_RUN);
        }
    }

    /// <summary>
    ///     A floor with no geometry has nowhere to walk, so the only point on it a route can end at is one a door lands on.
    /// </summary>
    [Test]
    public void APointOnAFloorNotYetSentThatNoDoorLandsOnHasNoRoute()
    {
        try
        {
            Pathfinder.RegisterGeneratedRun(GeneratedMapBundle.Parse(LiveBundle()));

            var act = () => Pathfinder.FindPath(
                new Location(LIVE_FLOOR_0, 50, 200),
                [new Destination(new Location(LIVE_FLOOR_1, 300, 200), 20)]);

            act.Should()
               .Throw<InvalidOperationException>();
        } finally
        {
            Pathfinder.UnregisterGeneratedRun(LIVE_RUN);
        }
    }

    private const string LIVE_RUN = "0a1b2c3d4e5f60718293a4b5";
    private static readonly string LIVE_FLOOR_0 = GeneratedMapBundle.FloorKey(LIVE_RUN, 0);
    private static readonly string LIVE_FLOOR_1 = GeneratedMapBundle.FloorKey(LIVE_RUN, 1);

    /// <summary>
    ///     The shape the live server sends on arrival: the current floor with its geometry, and the whole run's floor list in
    ///     the manifest.
    /// </summary>
    private static string LiveBundle()
        => $$"""
             {
               "run": "{{LIVE_RUN}}",
               "floors": [
                 {
                   "key": "{{LIVE_FLOOR_0}}",
                   "definition": {
                     "name": "Floor 1",
                     "key": "{{LIVE_FLOOR_0}}",
                     "generated": { "run": "{{LIVE_RUN}}", "floor": 0, "zone": "dreams" },
                     "spawns": [[200, 200], [360, 200]],
                     "doors": [[380, 200, 24, 32, "{{LIVE_FLOOR_1}}", 0, 1]],
                     "monsters": [],
                     "npcs": []
                   },
                   "geometry": { "min_x": 0, "max_x": 400, "min_y": 0, "max_y": 400, "x_lines": [], "y_lines": [] }
                 }
               ],
               "manifest": [
                 {
                   "key": "{{LIVE_FLOOR_0}}",
                   "definition": {
                     "name": "Floor 1",
                     "key": "{{LIVE_FLOOR_0}}",
                     "generated": { "run": "{{LIVE_RUN}}", "floor": 0, "zone": "dreams" },
                     "spawns": [[200, 200], [360, 200]],
                     "doors": [[380, 200, 24, 32, "{{LIVE_FLOOR_1}}", 0, 1]],
                     "monsters": [],
                     "npcs": []
                   }
                 },
                 {
                   "key": "{{LIVE_FLOOR_1}}",
                   "definition": {
                     "name": "Floor 2",
                     "key": "{{LIVE_FLOOR_1}}",
                     "generated": { "run": "{{LIVE_RUN}}", "floor": 1, "zone": "dreams" },
                     "spawns": [[20, 200], [200, 200]],
                     "doors": [[0, 200, 24, 32, "{{LIVE_FLOOR_0}}", 1, 0]],
                     "monsters": [],
                     "npcs": []
                   }
                 }
               ]
             }
             """;

    private static string Bundle()
        => $$"""
             {
               "run": "{{RUN}}",
               "floors": [
                 {
                   "key": "{{FLOOR_0}}",
                   "definition": {
                     "name": "Floor 1",
                     "key": "{{FLOOR_0}}",
                     "generated": { "run": "{{RUN}}", "floor": 0, "zone": "dreams" },
                     "spawns": [[200, 200], [360, 200]],
                     "doors": [[380, 200, 24, 32, "{{FLOOR_1}}", 0, 1]],
                     "monsters": [],
                     "npcs": []
                   },
                   "geometry": { "min_x": 0, "max_x": 400, "min_y": 0, "max_y": 400, "x_lines": [], "y_lines": [] }
                 },
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
                   },
                   "geometry": { "min_x": 0, "max_x": 400, "min_y": 0, "max_y": 400, "x_lines": [], "y_lines": [] }
                 }
               ],
               "manifest": []
             }
             """;
}