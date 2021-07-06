using System.Collections.Generic;
using System.Linq;
using AL.Client.Interfaces;
using AL.Client.Model;

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace AL.Client.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<ICompoundableGrouping<T>> CompoundableGroupBy<T>(this IEnumerable<T> enumerable)
            where T: IIndexedItem =>
            enumerable.GroupBy(indexed => $"{indexed.Item.Name}@{indexed.Item.Level}")
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