#region
using System.Collections.Generic;
using AL.Core.Json.Converters;
using AL.Core.Json.Interfaces;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents a prediction for an upgrade/compound action, or the title an item carries.
/// </summary>
/// <remarks>
///     The item's "p" key is overloaded. Most of the time it is a title name such as "shiny" or "legacy",
///     and it is <c>false</c> when the item has no title. It is only an object while that item is the
///     placeholder for an in-progress upgrade or compound.
/// </remarks>
[JsonConverter(typeof(StringOrObjectConverter<Prediction>), nameof(Title))]
public sealed record Prediction : IOptionalObject
{
    /// <summary>
    ///     The chance for the upgrade/compound to succeed.
    /// </summary>
    public float Chance { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool ContainsData { get; set; }

    /// <summary>
    ///     True once the roll is revealed to fail; <c>null</c>/absent while the outcome is still hidden.
    ///     <see cref="Success" /> is false in both states, so this is the only way to tell "will fail" from
    ///     "not yet revealed".
    /// </summary>
    [JsonProperty("failure")]
    public bool? Failure { get; init; }

    /// <summary>
    ///     The current level of the item.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    ///     The name of the item.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     TODO: What's this?
    /// </summary>
    public IReadOnlyList<int> Nums { get; init; } = new List<int>();

    /// <summary>
    ///     The name of the offering consumed by the upgrade/compound, if one was used.
    /// </summary>
    [JsonProperty("offering")]
    public string? OfferingName { get; init; }

    /// <summary>
    ///     The name of the scroll being used to upgrade the item.
    /// </summary>
    [JsonProperty("scroll")]
    public string ScrollName { get; init; } = null!;

    /// <summary>
    ///     Whether or not the upgrade succeeded. (This will always be false for predictions)
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    ///     The item's title, when "p" carried a title name rather than upgrade details.
    ///     <br />
    ///     Look this up in <c>GameData.Titles</c>. e.g. "shiny", "legacy", "superfast".
    /// </summary>
    [JsonIgnore]
    public string? Title { get; set; }
}
