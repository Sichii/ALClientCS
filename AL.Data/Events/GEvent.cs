namespace AL.Data.Events;

/// <summary>
///     Represents the static data for a timed world event.
/// </summary>
public record GEvent
{
    /// <summary>
    ///     How long the event runs once it starts, in seconds - minutes for the daily and nightly bosses, weeks
    ///     for a seasonal one (node/server_functions.js:2011).
    /// </summary>
    public int Duration { get; init; }

    /// <summary>
    ///     The event's display name — "Giga Crab", "Goo Brawl", "A/B Testing". Every entry in the table carries one.
    /// </summary>
    public string Name { get; init; } = null!;
}
