#region
using System;
using System.Collections.Concurrent;
using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endregion

namespace AL.Core.Json.Converters;

/// <summary>
///     A <see cref="StringEnumConverter" /> that degrades an unrecognized wire value to the enum's zero
///     member rather than throwing.
/// </summary>
/// <remarks>
///     The game's string enums are data driven and gain new values without notice. An exception here escapes
///     the whole deserialization, and the socket frame carrying it is discarded along with every other entity
///     batched into it. Losing one field is always better than losing the message.
/// </remarks>
/// <seealso cref="JsonConverter" />
public sealed class TolerantStringEnumConverter : StringEnumConverter
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TolerantStringEnumConverter));

    //tolerance that hides schema drift is worse than the drift, so each unknown value is reported once
    private static readonly ConcurrentDictionary<string, byte> Reported = new();

    public TolerantStringEnumConverter() { }

    /// <summary>
    ///     Mirrors <see cref="StringEnumConverter" />'s naming-strategy constructor so this converter can be
    ///     used as <c>[JsonConverter(typeof(TolerantStringEnumConverter), typeof(SomeNamingStrategy))]</c>.
    /// </summary>
    public TolerantStringEnumConverter(Type namingStrategyType)
        : base(namingStrategyType) { }

    /// <inheritdoc cref="TolerantStringEnumConverter(Type)" />
    public TolerantStringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters)
        : base(namingStrategyType, namingStrategyParameters) { }

    /// <inheritdoc cref="TolerantStringEnumConverter(Type)" />
    public TolerantStringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters, bool allowIntegerValues)
        : base(namingStrategyType, namingStrategyParameters, allowIntegerValues) { }

    public override object? ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        //captured up front because the base converter may advance the reader before it fails
        var raw = reader.Value?.ToString();

        //the base converter validates while it reads, so there is no cheap way to test a value
        //up front without reimplementing it - this is a trust boundary, not control flow
        try
        {
            return base.ReadJson(
                reader,
                objectType,
                existingValue,
                serializer);
        } catch (JsonException)
        {
            //a container token is left unconsumed when the base throws; skipping it keeps the
            //reader aligned so the rest of the payload still parses. no-op on a scalar
            reader.Skip();

            var enumType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            if (Reported.TryAdd($"{enumType.Name}:{raw}", 0))
                Log.Warn($"Unmapped {enumType.Name} value \"{raw}\", defaulted");

            return enumType.IsEnum ? Enum.ToObject(enumType, 0) : null;
        }
    }
}
