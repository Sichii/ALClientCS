using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AL.Client.Helpers
{
    internal class SimpleCache<TKey, TValue> : ConcurrentDictionary<TKey, TValue> where TKey: notnull
    {
        private readonly ConcurrentDictionary<TKey, TValue> Cache;
        private readonly Func<TKey, TValue>? ValueFactoryFunc;

        internal SimpleCache(IEqualityComparer<TKey>? comparer = null, Func<TKey, TValue>? valueFactoryFunc = null)
        {
            Cache = comparer != null ? new ConcurrentDictionary<TKey, TValue>(comparer) : new ConcurrentDictionary<TKey, TValue>();

            if (valueFactoryFunc != null)
                ValueFactoryFunc = valueFactoryFunc;
        }

        internal TValue GetOrAdd(TKey key) => ValueFactoryFunc != null
            ? Cache.GetOrAdd(key, ValueFactoryFunc)
            : throw new InvalidOperationException("Configure valueFactory first.");
    }
}