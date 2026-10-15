#region
using AL.Core.Geometry;
using AL.Data;
using AL.Pathfinding;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

public class HopBuilderTests : PathfindingTestBed
{
    private static float CrossesAt(Hop hop, float y)
        => hop.Takeoff.X + (hop.Landing.X - hop.Takeoff.X) * ((y - hop.Takeoff.Y) / (hop.Landing.Y - hop.Takeoff.Y));

    private static IEnumerable<(string Map, NavMesh Mesh, List<Hop> Hops)> BuildEveryMap()
    {
        foreach (var map in GameData.Maps.Entries.Keys)
            if (Pathfinder.GetNavMesh(map) is { } mesh)
                yield return (map, mesh, HopBuilder.FindHops(mesh));
    }

    /// <summary>
    ///     Calculates how far a line runs on walkable ground from its start, sampled every unit.
    /// </summary>
    /// <param name="mesh">The map's mesh.</param>
    /// <param name="start">The line's start.</param>
    /// <param name="end">The line's end.</param>
    /// <returns>
    ///     The distance of the first sample off the mesh, or the line's length if none is.
    /// </returns>
    private static float CalculateGroundRun(NavMesh mesh, Point start, Point end)
    {
        var length = MathF.Sqrt(MathF.Pow(end.X - start.X, 2) + MathF.Pow(end.Y - start.Y, 2));
        var distance = 0f;

        while ((distance < length)
               && mesh.IsWalkable(start.X + (end.X - start.X) * (distance / length), start.Y + (end.Y - start.Y) * (distance / length)))
            distance += 1f;

        return MathF.Min(distance, length);
    }

    [Test]
    public void CandidatesBetweenTwoPiecesAreFewAndSpreadOut()
    {
        foreach ((var map, _, var hops) in BuildEveryMap())
            foreach (var group in hops.Where(hop => hop.FromPiece != hop.ToPiece)
                                      .GroupBy(hop => (hop.FromPiece, hop.ToPiece)))
            {
                var takeoffs = group.Select(hop => hop.Takeoff)
                                    .ToList();

                takeoffs.Should()
                        .HaveCountLessThanOrEqualTo(8, $"{map} {group.Key}");

                for (var i = 0; i < takeoffs.Count; i++)
                    for (var j = i + 1; j < takeoffs.Count; j++)
                        MathF.Sqrt(MathF.Pow(takeoffs[i].X - takeoffs[j].X, 2) + MathF.Pow(takeoffs[i].Y - takeoffs[j].Y, 2))
                             .Should()
                             .BeGreaterThanOrEqualTo(50f, $"{map} {group.Key}");
            }
    }

    [Test]
    public void EveryHopHasItsReturn()
    {
        foreach ((var map, _, var hops) in BuildEveryMap())
            foreach (var hop in hops)
                hops.Should()
                    .Contain(
                        back => (back.FromPiece == hop.ToPiece)
                                && (back.ToPiece == hop.FromPiece)
                                && (back.Takeoff == hop.Landing)
                                && (back.Landing == hop.Takeoff)
                                && back.Depths.SequenceEqual(hop.Depths.Reverse()),
                        map);
    }

    [Test]
    public void EveryHopIsShortAndProfiledEndToEnd()
    {
        foreach ((var map, _, var hops) in BuildEveryMap())
            foreach (var hop in hops)
            {
                hop.Length
                   .Should()
                   .BeLessThanOrEqualTo(200f, map);

                hop.Depths
                   .Should()
                   .HaveCount((int)MathF.Floor(hop.Length / 5f) + 1, map);

                hop.Depths[0]
                   .Should()
                   .Be(0f, map);

                hop.Depths[^1]
                   .Should()
                   .Be(0f, map);
            }
    }

    [Test]
    public void EveryHopStartsAndEndsWhereTheServerAcceptsAMove()
    {
        foreach ((var map, var mesh, var hops) in BuildEveryMap())
            foreach (var hop in hops)
                foreach (var end in new[]
                         {
                             hop.Takeoff,
                             hop.Landing
                         })
                {
                    mesh.IsWalkable(end.X, end.Y)
                        .Should()
                        .BeTrue($"{map} {end}");

                    mesh.IsWalkable(MathF.Round(end.X / 10f) * 10f, MathF.Round(end.Y / 10f) * 10f)
                        .Should()
                        .BeTrue($"{map} {end}");
                }
    }

    [Test]
    public void EveryHopLeavesTheGroundNearItsEnds()
    {
        //every hop is listed both ways, so the takeoffs are every end of every hop
        foreach ((var map, var mesh, var hops) in BuildEveryMap())
            foreach (var hop in hops)
                CalculateGroundRun(mesh, hop.Takeoff, hop.Landing)
                    .Should()
                    .BeLessThanOrEqualTo(40f, $"{map} {hop.Takeoff} -> {hop.Landing}");
    }

