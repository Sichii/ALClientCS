#region
using Newtonsoft.Json;
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
    [JsonProperty("buy_to_sell")]
    public float BuyToSell { get; init; }

    /// <summary>
    ///     The number of bonus shells granted on a shell purchase.
    /// </summary>
    [JsonProperty("extra_shells")]
    public int ExtraShells { get; init; }

    /// <summary>
    ///     The multiplier applied to items recovered from the lost-and-found.
    /// </summary>
    [JsonProperty("lostandfound_mult")]
    public float LostAndFoundMult { get; init; }

    /// <summary>
    ///     The multiplier the second-hands NPC applies to cash-shop items, in place of
    ///     <see cref="BuyToSell" /> x <see cref="SecondHandsMult" />.
    /// </summary>
    [JsonProperty("secondhands_cash_mult")]
    public float SecondHandsCashMult { get; init; }

    /// <summary>
    ///     The multiplier the second-hands NPC applies to ordinary items, on top of <see cref="BuyToSell" />.
    /// </summary>
    [JsonProperty("secondhands_mult")]
    public float SecondHandsMult { get; init; }

    /// <summary>
    ///     The gold value of a single shell.
    /// </summary>
    [JsonProperty("shells_to_gold")]
    public int ShellsToGold { get; init; }
}
