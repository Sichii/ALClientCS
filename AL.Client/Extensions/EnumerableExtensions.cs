using System;
using System.Collections.Generic;
using System.Linq;
using AL.Client.Interfaces;
using AL.Client.Model;
using AL.SocketClient.Model;

// ReSharper disable ParameterTypeCanBeEnumerable.Global
namespace AL.Client.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for enumerables.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Lazily enumerates an enumeration of <see cref="IIndexedItem{T}" />s and finds groups of items that can be
        ///     compounded.
        /// </summary>
        /// <param name="enumerable">An enumerable of <see cref="IIndexedItem{T}" />s.</param>
        /// <typeparam name="T">A type that inherits from <see cref="IIndexedItem{T}" />.</typeparam>
        /// <returns>
        ///     <see cref="IEnumerable{T}" /> of <see cref="ICompoundableGrouping{T}" /> of <see cref="IIndexedItem{T}" /> <br />
        ///     A lazy enumeration of groups of compoundable items. If there are more than 3 of an item, the group will contains
        ///     all of them.
        /// </returns>
        /// <exception cref="ArgumentNullException">enumerable</exception>
        public static IEnumerable<ICompoundableGrouping<T>> CompoundableGroupBy<T>(this IEnumerable<T> enumerable)
            where T: IIndexedItem<InventoryItem>
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            return enumerable.GroupBy(indexed => $"{indexed.Item.Name}@{indexed.Item.Level}")
                .Where(group => group.Count() >= 3)
                .Select(group =>
                {
                    var arr = group.ToArray();
                    var indexed = arr[0];

                    return new CompoundableGrouping<T>
                    {
                        Name = indexed.Item.Name,
                        Level = indexed.Item.Level,
                        Items = arr
                    };
                });
        }
    }
}