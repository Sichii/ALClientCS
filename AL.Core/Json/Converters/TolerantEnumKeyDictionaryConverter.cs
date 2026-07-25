#region
using System;
using System.Collections.Concurrent;
using AL.Core.Helpers;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace AL.Core.Json.Converters;

/// <summary>
///     Reads an object into a dictionary keyed by <typeparamref name="TKey" />, skipping any key that does not
///     map to a known member instead of failing the whole payload.
/// </summary>
/// <typeparam name="TKey">
///     An enum type. Parsed with <see cref="EnumHelper" /> so <see cref="System.Runtime.Serialization.EnumMemberAttribute" />
///     names are honoured.
/// </typeparam>
/// <typeparam name="TValue">
///     The value type of the dictionary.
/// </typeparam>
/// <remarks>
///     Newtonsoft resolves dictionary keys with its own enum parser and never consults a value converter, so
///     <see cref="TolerantStringEnumConverter" /> cannot help here. One unrecognized key - a new monster
///     ability, a new buff - would otherwise discard every entity in the same frame.
/// </remarks>
/// <seealso cref="JsonConverter" />
public sealed class TolerantEnumKeyDictionaryConverter<TKey, TValue> : JsonConverter<ConcurrentDictionary<TKey, TValue>>
    where TKey: struct, Enum
    where TValue: notnull
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TolerantEnumKeyDictionaryConverter<TKey, TValue>));

    //tolerance that hides schema drift is worse than the drift, so each unknown key is reported once
    private static readonly ConcurrentDictionary<string, byte> Reported = new();

    public override ConcurrentDictionary<TKey, TValue>? ReadJson(
        JsonReader reader,
        Type objectType,
        ConcurrentDictionary<TKey, TValue>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var result = new ConcurrentDictionary<TKey, TValue>();

        if (reader.TokenType == JsonToken.Null)
            return result;

        var obj = serializer.Deserialize<JObject>(reader);

        if (obj == null)
            return result;

        foreach (var property in obj.Properties())
        {
            if (!EnumHelper.TryParse<TKey>(property.Name, out var key))
            {
                if (Reported.TryAdd($"{typeof(TKey).Name}:{property.Name}", 0))
                    Log.Warn($"Unmapped {typeof(TKey).Name} key \"{property.Name}\", skipped");

                continue;
            }

            var value = property.Value.ToObject<TValue>(serializer);

            if (value != null)
                result[key] = value;
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, ConcurrentDictionary<TKey, TValue>? value, JsonSerializer serializer)
        => throw new NotImplementedException();
}
