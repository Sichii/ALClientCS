using AL.Core.Interfaces;

namespace AL.Client.Interfaces
{
    public interface IIndexedItem
    {
        int Index { get; }
        IItem Item { get; }
    }
}