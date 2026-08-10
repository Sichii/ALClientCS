namespace AL.Data.Achievements;

/// <summary>
///     Represents an achievement and what earning it takes.
/// </summary>
public record GAchievement
{
    /// <summary>
    ///     The total this achievement's counter has to reach. What it counts differs per achievement - kills, hits
    ///     taken, damage dealt - and <see cref="Explanation" /> says which.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    ///     How to earn the achievement.
    /// </summary>
    public string? Explanation { get; init; }

    /// <summary>
    ///     If populated, the item this achievement declares as its reward. Nothing in the published server hands it
    ///     out.
    /// </summary>
    public string? Item { get; init; }

    /// <summary>
    ///     The achievement's display name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     How often progress is reported: the server sends an update each time the counter crosses a multiple of
    ///     this. Null means every increment.
    /// </summary>
    public int? Rr { get; init; }

    /// <summary>
    ///     The shells this achievement declares as its reward. Nothing in the published server hands them out.
    /// </summary>
    public int Shells { get; init; }

    /// <summary>
    ///     If populated, the key of the title stamped onto the item that earned this achievement.
    /// </summary>
    public string? Title { get; init; }
}