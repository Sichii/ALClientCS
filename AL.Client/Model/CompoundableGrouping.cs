using System.Collections;
using System.Collections.Generic;
using ALClientCS.Interfaces;

namespace ALClientCS.Model
{
    public record CompoundableGrouping<T> : ICompoundableGrouping<T> where T: IIndexedItem
    {
        public int Level { get; init; }
        public string Name { get; init; }
        internal IReadOnlyList<T> Items { get; init; }

        public T this[int index] => Items[index];
        public int Count => Items.Count;
        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}