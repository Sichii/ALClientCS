#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Multipliers;

/// <summary>
///     Represents the global economy ratios the server applies to item and currency values.
/// </summary>
public sealed record GMultipliers
{
    /// <summary>
    ///     The ratio of an item's base value an NPC pays when buying it back. The basis of every sell price.
    /// </summary>
    [JsonPropertyName("buy_to_sell")]
    public float BuyToSell { get; init; }

    /// <summary>
    ///     A percent bonus added on top of a shell purchase, so 20 means 20% more shells. Zero outside an event.
    /// </summary>
    [JsonPropertyName("extra_shells")]
    public int ExtraShells { get; init; }

    /// <summary>
    ///     The multiplier on an item's value when buying it back from the lost-and-found NPC. Cash items get no
    ///     separate rate here, unlike at second-hands.
    /// </summary>
    [JsonPropertyName("lostandfound_mult")]
    public float LostAndFoundMult { get; init; }

    /// <summary>
    ///     The multiplier the second-hands NPC applies to cash-shop items, in place of <see cref="BuyToSell" /> x
    ///     <see cref="SecondHandsMult" />.
    /// </summary>
    [JsonPropertyName("secondhands_cash_mult")]
    public float SecondHandsCashMult { get; init; }

    /// <summary>
    ///     The multiplier the second-hands NPC applies to ordinary items, on top of <see cref="BuyToSell" />.
    /// </summary>
    [JsonPropertyName("secondhands_mult")]
    public float SecondHandsMult { get; init; }

    /// <summary>
    ///     The gold a single shell is worth when buying shells with gold.
    /// </summary>
    [JsonPropertyName("shells_to_gold")]
    public int ShellsToGold { get; init; }
}