using AL.Client.Interfaces;
using AL.Core.Interfaces;

namespace AL.Client.Model
{
    public record IndexedItem : IIndexedItem
    {
        public int Index { get; init; }
        public IItem Item { get; init; }
    }
}