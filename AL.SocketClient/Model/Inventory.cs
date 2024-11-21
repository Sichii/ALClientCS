#region
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents the character's inventory.
/// </summary>
public sealed class Inventory : IReadOnlyList<Item?>
{
    private readonly IReadOnlyList<Item?> Items;
    public int Count => Items.Count;

    [JsonConstructor]
    internal Inventory(IEnumerable<Item>? items)
        => Items = items switch
        {
            null                         => new List<Item>(),
            IReadOnlyList<Item> itemList => itemList,
            _                            => items.ToList()
        };

    public IEnumerator<Item?> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Item? this[int index] => Items[index];

    internal void SetCapacity(int capacity)
    {
        var items = (List<Item?>)Items;

        if (items.Count < capacity)
            items.Capacity = capacity;
    }
}