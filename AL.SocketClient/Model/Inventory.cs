#region
using System.Collections;
#endregion

namespace AL.SocketClient.Model;

/// <summary>Represents the character's inventory.</summary>
public sealed class Inventory : IReadOnlyList<Item?>
{
    public IReadOnlyList<Item?> Items { get; }
    public int Count => Items.Count;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Inventory" /> class.
    /// </summary>
    /// <param name="items">
    ///     The slots, which must be a <c>List&lt;Item?&gt;</c> for <see cref="SetCapacity" /> and <see cref="SetPrediction" />
    ///     to cast back to.
    /// </param>
    internal Inventory(IReadOnlyList<Item?>? items) => Items = items ?? new List<Item?>();

    /// <summary>
    ///     Walks the slots by index rather than handing out the backing list's enumerator.
    /// </summary>
    /// <returns>The item in each slot, in slot order.</returns>
    /// <remarks>
    ///     <see cref="SetPrediction" /> replaces slots off the socket thread, which would invalidate a list enumerator.
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
    /// <param name="index">The inventory slot.</param>
    /// <param name="prediction">The new prediction.</param>
    /// <remarks>
    ///     The server publishes an in-progress upgrade or compound on <c>q_data</c> carrying only the slot number, and never
    ///     folds it into an inventory frame.
    /// </remarks>
    internal void SetPrediction(int index, Prediction? prediction)
    {
        var items = (List<Item?>)Items;

        //an out-of-range or empty slot is ordinary against a frame that raced the inventory
        if ((index < 0) || (index >= items.Count) || items[index] is not { } item)
            return;

        items[index] = item with
        {
            Prediction = prediction
        };
    }
}