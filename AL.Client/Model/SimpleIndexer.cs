using AL.Client.Interfaces;
using AL.SocketClient.Model;

namespace AL.Client.Model
{
    /// <inheritdoc cref="IIndexer{T}" />
    public class SimpleIndexer : IIndexer<SimpleItem>
    {
        public int Index { get; init; }
        public SimpleItem Item { get; init; } = null!;
    }
}