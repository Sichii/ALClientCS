#region
using AL.APIClient.Interfaces;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents an item looted out of a chest.
/// </summary>
public sealed class ChestItem : ISimpleItem
{
    public int Level { get; set; }

    /// <summary>
    ///     The name of the character that looted the item, or <c>null</c> for an item sent to lost-and-found.
    /// </summary>
    [JsonProperty("looter")]
    public string? LooterName { get; set; }

    /// <summary>
    ///     True when the item could not be placed and was sent to lost-and-found.
    /// </summary>
    [JsonProperty("lostandfound")]
    public bool LostAndFound { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>
    ///     True when the item is a PvP-loot award.
    /// </summary>
    [JsonProperty("pvp_loot")]
    public bool PvpLoot { get; set; }

    [JsonProperty("q")]
    public int Quantity { get; set; } = 1;
}