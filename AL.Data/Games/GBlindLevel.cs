#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     The two forced bets of one poker stake level. Rides the values of <see cref="GPoker.Blinds" />, which are
///     positional arrays.
/// </summary>
/// <param name="Small">
///     The small blind, in gold. Posted by the seat left of the dealer.
/// </param>
/// <param name="Big">
///     The big blind, in gold, and twice <paramref name="Small" /> at every published level. It is also the unit
///     <see cref="GPoker.BuyIn" /> is counted in.
/// </param>
public sealed record GBlindLevel(
    [property: JsonArrayIndex(0)]
    long Small,
    [property: JsonArrayIndex(1)]
    long Big);
