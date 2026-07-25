#region
using System.Text.Json.Nodes;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     The monster/drop tracker snapshot (node/server.js:5068): kill counts, exchange counts and drop tables.
///     Nested drop-table shapes are left as raw JSON tokens - they are deeply heterogeneous and not worth a
///     concrete model.
/// </summary>
public sealed record TrackerData
{
    /// <summary>
    ///     Lifetime kill counts by monster type.
    /// </summary>
    [JsonProperty("monsters")]
    public JsonObject Monsters { get; init; } = new();

    /// <summary>
    ///     Kill counts by monster type since the last snapshot.
    /// </summary>
    [JsonProperty("monsters_diff")]
    public JsonObject MonstersDiff { get; init; } = new();

    /// <summary>
    ///     Exchange (token/quest) counts by item.
    /// </summary>
    [JsonProperty("exchanges")]
    public JsonObject Exchanges { get; init; } = new();

    /// <summary>
    ///     Drop tables by map.
    /// </summary>
    [JsonProperty("maps")]
    public JsonObject Maps { get; init; } = new();

    /// <summary>
    ///     Named drop tables.
    /// </summary>
    [JsonProperty("tables")]
    public JsonObject Tables { get; init; } = new();

    /// <summary>
    ///     The character's max-stat records.
    /// </summary>
    [JsonProperty("max")]
    public JsonObject Max { get; init; } = new();

    /// <summary>
    ///     Home-server drop tables by monster type.
    /// </summary>
    [JsonProperty("monsters_home_server")]
    public JsonObject MonstersHomeServer { get; init; } = new();

    /// <summary>
    ///     Drop tables by monster type.
    /// </summary>
    [JsonProperty("drops")]
    public JsonObject Drops { get; init; } = new();

    /// <summary>
    ///     Home-server drop tables by monster type.
    /// </summary>
    [JsonProperty("drops_home")]
    public JsonObject DropsHome { get; init; } = new();

    /// <summary>
    ///     The global drop table; present only when the server has one configured.
    /// </summary>
    [JsonProperty("global")]
    public JsonArray? Global { get; init; }

    /// <summary>
    ///     The static global drop table; present only when the server has one configured.
    /// </summary>
    [JsonProperty("global_static")]
    public JsonArray? GlobalStatic { get; init; }
}
