#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using StjConverters = AL.Core.Json.SystemTextJson;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     <inheritdoc cref="IInventoryItem" />
/// </summary>
/// <seealso cref="IInventoryItem" />
public sealed record Item : IInventoryItem
{
    /// <summary>
    ///     True when this item is account-locked and cannot be sent or traded across accounts.
    /// </summary>
    [JsonPropertyName("acl")]
    public bool AccountLocked { get; init; }

    [JsonPropertyName("ach")]
    public string? AchievementName { get; init; }

    [JsonPropertyName("acc")]
    public float AchievementProgress { get; init; }

    /// <summary>
    ///     If populated, the remaining charges on a charge-consuming item.
    /// </summary>
    public int? Charges { get; init; }

    /// <summary>
    ///     If populated, free-form data carried by the item (e.g. the cosmetic/emotion name on a jar, a pet type).
    /// </summary>
    public string? Data { get; init; }

    [JsonConverter(typeof(StjConverters.LenientDateTimeConverter))]
    public DateTime? Expires { get; init; }

    public float Extra { get; init; }

    public float Gift { get; init; }

    [JsonPropertyName("gf")]
    public string? GiveawayFrom { get; init; }

    public float Grace { get; init; }

    public int Level { get; init; }

    [JsonPropertyName("l")]
    public LockType LockType { get; init; }

    /// <summary>
    ///     If populated, the name of the merchant whose mluck produced this drop.
    /// </summary>
    [JsonPropertyName("m")]
    public string? MerchantName { get; init; }

    public string Name { get; init; } = null!;

    /// <remarks>
    ///     <b>This property breaks <see cref="Item" />'s value equality, so do not compare two items with
    ///     <c>==</c>.</b> A record's synthesized <c>Equals</c> folds every property in through
    ///     <c>EqualityComparer&lt;T&gt;.Default</c>, which for a list is <i>reference</i> equality - and two
    ///     deserializations of the same item never share this instance. So <c>==</c> is false for every pair except
    ///     two nulls, silently, and a caller waiting for a frame to report an item it already knows waits forever.
    ///     Compare the fields that identify a slot's contents (name, level, quantity) instead.
    /// </remarks>
    [JsonPropertyName("ps")]
    public IReadOnlyList<string> PossiblePrefixes { get; init; } = new List<string>();

    /// <summary>
    ///     Carries the item's title (e.g. "shiny", "legacy") via <see cref="Model.Prediction.Title" />, or, while this item is
    ///     the placeholder for an in-progress upgrade or compound, the details of that operation. Null when the item has no
    ///     title.
    /// </summary>
    [JsonPropertyName("p")]
    public Prediction? Prediction { get; init; }

    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;

    /// <summary>
    ///     True when this item is rented / offered for rent.
    /// </summary>
    [JsonPropertyName("r")]
    public bool Rented { get; init; }

    /// <summary>
    ///     If populated, a cosmetic sprite/skin override for this item.
    /// </summary>
    public string? Skin { get; init; }

    [JsonPropertyName("stat_type")]
    public ALAttribute StatType { get; init; }

    [JsonPropertyName("v")]
    public string? Volatile { get; init; }
}