#region
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace AL.Core.Extensions;

public static class EnumerableExtensions
{
    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach ((var index, var item) in source.Index())
            if (predicate(item))
                return index;

        return -1;
    }

    public static IEnumerable<T> RecursiveSelectMany<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
    {
        foreach (var innerItem in source)
        {
            yield return innerItem;

            foreach (var recursiveItem in selector(innerItem)
                         .RecursiveSelectMany(selector))
                yield return recursiveItem;
        }
    }
}