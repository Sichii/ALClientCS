#region
using AL.Client.Interfaces;
using AL.Core.Definitions;
using AL.SocketClient.Model;
#endregion

namespace AL.Client.Model;

/// <summary>
///     Represents an item within a specific bank, and a specific index within that bank.
/// </summary>
/// <seealso cref="IIndexer{T}" />
public sealed record BankIndexer : IIndexer<Item>
{
    /// <summary>
    ///     The bank this item is located in.
    /// </summary>
    public BankPack BankPack { get; init; }

    public int Index { get; init; }
    public Item Item { get; init; } = null!;
}