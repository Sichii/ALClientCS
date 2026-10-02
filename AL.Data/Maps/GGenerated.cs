namespace AL.Data.Maps;

/// <summary>
///     Represents the dungeon run a generated floor belongs to. The floor's map key is spelled from these values as
///     <c>zone_{run}_{floor}</c>.
/// </summary>
public sealed record GGenerated
{
    /// <summary>The floor's number within the run, from zero.</summary>
    public int Floor { get; init; }

    /// <summary>
    ///     The run's id: 24 hex characters, minted when the party enters.
    /// </summary>
    public string Run { get; init; } = null!;

    /// <summary>
    ///     The dungeon that generated the floor. <c>dreams</c> is the Cave of Many Dreams.
    /// </summary>
    public string Zone { get; init; } = null!;
}