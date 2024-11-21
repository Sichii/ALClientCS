#region
using AL.Client.Interfaces;
using AL.SocketClient.Model;
#endregion

namespace AL.Client.Model;

/// <inheritdoc cref="IIndexer{T}" />
public sealed class SimpleIndexer : IIndexer<SimpleItem>
{
    public int Index { get; init; }
    public SimpleItem Item { get; init; } = null!;
}