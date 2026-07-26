#region
using System.Collections;
using System.Collections.Generic;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents the character's inventory.
/// </summary>
public sealed class Inventory : IReadOnlyList<Item?>
{
    public IReadOnlyList<Item?> Items { get; }
    public int Count => Items.Count;

    // Backing stays List<Item?> so SetCapacity's downcast holds; the deserializer supplies a List.
    internal Inventory(IReadOnlyList<Item?>? items) => Items = items ?? new List<Item?>();

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