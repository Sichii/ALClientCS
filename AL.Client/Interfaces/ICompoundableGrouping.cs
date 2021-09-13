using System.Collections.Generic;
using AL.SocketClient.Model;

namespace AL.Client.Interfaces
{
    /// <summary>
    ///     Represents a grouping of at least 3 items with a shared item name and level.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IReadOnlyList{T}" />
    /// <seealso cref="IIndexer{T}" />
    public interface ICompoundableGrouping<out T> : IReadOnlyList<T> where T: IIndexer<Item>
    {
        /// <summary>
        ///     The level of all items in the group.
        /// </summary>
        int Level { get; }

        /// <summary>
        ///     The name of all items in the group.
        /// </summary>
        string Name { get; }
    }
}