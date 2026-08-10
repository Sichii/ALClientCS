#region
using System.Text.Json.Serialization;
using AL.Core.Abstractions;
using AL.Data.Achievements;
#endregion

namespace AL.Data.Titles;

/// <summary>
///     Represents a title that can be applied to an item.
///     <br />
///     <inheritdoc cref="AttributedRecordBase" />
/// </summary>
/// <seealso cref="AttributedRecordBase" />
public sealed record GTitle : AttributedRecordBase
{
    /// <summary>
    ///     If populated, the key of the <see cref="GAchievement" /> that stamps this title onto the item that earned
    ///     it.
    /// </summary>
    public string? Achievement { get; init; }

    /// <summary>
    ///     What this title can land on - an item type, an equipment slot, or <c>all_items</c> for anything.
    /// </summary>
    [JsonPropertyName("type")]
    public string? AffectsItemType { get; init; }

    /// <summary>
    ///     The title as shown to a player. The client prefixes it to the item's own name - "Shiny Firebow".
    /// </summary>
    [JsonPropertyName("title")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If populated, how the title is come by - upgrading, exchanging, an achievement, or plain chance.
    /// </summary>
    public string? Source { get; init; }
}