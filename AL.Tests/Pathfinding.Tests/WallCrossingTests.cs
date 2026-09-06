#region
using AL.Core.Extensions;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Geometry;
using AL.Pathfinding;
using AL.Pathfinding.Definitions;
using AL.Pathfinding.Model;
using FluentAssertions;
#endregion

namespace AL.Tests.Pathfinding.Tests;

/// <summary>
///     Restates the server's own
///     <c>
///         can_move
///     </c>
///     as a line intersection test and holds every leg the search emits to it. Worth a whole file because the server does
///     not do this itself: its move handler hashes the two endpoints onto the smap lattice and jails on a miss, but never
///     traces what lies between them, so a straight leg whose ends are both on the fill is performed exactly as sent. The
///     pathfinder is the only thing standing between a character and a wall, and calling
///     <c>
///         CanMove
///     </c>
///     here would only ask the same raster the search already asked.
/// </summary>
public class WallCrossingTests : PathfindingTestBed
{
    //the player collision base the server measures a move with, as four corner offsets
    private static readonly (float MX, float MY)[] CORNERS =
    [
        (-8f, 2f),
        (8f, 2f),
        (-8f, -7f),
        (8f, -7f)
    ];

    //a spread of geometry rather than a sample of it - open maps, dungeons, and the two densest interiors
    private static readonly string[] MAPS =
    [
        "main",
        "cave",
        "mansion",
        "halloween",
        "winterland",
        "desertland",
        "level1",
        "spookytown",
        "tunnel",
        "winter_cave"
    ];

    private const int TRIALS_PER_MAP = 120;

    //the server's constants, restated rather than read from CONSTANTS so the oracle cannot inherit a port mistake
    private const double SERVER_EPS = 1e-8;
    private const double SERVER_REPS = 2.220446049250313e-16;
    private const double SERVER_H = 8;
    private const double SERVER_V = 7;
    private const double SERVER_VN = 2;

    [Test]
    public void AGrazePastTheEndpointDoesNotBlockAClearLine()
    {
        //the mesh vertex at main:(-1367, 616) sits on a padded wall corner, and a leg into it from the open ground
        //to its northeast ends exactly on the lattice corner the wall cell touches. The raster traversal used to
        //tie-break one cell past the endpoint into that wall, reading a clear line of hundreds of units as blocked
        var navMesh = Pathfinder.GetNavMesh("main")!;
        var corner = new Point(-1367, 616);

        Point[] sources =
        [
            new(-897, 61),
            new(-913, 101),
            new(-985, 317)
        ];

        foreach (var source in sources)
            navMesh.CanMove(source, corner)
                   .Should()
                   .BeTrue($"the line from {source} only grazes a wall cell past its endpoint");
    }

    /// <summary>
    ///     A start the flood fill never reached is the one input that cannot be answered without crossing something - the
    ///     character is already on the far side of a line. What is held here is that the crossing is the step back onto
    ///     walkable ground and nothing more: bounded by the search for it, and never repeated later in the path.
    /// </summary>
    [Test]
    public async Task AStartOffTheFillCrossesOnlyOnTheStepBackOntoIt()
    {
        var rng = new Random(4242);
        var failures = new List<string>();

        foreach ((var map, var geo) in MeshedMaps())
            for (var trial = 0; trial < TRIALS_PER_MAP; trial++)
            {
                var start = RandomOffTheFill(rng, map, geo);
                var end = RandomWalkable(rng, map, geo);

                if (start is null || end is null)
                    break;

                var path = await FindPathOrEmptyAsync(start, end);

                for (var index = 0; index < path.Length; index++)
                {
                    if (!IsSameMapWalk(path[index], map))
                        continue;

                    var from = path[index].Start;

                    var to = path[index].End;

                    if (ServerCanMove(geo, from, to))
                        continue;

                    if (index > 0)
                        failures.Add($"{map}: leg {index} of {path.Length} crosses. {Describe(geo, map, from, to)}");
                    else if (from.Distance(to) > CONSTANTS.MAX_UNSTICK_DISTANCE)
                        failures.Add(
                            $"{map}: the step back onto the fill ran {from.Distance(to):N0} units. {Describe(geo, map, from, to)}");
                }
            }

        failures.Should()
                .BeEmpty();
    }

