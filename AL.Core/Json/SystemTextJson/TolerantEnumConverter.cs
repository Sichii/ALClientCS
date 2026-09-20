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
    /// <summary>
    ///     Named from a string, not the type: a closed generic's logger name is its assembly-qualified name, which the
    ///     layout's shortName truncates at the last dot, to "0, Culture=neutral, PublicKeyToken=null]]".
    /// </summary>
    /// <remarks>
    ///     The warning below already carries the enum's name.
    /// </remarks>
    private static readonly ILog Log = LogManager.GetLogger(nameof(TolerantEnumConverter<TEnum>));

    /// <summary>
    ///     Tolerance that hides schema drift is worse than the drift, so each unknown value is reported once.
    /// </summary>
    /// <remarks>
    ///     Per-TEnum is equivalent to a global table here; the key is already prefixed with the enum's name.
    /// </remarks>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<string, byte> Reported = new();

    private readonly bool LowerCase;

    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null degrades to the zero member.
    /// </summary>
    public override bool HandleNull => true;

    public TolerantEnumConverter(bool lowerCase) => LowerCase = lowerCase;

    /// <summary>
    ///     Tolerant parse shared by <see cref="Read" />'s string branch and the dictionary-key path: Newtonsoft's numeric
    ///     fallback (a quoted "17" becomes the underlying value), then degrade to the zero member.
    /// </summary>
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

            //the game data writes some flags as real booleans rather than the strings the enum aliases
            //(GSkill.target is `true`, not `"true"`), so route them through the same aliases
            case JsonTokenType.True:
                return ParseTolerant("true");

            case JsonTokenType.False:
                return ParseTolerant("false");

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

    /// <summary>
    ///     Parses a dictionary key with the same tolerance as a value; unknown keys degrade, where Newtonsoft threw.
    /// </summary>
    /// <remarks>
    ///     Enum-keyed dictionaries route the key through the key type's converter, so without this a tolerant enum used as a
    ///     dictionary key (WeaponType in GClass.mainhand, TradeSlot and Slot in Character.slots) throws
    ///     <see cref="NotSupportedException" />.
    /// </remarks>
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