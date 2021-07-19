using AL.Client.Interfaces;
using AL.Core.Definitions;
using AL.SocketClient.Model;

namespace AL.Client.Model
{
    /// <summary>
    ///     Represents an item within a specific bank, and a specific index within that bank.
    /// </summary>
    /// <seealso cref="IIndexedItem{T}" />
    public record BankedItem : IIndexedItem<InventoryItem>
    {
        /// <summary>
        ///     The bank this item is located in.
        /// </summary>
        public BankPack BankPack { get; init; }

        public int Index { get; init; }
        public InventoryItem Item { get; init; } = null!;
    }
}