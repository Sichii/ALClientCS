#region
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Helpers;
using Common.Logging;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     A per-enum converter that degrades an unrecognized wire value to the enum's zero member rather than throwing, so
///     one unknown value never discards the whole socket frame. The System.Text.Json replacement for the Newtonsoft
///     <c>
///         TolerantStringEnumConverter
///     </c>
///     ; produced by <see cref="TolerantStringEnumConverterFactory" />.
/// </summary>
/// <typeparam name="TEnum">
///     An enum type. Parsed with <see cref="EnumHelper" /> so
///     <see cref="System.Runtime.Serialization.EnumMemberAttribute" /> aliases are honoured (case-insensitively).
/// </typeparam>
public sealed class TolerantEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum: struct, Enum
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(TolerantEnumConverter<TEnum>));

    //tolerance that hides schema drift is worse than the drift, so each unknown value is reported once.
    //Per-TEnum is equivalent to a global table here - the key is already prefixed with the enum's name.
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<string, byte> Reported = new();

    private readonly bool LowerCase;

    //a JSON null must reach Read so it degrades to the zero member instead of throwing (value type)
    public override bool HandleNull => true;

    public TolerantEnumConverter(bool lowerCase) => LowerCase = lowerCase;

    //shared by Read's string branch and the dictionary-key path: tolerant parse, then Newtonsoft's numeric
    //fallback (quoted "17" -> underlying value), then degrade to the zero member
    private static TEnum ParseTolerant(string? raw)
    {
        if (EnumHelper.TryParse<TEnum>(raw, out var parsed))
            return parsed;

        if (long.TryParse(
                raw,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var underlying))
            return (TEnum)Enum.ToObject(typeof(TEnum), underlying);

        if (Reported.TryAdd($"{typeof(TEnum).Name}:{raw}", 0))
            Log.Warn($"Unmapped {typeof(TEnum).Name} value \"{raw}\", defaulted");

        return default;
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return ParseTolerant(reader.GetString());

            //Newtonsoft's StringEnumConverter allows integer values by default; preserve that path
            case JsonTokenType.Number when reader.TryGetInt64(out var num):
                return (TEnum)Enum.ToObject(typeof(TEnum), num);

            case JsonTokenType.Null:
                return default;

            default:
                //a container token is left on the reader; skipping it keeps the rest of the payload aligned
                var token = reader.TokenType;
                reader.Skip();

                if (Reported.TryAdd($"{typeof(TEnum).Name}:<{token}>", 0))
                    Log.Warn($"Unmapped {typeof(TEnum).Name} token {token}, defaulted");

                return default;
        }
    }

    //enum-keyed dictionaries route the KEY through the key type's converter; without these a tolerant enum
    //used as a dictionary key (WeaponType in GClass.mainhand, TradeSlot/Slot in Character.slots) throws
    //NotSupportedException. Parse a key with the same tolerance as a value; unknown keys degrade (Newtonsoft
    //threw here - a conscious Phase-5 re-baseline).
    public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ParseTolerant(reader.GetString());

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var name = EnumHelper.ToString(value);

        writer.WriteStringValue(LowerCase ? name.ToLowerInvariant() : name);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var name = EnumHelper.ToString(value);

        writer.WritePropertyName(LowerCase ? name.ToLowerInvariant() : name);
    }
}

/// <summary>
///     Applies <see cref="TolerantEnumConverter{TEnum}" /> to any enum. Use as
///     <c>
///         [JsonConverter(typeof(TolerantStringEnumConverterFactory))]
///     </c>
///     on an enum type. Nullable enum members are handled by System.Text.Json's built-in nullable wrapper over the
///     produced converter.
/// </summary>
public class TolerantStringEnumConverterFactory : JsonConverterFactory
{
    private readonly bool LowerCase;

    public TolerantStringEnumConverterFactory()
        : this(false) { }

    protected TolerantStringEnumConverterFactory(bool lowerCase) => LowerCase = lowerCase;

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)

        //object[] forces the params-array overload; Activator.CreateInstance(type, bool) is the "nonPublic ctor" overload
        => (JsonConverter)Activator.CreateInstance(
            typeof(TolerantEnumConverter<>).MakeGenericType(typeToConvert),
            new object[]
            {
                LowerCase
            })!;
}

/// <summary>
///     The lowercase-emitting variant, for enums whose wire form is the bare lowercase member name (
///     <c>
///         Slot
///     </c>
///     -&gt; "mainhand",
///     <c>
///         TradeSlot
///     </c>
///     -&gt; "trade1",
///     <c>
///         BankPack
///     </c>
///     -&gt; "items0"). Applied as
///     <c>
///         [JsonConverter(typeof(LowerCaseTolerantStringEnumConverterFactory))]
///     </c>
///     on the enum, replacing the Newtonsoft two-arg
///     <c>
///         [JsonConverter(typeof(TolerantStringEnumConverter), typeof(LowerCaseNamingStrategy))]
///     </c>
///     .
/// </summary>
public sealed class LowerCaseTolerantStringEnumConverterFactory : TolerantStringEnumConverterFactory
{
    public LowerCaseTolerantStringEnumConverterFactory()
        : base(true) { }
}

//tolerance is opted into per enum with [JsonConverter(typeof(TolerantStringEnumConverterFactory))] (or the
//LowerCase variant) on the enum itself. Registering either factory in the shared options would make EVERY enum
//tolerant, which Newtonsoft did not do — an unmarked enum keeps System.Text.Json's numeric default.