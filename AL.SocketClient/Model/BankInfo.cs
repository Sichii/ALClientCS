using System.Collections;
using System.Collections.Generic;
using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents all data for the bank system.
    /// </summary>
    [JsonObject]
    public record BankInfo : IReadOnlyDictionary<BankPack, IReadOnlyList<InventoryItem?>>
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
        internal IReadOnlyDictionary<BankPack, IReadOnlyList<InventoryItem?>> Items { get; init; } =
            new Dictionary<BankPack, IReadOnlyList<InventoryItem?>>();

        public IReadOnlyList<InventoryItem?> this[BankPack bankPack] => Items[bankPack];
        public int Count { get; }
        public IEnumerable<BankPack> Keys => Items.Keys;
        public IEnumerable<IReadOnlyList<InventoryItem?>> Values => Items.Values;

        public bool ContainsKey(BankPack key) => Items.ContainsKey(key);

        public IEnumerator<KeyValuePair<BankPack, IReadOnlyList<InventoryItem?>>> GetEnumerator() =>
            Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetValue(BankPack key, out IReadOnlyList<InventoryItem?> value) =>
            Items.TryGetValue(key, out value!);
    }
}