using AL.Core.Interfaces;

namespace ALClientCS.Interfaces
{
    public interface IIndexedItem
    {
        int Index { get; }
        IItem Item { get; }
    }
}