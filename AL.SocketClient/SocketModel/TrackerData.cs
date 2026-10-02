#region
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the monster and drop tracker snapshot: kill counts, exchange counts and drop tables. The heterogeneous
///     drop-table shapes stay raw JSON.
/// </summary>
public sealed record TrackerData
{
    /// <summary>Drop tables by monster type.</summary>
    [JsonPropertyName("drops")]
    public JsonObject Drops { get; init; } = new();

    /// <summary>Home-server drop tables by monster type.</summary>
    [JsonPropertyName("drops_home")]
    public JsonObject DropsHome { get; init; } = new();

    /// <summary>Exchange (token/quest) counts by item.</summary>
    [JsonPropertyName("exchanges")]
    public JsonObject Exchanges { get; init; } = new();

    /// <summary>
    ///     The global drop table; present only when the server has one configured.
    /// </summary>
    [JsonPropertyName("global")]
    public JsonArray? Global { get; init; }

    /// <summary>
    ///     The static global drop table; present only when the server has one configured.
    /// </summary>
    [JsonPropertyName("global_static")]
    public JsonArray? GlobalStatic { get; init; }

    /// <summary>Drop tables by map.</summary>
    [JsonPropertyName("maps")]
    public JsonObject Maps { get; init; } = new();

    /// <summary>The character's max-stat records.</summary>
    [JsonPropertyName("max")]
    public JsonObject Max { get; init; } = new();

    /// <summary>
    ///     Kill counts by monster type as of the last snapshot. The lifetime total is this plus <see cref="MonstersDiff" />.
    /// </summary>
    [JsonPropertyName("monsters")]
    public JsonObject Monsters { get; init; } = new();

    /// <summary>
    ///     Kill counts by monster type since the last snapshot. These are not counted in <see cref="Monsters" /> yet.
    /// </summary>
    [JsonPropertyName("monsters_diff")]
    public JsonObject MonstersDiff { get; init; } = new();

    /// <summary>Home-server drop tables by monster type.</summary>
    [JsonPropertyName("monsters_home_server")]
    public JsonObject MonstersHomeServer { get; init; } = new();

    /// <summary>Named drop tables.</summary>
    [JsonPropertyName("tables")]
    public JsonObject Tables { get; init; } = new();
}