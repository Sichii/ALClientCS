#region
using System.Text.Json.Serialization;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents what a trade offer asks for in exchange for the item in its stand slot: an item by name, and optionally
///     the lowest level, the title and the stack size it accepts.
/// </summary>
public sealed record TradeWant
{
    /// <summary>
    ///     If populated, the lowest level accepted. Unset accepts any level.
    /// </summary>
    [JsonPropertyName("level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Level { get; init; }

    /// <summary>The wanted item's key in the game's item table.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    ///     If populated, how many of a stackable item the offer asks for. Unset asks for one.
    /// </summary>
    [JsonPropertyName("q")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Quantity { get; init; }

    /// <summary>
    ///     If populated, the title the item must carry, such as <c>shiny</c> or <c>glitched</c>. Unset accepts any title or
    ///     none.
    /// </summary>
    [JsonPropertyName("p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    /// <summary>
    ///     Determines whether an item with these properties satisfies the offer: the same name, at least the wanted level and
    ///     stack, and the wanted title when one is set.
    /// </summary>
    /// <param name="name">The item's name.</param>
    /// <param name="level">The item's level.</param>
    /// <param name="title">The item's title, if any.</param>
    /// <param name="quantity">The item's stack size.</param>
    /// <returns>
    ///     <c>true</c> if the item satisfies the offer; otherwise, <c>false</c> .
    /// </returns>
    /// <remarks>
    ///     A locked or bound item passes this and is still refused; the game's stand leaves those out of the choice before it
    ///     asks.
    /// </remarks>
    public bool Accepts(
        string name,
        int level,
        string? title,
        int quantity)
    {
        if (!string.Equals(name, Name, StringComparison.Ordinal))
            return false;

        if (Level is { } minimum and > 0 && (level < minimum))
            return false;

        if (!string.IsNullOrEmpty(Title) && !string.Equals(title, Title, StringComparison.Ordinal))
            return false;

        return Math.Max(1, quantity) >= Math.Max(1, Quantity ?? 1);
    }
}