#region
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Games;

/// <summary>
///     One winning outcome of the slots machine. Rides <c>games.slots.prizes</c>, whose entries are positional arrays.
/// </summary>
/// <param name="Item">
///     The item name the outcome is keyed by, which is also the reel symbol three of a kind of it shows. Every one of the
///     seven is a real item on <see cref="GameData.Items" />.
/// </param>
/// <param name="Payout">What the outcome pays, in gold.</param>
/// <param name="Weight">
///     How many of <see cref="GSlots.Draws" /> land on this outcome. Divide by <see cref="GSlots.Draws" /> for the
///     probability of one pull.
/// </param>
public sealed record GSlotsPrize(
    [property: JsonArrayIndex(0)]
    string Item,
    [property: JsonArrayIndex(1)]
    long Payout,
    [property: JsonArrayIndex(2)]
    int Weight);