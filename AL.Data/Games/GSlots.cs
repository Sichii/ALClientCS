namespace AL.Data.Games;

/// <summary>
///     Represents the tavern's slots machine: a fixed-price pull against a weighted prize table.
/// </summary>
/// <remarks>
///     The published table returns exactly <see cref="Gold" /> per pull on average, most of it from the two rarest prizes.
///     The socket event that starts a pull is not modelled.
/// </remarks>
public record GSlots
{
    /// <summary>
    ///     The denominator <see cref="GSlotsPrize.Weight" /> is measured against. The weight left over is the losing share.
    /// </summary>
    public int Draws { get; init; }

    /// <summary>
    ///     What one pull costs, in gold.
    /// </summary>
    public long Gold { get; init; }

    /// <summary>The winning outcomes, richest first.</summary>
    public IReadOnlyList<GSlotsPrize> Prizes { get; init; } = [];

    /// <summary>
    ///     The three reels, in reel order.
    /// </summary>
    /// <remarks>
    ///     Display only: the outcome is drawn from the prize weights and the reels are arranged to show it.
    /// </remarks>
    public IReadOnlyList<IReadOnlyList<string>> Reels { get; init; } = [];

    /// <summary>
    ///     How long the reels spin before they settle, in milliseconds. A result cannot arrive sooner than this.
    /// </summary>
    public int Spin { get; init; }
}