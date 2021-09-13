using AL.Client.Interfaces;
using AL.SocketClient.Model;

namespace AL.Client.Model
{
    /// <seealso cref="IIndexer{T}" />
    /// <inheritdoc cref="IIndexer{T}" />
    public record InventoryIndexer : IIndexer<Item>
    {
        public int Index { get; init; }
        public Item Item { get; init; } = null!;
    }
}