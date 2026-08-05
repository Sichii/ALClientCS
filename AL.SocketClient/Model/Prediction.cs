#region
using System.Text.Json.Serialization;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents a prediction for an upgrade/compound action, or the title an item carries.
/// </summary>
/// <remarks>
///     The item's "p" key is overloaded. Most of the time it is a title name such as "shiny" or "legacy", and it is
///     <c>
///         false
///     </c>
///     when the item has no title. It is only an object while that item is the placeholder for an in-progress upgrade or
///     compound.
/// </remarks>
[JsonStringOrObject(nameof(Title))]
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
    ///     True once the roll is revealed to fail;
    ///     <c>
    ///         null
    ///     </c>
    ///     /absent while the outcome is still hidden. <see cref="Success" /> is false in both states, so this is the only way
    ///     to tell "will fail" from "not yet revealed".
    /// </summary>
    [JsonPropertyName("failure")]
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
    ///     The four decimal digits of the roll this attempt was decided by, least significant first: <c>Nums[3]</c> is
    ///     the first decimal place and <c>Nums[0]</c> the fourth, so the roll is
    ///     <c>Nums[3]/10 + Nums[2]/100 + Nums[1]/1000 + Nums[0]/10000</c>.
    ///     <br />
    ///     They arrive one at a time as the animation counts down, each published once the remaining time falls under a
    ///     fraction of the whole - 80%, 64%, 40%, then 30% capped at 3s (<c>node/server.js:13215-13230</c>) - so a list
    ///     shorter than four is a roll still being revealed rather than a small one. On the 500ms a +0 attempt takes,
    ///     the last of them lands with 150ms to spare.
    ///     <br />
    ///     <b>Only readable while the attempt is in flight</b>: this object lives on the placeholder occupying the
    ///     item's slot, and the placeholder is replaced by the result the moment <c>upgrade_success</c> or
    ///     <c>upgrade_fail</c> lands.
    ///     <br />
    ///     It is the only window onto the roll the server actually used, which is what makes the lucky-slot bonus
    ///     measurable at all - that bonus deforms the roll and never the quoted chance, so nothing a <c>calculate</c>
    ///     returns can see it.
    /// </summary>
    public IReadOnlyList<int> Nums { get; init; } = new List<int>();

    /// <summary>
    ///     The name of the offering consumed by the upgrade/compound, if one was used.
    /// </summary>
    [JsonPropertyName("offering")]
    public string? OfferingName { get; init; }

    /// <summary>
    ///     The name of the scroll being used to upgrade the item.
    /// </summary>
    [JsonPropertyName("scroll")]
    public string ScrollName { get; init; } = null!;

    /// <summary>
    ///     True once the roll is revealed to succeed. Like <see cref="Failure" /> it is published ahead of the result
    ///     itself - with 22% of the animation left, capped at 2.2s - so it is false both before the reveal and on an
    ///     attempt that is going to fail.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    ///     The item's title, when "p" carried a title name rather than upgrade details.
    ///     <br />
    ///     Look this up in
    ///     <c>
    ///         GameData.Titles
    ///     </c>
    ///     . e.g. "shiny", "legacy", "superfast".
    /// </summary>
    [JsonIgnore]
    public string? Title { get; set; }
}