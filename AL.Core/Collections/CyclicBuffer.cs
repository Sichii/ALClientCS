using System.Collections;
using System.Collections.Generic;

namespace AL.Core.Collections
{
    public class CyclicBuffer<T> : IEnumerable<T>
    {
        private readonly T[] Items;
        public int Count => Items.Length;
        private int Index;

        public CyclicBuffer(int size) => Items = new T[size];

        public void Add(T item)
        {
            if (Index >= Count)
                Index = 0;
            
            Items[Index++] = item;
        }

        public IEnumerator<T> GetEnumerator() => throw new System.NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}