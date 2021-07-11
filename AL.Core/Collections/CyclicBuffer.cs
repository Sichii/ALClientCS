using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AL.Core.Collections
{
    /// <summary>
    ///     Represents a fixed size array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    public class CyclicBuffer<T> : IEnumerable<T?>
    {
        private readonly T?[] Items;
        private int Index;
        /// <summary>
        ///     <inheritdoc cref="Array.Length" />
        /// </summary>
        public int Count => Items.Length;


        /// <summary>
        ///     Initializes a new instance of the <see cref="CyclicBuffer{T}" /> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public CyclicBuffer(int size) => Items = new T?[size];

        /// <summary>
        ///     Adds a new item to the buffer, overwriting the oldest item if full.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T? item)
        {
            if (Index >= Count)
                Index = 0;

            Items[Index++] = item;
        }

        /// <inheritdoc />
        public IEnumerator<T?> GetEnumerator() => Items.AsEnumerable().GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}