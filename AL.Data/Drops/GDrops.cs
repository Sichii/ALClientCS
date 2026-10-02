#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Drops;

/// <summary>Represents the game's drop tables.</summary>
public sealed record GDrops
{
    /// <summary>
    ///     The constants behind a kill's gold reward, which scale the monster's own gold figure before it is multiplied by
    ///     its level and your share of the kill.
    /// </summary>
    [JsonPropertyName("gold")]
    public GGoldDrop Gold { get; init; } = new();

    /// <summary>What the konami code rolls on.</summary>
    [JsonPropertyName("konami")]
    public IReadOnlyList<GDrop> Konami { get; init; } = [];

    /// <summary>
    ///     The per-map and global tables, keyed by map accessor. These divide by the monster's HP multiplier where a monster's
    ///     own table does not, so a rate here is not comparable with a rate in <see cref="Monsters" />.
    /// </summary>
    [JsonPropertyName("maps")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Maps { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Each monster's own drop table, keyed by monster accessor. Every entry is rolled separately on every kill, so a
    ///     table naming one item twice gives it two independent chances and the rates add.
    /// </summary>
    [JsonPropertyName("monsters")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Monsters { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     A second table a few monsters carry, rolled beside their own only when the killer is on the server they call home.
    ///     Keyed by monster accessor; entries roll and scale exactly as <see cref="Monsters" /> do.
    /// </summary>
    [JsonPropertyName("monsters_home_server")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> MonstersHomeServer { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Every other table, keyed by the drop id an exchange or an opened chest rolls under. Most ids are item names; some,
    ///     such as <c>xN</c>, <c>f1</c> and <c>skins</c>, name no item.
    /// </summary>
    /// <remarks>
    ///     Enriched property
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Tables { get; internal set; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The wire's remaining keys, held until <c>GameData.EnrichDrops</c> turns them into <see cref="Tables" />.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Unbound { get; init; }
}

/// <summary>
///     Represents the constants in a kill's gold reward, <c>round(1 + gold * (BASE + rand() * RANDOM)) * level * mult</c>.
///     The monster's own <c>gold</c> reaches a client only in the start frame's <c>base_gold</c> table.
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