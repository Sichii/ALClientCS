#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     Represents one winning outcome of the slots machine, positional in <c>games.slots.prizes</c>.
/// </summary>
/// <param name="Item">
///     The item the outcome is keyed by, which is also the reel symbol it shows three of.
/// </param>
/// <param name="Payout">
///     What the outcome pays, in gold.
/// </param>
/// <param name="Weight">
///     How many of <see cref="GSlots.Draws" /> land on this outcome.
/// </param>
public sealed record GSlotsPrize(
    [property: JsonArrayIndex(0)]
    string Item,
    [property: JsonArrayIndex(1)]
    long Payout,
    [property: JsonArrayIndex(2)]
    int Weight);