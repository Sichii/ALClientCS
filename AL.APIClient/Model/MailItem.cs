#region
using System.Text.Json.Serialization;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     A simplified item attached to a piece of <see cref="Mail" />.
/// </summary>
public sealed record MailItem
{
    /// <summary>
    ///     The item's level.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    ///     The item's name.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     The quantity, for stackable items.
    /// </summary>
    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;
}