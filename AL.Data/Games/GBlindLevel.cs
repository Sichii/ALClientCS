#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     Represents the two forced bets of one poker stake level, positional in the values of <see cref="GPoker.Blinds" />.
/// </summary>
/// <param name="Small">
///     The small blind, in gold, posted by the seat left of the dealer.
/// </param>
/// <param name="Big">
///     The big blind, in gold. Also the unit <see cref="GPoker.BuyIn" /> is counted in.
/// </param>
public sealed record GBlindLevel(
    [property: JsonArrayIndex(0)]
    long Small,
    [property: JsonArrayIndex(1)]
    long Big);