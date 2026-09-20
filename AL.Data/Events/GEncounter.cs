namespace AL.Data.Events;

/// <summary>
///     One encounter the daily dungeon can put to the party, and the replies it offers. Rides events.dreams.encounters. A
///     choice on the wire carries option ids and no effects; the effect of each id is read from here.
/// </summary>
public record GEncounter
{
    public string? Actor { get; init; }

    public string? Group { get; init; }

    public string Id { get; init; } = null!;

    public string Kind { get; init; } = null!;

    public string Name { get; init; } = null!;

    public IReadOnlyList<GEncounterOption> Options { get; init; } = [];

    public string? Text { get; init; }
}

/// <summary>
///     One reply. <see cref="Cost" /> is cave gold and <see cref="Amber" /> is amber, both from the party's purse;
///     <see cref="Needs" /> names a supply the party must hold; <see cref="Offer" /> marks the reply the actor leads with.
/// </summary>
public record GEncounterOption
{
    public int Amber { get; init; }

    public long Cost { get; init; }

    public string Effect { get; init; } = null!;

    public string Id { get; init; } = null!;

    public string? Label { get; init; }

    public string? Needs { get; init; }

    public bool Offer { get; init; }

    /// <summary>
    ///     What the reply can lead to, each weighted against the others. Empty on a reply whose result is fixed.
    /// </summary>
    public IReadOnlyList<GEncounterOutcome> Outcomes { get; init; } = [];
}

/// <summary>
///     One thing a reply can lead to. <see cref="Fight" /> is the monster and count it starts, when it starts a fight; the
///     rest is what the party is told or handed.
/// </summary>
public record GEncounterOutcome
{
    public GMonsterCount? Fight { get; init; }

    public long Gold { get; init; }

    public string? Reward { get; init; }

    public string? Text { get; init; }

    public float Weight { get; init; } = 1f;
}