    private static bool BoxHitsALine(GGeometry geo, float x, float y)
    {
        var left = x - 8f;
        var right = x + 8f;
        var top = y - 7f;
        var bottom = y + 2f;

        foreach (var line in geo.VerticalLines)
            if ((line.On >= left)
                && (line.On <= right)
                && (Math.Min(line.Start, line.End) <= bottom)
                && (Math.Max(line.Start, line.End) >= top))
                return true;

        foreach (var line in geo.HorizontalLines)
            if ((line.On >= top)
                && (line.On <= bottom)
                && (Math.Min(line.Start, line.End) <= right)
                && (Math.Max(line.Start, line.End) >= left))
                return true;

        return false;
    }

    /// <summary>
    ///     CanMove restated against the server's own move test, transcribed below in double precision with a linear scan: four
    ///     corner tracks, then the two fence tracks at the destination. The port has to agree on every sampled move in both
    ///     directions; there is no tolerance.
    /// </summary>
    [Test]
    public void CanMoveAgreesWithTheServerExactly()
    {
        var rng = new Random(2024);
        var disagreements = new List<string>();
        var checkedMoves = 0;

        foreach ((var map, var geo) in MeshedMaps())
            for (var trial = 0; trial < (TRIALS_PER_MAP * 10); trial++)
            {
                var from = RandomWalkable(rng, map, geo);

                if (from is null)
                    break;

                var angle = (float)(rng.NextDouble() * Math.Tau);
                var length = (float)(rng.NextDouble() * 300);
                var to = new Location(map, from.X + MathF.Cos(angle) * length, from.Y + MathF.Sin(angle) * length);
                var expected = ServerCanMove(geo, from, to);
                var actual = Pathfinder.CanMove(from, to);
                checkedMoves++;

                if (expected != actual)
                    disagreements.Add($"{map}: {Describe(geo, map, from, to)} server={expected} port={actual}");
            }

        checkedMoves.Should()
                    .BeGreaterThan(0);

        disagreements.Should()
                     .BeEmpty();
    }

    private static async Task<List<string>> CrossingsAsync(
        string map,
        GGeometry geo,
        ILocation start,
        ILocation end)
    {
        var failures = new List<string>();
        var path = await FindPathOrEmptyAsync(start, end);

        foreach (var edge in path)
        {
            if (!IsSameMapWalk(edge, map))
                continue;

            var from = edge.Start;
            var to = edge.End;

            if (!ServerCanMove(geo, from, to))
                failures.Add($"{map}: {Describe(geo, map, from, to)}");
        }

        return failures;
    }

    //names the first line one of the four corner tracks crosses, so a failure reads as geometry rather than numbers
    private static string Describe(
        GGeometry geo,
        string map,
        IPoint from,
        IPoint to)
    {
        var detail = $"({from.X:N1}, {from.Y:N1}) -> ({to.X:N1}, {to.Y:N1}) over {from.Distance(to):N0} units";

        foreach ((var mx, var my) in CORNERS)
        {
            var x0 = from.X + mx;
            var y0 = from.Y + my;
            var x1 = to.X + mx;
            var y1 = to.Y + my;

            if (ServerTrack(
                    geo,
                    x0,
                    y0,
                    x1,
                    y1))
                continue;

            var dx = x1 - x0;
            var dy = y1 - y0;

            foreach (var line in geo.VerticalLines)
            {
                var lx = line.Point1.X;

                if ((MathF.Min(x0, x1) > lx) || (MathF.Max(x0, x1) < lx) || (MathF.Abs(dx) < 1e-6f))
                    continue;

                var lyLow = MathF.Min(line.Point1.Y, line.Point2.Y);
                var lyHigh = MathF.Max(line.Point1.Y, line.Point2.Y);
                var next = y0 + dy * (lx - x0) / dx;

                if ((next < lyLow) || (next > lyHigh))
                    continue;

                var centre = new Location(map, lx - mx, next - my);

                return $"{detail}, corner ({mx:N0}, {my:N0}) crosses x_line x={lx:N0} y[{lyLow:N0}, {lyHigh:N0}] "
                       + $"at y={next:N0}; raster there says wall={Pathfinder.IsWall(centre)}";
            }

            foreach (var line in geo.HorizontalLines)
            {
                var ly = line.Point1.Y;

                if ((MathF.Min(y0, y1) > ly) || (MathF.Max(y0, y1) < ly) || (MathF.Abs(dy) < 1e-6f))
                    continue;

                var lxLow = MathF.Min(line.Point1.X, line.Point2.X);
                var lxHigh = MathF.Max(line.Point1.X, line.Point2.X);
                var next = x0 + dx * (ly - y0) / dy;

                if ((next < lxLow) || (next > lxHigh))
                    continue;

                var centre = new Location(map, next - mx, ly - my);

                return $"{detail}, corner ({mx:N0}, {my:N0}) crosses y_line y={ly:N0} x[{lxLow:N0}, {lxHigh:N0}] "
                       + $"at x={next:N0}; raster there says wall={Pathfinder.IsWall(centre)}";
            }
        }

        return detail;
    }

