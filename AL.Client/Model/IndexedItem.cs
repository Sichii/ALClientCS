using AL.Core.Interfaces;
using ALClientCS.Interfaces;

namespace ALClientCS.Model
{
    public record IndexedItem : IIndexedItem
    {
        public int Index { get; init; }
        public IItem Item { get; init; }
    }
}