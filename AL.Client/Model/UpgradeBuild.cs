namespace AL.Client.Model;

/// <summary>
///     Represents one build: the expected copies it consumes, the expected gold it spends on scrolls and offerings, and
///     its steps.
/// </summary>
/// <param name="Copies">
///     The expected copies at the starting level the build consumes.
/// </param>
/// <param name="Gold">
///     The expected gold the build spends on scrolls and offerings.
/// </param>
/// <param name="Steps">One step per level climbed, in level order.</param>
public sealed record UpgradeBuild(double Copies, double Gold, IReadOnlyList<UpgradeBuildStep> Steps);

/// <summary>
///     Represents one level of a build, at the chance the planner priced it at.
/// </summary>
/// <param name="ScrollGrade">The scroll's grade.</param>
/// <param name="Offering">The offering's name, or null for none.</param>
/// <param name="Deposits">
///     The offerings used without a scroll before every attempt.
/// </param>
/// <param name="ItemGrace">
///     The grace each staked copy carries when the attempt rolls.
/// </param>
/// <param name="Chance">
///     The level's average chance of success per attempt, failstacks and offering pity included.
/// </param>
public sealed record UpgradeBuildStep(
    int ScrollGrade,
    string? Offering,
    int Deposits,
    double ItemGrace,
    double Chance);