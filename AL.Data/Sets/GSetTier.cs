#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Sets;

/// <summary>
///     What one piece count of a set is worth.
/// </summary>
/// <remarks>
///     Two readings of the same fact, because the panel asks two questions of it. <see cref="InEffect" /> is what
///     the server applies at this count; <see cref="Adds" /> is what this tier alone contributes, which is also
///     exactly what wearing one more piece would buy over the tier below.
/// </remarks>
public sealed record GSetTier
{
    /// <summary>How many pieces of the set are worn.</summary>
    public int Pieces { get; init; }

    /// <summary>
    ///     This tier's own line, as the wire carries it. Empty on the tiers that are authored as <c>{}</c> - most
    ///     sets give nothing for a single piece.
    /// </summary>
    public GSetBonus Adds { get; init; } = new();

    /// <summary>
    ///     What the server actually applies at this count: every tier up to and including this one, summed. See
    ///     <c>GameData.EnrichSets</c> for why this is not the same as <see cref="Adds" />.
    /// </summary>
    public GSetBonus InEffect { get; init; } = new();

    /// <summary>
    ///     How the tier heads its own group in the game-data explorer, which reads a record's <c>Name</c> where it
    ///     has one and falls back to the list index. Derived rather than stored - an index reads as "0 pieces".
    /// </summary>
    [JsonIgnore]
    public string Name => Pieces == 1 ? "1 piece" : $"{Pieces} pieces";
}
