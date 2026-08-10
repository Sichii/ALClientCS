namespace AL.Data;

/// <summary>
///     Represents the five character stats, either as a starting set or as a per-level gain.
/// </summary>
public record Stats
{
    /// <summary>
    ///     Dexterity. Raises movement speed and attack frequency.
    /// </summary>
    public float Dex { get; init; }

    /// <summary>
    ///     Fortitude. Cuts damage taken from other players only - it does nothing against monsters.
    /// </summary>
    public float For { get; init; }

    /// <summary>
    ///     Intelligence. Raises max mp, resistance, attack frequency, and tolerance for magical attackers.
    /// </summary>
    public float Int { get; init; }

    /// <summary>
    ///     Strength. Raises max hp, armor, movement speed, and tolerance for physical attackers.
    /// </summary>
    public float Str { get; init; }

    /// <summary>
    ///     Vitality. Raises max hp, by more per point the higher the character's level.
    /// </summary>
    public float Vit { get; init; }
}