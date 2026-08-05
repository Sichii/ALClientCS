namespace AL.Client.Definitions;

/// <summary>
///     An item's scroll tier, from the level thresholds in its <c>grades</c> table.
/// </summary>
/// <remarks>
///     <see cref="None" /> is <b>-1 and is a sentinel, not the bottom tier</b>: it means the item has no
///     <c>grades</c> table at all, which the server still prices at grade zero. The bottom tier - what an ordinary
///     item is before its first threshold, and what <c>scroll0</c>/<c>cscroll0</c> apply to - is
///     <see cref="Normal" />. Anything selecting the cheapest tier wants <c>&lt;= Normal</c>; a test for
///     <c>== None</c> matches nothing an item shop sells.
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