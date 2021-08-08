using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents the character's inventory.
    /// </summary>
    public class Inventory : IReadOnlyList<InventoryItem?>
    {
        private readonly IReadOnlyList<InventoryItem?> Items;

        public InventoryItem? this[int index] => Items[index];
        public int Count => Items.Count;

        [JsonConstructor]
        internal Inventory(IEnumerable<InventoryItem>? items) =>
            Items = items switch
            {
                null                                  => new List<InventoryItem>(),
                IReadOnlyList<InventoryItem> itemList => itemList,
                _                                     => items.ToList()
            };

        public IEnumerator<InventoryItem?> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void SetCapacity(int capacity)
        {
            var items = (List<InventoryItem?>)Items;

            if (items.Count < capacity)
                items.Capacity = capacity;
        }
    }
}