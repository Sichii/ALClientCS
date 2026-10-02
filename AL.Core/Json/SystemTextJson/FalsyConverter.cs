#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.Core.Helpers;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads a value the server may send as literal <c>false</c> (JavaScript's <c>a &amp;&amp; a</c> idiom): a null or
///     bool token reads as the configured default, and anything else as <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     The default cannot travel on a <c>[JsonConverter]</c> attribute, so apply it through a parameterless subclass such
///     as <see cref="FalsyStackSizeConverter" />.
/// </remarks>
public class FalsyConverter<T> : JsonConverter<T>
{
    private readonly T Default;

    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, because null, true and false are all falsy and map to
    ///     <see cref="Default" />.
    /// </summary>
    public override bool HandleNull => true;

    public FalsyConverter(T @default) => Default = @default;

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var underlying = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
            case JsonTokenType.True:
            case JsonTokenType.False:
                return Default;

            //an unrecognized enum name degrades to Default rather than taking the whole frame with it
            case JsonTokenType.String when underlying.IsEnum:
                return EnumHelper.TryParse(underlying, reader.GetString(), out var enumValue) ? (T)enumValue! : Default;

            case JsonTokenType.Number when underlying.IsEnum:
                return (T)Enum.ToObject(underlying, reader.GetInt64());

            case JsonTokenType.StartObject:
            case JsonTokenType.StartArray:
                //a container degrades to Default rather than binding an object
                reader.Skip();

                return Default;

            default:
                //a non-falsy scalar: deserialize T normally, dropping this converter so it cannot re-enter
                return JsonSerializer.Deserialize<T>(ref reader, RecursionSafeOptions.Without(options, typeof(FalsyConverter<T>)))!;
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotSupportedException();
}

/// <summary>
///     <see cref="FalsyConverter{T}" /> for <c>GItem.StackSize</c> , whose falsy fallback is 1.
/// </summary>
/// <remarks>
///     A bare <c>FalsyConverter&lt;int&gt;</c> would fall back to 0, a plausible stack size that corrupts silently.
/// </remarks>
public sealed class FalsyStackSizeConverter() : FalsyConverter<int>(1);