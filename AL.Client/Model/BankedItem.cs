using AL.Client.Interfaces;
using AL.Core.Definitions;
using AL.SocketClient.Interfaces;

namespace AL.Client.Model
{
    public record BankedItem : IIndexedItem
    {
        public BankPack BankPack { get; init; }
        public int Index { get; init; }
        public IItem Item { get; init; }
    }
}