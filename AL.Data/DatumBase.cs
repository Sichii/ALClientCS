using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AL.Core.Helpers;
using Newtonsoft.Json;

#nullable enable

namespace AL.Data
{
    /// <summary>
    ///     Provides dictionary-like access to contained properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonObject]
    public abstract class DatumBase<T> : IEnumerable<KeyValuePair<string, T>>
    {
        /// <summary>
        ///     Allows using a string to access properties.
        /// </summary>
        /// <param name="datumName"></param>
        [JsonIgnore]
        public T? this[string datumName] => LookupCache.TryGetValue(datumName, out var value) ? value : default;

        /// <summary>
        ///     Allows using string representation of an enum to access properties.
        /// </summary>
        /// <param name="enum"></param>
        [JsonIgnore]
        public T? this[Enum @enum] => this[EnumHelper.ToString(@enum)];

        /// <summary>
        ///     Gets all property names.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string> Keys => LookupCache.Keys;

        /// <summary>
        ///     Gets all property values.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<T> Values => LookupCache.Values;

        private IReadOnlyDictionary<string, T> LookupCache { get; } = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        internal virtual void ConstructCache()
        {
            var cache = (Dictionary<string, T>)LookupCache;

            foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!propertyInfo.CanRead || propertyInfo.GetIndexParameters().Any())
                    continue;

                var jsonIgnoreInfo = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();

                if (jsonIgnoreInfo != null)
                    continue;

                var value = (T?)propertyInfo.GetValue(this);

                if (value == null)
                    continue;

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