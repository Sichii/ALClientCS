#region
using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    [JsonProperty("acl")]
    public bool AccountLocked { get; init; }

    [JsonProperty("ach")]
    public string? AchievementName { get; init; }

    [JsonProperty("acc")]
    public float AchievementProgress { get; init; }

    /// <summary>
    ///     If populated, the remaining charges on a charge-consuming item.
    /// </summary>
    public int? Charges { get; init; }

    /// <summary>
    ///     If populated, free-form data carried by the item (e.g. the cosmetic/emotion name on a jar, a pet type).
    /// </summary>
    public string? Data { get; init; }

    [JsonProperty, JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Expires { get; init; }

    public float Extra { get; init; }

    public float Gift { get; init; }

    [JsonProperty("gf")]
    public string? GiveawayFrom { get; init; }

    public float Grace { get; init; }

    public int Level { get; init; }

    [JsonProperty("l")]
    public LockType LockType { get; init; }

    /// <summary>
    ///     If populated, the name of the merchant whose mluck produced this drop.
    /// </summary>
    [JsonProperty("m")]
    public string? MerchantName { get; init; }

    public string Name { get; init; } = null!;

    [JsonProperty("ps")]
    public IReadOnlyList<string> PossiblePrefixes { get; init; } = new List<string>();

    /// <summary>
    ///     Carries the item's title (e.g. "shiny", "legacy") via <see cref="Model.Prediction.Title" />, or,
    ///     while this item is the placeholder for an in-progress upgrade or compound, the details of that
    ///     operation. Null when the item has no title.
    /// </summary>
    [JsonProperty("p")]
    public Prediction? Prediction { get; init; }

    [JsonProperty("q")]
    public int Quantity { get; init; } = 1;

    /// <summary>
    ///     True when this item is rented / offered for rent.
    /// </summary>
    [JsonProperty("r")]
    public bool Rented { get; init; }

    /// <summary>
    ///     If populated, a cosmetic sprite/skin override for this item.
    /// </summary>
    public string? Skin { get; init; }

    [JsonProperty("stat_type")]
    public ALAttribute StatType { get; init; }

    [JsonProperty("v")]
    public string? Volatile { get; init; }
}