namespace AL.Client.Model;

/// <summary>
///     Represents what goes on the bench for every attempt at one level.
/// </summary>
/// <param name="ScrollGrade">The scroll's grade.</param>
/// <param name="ScrollPrice">The scroll's price.</param>
/// <param name="Offering">
///     The offering spent with the scroll, or null for none.
/// </param>
/// <param name="Deposits">
///     The offerings used without a scroll before the attempt.
/// </param>
/// <param name="DepositPrice">The price of one deposited offering.</param>
internal sealed record PlanChoice(
    int ScrollGrade,
    double ScrollPrice,
    OfferingChoice? Offering,
    int Deposits,
    double DepositPrice)
{
    /// <summary>The gold one attempt spends on the bench.</summary>
    public double Fees => ScrollPrice + (Offering?.Price ?? 0) + Deposits * DepositPrice;
}

/// <summary>
///     Represents a plan priced as a long-run average over repeated climbs.
/// </summary>
/// <param name="Choices">One choice per level climbed, in level order.</param>
/// <param name="Copies">
///     The expected copies at the starting level one finished copy consumes.
/// </param>
/// <param name="Gold">
///     The expected gold one finished copy spends on the bench.
/// </param>
/// <param name="OfferingPity">
///     The offering pity counter the plan settles at when a climb starts.
/// </param>
/// <param name="ItemGrace">
///     The grace each staked copy carries at each level, deposits included.
/// </param>
/// <param name="Attempts">
///     The expected attempts each level takes per success.
/// </param>
internal sealed record PricedPlan(
    IReadOnlyList<PlanChoice> Choices,
    double Copies,
    double Gold,
    double OfferingPity,
    IReadOnlyList<double> ItemGrace,
    IReadOnlyList<double> Attempts)
{
    /// <summary>
    ///     Calculates the expected gold to own one finished copy, the copies it consumes bought at the given price.
    /// </summary>
    /// <param name="copyPrice">The price of one copy at the starting level.</param>
    /// <returns>The expected gold.</returns>
    public double CalculateCost(double copyPrice) => Copies * copyPrice + Gold;
}