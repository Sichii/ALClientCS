namespace AL.Data.Games;

/// <summary>
///     The tavern's wheel: a bet on which of two sides the wheel stops on, rather than a prize wheel. It replaced a
///     thirteen-slice gold-prize wheel in the game data that shipped with version 16846.
/// </summary>
/// <remarks>
///     The socket event that places the spin is not modelled. The script function is <c>bet_wheel</c>, but the handler is
///     absent from the published server source, so the payload and the reply shape are unknown. This record covers the
///     static half only.
/// </remarks>
public record GWheel
{
    /// <summary>
    ///     The smallest stake the wheel accepts, in gold. Two orders of magnitude under the slots' fixed pull, so this is the
    ///     cheap seat of the three machines.
    /// </summary>
    public long Min { get; init; }

    /// <summary>
    ///     The two outcomes a stake can be placed on - <c>sun</c> and <c>moon</c>. A slice names one of these in
    ///     <see cref="GWheelSlice.Side" />.
    /// </summary>
    public IReadOnlyList<string> Sides { get; init; } = [];

    /// <summary>
    ///     The fourteen wedges in wheel order. They split evenly, seven per side, so a spin is an even-money proposition
    ///     before whatever the house takes.
    /// </summary>
    public IReadOnlyList<GWheelSlice> Slices { get; init; } = [];

    /// <summary>
    ///     How long the wheel spins before it settles, in milliseconds. A result cannot arrive sooner than this.
    /// </summary>
    public int Spin { get; init; }
}