#region
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
#endregion

namespace AL.SocketClient.Model;

/// <summary>Represents the character's inventory.</summary>
public sealed class Inventory : IReadOnlyList<Item?>
{
    public IReadOnlyList<Item?> Items { get; }
    public int Count => Items.Count;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Inventory" /> class with no slots as the server sent them.
    /// </summary>
    /// <param name="items">The items in each slot.</param>
    internal Inventory(IReadOnlyList<Item?>? items)
        : this(items, null) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Inventory" /> class.
    /// </summary>
    /// <param name="items">
    ///     The slots, which must be a <c>List&lt;Item?&gt;</c> for <see cref="SetCapacity" /> and <see cref="SetPrediction" />
    ///     to cast back to.
    /// </param>
    /// <param name="sentSlots">
    ///     If populated, each slot as the server sent it, in step with <paramref name="items" />.
    /// </param>
    internal Inventory(IReadOnlyList<Item?>? items, IReadOnlyList<JsonNode?>? sentSlots)
    {
        Items = items ?? new List<Item?>();
        SentSlots = sentSlots ?? [];
    }

    /// <summary>
    ///     Each slot as the server sent it, which a sell or destroy echoes back so the server can refuse a slot that changed.
    /// </summary>
    private readonly IReadOnlyList<JsonNode?> SentSlots;

    /// <summary>
    ///     Gets a slot's item exactly as the server sent it.
    /// </summary>
    /// <param name="index">The inventory slot.</param>
    /// <param name="sentItem">The item's JSON, field order kept.</param>
    /// <returns>
    ///     <c>true</c> if the slot arrived holding an item; otherwise, <c>false</c> .
    /// </returns>
    public bool TryGetSentItem(int index, [NotNullWhen(true)] out JsonNode? sentItem)
    {
        sentItem = null;

        if ((index < 0) || (index >= SentSlots.Count) || SentSlots[index] is not JsonObject slot)
            return false;

        sentItem = slot;

        return true;
    }

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