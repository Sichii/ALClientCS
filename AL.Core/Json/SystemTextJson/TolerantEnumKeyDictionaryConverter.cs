#region
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Helpers;
using Common.Logging;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads an object into a dictionary keyed by <typeparamref name="TKey" />, skipping any key that does not map to a
///     known member instead of failing the whole payload. The System.Text.Json replacement for the Newtonsoft
///     <c>
///         TolerantEnumKeyDictionaryConverter
///     </c>
///     .
/// </summary>
/// <remarks>
///     System.Text.Json resolves dictionary keys with a key converter that throws on an unparseable key, so a
///     whole-dictionary converter is required to skip-and-continue. One unrecognized key - a new monster ability, a new
///     buff - would otherwise discard every entity in the same frame.
/// </remarks>
public sealed class TolerantEnumKeyDictionaryConverter<TKey, TValue> : JsonConverter<ConcurrentDictionary<TKey, TValue>>
    where TKey: struct, Enum
    where TValue: notnull
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TolerantEnumKeyDictionaryConverter<TKey, TValue>));

    //tolerance that hides schema drift is worse than the drift, so each unknown key is reported once per closed
    //generic - a TKey shared by two TValues can warn twice, which is cheaper than a global table to dedupe it
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<string, byte> Reported = new();

    //a JSON null must reach Read so it maps to an empty dictionary instead of a null reference
    public override bool HandleNull => true;

    public override ConcurrentDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new ConcurrentDictionary<TKey, TValue>();

        if (reader.TokenType == JsonTokenType.Null)
            return result;

        var obj = JsonNode.Parse(ref reader)
                          ?.AsObject();

        if (obj is null)
            return result;

        foreach ((var name, var node) in obj)
        {
            if (!EnumHelper.TryParse<TKey>(name, out var key))
            {
                if (Reported.TryAdd($"{typeof(TKey).Name}:{name}", 0))
                    Log.Warn($"Unmapped {typeof(TKey).Name} key \"{name}\", skipped");

                continue;
            }

            //a null value token is skipped (the null filter), matching Newtonsoft's `value != null` guard
            if (node is not null && node.Deserialize<TValue>(options) is { } value)
                result[key] = value;
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, ConcurrentDictionary<TKey, TValue> value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}