    [Test]
    public void MansionUpperHopsLeaveTheGroundNearTheirEnds()
    {
        var mesh = Pathfinder.GetNavMesh("mansion")!;

        //the hops beside the central hall's top corners, each listed both ways
        var upper = HopBuilder.FindHops(mesh)
                              .Where(hop => MathF.Max(hop.Takeoff.Y, hop.Landing.Y) < -250f)
                              .ToList();

        upper.Should()
             .HaveCount(4);

        foreach (var hop in upper)
            CalculateGroundRun(mesh, hop.Takeoff, hop.Landing)
                .Should()
                .BeLessThanOrEqualTo(40f, $"{hop.Takeoff} -> {hop.Landing}");
    }

    [Test]
    public void ShortcutsInsideOnePieceAreFewAndSpreadOut()
    {
        foreach ((var map, _, var hops) in BuildEveryMap())
            foreach (var group in hops.Where(hop => hop.FromPiece == hop.ToPiece)
                                      .GroupBy(hop => hop.FromPiece))
            {
                //every shortcut is listed both ways, so the takeoffs are every end of every shortcut
                var shortcuts = group.ToList();

                shortcuts.Should()
                         .HaveCountLessThanOrEqualTo(2 * 64, $"{map} {group.Key}");

                for (var i = 0; i < shortcuts.Count; i++)
                    for (var j = i + 1; j < shortcuts.Count; j++)
                    {
                        if ((shortcuts[i].Takeoff == shortcuts[j].Landing) && (shortcuts[i].Landing == shortcuts[j].Takeoff))
                            continue;

                        MathF.Sqrt(
                                 MathF.Pow(shortcuts[i].Takeoff.X - shortcuts[j].Takeoff.X, 2)
                                 + MathF.Pow(shortcuts[i].Takeoff.Y - shortcuts[j].Takeoff.Y, 2))
                             .Should()
                             .BeGreaterThanOrEqualTo(50f, $"{map} {group.Key}");
                    }
            }
    }

    [Test]
    public void EveryShortcutSavesALongWalk()
    {
        foreach ((var map, var mesh, var hops) in BuildEveryMap())
        {
            //the map alone, so no door or transport through another map stands in for the walk
            var graph = new PortalGraph(
                new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase)
                {
                    [map] = mesh
                });

            foreach (var hop in hops.Where(hop => hop.FromPiece == hop.ToPiece))
            {
                var walk = graph.FindPath(
                                    new Location(map, hop.Takeoff),
                                    [new Destination(new Location(map, hop.Landing), 0f)],
                                    PathOptions.NoTown)
                                .Sum(edge => edge.Cost);

                walk.Should()
                    .BeGreaterThanOrEqualTo(2f * hop.Length, $"{map} {hop.Takeoff} -> {hop.Landing}");

                (walk - hop.Length).Should()
                                   .BeGreaterThanOrEqualTo(100f, $"{map} {hop.Takeoff} -> {hop.Landing}");

                hop.Depths
                   .Should()
                   .Contain(depth => depth > 0f, $"{map} {hop.Takeoff} -> {hop.Landing}");
            }
        }
    }

    [Test]
    public void SomeShortcutCrossesAWall()
        => BuildEveryMap()
           .Should()
           .Contain(entry => entry.Hops.Any(hop => (hop.FromPiece == hop.ToPiece) && !entry.Mesh.CanMove(hop.Takeoff, hop.Landing)));

    [Test]
    public void WinterlandHasAHopAcrossTheLakeToTheIceGolemIsland()
    {
        var hops = HopBuilder.FindHops(Pathfinder.GetNavMesh("winterland")!);

        hops.Where(hop => (MathF.Min(hop.Takeoff.Y, hop.Landing.Y) < 290f)
                          && (MathF.Max(hop.Takeoff.Y, hop.Landing.Y) > 330f)
                          && CrossesAt(hop, 310f) is > 700f and < 800f)
            .Should()
            .NotBeEmpty();
    }

    [Test]
    public void TheLakeHopsDeepestPointIsMeasuredNotCapped()
    {
        var lake = HopBuilder.FindHops(Pathfinder.GetNavMesh("winterland")!)
                             .First(hop => (MathF.Min(hop.Takeoff.Y, hop.Landing.Y) < 290f)
                                           && (MathF.Max(hop.Takeoff.Y, hop.Landing.Y) > 330f)
                                           && CrossesAt(hop, 310f) is > 700f and < 800f);

        //a point in the gap is never further from ground than from the nearer end
        lake.Depths
            .Max()
            .Should()
            .BeGreaterThan(24f)
            .And
            .BeLessThanOrEqualTo(lake.Length / 2f);
    }
}