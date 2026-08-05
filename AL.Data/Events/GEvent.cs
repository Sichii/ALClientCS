namespace AL.Data.Events;

public record GEvent
{
    public int Duration { get; init; }

    /// <summary>
    ///     The event's display name — "Giga Crab", "Goo Brawl", "A/B Testing". Every entry in the table carries one.
    /// </summary>
    public string Name { get; init; } = null!;
}
