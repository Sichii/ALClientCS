using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace AL.Data
{
    [JsonObject]
    public abstract class DatumBase<T> : IEnumerable<KeyValuePair<string, T>>
    {
        [JsonIgnore]
        public T this[string datumName] => LookupCache.TryGetValue(datumName, out var value) ? value : default;

        [JsonIgnore]
        public T this[Enum @enum] => this[@enum.ToString()];

        [JsonIgnore]
        public IEnumerable<string> Keys => LookupCache.Keys;

        [JsonIgnore]
        public IEnumerable<T> Values => LookupCache.Values;

        private IReadOnlyDictionary<string, T> LookupCache { get; } =
            new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        internal void ConstructCache()
        {
            var cache = (Dictionary<string, T>) LookupCache;

            foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!propertyInfo.CanRead || propertyInfo.GetIndexParameters().Any())
                    continue;

                var jsonIgnoreInfo = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();

                if (jsonIgnoreInfo != null)
                    continue;

                var value = (T) propertyInfo.GetValue(this);

                cache[propertyInfo.Name] = value;

                var jsonPropertyInfo = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();

                if (jsonPropertyInfo?.PropertyName != null)
                    cache[jsonPropertyInfo.PropertyName] = value;
            }
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => LookupCache.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}