#region
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Helpers;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Handles a value the server may send as literal <c>false</c> (JavaScript's <c>a &amp;&amp; a</c> idiom):
///     any falsy token maps to the configured default; anything else deserializes as <typeparamref name="T" />.
///     The System.Text.Json replacement for the Newtonsoft <c>FalsyConverter</c>. It targets the non-nullable
///     value type (its call sites are non-nullable — <c>int StackSize</c>, <c>Stand</c>), so <c>HandleNull</c>
///     routes a null token here instead of System.Text.Json's nullable wrapper swallowing it. Its default cannot
///     be passed through a System.Text.Json <c>[JsonConverter]</c> attribute, so apply it via a parameterless
///     subclass on the property (int case) or on the enum type (enum case).
/// </summary>
public sealed class FalsyConverter<T> : JsonConverter<T>
{
    private readonly T Default;

    public FalsyConverter(T @default) => Default = @default;

    //null/true/false are all "falsy" and map to Default, so null must reach Read (STJ would otherwise throw)
    public override bool HandleNull => true;

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var underlying = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
            case JsonTokenType.True:
            case JsonTokenType.False:
                return Default;

            //an unrecognized enum name degrades to Default (Newtonsoft's tolerant zero, which equals the Default
            //at every current call site) instead of escaping and taking the whole frame with it
            case JsonTokenType.String when underlying.IsEnum:
                return EnumHelper.TryParse(underlying, reader.GetString(), out var enumValue) ? (T)enumValue! : Default;

            case JsonTokenType.Number when underlying.IsEnum:
                return (T)Enum.ToObject(underlying, reader.GetInt64());

            case JsonTokenType.StartObject:
            case JsonTokenType.StartArray:
                //Newtonsoft's default branch new'd up T and populated it, which for its value-type call sites
                //(int/enum) collapsed to the default; mirror that (and never throw) rather than bind an object
                reader.Skip();

                return Default;

            default:
                //a non-falsy scalar: deserialize T normally, dropping this converter so it cannot re-enter
                return JsonSerializer.Deserialize<T>(ref reader, RecursionSafeOptions.Without(options, typeof(FalsyConverter<T>)))!;
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotSupportedException();
}
