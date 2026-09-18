namespace AL.Data.Games;

/// <summary>
///     The tavern's slots machine: a fixed-price pull against a weighted prize table. It replaced a single-jackpot machine
///     in the game data that shipped with version 16846.
/// </summary>
/// <remarks>
///     <b>The table as published is exactly break-even.</b> Payout times weight summed over <see cref="Prizes" /> is
///     30,000,000,000 against <see cref="Draws" /> of 30,000, which is <see cref="Gold" /> per pull to the gold. The
///     weights total 5,216, so roughly one pull in six wins anything and the rest of the return rides on the two rarest
///     outcomes. Anything sizing a bankroll off the average return should carry that variance, not the mean.
///     <br />
///     The socket event that starts a pull is not modelled. The script function is <c>play_slots</c>, and the published
///     server source has only the handler this table replaced, so the reply shape is unknown.
/// </remarks>
public record GSlots
{
    /// <summary>
    ///     The denominator <see cref="GSlotsPrize.Weight" /> is measured against. Weights that do not reach it are the losing
    ///     share of a pull.
    /// </summary>
    public int Draws { get; init; }

    /// <summary>
    ///     What one pull costs, in gold. Fixed - the machine takes no stake.
    /// </summary>
    public long Gold { get; init; }

    /// <summary>
    ///     The winning outcomes, richest first.
    /// </summary>
    public IReadOnlyList<GSlotsPrize> Prizes { get; init; } = [];

    /// <summary>
    ///     The three reels, twenty symbols each, in reel order.
    /// </summary>
    /// <remarks>
    ///     Treat these as the animation rather than the odds. Three of a kind read straight off these reels gives
    ///     probabilities that do not match <see cref="GSlotsPrize.Weight" /> for any prize, so the outcome is drawn from the
    ///     weights and the reels are then arranged to show it.
    /// </remarks>
    public IReadOnlyList<IReadOnlyList<string>> Reels { get; init; } = [];

    /// <summary>
    ///     How long the reels spin before they settle, in milliseconds. A result cannot arrive sooner than this.
    /// </summary>
    public int Spin { get; init; }
}
