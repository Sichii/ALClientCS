using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    /// <summary>
    /// Represents a merchant that has a stand open somewhere in the world.
    /// </summary>
    /// <seealso cref="ILocation"/>
    public record Merchant : ILocation
    {
        /// <summary>
        /// The level of the merchant.
        /// </summary>
        public int Level { get; init; }

        public string Map { get; init; } = null!;

        /// <summary>
        /// The name of the merchant.
        /// </summary>
        public string Name { get; init; } = null!;
        
        /// <summary>
        /// This is the <see cref="AL.APIClient.Definitions.ServerRegion"/> and <see cref="AL.APIClient.Definitions.ServerId"/>
        /// </summary>
        [JsonProperty("server")]
        public string ServerKey { get; init; } = null!;
        
        /// <summary>
        /// The items this merchant has for sale, or is buying.
        /// </summary>
        public IReadOnlyDictionary<TradeSlot, TradeItem> Slots { get; init; } = new Dictionary<TradeSlot, TradeItem>();
        
        /// <summary>
        /// The type of stand this merchant is using.
        /// </summary>
        public Stand Stand { get; init; }
        
        public float X { get; init; }
        public float Y { get; init; }
    }
}