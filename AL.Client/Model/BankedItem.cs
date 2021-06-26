using AL.Core.Definitions;
using AL.Core.Interfaces;
using ALClientCS.Interfaces;

namespace ALClientCS.Model
{
    public record BankedItem : IIndexedItem
    {
        public BankPack BankPack { get; init; }
        public int Index { get; init; }
        public IItem Item { get; init; }
    }
}