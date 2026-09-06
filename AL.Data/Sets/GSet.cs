#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Sets;

/// <summary>
///     An armor set. Wearing several pieces of one set adds a bonus that grows with the count.
/// </summary>
public sealed record GSet
{
    /// <summary>
    ///     The game's own key for this set, which is what <see cref="AL.Core.Definitions.ArmorSet" /> spells.
    /// </summary>
    public string Accessor { get; internal set; } = null!;

    /// <summary>
    ///     The game's own line about the set, or null for one that carries none.
    /// </summary>
    public string? Explanation { get; init; }

    /// <summary>
    ///     The item keys that belong to the set. Its length is also the highest count that can be worn.
    /// </summary>
    public IReadOnlyList<string> Items { get; init; } = [];

    /// <summary>
    ///     The set as shown to a player - "Rugged Set", "Monster Hunter Ranger".
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     What each piece count is worth, ascending from one piece to <see cref="Items" />'s length.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<GSetTier> Tiers { get; internal set; } = [];

    /// <summary>
    ///     The tier objects as they arrive, keyed by the count as a string - the wire files them beside
    ///     <c>
    ///         name
    ///     </c>
    ///     and
    ///     <c>
    ///         items
    ///     </c>
    ///     rather than under a container of their own, so there is nothing for a declared property to bind.
    /// </summary>
    /// <remarks>
    ///     Cleared to null by
    ///     <c>
    ///         GameData.EnrichSets
    ///     </c>
    ///     once <see cref="Tiers" /> is built, which is what keeps raw JSON off the game-data explorer: a null field is absent
    ///     and draws nothing. It is also what makes the pass safe to run over a set twice.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? WireTiers { get; set; }
}