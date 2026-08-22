#region
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AL.Core.Interfaces;
using AL.Data;
using AL.Data.Maps;
using AL.Pathfinding.Abstractions;
using AL.Pathfinding.Model;
using Chaos.Extensions.Common;
using Common.Logging;
#endregion

namespace AL.Pathfinding;

/// <summary>
///     Provides static access to pathfinding methods in <see cref="DirectedGraph" />.
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
    internal static DirectedGraph DirectedGraph = null!;

    /// <summary>
    ///     Determines whether or not it's possible to move from one location to another.
    /// </summary>
    /// <param name="mapAccessor">
    ///     The map to check against.
    /// </param>
    /// <param name="start">
    ///     The starting point.
    /// </param>
    /// <param name="end">
    ///     The ending point.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if you can move from <paramref name="start" /> to <paramref name="end" />, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public static bool CanMove(string mapAccessor, IPoint start, IPoint end)
    {
        if (string.IsNullOrEmpty(mapAccessor))
            throw new ArgumentNullException(nameof(mapAccessor));

        ArgumentNullException.ThrowIfNull(start);

        ArgumentNullException.ThrowIfNull(end);

        return DirectedGraph.CanMove(mapAccessor, start, end);
    }

    /// <summary>
    ///     Determines whether or not it's possible to move from one location to another.
    /// </summary>
    /// <param name="start">
    ///     The starting location.
    /// </param>
    /// <param name="end">
    ///     The ending location.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if you can move from <paramref name="start" /> to <paramref name="end" />, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public static bool CanMove(ILocation start, ILocation end) => DirectedGraph.CanMove(start, end);

    /// <inheritdoc cref="GraphBase{TMesh,TNode,TEdge}.FindPathAsync" />
    public static IAsyncEnumerable<GraphEdge> FindPathAsync<T>(
        ILocation start,
        IEnumerable<T> ends,
        bool useTownIfOptimal = true,
        float? walkSpeed = null)
        where T: ILocation, ICircle
        =>

            //do other things
            DirectedGraph.FindPathAsync(start, ends, useTownIfOptimal, walkSpeed);

    /// <summary>
    ///     Retreives the navmesh associated with a given map.
    /// </summary>
    /// <param name="name">
    ///     The map whose navmesh to retreive.
    /// </param>
    /// <returns>
    ///     <see cref="NavMesh" />
    ///     <br />
    ///     The navmesh for the given map.
    /// </returns>
    /// <remarks>
    ///     Null-safe on the graph itself, not just on the name. Callers ask this to find out whether a mesh can
    ///     answer at all before they trust what it says - the walk guard in MoveAsync and the jail diagnostic both
    ///     do - and before Initialize runs the honest answer to that is "no mesh", the same answer an unknown map
    ///     gets. Throwing instead turned the two guards written to fall back into the ones that broke first.
    ///     <br />
    ///     The name is nullable for the same reason. A character holds no map until its first new_map, and that
    ///     frame is itself where the jail diagnostic reads one - so a character that was jailed when it logged out
    ///     asks this about a null map on the way back in, and a raw TryGetValue answers that with an
    ///     ArgumentNullException from inside the handler that was relocating it.
    /// </remarks>
    public static NavMesh? GetNavMesh(string? name)
        => (name is not null) && (DirectedGraph?.NavMeshes.TryGetValue(name, out var mesh) == true) ? mesh : null;

    /// <summary>
    ///     Initializes pathfinding, building navmeshes for all maps and constructing a directed graph from them
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void Initialize()
    {
        var navMeshes = new ConcurrentDictionary<GMap, NavMesh>();

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
                    navMeshes.TryAdd(map, navMesh);
            });

        timer.Stop();
        Logger.Info($"Prepared maps in {timer.ElapsedMilliseconds}ms");

        var navs = navMeshes.ToDictionary(kvp => kvp.Key.Accessor, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

        DirectedGraph = new DirectedGraph(navs);
    }

    /// <summary>
    ///     Checks if a location is a wall.
    /// </summary>
    /// <param name="location">
    ///     The location to check.
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>
    ///         true
    ///     </c>
    ///     if the location is a wall, otherwise
    ///     <c>
    ///         false
    ///     </c>
    ///     .
    /// </returns>
    public static bool IsWall(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        var mesh = GetNavMesh(location.Map) ?? throw new InvalidOperationException($"No mesh found for the map \"{location.Map}\"");

        return mesh.IsWall(location);
    }

    /// <inheritdoc cref="MeshBase{TNode,TEdge}.IsWalkable" />
    /// <param name="location">
    ///     The location to check.
    /// </param>
    /// <remarks>
    ///     Answers <c>false</c> for a map with no mesh rather than throwing the way <see cref="IsWall" /> does: a map
    ///     the pathfinder never modelled is one nothing can be routed onto, which is the same answer callers want.
    /// </remarks>
    public static bool IsWalkable(ILocation location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return GetNavMesh(location.Map)
                   ?.IsWalkable(location)
               ?? false;
    }

    private static NavMesh? TryBuildNavMesh(string name, GMap map)
    {
        var geometry = GameData.Geometry[name];

        if ((geometry == null) || (geometry.VerticalLines.Count == 0) || (geometry.HorizontalLines.Count == 0))
        {
            Logger.Debug($"Ignored {name}");

            return null;
        }

        var generator = new NavMeshBuilder(map, geometry);
        var navMesh = generator.Build();
        Logger.Debug($"Prepared {name}");

        return navMesh;
    }
}