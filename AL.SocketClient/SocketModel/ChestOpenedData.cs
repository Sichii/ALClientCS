#region
using System.Collections.Generic;
using AL.SocketClient.Model;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received when a chest is opened.
/// </summary>
public sealed record ChestOpenedData
{
    /// <summary>
    ///     The amount of gold received from opening the chest.
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    ///     The modifier applied to the amount of gold in the chest.
    /// </summary>
    [JsonProperty("goldm")]
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
    [JsonProperty("opener")]
    public string OpenerName { get; set; } = null!;

    /// <summary>
    ///     Whether or not the contents of the chest will be distributed to the party.
    /// </summary>
    public bool Party { get; set; }
}