using System.Collections.Generic;
using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents all data for the bank system.
    /// </summary>
    public record BankInfo
    {
        /// <summary>
        ///     The amount of gold in the bank. (there is only 1 bank that holds gold)
        /// </summary>
        [JsonIgnore]
        public long Gold { get; init; }

        /// <summary>
        ///     A dictionary of banks, and the items contained within each of those banks.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<BankPack, IReadOnlyList<InventoryItem?>> Items { get; init; } =
            new Dictionary<BankPack, IReadOnlyList<InventoryItem?>>();
    }
}