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
/// </typeparam>
public abstract class DatumBase<T>
{
    private IReadOnlyDictionary<string, T> LookupCache { get; } = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Gets the backing lookup as a read-only dictionary for enumeration.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, T> Entries => LookupCache;

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

            //every T is a reference type, and a datum property is null when its key is absent from the payload
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (value == null)
                continue;

            //the wire name goes in first so that it is the key the entry keeps. This cache is case-insensitive, so
            //writing the CLR spelling afterwards updates the value and leaves the original key in place - which is
            //why every accessor read back off these keys used to come out PascalCase
            var jsonPropertyNameInfo = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();

            if (jsonPropertyNameInfo != null)
                cache[jsonPropertyNameInfo.Name] = value;

            //a distinct key only for the wire names that differ from the CLR name by more than case
            if (!cache.ContainsKey(propertyInfo.Name))
                cache[propertyInfo.Name] = value;
        }
    }

    /// <summary>
    ///     Allows using a string to access properties.
    /// </summary>
    /// <param name="datumName">
    /// </param>
    [JsonIgnore]
    public T? this[string datumName] => LookupCache.TryGetValue(datumName, out var value) ? value : default;

    /// <summary>
    ///     Allows using string representation of an enum to access properties.
    /// </summary>
    /// <param name="enum">
    /// </param>
    [JsonIgnore]
    public T? this[Enum @enum] => this[EnumHelper.ToString(@enum)];
}