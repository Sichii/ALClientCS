namespace AL.Data.Events;

/// <summary>
///     One farm room the daily dungeon can generate: a named site and the packs it spawns, in order, a pack being what
///     one wave holds. Rides events.dreams.camps, one list per floor.
/// </summary>
public record GCamp
{
    public string Name { get; init; } = null!;

    /// <summary>
    ///     The waves in spawn order. A malformed pack entry reads as null and is skipped by anything walking the list.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<GMonsterCount?>> Packs { get; init; } = [];
}
