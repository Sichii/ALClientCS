using System;
using System.Collections.Generic;

namespace AL.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static TResult? MaxBy<TResult, TSelected>(this IEnumerable<TResult?> source, Func<TResult, TSelected> selectorFunc)
            where TSelected: IComparable<TSelected>
        {
            TResult? max = default;

            foreach (var item in source)
                if ((item != null) && ((max == null) || (selectorFunc(max).CompareTo(selectorFunc(item)) <= 0)))
                    max = item;

            return max;
        }

        public static TResult? MinBy<TResult, TSelected>(this IEnumerable<TResult> source, Func<TResult, TSelected> selectorFunc)
            where TSelected: IComparable<TSelected>
        {
            TResult? min = default;

            foreach (var item in source)
                if ((item != null) && ((min == null) || (selectorFunc(min).CompareTo(selectorFunc(item)) >= 0)))
                    min = item;

            return min;
        }

        public static IEnumerable<T> RecursiveSelectMany<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (var innerItem in source)
            {
                yield return innerItem;

                foreach (var recursiveItem in selector(innerItem).RecursiveSelectMany(selector))
                    yield return recursiveItem;
            }
        }
    }
}