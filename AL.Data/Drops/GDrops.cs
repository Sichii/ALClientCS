#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Drops;

/// <summary>
///     The game's drop tables.
/// </summary>
/// <remarks>
///     Only the two the client can act on are bound. The rest of the wire object is the chest tables (one key per
///     chest, each the same positional shape as <see cref="Monsters" />' entries) and <c>maps</c>, the per-map and
///     global tables - those still divide by the monster's HP multiplier, so they are a different mechanic and
///     comparing them against a monster's own table compares unlike quantities.
/// </remarks>
public sealed record GDrops
{
    /// <summary>
    ///     The constants behind a kill's gold reward.
    /// </summary>
    [JsonPropertyName("gold")]
    public GGoldDrop Gold { get; init; } = new();

    /// <summary>
    ///     Each monster's own drop table, keyed by monster accessor. Rolled once per entry on every kill.
    /// </summary>
    [JsonPropertyName("monsters")]
    public IReadOnlyDictionary<string, IReadOnlyList<GDrop>> Monsters { get; init; }
        = new Dictionary<string, IReadOnlyList<GDrop>>(StringComparer.OrdinalIgnoreCase);
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
