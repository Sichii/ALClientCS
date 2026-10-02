namespace AL.Client.Definitions;

/// <summary>
///     Represents an item's scroll tier, from the level thresholds in its <c>grades</c> table.
/// </summary>
/// <remarks>
///     <see cref="None" /> means the item has no <c>grades</c> table, which the server prices at grade zero; the bottom tier
///     is <see cref="Normal" />, so selecting the cheapest tier wants <c>&lt;= Normal</c>.
/// </remarks>
public enum Grade
{
    None = -1,
    Normal,
    High,
    Rare,
    Legendary,
    Exalted
}

public enum DistanceType
{
    CenterToCenter,
    EdgeToCenter,
    EdgeToEdge
}

public enum MutationType
{
    None,
    Hp
}