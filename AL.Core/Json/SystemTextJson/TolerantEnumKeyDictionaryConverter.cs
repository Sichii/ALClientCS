#region
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
///     known member instead of failing the whole payload.
/// </summary>
/// <remarks>
///     One unrecognized key, such as a new monster ability, would otherwise discard every entity in the same frame.
/// </remarks>
public sealed class TolerantEnumKeyDictionaryConverter<TKey, TValue> : JsonConverter<ConcurrentDictionary<TKey, TValue>>
    where TKey: struct, Enum
    where TValue: notnull
{
    /// <summary>
    ///     The logger, named from a string because the layout's shortName truncates a closed generic's assembly-qualified
    ///     name to the tail of the last type argument.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(nameof(TolerantEnumKeyDictionaryConverter<TKey, TValue>));

    /// <summary>
    ///     The unknown keys already warned about, so each is reported once per closed generic.
    /// </summary>

    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<string, byte> Reported = new();

    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null maps to an empty dictionary.
    /// </summary>
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

            //a null value is skipped
            if (node is not null && node.Deserialize<TValue>(options) is { } value)
                result[key] = value;
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, ConcurrentDictionary<TKey, TValue> value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}