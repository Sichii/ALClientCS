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
    ///     The server refuses a recall anywhere in a run with <c>cant_escape</c> (<c>node/server.js</c>'s <c>town</c>
    ///     handler), so a walk starting on a floor walks even where the recall would have been cheaper.
    /// </summary>
    [Test]
    public void AWalkStartingOnAFloorNeverRecalls()
    {
        try
        {
            Pathfinder.RegisterGeneratedRun(GeneratedMapBundle.Parse(Bundle()));

            var path = Pathfinder.FindPath(new Location(FLOOR_0, 380, 380), [new Destination(new Location(FLOOR_0, 200, 200), 5)]);

            path.Should()
                .NotBeEmpty()
                .And
                .NotContain(edge => edge.Type == EdgeType.Town);
        } finally
        {
            Pathfinder.UnregisterGeneratedRun(RUN);
        }
    }

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