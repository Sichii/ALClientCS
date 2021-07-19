using AL.Client.Interfaces;
using AL.SocketClient.Model;

namespace AL.Client.Model
{
    /// <inheritdoc cref="IIndexedItem{T}" />
    public class IndexedSimpleItem : IIndexedItem<SimpleItem>
    {
        public int Index { get; init; }
        public SimpleItem Item { get; init; } = null!;
    }
}