#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     Represents how much a player may bring to a poker seat, in big blinds. Positional in <see cref="GPoker.BuyIn" />.
/// </summary>
/// <param name="Min">The smallest stack a seat accepts.</param>
/// <param name="Max">The largest stack a seat accepts.</param>
public sealed record GBuyIn(
    [property: JsonArrayIndex(0)]
    int Min,
    [property: JsonArrayIndex(1)]
    int Max);