    //an unreachable destination is an ordinary answer here - instance maps are not connected to the walkable graph
    private static async Task<PathEdge[]> FindPathOrEmptyAsync(ILocation start, ILocation end)
    {
        try
        {
            return await Pathfinder.FindPathAsync(start, [new Destination(end, 0f)], false)
                                   .ToArrayAsync();
        } catch (InvalidOperationException)
        {
            return [];
        }
    }

    private static bool IsSameMapWalk(PathEdge edge, string map)
        => (edge.Type == EdgeType.Walk)
           && edge.Start.OnSameMapAs(edge.End)
           && edge.Start.Map.Equals(map, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     IsWall restated: a point is a wall exactly when a line passes through the collision box hanging on it, eight wide
    ///     each side, seven up and two down. The old raster answered this a unit at a time; the exact test has to agree with
    ///     the geometry at every point, integer or not.
    /// </summary>
    [Test]
    public void IsWallAgreesWithTheBoxOracle()
    {
        var rng = new Random(5150);
        var disagreements = new List<string>();

        foreach ((var map, var geo) in MeshedMaps())
            for (var trial = 0; trial < (TRIALS_PER_MAP * 10); trial++)
            {
                var x = geo.MinX + (float)(rng.NextDouble() * (geo.MaxX - geo.MinX));
                var y = geo.MinY + (float)(rng.NextDouble() * (geo.MaxY - geo.MinY));
                var expected = BoxHitsALine(geo, x, y);

                if (Pathfinder.IsWall(new Location(map, x, y)) != expected)
                    disagreements.Add($"{map}: ({x:N1}, {y:N1}) oracle says wall={expected}");
            }

        disagreements.Should()
                     .BeEmpty();
    }

    /// <summary>
    ///     A point off the map's own extents is answered rather than thrown over. The point map is sized to those extents and
    ///     indexed directly, and the throw took down whichever handler asked - on a jail frame that was the handler which
    ///     relocates the character, so the local position stayed on the map it had just left.
    /// </summary>
    [Test]
    public void IsWallAnswersForAPointOffTheMap()
    {
        foreach ((var map, var geo) in MeshedMaps())
        {
            Pathfinder.IsWall(new Location(map, geo.MinX - 500, geo.MinY - 500))
                      .Should()
                      .BeTrue();

            Pathfinder.IsWall(new Location(map, geo.MaxX + 500, geo.MaxY + 500))
                      .Should()
                      .BeTrue();
        }
    }

    private static IEnumerable<(string Map, GGeometry Geometry)> MeshedMaps()
    {
        foreach (var map in MAPS)
        {
            if (Pathfinder.GetNavMesh(map) is null)
                continue;

            if (GameData.Geometry[map] is not { } geo)
                continue;

            yield return (map, geo);
        }
    }

    /// <summary>
    ///     The bend between two legs, which is what actually gets walked when a leg's emit lands before the server has
    ///     finished the one before it: the move handler re-aims from part-way along the previous leg to this leg's end, over a
    ///     line the search never validated.
    ///     <c>
    ///         SmartMoveAsync
    ///     </c>
    ///     stands still for a round trip wherever the raster refuses that line, so what has to hold is that the raster is not
    ///     the more permissive of the two - a bend it waves through and the geometry refuses is a bend the guard never fires
    ///     on.
    /// </summary>
    [Test]
    public async Task NoBendTheGuardAllowsCrossesGeometry()
    {
        var rng = new Random(31337);
        var failures = new List<string>();

        foreach ((var map, var geo) in MeshedMaps())
            for (var trial = 0; trial < TRIALS_PER_MAP; trial++)
            {
                var start = RandomWalkable(rng, map, geo);
                var end = RandomWalkable(rng, map, geo);

                if (start is null || end is null)
                    break;

                var path = await FindPathOrEmptyAsync(start, end);

                for (var index = 1; index < path.Length; index++)
                {
                    if (!IsSameMapWalk(path[index], map) || !IsSameMapWalk(path[index - 1], map))
                        continue;

                    var bendFrom = path[index - 1].Start;

                    var bendTo = path[index].End;

                    //the guard's own question. Where it answers no the walk stands still and there is no bend to make
                    if (!Pathfinder.CanMove(bendFrom, bendTo))
                        continue;

                    if (ServerCanMove(geo, bendFrom, bendTo))
                        continue;

                    failures.Add($"{map}: bend over leg {index} of {path.Length}. {Describe(geo, map, bendFrom, bendTo)}");
                }
            }

        failures.Should()
                .BeEmpty();
    }

    /// <summary>
    ///     Sampling biased hard against the wall. Open ground draws too few wall-hugging legs to say anything, and those are
    ///     the legs where a one unit raster and an exact line test have the most room to disagree - the first leg of a path is
    ///     stitched from wherever the character is, and a character parked against a wall sits outside every triangle the
    ///     strict containment test recognises.
    /// </summary>
    [Test]
    public async Task NoLegCrossesGeometryFromAStartAgainstAWall()
    {
        var rng = new Random(77);
        var failures = new List<string>();

        foreach ((var map, var geo) in MeshedMaps())
            for (var trial = 0; trial < TRIALS_PER_MAP; trial++)
            {
                var start = RandomAgainstAWall(rng, map, geo);
                var end = RandomWalkable(rng, map, geo);

                if (start is null || end is null)
                    break;

                failures.AddRange(
                    await CrossingsAsync(
                        map,
                        geo,
                        start,
                        end));
            }

        failures.Should()
                .BeEmpty();
    }

    /// <summary>
    ///     The broad sweep, from open ground to open ground. Covers the smoothing collapse, which replaces a run of short legs
    ///     with one long one and is the only place a path grows a leg nothing planned.
    /// </summary>
    [Test]
    public async Task NoLegCrossesGeometryFromOpenGround()
    {
        var rng = new Random(1337);
        var failures = new List<string>();

        foreach ((var map, var geo) in MeshedMaps())
            for (var trial = 0; trial < TRIALS_PER_MAP; trial++)
            {
                var start = RandomWalkable(rng, map, geo);
                var end = RandomWalkable(rng, map, geo);

                if (start is null || end is null)
                    break;

                failures.AddRange(
                    await CrossingsAsync(
                        map,
                        geo,
                        start,
                        end));
            }

        failures.Should()
                .BeEmpty();
    }

    /// <summary>
    ///     The mirror of the start: a destination off the fill is an ordinary input, since callers derive their own stopping
    ///     points and the server parks entities against lines.
    /// </summary>
    [Test]
    public async Task NoLegCrossesGeometryReachingADestinationOffTheFill()
    {
        var rng = new Random(909);
        var failures = new List<string>();

        foreach ((var map, var geo) in MeshedMaps())
            for (var trial = 0; trial < TRIALS_PER_MAP; trial++)
            {
                var start = RandomWalkable(rng, map, geo);
                var end = RandomOffTheFill(rng, map, geo);

                if (start is null || end is null)
                    break;

                failures.AddRange(
                    await CrossingsAsync(
                        map,
                        geo,
                        start,
                        end));
            }

        failures.Should()
                .BeEmpty();
    }

    //the last walkable point before the fill runs out, which is where a character parked against a wall stands
    private static ILocation? RandomAgainstAWall(Random rng, string map, GGeometry geo)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            if (RandomWalkable(rng, map, geo) is not { } seed)
                return null;

            var angle = (float)(rng.NextDouble() * Math.Tau);
            var dx = MathF.Cos(angle);
            var dy = MathF.Sin(angle);

            for (var step = 1; step <= 24; step++)
                if (!Pathfinder.IsWalkable(new Location(map, seed.X + dx * step, seed.Y + dy * step)))
                    return new Location(map, seed.X + dx * (step - 1), seed.Y + dy * (step - 1));
        }

