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
///     The static entry point to pathfinding: builds every map's mesh once, then answers walks, wall checks and routes
///     from any thread.
/// </summary>
public static class Pathfinder
{
    /// <summary>
    ///     Compared against datum keys at <see cref="Initialize" />, so these are accessors, not display names. The three
    ///     staging maps carry neither <see cref="GMap.Ignore" /> nor <see cref="GMap.Unlist" />, so the flag filter below does
    ///     not catch them.
    /// </summary>
    private static readonly string[] IGNORED_MAPS =
    [
        "abtesting",
        "shellsisland",
        "test"
    ];

    private static readonly ILog Logger = LogManager.GetLogger(typeof(Pathfinder).FullName);

    /// <summary>
    ///     A dungeon run's floors join the mesh table while the run lasts and route over a graph of their own: nothing walks
    ///     into a run, the keeper pulls the party in, so the world graph never needs to know one exists. Both tables are
    ///     copy-on-write like the datums, so every query stays lock-free.
    /// </summary>
    private static readonly Lock GeneratedLock = new();

    //ponytail: a run is dropped when a later run arrives this long after it. Nothing here knows when the last
    //character in the process has left a run, so a run outlives its 24 minutes by this margin at most
    private static readonly TimeSpan RUN_LIFETIME = TimeSpan.FromHours(2);
    private static IReadOnlyDictionary<string, NavMesh> Meshes = new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase);
    private static PortalGraph? Graph;

    private static IReadOnlyDictionary<string, (PortalGraph Graph, DateTime RegisteredAt)> RunGraphs
        = new Dictionary<string, (PortalGraph, DateTime)>(StringComparer.OrdinalIgnoreCase);

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
    ///     The cheapest route from <paramref name="start" /> to any of <paramref name="ends" />, as legs. A walk stops inside
    ///     an end's radius rather than on it. "No path" is an <see cref="InvalidOperationException" />.
    /// </summary>
    /// <param name="start">Where the character is.</param>
    /// <param name="ends">
    ///     Any of these is an acceptable destination; the cheapest to reach is chosen.
    /// </param>
    /// <param name="options">
    ///     How the route is priced; <see cref="PathOptions.Default" /> when null.
    /// </param>
    public static IReadOnlyList<PathEdge> FindPath<T>(ILocation start, IEnumerable<T> ends, PathOptions? options = null)
        where T: ILocation, ICircle
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(ends);

        return GraphFor(start.Map)
            .FindPath(start, ends, options ?? PathOptions.Default);
    }

    /// <inheritdoc cref="FindPath{T}" />
    /// <remarks>
    ///     For callers that iterate the path with <c>await foreach</c> . The search itself runs to completion on the calling
    ///     thread before the first leg is yielded.
    /// </remarks>
    public static IAsyncEnumerable<PathEdge> FindPathAsync<T>(ILocation start, IEnumerable<T> ends, PathOptions? options = null)
        where T: ILocation, ICircle
        => FindPath(start, ends, options)
            .ToAsyncEnumerable();

    private static IEnumerable<GMap> FloorsOf(string run)
        => GameData.Maps
                   .Values
                   .DistinctBy(map => map.Accessor)
                   .Where(map => map.Generated is { } generated && run.EqualsI(generated.Run));

    /// <summary>
    ///     Retrieves the navmesh for a map, or null before <see cref="Initialize" /> has run, for an unknown map, or for a
    ///     null name. Callers ask this to find out whether a mesh can answer at all before they trust what it says, and a
    ///     character holds no map until its first new_map.
    /// </summary>
    public static NavMesh? GetNavMesh(string? name) => name is not null && Meshes.TryGetValue(name, out var mesh) ? mesh : null;

    /// <summary>
    ///     A search starting on a run's floor routes over that run's graph; everywhere else is the world.
    /// </summary>
    private static PortalGraph GraphFor(string map)
    {
        if (GameData.Maps[map]?.Generated is { } generated && RunGraphs.TryGetValue(generated.Run, out var run))
            return run.Graph;

        return Graph ?? throw new InvalidOperationException("Pathfinder.Initialize has not run.");
    }

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
    ///     Whether a walk may end here: inside the ground the mesh was built from. False for a map with no mesh rather than a
    ///     throw: a map the pathfinder never modelled is one nothing can be routed onto.
    /// </summary>
    public static bool IsWalkable(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return GetNavMesh(location.Map)
                   ?.IsWalkable(location)
               ?? false;
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
    ///     Files a dungeon run's floors into the game data and builds their meshes and the run's own portal graph. A floor the
    ///     bundle only lists in its manifest gets its mesh when its own delivery arrives; every delivery rebuilds the run's
    ///     graph over the floors it has so far.
    /// </summary>
    public static void RegisterGeneratedRun(GeneratedMapBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        GameData.RegisterGeneratedFloors(bundle);

        lock (GeneratedLock)
        {
            foreach ((var run, var entry) in RunGraphs.ToList())
                if (!run.EqualsI(bundle.Run) && ((DateTime.UtcNow - entry.RegisteredAt) > RUN_LIFETIME))
                    UnregisterGeneratedRun(run);

            var meshes = new Dictionary<string, NavMesh>(Meshes, StringComparer.OrdinalIgnoreCase);
            var runMeshes = new Dictionary<string, NavMesh>(StringComparer.OrdinalIgnoreCase);

            foreach (var map in FloorsOf(bundle.Run))
            {
                if (!meshes.TryGetValue(map.Accessor, out var mesh))
                {
                    //null for a manifest entry, whose geometry has not arrived yet
                    mesh = TryBuildNavMesh(map.Accessor, map);

                    if (mesh is null)
                        continue;

                    meshes[map.Accessor] = mesh;
                }

                runMeshes[map.Accessor] = mesh;
            }

            var registeredAt = RunGraphs.TryGetValue(bundle.Run, out var existing) ? existing.RegisteredAt : DateTime.UtcNow;

            Meshes = meshes;

            RunGraphs = new Dictionary<string, (PortalGraph, DateTime)>(RunGraphs, StringComparer.OrdinalIgnoreCase)
            {
                [bundle.Run] = (new PortalGraph(runMeshes), registeredAt)
            };
        }
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

    /// <summary>
    ///     The nearest point inside the ground, within <c>CONSTANTS.MAX_UNSTICK_DISTANCE</c> . False for a map with no mesh,
    ///     for the reason <see cref="IsWalkable" /> is.
    /// </summary>
    public static bool TryFindNearestWalkable(ILocation location, out IPoint walkable)
    {
        ArgumentNullException.ThrowIfNull(location);

        walkable = Point.None;

        return GetNavMesh(location.Map) is { } mesh && mesh.TryFindNearestWalkable(location, out walkable);
    }

    /// <summary>
    ///     Drops a run's meshes and graph, and takes its floors back out of the game data.
    /// </summary>
    public static void UnregisterGeneratedRun(string run)
    {
        ArgumentNullException.ThrowIfNull(run);

        lock (GeneratedLock)
        {
            var meshes = new Dictionary<string, NavMesh>(Meshes, StringComparer.OrdinalIgnoreCase);

            foreach (var map in FloorsOf(run))
                meshes.Remove(map.Accessor);

            var graphs = new Dictionary<string, (PortalGraph, DateTime)>(RunGraphs, StringComparer.OrdinalIgnoreCase);
            graphs.Remove(run);

            Meshes = meshes;
            RunGraphs = graphs;
        }

        GameData.UnregisterGeneratedRun(run);
    }
}