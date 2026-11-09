#region
using AL.Pathfinding;
using AL.Pathfinding.Extensions;
using AL.Pathfinding.Model;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class NodeMeshTests : PathfindingTestBed
{
    private const int SEED = 9;

    [Test]
    [Arguments("main")]
    [Arguments("desertland")]
    [Arguments("winterland")]
    public async Task EveryWalkablePointHasATriangle(string map)
    {
        var mesh = Pathfinder.Graph!.NavMeshes[map];
        var rng = new Random(SEED);

        foreach (var point in SampleWalkable(rng, map, 1_000))
        {
            await Assert.That(mesh.TryFindTriangle(point, out var triangle))
                        .IsTrue();

            //an interior point lies strictly inside exactly one triangle, an edge point strictly inside none
            var strictCount = mesh.Triangles.Count(t => t.Contains<GraphNode, GraphEdge>(point));

            await Assert.That(strictCount)
                        .IsLessThanOrEqualTo(1);

            if (triangle!.Contains<GraphNode, GraphEdge>(point))
                await Assert.That(strictCount)
                            .IsEqualTo(1);
        }
    }

    [Test]
    public async Task CornersListTheirTriangles()
    {
        var mesh = Pathfinder.Graph!.NavMeshes["main"];
        var rng = new Random(SEED);

        foreach (var point in SampleWalkable(rng, "main", 1_000))
        {
            await Assert.That(mesh.TryFindTriangle(point, out var triangle))
                        .IsTrue();

            foreach (var corner in triangle!.Vertices)
                await Assert.That(mesh.FindTrianglesAt(corner))
                            .Contains(triangle);
        }
    }
}