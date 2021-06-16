using System.Collections.Generic;

namespace AL.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ElementsAt<T>(this IList<T> arr, IEnumerable<int> indexes)
        {
            foreach (var index in indexes)
                yield return arr[index];
        }
    }
}