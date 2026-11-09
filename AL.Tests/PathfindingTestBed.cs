#region
using AL.Core.Geometry;
using AL.Core.Interfaces;
using AL.Data;
using AL.Pathfinding;
#endregion

namespace AL.Tests;

public abstract class PathfindingTestBed : GameDataTestBed
{
    [Before(Test)]
    public async Task InitializePathfinderAsync()
    {
        await Sync.WaitAsync();

        try
        {
            if (Pathfinder.GetNavMesh("main") is null)
                Pathfinder.Initialize();
        } finally
        {
            Sync.Release();
        }
    }

    /// <summary>
    ///     Samples pairs of walkable points, both on one map, the map drawn at random from those the pathfinder meshes.
    /// </summary>
    /// <param name="rng">The seeded source of the draws.</param>
    /// <param name="count">How many pairs to sample.</param>
    /// <returns>The sampled pairs.</returns>
    /// <exception cref="InvalidOperationException">50 map draws in a row found no walkable pair.</exception>
    protected static IEnumerable<(ILocation Start, ILocation End)> SamplePairs(Random rng, int count)
    {
        const int MAX_FAILED_MAPS = 50;

        var maps = GameData.Geometry
                           .Keys
                           .Where(map => Pathfinder.GetNavMesh(map) is not null)
                           .Order(StringComparer.Ordinal)
                           .ToList();

        var found = 0;
        var failedMaps = 0;

        while (found < count)
        {
            var map = maps[rng.Next(maps.Count)];

            var pair = SampleWalkable(rng, map, 2)
                .ToList();

            if (pair.Count < 2)
            {
                if (++failedMaps >= MAX_FAILED_MAPS)
                    throw new InvalidOperationException($"{MAX_FAILED_MAPS} map draws in a row found no walkable pair");

                continue;
            }

            failedMaps = 0;
            found++;

            yield return (pair[0], pair[1]);
        }
    }

    /// <summary>
    ///     Samples random points on a map that <see cref="Pathfinder.IsWalkable(ILocation)" /> accepts, stopping early when
    ///     400 draws in a row miss.
    /// </summary>
    /// <param name="rng">The seeded source of the draws.</param>
    /// <param name="map">The map to sample.</param>
    /// <param name="count">How many points to sample.</param>
    /// <returns>The sampled points.</returns>
    protected static IEnumerable<ILocation> SampleWalkable(Random rng, string map, int count)
    {
        const int ATTEMPTS_PER_POINT = 400;

        var geometry = GameData.Geometry[map]!;
        var found = 0;
        var attempts = 0;

        while ((found < count) && (attempts < ATTEMPTS_PER_POINT))
        {
            attempts++;

            var x = geometry.MinX + (float)(rng.NextDouble() * (geometry.MaxX - geometry.MinX));
            var y = geometry.MinY + (float)(rng.NextDouble() * (geometry.MaxY - geometry.MinY));
            var location = new Location(map, x, y);

            if (!Pathfinder.IsWalkable(location))
                continue;

            found++;
            attempts = 0;

            yield return location;
        }
    }
}
