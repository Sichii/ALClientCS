#region
using System.Reflection;
using System.Text.Json.Serialization;
using AL.Core.Helpers;
#endregion

namespace AL.Data;

/// <summary>
///     Provides dictionary-like access to contained properties.
/// </summary>
/// <typeparam name="T">
///     The type of each entry.
/// </typeparam>
public abstract class DatumBase<T>
{
    private IReadOnlyDictionary<string, T> LookupCache { get; set; } = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Every entry, keyed by name.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, T> Entries => LookupCache;

    /// <summary>Gets all property names.</summary>
    [JsonIgnore]
    public IEnumerable<string> Keys => LookupCache.Keys;

    /// <summary>Gets all property values.</summary>
    [JsonIgnore]
    public IEnumerable<T> Values => LookupCache.Values;

    /// <summary>
    ///     Adds an entry by swapping in a copy of the table, so a reader without a lock keeps a consistent one.
    /// </summary>
    /// <param name="key">
    ///     The entry's name.
    /// </param>
    /// <param name="value">
    ///     The entry.
    /// </param>
    internal void Add(string key, T value)
        => LookupCache = new Dictionary<string, T>(LookupCache, StringComparer.OrdinalIgnoreCase)
        {
            [key] = value
        };

    internal virtual void BuildLookupTable()
    {
        var cache = (Dictionary<string, T>)LookupCache;

        foreach (var propertyInfo in GetType()
                     .GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!propertyInfo.CanRead
                || (propertyInfo.GetIndexParameters()
                                .Length
                    != 0))
                continue;

            var jsonIgnoreInfo = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();

            if (jsonIgnoreInfo != null)
                continue;

            var value = (T?)propertyInfo.GetValue(this);

            //a datum property is null when its key is absent from the payload
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (value == null)
                continue;

            //the wire name goes in first so it is the key the entry keeps; the cache is case-insensitive
            var jsonPropertyNameInfo = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();

            if (jsonPropertyNameInfo != null)
                cache[jsonPropertyNameInfo.Name] = value;

            //a distinct key only for the wire names that differ from the CLR name by more than case
            if (!cache.ContainsKey(propertyInfo.Name))
                cache[propertyInfo.Name] = value;
        }
    }

    /// <summary>Allows using a string to access properties.</summary>
    /// <param name="datumName">The property's wire or CLR name.</param>
    [JsonIgnore]
    public T? this[string datumName] => LookupCache.TryGetValue(datumName, out var value) ? value : default;

    /// <summary>
    ///     Allows using string representation of an enum to access properties.
    /// </summary>
    /// <param name="enum">
    ///     An enum value whose name is the property's name.
    /// </param>
    [JsonIgnore]
    public T? this[Enum @enum] => this[EnumHelper.ToString(@enum)];

    internal void Remove(string key)
    {
        var copy = new Dictionary<string, T>(LookupCache, StringComparer.OrdinalIgnoreCase);
        copy.Remove(key);
        LookupCache = copy;
    }
}