using System.Collections.Generic;
using System.Linq;
using AL.Client.Interfaces;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.Core.Interfaces;

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace AL.Client.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<ICompoundableGrouping<T>> CompoundableGroupBy<T>(this IEnumerable<T> enumerable)
            where T: IIndexedItem =>
            enumerable.GroupBy(indexed => indexed.Item.Name)
                .SelectMany(group => group.GroupBy(indexed => indexed.Item.Level))
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

        public static IEnumerable<BankedItem> AsIndexed(
            this IReadOnlyDictionary<BankPack, IReadOnlyList<IItem>> dic) =>
            dic.SelectMany(kvp => kvp.Value.Select((item, index) => new BankedItem
            {
                BankPack = kvp.Key,
                Index = index,
                Item = item
            }));

        public static IEnumerable<IndexedItem> AsIndexed(this IReadOnlyList<IItem> list) =>
            list.Select((item, index) => new IndexedItem
            {
                Index = index,
                Item = item
            });
    }
}