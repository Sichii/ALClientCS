#region
using System.Text.Json.Serialization;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when a chest is opened.
/// </summary>
public sealed record ChestOpenedData
{
    /// <summary>
    ///     True when the chest was too far away (distance &gt; 400) to loot fully; the server forces <see cref="GoldMod" /> to
    ///     1 (the gold-find bonus is dropped, but base gold and items still pay).
    /// </summary>
    [JsonPropertyName("dry")]
    public bool Dry { get; set; }

    /// <summary>
    ///     The amount of gold received from opening the chest.
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    ///     The modifier applied to the amount of gold in the chest.
    /// </summary>
    [JsonPropertyName("goldm")]
    public float GoldMod { get; set; }

    /// <summary>
    ///     TODO: unknown
    /// </summary>
    public bool Gone { get; set; }

    /// <summary>
    ///     The id of the chest.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    ///     A list of items received from the chest.
    /// </summary>
    public IReadOnlyList<ChestItem> Items { get; set; } = new List<ChestItem>();

    /// <summary>
    ///     The name of the player that opened the chest.
    /// </summary>
    [JsonPropertyName("opener")]
    public string OpenerName { get; set; } = null!;

    /// <summary>
    ///     Whether or not the contents of the chest will be distributed to the party.
    /// </summary>
    public bool Party { get; set; }

    /// <summary>
    ///     True when the chest was older than 8 minutes; the server forces <see cref="GoldMod" /> to 1.
    /// </summary>
    [JsonPropertyName("stale")]
    public bool Stale { get; set; }
}