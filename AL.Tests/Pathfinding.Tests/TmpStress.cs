#region
using System.Diagnostics;
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class TmpStress : PathfindingTestBed
{
    [Test]
    public void Stress()
    {
        var output = "D:/Temp/stress.txt";
        var meshes = new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase);

        foreach ((var name, _) in GameData.Maps.Entries)
            if (Pathfinder.GetNavMesh(name) is { } mesh)
                meshes[name] = mesh;

        var maps = meshes.Keys
                         .Where(m => GameData.Maps[m] is { } g && PortalGraph.AllowsHops(g))
                         .ToArray();
        var graph = new PortalGraph(meshes, true);
        var random = new Random(11);

        foreach ((var label, var options) in new[]
                 {
                     ("no blink", PathOptions.NoTown with
                     {
                         WalkSpeed = 90f
                     }),
                     ("blink", PathOptions.NoTown with
                     {
                         WalkSpeed = 90f,
                         BlinkCost = 500f,
                         Mp = 5000f,
                         MaxMp = 5000f
                     })
                 })
        {
            var worst = 0.0;
            var worstRoute = "";
            var noPath = 0;

            for (var i = 0; i < 2000; i++)
            {
                var a = maps[random.Next(maps.Length)];
                var b = random.Next(2) == 0 ? a : maps[random.Next(maps.Length)];
                var va = meshes[a].Mesh.Vertices;
                var vb = meshes[b].Mesh.Vertices;
                var s = new Location(a, va[random.Next(va.Length)]);
                var e = new Location(b, vb[random.Next(vb.Length)]);
                var speed = 40f + (float)random.NextDouble() * 120f;
                var t = Stopwatch.GetTimestamp();

                try
                {
                    graph.FindPath(
                        s,
                        [new Destination(e, 0f)],
                        options with
                        {
                            WalkSpeed = speed,
                            HopScore = (float)random.NextDouble() * 50f
                        });
                } catch (InvalidOperationException)
                {
                    noPath++;
                }

                var ms = Stopwatch.GetElapsedTime(t)
                                  .TotalMilliseconds;

                if (ms > worst)
                {
                    worst = ms;
                    worstRoute = $"{s} -> {e} at {speed:F0}";
                }
            }

            File.AppendAllText(
                output,
                $"{label}: 2000 searches, {noPath} without a route, slowest {worst:F1}ms ({worstRoute}){Environment.NewLine}");
        }
    }
}