using System.Collections.Generic;

namespace AL.Client.Interfaces
{
    public interface ICompoundableGrouping<out T> : IReadOnlyList<T> where T: IIndexedItem
    {
        int Level { get; }
        string Name { get; }
    }
}