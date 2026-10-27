#region
using System.Text.Json.Serialization;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents a simplified item attached to a <see cref="Mail" />.
/// </summary>
public sealed record MailItem
{
    /// <summary>
    ///     The gold a cave reward carries instead of an item. Zero on an item.
    /// </summary>
    public long Gold { get; init; }

    /// <summary>The item's level.</summary>
    public int Level { get; init; }

    /// <summary>
    ///     If populated, the item's name. Null on a gold attachment.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>The quantity, for stackable items.</summary>
    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;
}