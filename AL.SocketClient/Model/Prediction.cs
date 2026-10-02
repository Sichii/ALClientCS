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
///     The item's <c>p</c> key is overloaded. Most of the time it is a title name such as <c>shiny</c> or <c>legacy</c>,
///     and it is <c>false</c> when the item has no title. It is only an object while that item is the placeholder for an
///     in-progress upgrade or compound.
/// </remarks>
[JsonStringOrObject(nameof(Title))]
public sealed record Prediction : IOptionalObject
{
    /// <summary>The chance for the upgrade/compound to succeed.</summary>
    public float Chance { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool ContainsData { get; set; }

    /// <summary>
    ///     Whether the roll has been revealed to fail; <c>null</c> while the outcome is still hidden.
    ///     <see cref="Success" /> is false in both states.
    /// </summary>
    [JsonPropertyName("failure")]
    public bool? Failure { get; init; }

    /// <summary>The current level of the item.</summary>
    public int Level { get; init; }

    /// <summary>The name of the item.</summary>
    public string Name { get; init; } = null!;

    /// <summary>
    ///     The four decimal digits of the roll this attempt was decided by, least significant first: <c>Nums[3]</c> is the
    ///     first decimal place and <c>Nums[0]</c> the fourth, so the roll is <c>
    ///         Nums[3]/10 + Nums[2]/100 + Nums[1]/1000 + Nums[0]/10000
    ///     </c> .
    ///     <br />
    ///     <b>Only readable while the attempt is in flight</b>: the placeholder holding this is replaced the moment
    ///     <c>upgrade_success</c> or <c>upgrade_fail</c> lands.
    /// </summary>
    /// <remarks>
    ///     The digits arrive one at a time, each once the remaining time falls under 80%, 64%, 40%, then 30% (capped at 3s) of
    ///     the whole. The lucky-slot bonus deforms this roll and never the quoted chance.
    /// </remarks>
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
    ///     Whether the roll has been revealed to succeed, published with 22% of the animation left (capped at 2.2s). False
    ///     both before the reveal and on an attempt that is going to fail.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    ///     The item's title, when <c>p</c> carried a title name rather than upgrade details, such as <c>shiny</c> or
    ///     <c>superfast</c>. Look it up in <c>GameData.Titles</c>.
    /// </summary>
    [JsonIgnore]
    public string? Title { get; set; }
}