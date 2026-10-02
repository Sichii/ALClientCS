namespace AL.Client.Model;

/// <summary>
///     Represents the cheapest expected climb, and the cost of the same climb with matching scrolls and no offerings.
/// </summary>
/// <param name="Steps">
///     One step per level climbed, in level order.
/// </param>
/// <param name="BaselineCost">
///     The expected gold to own one copy at the target with matching scrolls and no offerings.
/// </param>
/// <param name="Unreachable">
///     Why the climb cannot be planned, or null when it can.
/// </param>
public sealed record UpgradePlan(IReadOnlyList<UpgradePlanStep> Steps, double BaselineCost, string? Unreachable)
{
    /// <summary>
    ///     The expected gold to own one copy at the target, from nothing.
    /// </summary>
    public double TotalCost => Steps.Count == 0 ? 0 : Steps[^1].ExpectedCost;
}

/// <summary>
///     Represents one level of a plan: what to put on the bench and what reaching the next level is expected to cost.
/// </summary>
/// <param name="FromLevel">
///     The level the attempt is made from.
/// </param>
/// <param name="ScrollGrade">
///     The scroll's grade.
/// </param>
/// <param name="Offering">
///     The offering's name, or null for none.
/// </param>
/// <param name="Deposits">
///     The offerings used without a scroll before every attempt, each worth +0.5 grace. Always 0 on the compound bench.
/// </param>
/// <param name="ItemGrace">
///     The grace each staked copy carries when the attempt rolls, this level's deposits included.
/// </param>
/// <param name="Chance">
///     The attempt's chance of success.
/// </param>
/// <param name="ExpectedCost">
///     The expected gold to own one copy at <c>FromLevel + 1</c>, from nothing.
/// </param>
public sealed record UpgradePlanStep(
    int FromLevel,
    int ScrollGrade,
    string? Offering,
    int Deposits,
    double ItemGrace,
    double Chance,
    double ExpectedCost);

/// <summary>
///     Represents one level of a plan to price rather than search for.
/// </summary>
/// <param name="FromLevel">
///     The level the attempt is made from.
/// </param>
/// <param name="ScrollGrade">
///     The scroll's grade.
/// </param>
/// <param name="Offering">
///     The offering's name, or null for none.
/// </param>
/// <param name="Deposits">
///     The offerings used without a scroll before every attempt. Upgrade bench only.
/// </param>
/// <param name="DepositOffering">
///     The offering the deposits spend, or null for the cheapest one priced.
/// </param>
public sealed record ForcedStep(
    int FromLevel,
    int ScrollGrade,
    string? Offering,
    int Deposits = 0,
    string? DepositOffering = null);

/// <summary>
///     Represents an offering that could go on the bench, at the price it can actually be had for.
/// </summary>
/// <param name="Name">
///     The offering's key.
/// </param>
/// <param name="Grade">
///     The offering's grade.
/// </param>
/// <param name="Price">
///     The price one can be had for, or 0 when none can.
/// </param>
public sealed record OfferingChoice(string Name, int Grade, double Price);