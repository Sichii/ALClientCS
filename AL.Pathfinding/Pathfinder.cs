#region
using System.Collections.Concurrent;
using System.Diagnostics;
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Model;
using Chaos.Extensions.Common;
using Common.Logging;
#endregion

namespace AL.Pathfinding;

/// <summary>
///     The static entry point to pathfinding: builds every map's mesh once, then answers walks, wall checks and
///     routes from any thread.
/// </summary>
public static class Pathfinder
{
    //compared against datum keys at Initialize, so these are accessors, not display names - the three
    //staging maps carry neither Ignore nor Unlist, so the flag filter below does not catch them
    private static readonly string[] IGNORED_MAPS =
    [
        "abtesting",
        "shellsisland",
        "test"
    ];

    private static readonly ILog Logger = LogManager.GetLogger(typeof(Pathfinder).FullName);
    private static IReadOnlyDictionary<string, NavMesh> Meshes = new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase);
    private static PortalGraph? Graph;

    /// <summary>
    ///     Whether a character can move in a straight line from start to end on a map: the server's own test.
    /// </summary>
    /// <exception cref="InvalidOperationException">The map has no mesh.</exception>
    public static bool CanMove(string mapAccessor, IPoint start, IPoint end)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapAccessor);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        var mesh = GetNavMesh(mapAccessor) ?? throw new InvalidOperationException($"No mesh found for the map \"{mapAccessor}\"");

        return mesh.CanMove(start, end);
    }

    /// <inheritdoc cref="CanMove(string, IPoint, IPoint)" />
    public static bool CanMove(ILocation start, ILocation end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        if (!start.Map.EqualsI(end.Map))
            return false;

        return CanMove(start.Map, start, end);
    }

    /// <summary>
    ///     The cheapest route from <paramref name="start" /> to any of <paramref name="ends" />, as legs. A walk
    ///     stops inside an end's radius rather than on it. "No path" is an <see cref="InvalidOperationException" />.
    /// </summary>
    /// <param name="start">Where the character is.</param>
    /// <param name="ends">Any of these is an acceptable destination; the cheapest to reach is chosen.</param>
    /// <param name="useTownIfOptimal">
    ///     Whether a recall counts as a move. True prices one from anywhere on the route, the start map and every map
    ///     the route lands on alike; false leaves the route without a single recall leg.
    /// </param>
    /// <param name="walkSpeed">The character's speed, which prices a recall; nominal when null.</param>
    public static IReadOnlyList<PathEdge> FindPath<T>(
        ILocation start,
        IEnumerable<T> ends,
        bool useTownIfOptimal = true,
        float? walkSpeed = null)
        where T: ILocation, ICircle
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(ends);

        var graph = Graph ?? throw new InvalidOperationException("Pathfinder.Initialize has not run.");

        return graph.FindPath(
            start,
            ends,
            useTownIfOptimal,
            walkSpeed);
    }

    /// <inheritdoc cref="FindPath{T}" />
    /// <remarks>
    ///     For callers that iterate the path with <c>await foreach</c>. The search itself runs to completion on the
    ///     calling thread before the first leg is yielded.
    /// </remarks>
    public static IAsyncEnumerable<PathEdge> FindPathAsync<T>(
        ILocation start,
        IEnumerable<T> ends,
        bool useTownIfOptimal = true,
        float? walkSpeed = null)
        where T: ILocation, ICircle
        => FindPath(
                start,
                ends,
                useTownIfOptimal,
                walkSpeed)
            .ToAsyncEnumerable();

    /// <summary>
    ///     Retrieves the navmesh for a map, or null before <see cref="Initialize" /> has run, for an unknown map, or
    ///     for a null name. Callers ask this to find out whether a mesh can answer at all before they trust what it
    ///     says, and a character holds no map until its first new_map.
    /// </summary>
    public static NavMesh? GetNavMesh(string? name) => name is not null && Meshes.TryGetValue(name, out var mesh) ? mesh : null;

    /// <summary>
    ///     Builds every map's mesh and the portal graph between them. CPU-heavy; several hundred milliseconds.
    /// </summary>
    public static void Initialize()
    {
        var built = new ConcurrentDictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase);

        var maps = GameData.Maps
                           .Entries
                           .DistinctBy(kvp => kvp.Value.Accessor)
                           .Where(kvp => !kvp.Value.Ignore)
                           .Where(kvp => !IGNORED_MAPS.ContainsI(kvp.Key))
                           .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var timer = Stopwatch.StartNew();
        Logger.Info("Preparing map navigation");

        Parallel.ForEach(
            maps,
            kvp =>
            {
                (var name, var map) = kvp;
                var navMesh = TryBuildNavMesh(name, map);

                if (navMesh != null)
                    built.TryAdd(map.Accessor, navMesh);
            });

        Meshes = new Dictionary<string, NavMesh>(built, StringComparer.OrdinalIgnoreCase);
        Graph = new PortalGraph(Meshes);

        timer.Stop();
        Logger.Info($"Prepared maps in {timer.ElapsedMilliseconds}ms");
    }

    /// <summary>
    ///     Whether a character standing here has a wall inside its collision box, or is off the map.
    /// </summary>
    /// <exception cref="InvalidOperationException">The map has no mesh.</exception>
    public static bool IsWall(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        var mesh = GetNavMesh(location.Map) ?? throw new InvalidOperationException($"No mesh found for the map \"{location.Map}\"");

        return mesh.IsWall(location);
    }

    /// <summary>
    ///     Whether a walk may end here: inside the ground the mesh was built from. False for a map with no
    ///     mesh rather than a throw: a map the pathfinder never modelled is one nothing can be routed onto.
    /// </summary>
    public static bool IsWalkable(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return GetNavMesh(location.Map)
                   ?.IsWalkable(location)
               ?? false;
    }

    /// <summary>
    ///     The nearest point inside the ground, within <c>CONSTANTS.MAX_UNSTICK_DISTANCE</c>.
    ///     False for a map with no mesh, for the reason <see cref="IsWalkable" /> is.
    /// </summary>
    public static bool TryFindNearestWalkable(ILocation location, out IPoint walkable)
    {
        ArgumentNullException.ThrowIfNull(location);

        walkable = Point.None;

        return GetNavMesh(location.Map) is { } mesh && mesh.TryFindNearestWalkable(location, out walkable);
    }

    private static NavMesh? TryBuildNavMesh(string name, GMap map)
    {
        var geometry = GameData.Geometry[name];

        if ((geometry == null) || (geometry.VerticalLines.Count == 0) || (geometry.HorizontalLines.Count == 0))
        {
            Logger.Debug($"Ignored {name}");

            return null;
        }

        var mesh = new NavMeshBuilder(map, geometry).BuildMesh();
        Logger.Debug($"Prepared {name}");

        return new NavMesh(map, geometry, mesh);
    }
}