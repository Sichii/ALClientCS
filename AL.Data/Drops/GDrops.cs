#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Drops;

/// <summary>
///     The game's drop tables.
/// </summary>
/// <remarks>
///     Every key of the wire object is reachable: <see cref="Gold" />, <see cref="Monsters" />, <see cref="Maps" />
///     and <see cref="Konami" /> by name, and every remaining table - one per drop id an exchange or a chest rolls
///     - through <see cref="Tables" />.
/// </remarks>
public sealed record GDrops
{
    /// <summary>
    ///     The constants behind a kill's gold reward. They scale the monster's own gold figure, which is then
    ///     multiplied by its level and your share of the kill (node/server.js:2118).
    /// </summary>
    [JsonPropertyName("gold")]
    public GGoldDrop Gold { get; init; } = new();

    /// <summary>
    ///     Each monster's own drop table, keyed by monster accessor. Every entry is rolled separately on every
    ///     kill, so a table naming one item twice gives it two independent chances and the rates add
    ///     (node/server.js:2197).
    /// </summary>
    [JsonPropertyName("monsters")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Monsters { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The per-map and global tables, keyed by map accessor. These divide by the monster's HP multiplier where a
    ///     monster's own table does not, so a rate here is not comparable with a rate in <see cref="Monsters" />.
    /// </summary>
    [JsonPropertyName("maps")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Maps { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     What the konami code rolls on.
    /// </summary>
    [JsonPropertyName("konami")]
    public IReadOnlyList<GDrop> Konami { get; init; } = [];

    /// <summary>
    ///     Every other table, keyed by the drop id the server rolls it under: what an exchange or an opened chest
    ///     hands back. The id is an item's name for most of them, an item's name plus its level for the one
    ///     exchangeable that takes levels, and neither for the rest - <c>xN</c>, <c>f1</c>, <c>skins</c> and their
    ///     like name no item at all, which is why this stays reachable rather than being folded entirely into
    ///     <see cref="AL.Data.Items.GItem.ExchangeRewards" />.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Tables { get; internal set; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The wire's remaining keys, held as they arrived until <c>GameData.EnrichDrops</c> turns them into
    ///     <see cref="Tables" />. Anything the game adds to this object lands here rather than being discarded.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Unbound { get; init; }
}

/// <summary>
///     The constants in the gold reward for a kill:
///     <c>
///         round(1 + gold * (BASE + rand() * RANDOM)) * level * mult
///     </c>
///     (node/server.js:2119), then two independent jackpots at :2247. The monster's own <c>gold</c> is not in the
///     game data - it reaches a client only as the start frame's base_gold table.
/// </summary>
public sealed record GGoldDrop
{
    /// <summary>
    ///     The share of the monster's gold value paid on every kill.
    /// </summary>
    [JsonPropertyName("base")]
    public float Base { get; init; }

    /// <summary>
    ///     The share of the monster's gold value paid on a uniform roll on top of <see cref="Base" />.
    /// </summary>
    [JsonPropertyName("random")]
    public float Random { get; init; }

    /// <summary>
    ///     The chance the whole reward is multiplied by ten.
    /// </summary>
    [JsonPropertyName("x10")]
    public float X10 { get; init; }

    /// <summary>
    ///     The chance the whole reward is multiplied by fifty, rolled independently of <see cref="X10" />.
    /// </summary>
    [JsonPropertyName("x50")]
    public float X50 { get; init; }
}
