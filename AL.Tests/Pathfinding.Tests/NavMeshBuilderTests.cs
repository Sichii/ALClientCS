#region
using AL.Data;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class NavMeshBuilderTests : GameDataTestBed
{
    private static TriangleMesh BuildMain()
    {
        var map = GameData.Maps.Main;
        var geometry = GameData.Geometry[map.Accessor]!;

        return new NavMeshBuilder(map, geometry).BuildMesh();
    }

    [Test]
    public void BuildingMainAllocatesFarLessThanTheOldStackDid()
    {
        //main used to pre-size a 2.6M-entry flood stack up front; the build is now bounded by the raster. Measured
        //per thread, since the suite runs other classes alongside this one and a process-wide count reads theirs too
        BuildMain();

        var before = GC.GetAllocatedBytesForCurrentThread();
        BuildMain();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        allocated.Should()
                 .BeLessThan(60L * 1024 * 1024);
    }

    [Test]
    public void EveryTriangleIsFoundAtItsOwnCentroid()
    {
        var mesh = BuildMain();
        var misses = 0;

        for (var triangle = 0; triangle < mesh.TriangleCount; triangle++)
        {
            (var x, var y) = mesh.Centroid(triangle);

            if (mesh.TriangleAt(x, y) != triangle)
                misses++;
        }

        misses.Should()
              .Be(0);
    }

    [Test]
    public void MainTriangulatesAndContainsItsSpawn()
    {
        var mesh = BuildMain();
        var spawn = GameData.Maps.Main.Spawns[0];

        mesh.TriangleCount
            .Should()
            .BeGreaterThan(1000);

        mesh.TriangleAt(spawn.X, spawn.Y)
            .Should()
            .NotBe(-1);
    }

    /// <summary>
    ///     Both halves of the builder's rule on a live mesh: a vertex whose incident triangles form one fan carries adjacency,
    ///     a vertex whose fan is only part of them carries none. Main has no pinch, so the first half is what this holds here
    ///     and
    ///     <c>
    ///         TriangleMeshTests.PinchVertexGetsNoEdges
    ///     </c>
    ///     covers the second; a mesh whose adjacency was dropped wholesale used to pass this and now does not.
    /// </summary>
    [Test]
    public void MainVerticesCarryEdgesExactlyWhenTheyAreNotAPinch()
    {
        var mesh = BuildMain();
        var incident = new List<int>[mesh.Vertices.Length];

        for (var triangle = 0; triangle < mesh.TriangleCount; triangle++)
            for (var slot = 0; slot < 3; slot++)
                (incident[mesh.Corners[triangle * 3 + slot]] ??= []).Add(triangle);

        var classified = 0;
        var pinched = 0;

        for (var vertex = 0; vertex < incident.Length; vertex++)
        {
            //a vertex no triangle names is outside the rule: the builder only asks about the ones it triangulated
            if (incident[vertex] is not { Count: > 0 } triangles)
                continue;

            //flood the triangles at the vertex across the two edges that meet there, which is the walk the builder
            //counts with FanSize; one sector reaching them all is its non-pinch case
            var reached = new HashSet<int>
            {
                triangles[0]
            };
            var pending = new Stack<int>();
            pending.Push(triangles[0]);

            while (pending.TryPop(out var triangle))
            {
                var slot = mesh.SlotOfVertex(triangle, vertex);

                for (var step = 1; step <= 2; step++)
                {
                    var neighbour = mesh.Neighbour(triangle, (slot + step) % 3);

                    if ((neighbour >= 0) && reached.Add(neighbour))
                        pending.Push(neighbour);
                }
            }

            classified++;

            if (reached.Count == triangles.Count)
            {
                mesh.EdgesFrom(vertex)
                    .Length
                    .Should()
                    .BeGreaterThan(0, $"vertex {vertex} is one fan of {triangles.Count} triangle(s)");

                continue;
            }

            pinched++;

            mesh.EdgesFrom(vertex)
                .Length
                .Should()
                .Be(0, $"vertex {vertex} is a pinch");
        }

        classified.Should()
                  .BeGreaterThan(0, "the rule is only worth anything if some vertex was put to it");

        Console.WriteLine($"PINCH main vertices={mesh.Vertices.Length} classified={classified} pinched={pinched}");
    }

    [Test]
    public void NeighbourRelationsAreSymmetric()
    {
        var mesh = BuildMain();

        for (var triangle = 0; triangle < mesh.TriangleCount; triangle++)
            for (var slot = 0; slot < 3; slot++)
            {
                var neighbour = mesh.Neighbour(triangle, slot);

                if (neighbour < 0)
                    continue;

                ((mesh.Neighbour(neighbour, 0) == triangle)
                 || (mesh.Neighbour(neighbour, 1) == triangle)
                 || (mesh.Neighbour(neighbour, 2) == triangle)).Should()
                                                               .BeTrue(
                                                                   $"triangle {triangle} names {neighbour} across slot {slot}, which must name it back");
            }
    }
}