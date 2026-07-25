using System;
using System.Collections.Generic;
using AL.APIClient.Interfaces;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents an item in a <see cref="Player" />'s <see cref="Player.Slots" />. <br />
    ///     These items can be either <see cref="ITradeItem" />s or an equipped <see cref="IInventoryItem" />.
    /// </summary>
    /// <seealso cref="ITradeItem" />
    /// <seealso cref="IInventoryItem" />
    public record SlotItem : ITradeItem, IInventoryItem
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
        [JsonProperty("b")]
        public bool Buying { get; init; }
        /// <summary>
        ///     If populated, the remaining charges on a charge-consuming item.
        /// </summary>
        public int? Charges { get; init; }
        /// <summary>
        ///     If populated, free-form data carried by the item (e.g. a cosmetic/emotion name or pet type).
        /// </summary>
        public string? Data { get; init; }
        [JsonProperty, JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime? Expires { get; init; }
        public float Extra { get; init; }
        public float Gift { get; init; }
        [JsonProperty("gf")]
        public string? GiveawayFrom { get; init; }
        [JsonProperty("giveaway")]
        public float? GiveawayMins { get; init; }
        [JsonProperty("list")]
        public IReadOnlyList<string>? GiveawayParticipants { get; init; }
        public float Grace { get; init; }
 #pragma warning disable 8766
        /// <summary>
        ///     If populated, this is an <see cref="ITradeItem" />. <br />
        ///     <inheritdoc cref="ITradeItem.Id" />
        /// </summary>
        [JsonProperty("rid")]
        public string? Id { get; init; }
 #pragma warning restore 8766
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
        public long Price { get; init; }
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
}