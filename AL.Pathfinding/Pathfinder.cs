#region
using System.Collections.Concurrent;
using System.Diagnostics;
using AL.Core.Definitions;
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
///     Provides static access to pathfinding: builds every map's mesh once, then answers walks, wall checks and routes
///     from any thread.
/// </summary>
public static class Pathfinder
{
    /// <summary>
    ///     The map keys <see cref="Initialize" /> skips that carry no <see cref="GMap.Ignore" /> flag.
    /// </summary>
    private static readonly string[] IGNORED_MAPS =
    [
        "abtesting",
        "shellsisland",
        "test"
    ];

    private static readonly ILog Logger = LogManager.GetLogger(typeof(Pathfinder).FullName);

    /// <summary>
    ///     Serializes writers of the mesh and run graph tables. Both are copy-on-write, so readers take no lock.
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
    ///     Determines whether a character can move in a straight line from start to end on a map, by the server's own test.
    /// </summary>
    /// <param name="mapAccessor">The map to check against.</param>
    /// <param name="start">The starting point.</param>
    /// <param name="end">The ending point.</param>
    /// <returns>
    ///     <c>true</c> if the move crosses no wall; otherwise, <c>false</c> .
    /// </returns>
    /// <exception cref="System.ArgumentException">mapAccessor</exception>
    /// <exception cref="System.ArgumentNullException">start</exception>
    /// <exception cref="System.ArgumentNullException">end</exception>
    /// <exception cref="InvalidOperationException">The map has no mesh.</exception>
    public static bool CanMove(string mapAccessor, IPoint start, IPoint end)
    {
        ArgumentException.ThrowIfNullOrEmpty(mapAccessor);

        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(end);

        var mesh = GetNavMesh(mapAccessor) ?? throw new InvalidOperationException($"No mesh found for the map \"{mapAccessor}\"");

        return mesh.CanMove(start, end);
    }

    /// <summary>
    ///     Determines whether a character can move in a straight line from one location to another, by the server's own test.
    /// </summary>
    /// <param name="start">The starting location.</param>
    /// <param name="end">The ending location.</param>
    /// <returns>
    ///     <c>true</c> if both are on the same map and the move crosses no wall; otherwise, <c>false</c> .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">start</exception>
    /// <exception cref="System.ArgumentNullException">end</exception>
    /// <exception cref="InvalidOperationException">The map has no mesh.</exception>
    public static bool CanMove(ILocation start, ILocation end)
    {
        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(end);

        if (!start.Map.EqualsI(end.Map))
            return false;

        return CanMove(start.Map, start, end);
    }

    /// <summary>
    ///     Finds the cheapest route from a start to any of several ends. A walk stops inside an end's radius rather than on
    ///     it.
    /// </summary>
    /// <param name="start">Where the character is.</param>
    /// <param name="ends">
    ///     Any of these is an acceptable destination; the cheapest to reach is chosen.
    /// </param>
    /// <param name="options">
    ///     How the route is priced; <see cref="PathOptions.Default" /> when null.
    /// </param>
    /// <typeparam name="T">The type of the ends.</typeparam>
    /// <returns>The route's legs.</returns>
    /// <exception cref="System.ArgumentNullException">start</exception>
    /// <exception cref="System.ArgumentNullException">ends</exception>
    /// <exception cref="InvalidOperationException">
    ///     Pathfinding is not initialized, or no end can be reached.
    /// </exception>
    public static IReadOnlyList<PathEdge> FindPath<T>(ILocation start, IEnumerable<T> ends, PathOptions? options = null)
        where T: ILocation, ICircle
    {
        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(ends);

        return GetGraph(start.Map)
            .FindPath(start, ends, options ?? PathOptions.Default);
    }

    /// <inheritdoc cref="FindPath{T}" />
    /// <remarks>
    ///     The search runs to completion on the calling thread before the first leg is yielded.
    /// </remarks>
    public static IAsyncEnumerable<PathEdge> FindPathAsync<T>(ILocation start, IEnumerable<T> ends, PathOptions? options = null)
        where T: ILocation, ICircle
        => FindPath(start, ends, options)
            .ToAsyncEnumerable();

    private static IEnumerable<GMap> GetFloors(string run)
        => GameData.Maps
                   .Values
                   .DistinctBy(map => map.Accessor)
                   .Where(map => map.Generated is { } generated && run.EqualsI(generated.Run));

