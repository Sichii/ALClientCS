namespace AL.Data.Events;

/// <summary>
///     Represents one farm room the daily dungeon can generate: a named site and the packs it spawns, one pack per wave.
///     Read from <c>events.dreams.camps</c>, one list per floor.
/// </summary>
public record GCamp
{
    public string Name { get; init; } = null!;

    /// <summary>
    ///     The waves in spawn order. A malformed pack entry reads as null.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<GMonsterCount?>> Packs { get; init; } = [];
}