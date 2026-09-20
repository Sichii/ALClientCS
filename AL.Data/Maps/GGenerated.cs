namespace AL.Data.Maps;

/// <summary>
///     Which dungeon run a generated floor belongs to. The floor's map key is spelled from these two values as
///     <c>zone_{run}_{floor}</c> .
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
    ///     Which dungeon generated the floor. "dreams" is the Cave of Many Dreams.
    /// </summary>
    public string Zone { get; init; } = null!;
}