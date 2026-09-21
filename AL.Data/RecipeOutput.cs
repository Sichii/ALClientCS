#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data;

/// <summary>
///     Represents what a recipe hands back, where that is not the item the recipe is filed under.
/// </summary>
public sealed record RecipeOutput
{
    /// <summary>
    ///     If populated, the variant the crafted item carries - a wish jar's wish, for instance.
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; init; }

    /// <summary>The item the craft actually produces.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}