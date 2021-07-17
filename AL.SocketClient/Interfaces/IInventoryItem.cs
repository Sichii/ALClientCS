using System;
using System.Collections.Generic;
using AL.APIClient.Interfaces;
using AL.Core.Definitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.SocketClient.Interfaces
{
    /// <summary>
    ///     Represents an item in the inventory.
    /// </summary>
    /// <seealso cref="ICommonItem" />
    public interface IInventoryItem : ICommonItem
    {
        /// <summary>
        ///     The progress this item has toward's it's achievement.
        /// </summary>
        [JsonProperty("acc")]
        float AchievementProgress { get; init; }

        /// <summary>
        ///     The date/time this item expires. (it will disappear)
        /// </summary>
        [JsonProperty, JsonConverter(typeof(IsoDateTimeConverter))]
        DateTime? Expires { get; init; }

        /// <summary>
        ///     TODO: Something to do with boosters
        /// </summary>
        [JsonProperty]
        float Extra { get; init; }

        /// <summary>
        ///     This item was a gift, and is only worth 1 gold. <br />
        ///     TODO: Is this a number?
        /// </summary>
        [JsonProperty]
        float Gift { get; init; }

        /// <summary>
        ///     If populated, the name of the player that held a giveaway that gave out this item.
        /// </summary>
        [JsonProperty("gf")]
        string? GiveawayFrom { get; init; }

        /// <summary>
        ///     The type of lock on the item.
        /// </summary>
        [JsonProperty("l")]
        LockType LockType { get; init; }

        /// <summary>
        ///     A list of possible prefixes for this item.
        /// </summary>
        [JsonProperty("ps")]
        IReadOnlyList<string> PossiblePrefixes { get; init; }

        /// <summary>
        ///     This item is volatile until this date, if you die to another player you may lose it.
        /// </summary>
        [JsonProperty("v")]
        string? Volatile { get; init; }
    }
}