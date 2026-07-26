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
    ///     If populated, the <see cref="GAchievement" /> this title is associated with.
    /// </summary>
    public string? Achievement { get; init; }

    /// <summary>
    ///     If populated, the type of item this title can affect.
    /// </summary>
    [JsonPropertyName("type")]
    public string? AffectsItemType { get; init; }

    /// <summary>
    ///     The name of this title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If populated, the source of the title.
    /// </summary>
    public string? Source { get; init; }
}