        return null;
    }

    //one to three units past the boundary, which is the scale of a server correction rather than a teleport
    private static ILocation? RandomOffTheFill(Random rng, string map, GGeometry geo)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            if (RandomWalkable(rng, map, geo) is not { } seed)
                return null;

            var angle = (float)(rng.NextDouble() * Math.Tau);
            var dx = MathF.Cos(angle);
            var dy = MathF.Sin(angle);

            for (var step = 1; step <= 24; step++)
            {
                if (Pathfinder.IsWalkable(new Location(map, seed.X + dx * step, seed.Y + dy * step)))
                    continue;

                var depth = step + 1 + (float)(rng.NextDouble() * 2);

                return new Location(map, seed.X + dx * depth, seed.Y + dy * depth);
            }
        }

        return null;
    }

    private static ILocation? RandomWalkable(Random rng, string map, GGeometry geo)
    {
        for (var attempt = 0; attempt < 400; attempt++)
        {
            var x = geo.MinX + (float)(rng.NextDouble() * (geo.MaxX - geo.MinX));
            var y = geo.MinY + (float)(rng.NextDouble() * (geo.MaxY - geo.MinY));
            var loc = new Location(map, x, y);

            if (Pathfinder.IsWalkable(loc))
                return loc;
        }

        return null;
    }

    /// <summary>
    ///     The server's character move test: the four corners of the collision box each track the move, then two fence tracks
    ///     across the box at the destination catch an orphan line that slipped between the corners.
    /// </summary>
    private static bool ServerCanMove(GGeometry geo, IPoint from, IPoint to)
    {
        double x0 = from.X;
        double y0 = from.Y;
        double x1 = to.X;
        double y1 = to.Y;

        foreach ((var mx, var my) in new[]
                 {
                     (-SERVER_H, SERVER_VN),
                     (SERVER_H, SERVER_VN),
                     (-SERVER_H, -SERVER_V),
                     (SERVER_H, -SERVER_V)
                 })
            if (!ServerTrack(
                    geo,
                    x0 + mx,
                    y0 + my,
                    x1 + mx,
                    y1 + my))
                return false;

        var px0 = SERVER_H;
        var px1 = -SERVER_H;

        if (x1 > x0)
        {
            px0 = -SERVER_H;
            px1 = SERVER_H;
        }

        var py0 = SERVER_VN;
        var py1 = -SERVER_V;

        if (y1 > y0)
        {
            py0 = -SERVER_V;
            py1 = SERVER_VN;
        }

        if (!ServerTrack(
                geo,
                x1 + px1,
                y1 + py0,
                x1 + px1,
                y1 + py1))
            return false;

        return ServerTrack(
            geo,
            x1 + px0,
            y1 + py1,
            x1 + px1,
            y1 + py1);
    }

    /// <summary>
    ///     One track of the server's move test over every line, in the server's own clause order. Lines are walked in
    ///     ascending position so the early break means what it means on the server; the span ends are taken low-to-high
    ///     because that is how the map data lists them.
    /// </summary>
    private static bool ServerTrack(
        GGeometry geo,
        double x0,
        double y0,
        double x1,
        double y1)
    {
        var minx = Math.Min(x0, x1);
        var maxx = Math.Max(x0, x1);
        var miny = Math.Min(y0, y1);
        var maxy = Math.Max(y0, y1);

        foreach (var line in geo.VerticalLines.OrderBy(line => line.On))
        {
            double on = line.On;
            double a = Math.Min(line.Start, line.End);
            double b = Math.Max(line.Start, line.End);

            //moving onto the line, or along it
            if ((on == x1) && (((a <= y1) && (b >= y1)) || ((on == x0) && (y0 <= a) && (y1 > a))))
                return false;

            if (minx > on)
                continue;

            if (maxx < on)
                break;

            var next = y0 + (y1 - y0) * (on - x0) / (x1 - x0 + SERVER_REPS);

            if (!(((a - SERVER_EPS) <= next) && (next <= (b + SERVER_EPS))))
                continue;

            return false;
        }

        foreach (var line in geo.HorizontalLines.OrderBy(line => line.On))
        {
            double on = line.On;
            double a = Math.Min(line.Start, line.End);
            double b = Math.Max(line.Start, line.End);

            if ((on == y1) && (((a <= x1) && (b >= x1)) || ((on == y0) && (x0 <= a) && (x1 > a))))
                return false;

            if (miny > on)
                continue;

            if (maxy < on)
                break;

            var next = x0 + (x1 - x0) * (on - y0) / (y1 - y0 + SERVER_REPS);

            if (!(((a - SERVER_EPS) <= next) && (next <= (b + SERVER_EPS))))
                continue;

            return false;
        }

        return true;
    }
}