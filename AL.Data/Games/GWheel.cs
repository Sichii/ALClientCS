namespace AL.Data.Games;

/// <summary>
///     Represents the tavern's wheel: a bet on which of two sides the wheel stops on.
/// </summary>
/// <remarks>
///     The socket event that places the spin is not modelled.
/// </remarks>
public record GWheel
{
    /// <summary>The smallest stake the wheel accepts, in gold.</summary>
    public long Min { get; init; }

    /// <summary>
    ///     The two outcomes a stake can be placed on - <c>sun</c> and <c>moon</c>. A slice names one of these in
    ///     <see cref="GWheelSlice.Side" />.
    /// </summary>
    public IReadOnlyList<string> Sides { get; init; } = [];

    /// <summary>
    ///     The wedges in wheel order, split evenly between the sides.
    /// </summary>
    public IReadOnlyList<GWheelSlice> Slices { get; init; } = [];

    /// <summary>
    ///     How long the wheel spins before it settles, in milliseconds. A result cannot arrive sooner than this.
    /// </summary>
    public int Spin { get; init; }
}