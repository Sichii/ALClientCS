#region
using AL.Core.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Holds the carved winterland corridor to the two rules the server enforces on the crossing: a path onto the
///     ice golem island must exist, and no leg of it may start or end over the lake, where the server's
///     move-endpoint check rejects the position. The jail band is restated here from sampled values of the
///     server's walkability lattice rather than read from the production carve, so a wrong carve cannot agree
///     with itself.
/// </summary>
public class IceGolemCorridorTests : PathfindingTestBed
{
    //the ice golem spawn on the island (winterland spawn 6)
    private static readonly Location GOLEM_SPAWN = new("winterland", 864.5f, 429.5f);

    //winterland's town spawn - guaranteed walkable
    private static readonly Location TOWN = new("winterland", 0f, 0f);

    //lake cells the server rejects as move endpoints (lattice value >= 2) span y 250..330 on the corridor
    //columns, and the server rounds an endpoint to its nearest lattice point - so anything within 5 units of
    //those cells jails. bounds are those cells plus that rounding reach
    private const float JAIL_MIN_X = 838f;
    private const float JAIL_MAX_X = 874f;
    private const float JAIL_MIN_Y = 245f;
    private const float JAIL_MAX_Y = 335f;

    [Test]
    public async Task TheCorridorReachesTheIslandWithoutAJailableLeg()
    {
        var path = await Pathfinder.FindPathAsync(TOWN, [new Destination(GOLEM_SPAWN, 0f)], false)
                                   .ToArrayAsync();

        path.Should()
            .NotBeEmpty("the carved corridor connects the winterland mainland to the ice golem island");

        //the corridor is straight, so smoothing should hand the whole water span to a single leg - one move
        //command, which is what keeps a mid-lake re-issue (and its jail) out of the design entirely
        var lakeCrossings = path.Count(edge => (Math.Min(edge.Start.Vertex.Y, edge.End.Vertex.Y) < JAIL_MIN_Y)
                                               && (Math.Max(edge.Start.Vertex.Y, edge.End.Vertex.Y) > JAIL_MAX_Y)
                                               && (edge.Start.Vertex.X > JAIL_MIN_X)
                                               && (edge.Start.Vertex.X < JAIL_MAX_X));

        lakeCrossings.Should()
                     .Be(1, "the water span should be walked as exactly one leg");

        foreach (var edge in path)
            foreach (var point in new[]
                     {
                         edge.Start.Vertex,
                         edge.End.Vertex
                     })
            {
                var jailable = (point.X > JAIL_MIN_X)
                               && (point.X < JAIL_MAX_X)
                               && (point.Y > JAIL_MIN_Y)
                               && (point.Y < JAIL_MAX_Y);

                jailable.Should()
                        .BeFalse($"no leg may start or end over the lake, but ({point.X}, {point.Y}) does");
            }
    }
}
