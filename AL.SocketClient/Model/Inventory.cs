#region
using System.Collections;
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

    /// <summary>
    ///     Walks the slots by index rather than handing out the backing list's enumerator.
    /// </summary>
    /// <remarks>
    ///     <see cref="SetPrediction" /> writes a slot in place off the socket thread, which bumps the list's version
    ///     and makes every enumerator a consumer is holding throw. Slots are only ever replaced, never inserted or
    ///     removed, so reading by index sees either the old item or the new one, and neither is a torn read.
    /// </remarks>
    public IEnumerator<Item?> GetEnumerator()
    {
        for (var index = 0; index < Items.Count; index++)
            yield return Items[index];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Item? this[int index] => Items[index];

    internal void SetCapacity(int capacity)
    {
        var items = (List<Item?>)Items;

        if (items.Count < capacity)
            items.Capacity = capacity;
    }

    /// <summary>
    ///     Replaces one slot's <see cref="Item.Prediction" />, leaving the rest of the item alone.
    /// </summary>
    /// <remarks>
    ///     This is the only way an in-progress upgrade or compound's detail reaches the item it belongs to. The server
    ///     publishes it on <c>q_data</c> carrying nothing but the slot number (<c>node/server.js:13240</c>) and never
    ///     folds it into an inventory frame, so without this the placeholder occupying that slot keeps whatever
    ///     prediction it was deserialized with - which for the roll's digits means an empty list for the whole
    ///     operation.
    /// </remarks>
    internal void SetPrediction(int index, Prediction? prediction)
    {
        var items = (List<Item?>)Items;

        //a slot index the server named and this client has not grown into yet, or an empty one - both are ordinary
        //against a frame that raced the inventory, and neither is worth throwing over
        if ((index < 0) || (index >= items.Count) || (items[index] is not { } item))
            return;

        items[index] = item with { Prediction = prediction };
    }
}