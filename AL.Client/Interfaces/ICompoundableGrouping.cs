using System.Collections.Generic;

namespace ALClientCS.Interfaces
{
    public interface ICompoundableGrouping<out T> : IReadOnlyList<T> where T: IIndexedItem
    {
        int Level { get; }
        string Name { get; }
    }
}