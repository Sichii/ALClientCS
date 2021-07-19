using AL.Client.Interfaces;
using AL.SocketClient.Model;

namespace AL.Client.Model
{
    /// <seealso cref="IIndexedItem{T}" />
    /// <inheritdoc cref="IIndexedItem{T}" />
    public record IndexedInventoryItem : IIndexedItem<InventoryItem>
    {
        public int Index { get; init; }
        public InventoryItem Item { get; init; } = null!;
    }
}