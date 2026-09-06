namespace AL.Data.Events;

/// <summary>
///     Represents the static data for a timed world event.
/// </summary>
public record GEvent
{
    /// <summary>
    ///     How long the event runs once it starts, in seconds - minutes for the daily and nightly bosses, weeks for a seasonal
    ///     one (node/server_functions.js:2011).
    /// </summary>
    public int Duration { get; init; }

    /// <summary>
    ///     Whether the event can be joined from wherever the character is standing, rather than only by walking to where it is
    ///     happening. Carried by the daily and nightly bosses; absent, and so false, on every seasonal one.
    /// </summary>
    public bool Join { get; init; }

    /// <summary>
    ///     The event's display name — "Giga Crab", "Goo Brawl", "A/B Testing". Every entry in the table carries one.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     What the client draws the event as. Usually a monster skin - the boss the event is about, or one of the things it
    ///     brings out - and otherwise an item the event hands out, such as the Lunar New Year's red envelope.
    /// </summary>
    /// <remarks>
    ///     Not always either of those: A/B Testing's names a character skin, which is on neither the monster table nor the
    ///     item table. Anything resolving it has to try each source rather than assume one.
    /// </remarks>
    public string? Sprite { get; init; }

    /// <summary>
    ///     The schedule the event runs on -
    ///     <c>
    ///         daily
    ///     </c>
    ///     ,
    ///     <c>
    ///         nightly
    ///     </c>
    ///     or
    ///     <c>
    ///         seasonal
    ///     </c>
    ///     . It says how often the event comes round rather than what kind of content it is: the two nightly ones are world
    ///     bosses, and so is one of the seasonal ones.
    /// </summary>
    public string? Type { get; init; }
}