    /// <summary>
    ///     Gets the graph a search starting on a map routes over: a run's own on its floors, the world's everywhere else.
    /// </summary>
    /// <param name="map">The map the search starts on.</param>
    /// <returns>The portal graph.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Initialize" /> has not run.</exception>
    private static PortalGraph GetGraph(string map)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if (GameData.Maps[map]?.Generated is { } generated && RunGraphs.TryGetValue(generated.Run, out var run))
            return run.Graph;

        return Graph ?? throw new InvalidOperationException("Pathfinder.Initialize has not run.");
    }

    /// <summary>Retrieves the navmesh for a map.</summary>
    /// <param name="name">
    ///     The map whose navmesh to retrieve; null before a character's first <c>new_map</c>.
    /// </param>
    /// <returns>
    ///     The map's navmesh, or null before <see cref="Initialize" /> has run, for an unknown map, or for a null name.
    /// </returns>

    // ReSharper disable once InconsistentlySynchronizedField
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

                           //Dungeon World maps are only instanced on that server, so nothing can go there from an ordinary one
                           .Where(kvp => kvp.Value.World == WorldType.None)
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
    ///     Determines whether a walk may end at a location, inside the ground the mesh was built from.
    /// </summary>
    /// <param name="location">The location to check.</param>
    /// <returns>
    ///     <c>true</c> if the location is on the mesh; false for a map with no mesh; otherwise, <c>false</c> .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">location</exception>
    public static bool IsWalkable(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return GetNavMesh(location.Map)
                   ?.IsWalkable(location)
               ?? false;
    }

    /// <summary>
    ///     Determines whether a character standing at a location has a wall inside its collision box, or is off the map.
    /// </summary>
    /// <param name="location">The location to check.</param>
    /// <returns>
    ///     <c>true</c> if the location is a wall; otherwise, <c>false</c> .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">location</exception>
    /// <exception cref="InvalidOperationException">The map has no mesh.</exception>
    public static bool IsWall(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        var mesh = GetNavMesh(location.Map) ?? throw new InvalidOperationException($"No mesh found for the map \"{location.Map}\"");

        return mesh.IsWall(location);
    }

    /// <summary>
    ///     Files a dungeon run's floors into the game data and builds their meshes and the run's own portal graph.
    /// </summary>
    /// <param name="bundle">The run's floors as the server sent them.</param>
    /// <exception cref="System.ArgumentNullException">bundle</exception>
    /// <remarks>
    ///     A floor listed only in the manifest gets its mesh when its own delivery arrives; each delivery rebuilds the graph.
    /// </remarks>
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

            foreach (var map in GetFloors(bundle.Run))
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
    ///     Finds the nearest point inside the ground, within <see cref="Definitions.CONSTANTS.MAX_UNSTICK_DISTANCE" />.
    /// </summary>
    /// <param name="location">The location to start from.</param>
    /// <param name="walkable">
    ///     The nearest walkable point, or <see cref="Point.None" />.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a walkable point was found; false for a map with no mesh; otherwise, <c>false</c> .
    /// </returns>
    /// <exception cref="System.ArgumentNullException">location</exception>
    public static bool TryFindNearestWalkable(ILocation location, out IPoint walkable)
    {
        ArgumentNullException.ThrowIfNull(location);

        walkable = Point.None;

        return GetNavMesh(location.Map) is { } mesh && mesh.TryFindNearestWalkable(location, out walkable);
    }

    /// <summary>
    ///     Drops a run's meshes and graph, and takes its floors back out of the game data.
    /// </summary>
    /// <param name="run">The run's id.</param>
    /// <exception cref="System.ArgumentNullException">run</exception>
    public static void UnregisterGeneratedRun(string run)
    {
        ArgumentNullException.ThrowIfNull(run);

        lock (GeneratedLock)
        {
            var meshes = new Dictionary<string, NavMesh>(Meshes, StringComparer.OrdinalIgnoreCase);

            foreach (var map in GetFloors(run))
                meshes.Remove(map.Accessor);

            var graphs = new Dictionary<string, (PortalGraph, DateTime)>(RunGraphs, StringComparer.OrdinalIgnoreCase);
            graphs.Remove(run);

            Meshes = meshes;
            RunGraphs = graphs;
        }

        GameData.UnregisterGeneratedRun(run);
    }
}