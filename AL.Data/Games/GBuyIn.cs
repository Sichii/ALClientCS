#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     How much a player may bring to a poker seat, counted in big blinds rather than gold. Rides
///     <see cref="GPoker.BuyIn" />, which is a positional array.
/// </summary>
/// <param name="Min">
///     The smallest stack a seat accepts.
/// </param>
/// <param name="Max">
///     The largest stack a seat accepts.
/// </param>
public sealed record GBuyIn(
    [property: JsonArrayIndex(0)]
    int Min,
    [property: JsonArrayIndex(1)]
    int Max);
