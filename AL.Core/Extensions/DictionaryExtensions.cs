using System.Collections.Generic;

namespace AL.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static IEnumerable<TValue> TryGetValues<TKey, TValue>(
            this IDictionary<TKey, TValue> dic,
            IEnumerable<TKey> keys)
        {
            foreach(var key in keys)
                if (dic.TryGetValue(key, out var value))
                    yield return value;
        }
    }
}