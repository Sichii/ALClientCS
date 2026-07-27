#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Normalizes a field the server sends as either a single enum value or an array of them into a list. The
///     System.Text.Json replacement for the Newtonsoft
///     <c>
///         ArrayOrSingleConverter
///     </c>
///     . Targets <see cref="IReadOnlyList{T}" /> (not
///     <c>
///         T[]
///     </c>
///     ) so it matches the declared property type when applied as a property-level converter — System.Text.Json, unlike
///     Newtonsoft, requires an exact converter/member type match.
/// </summary>
public sealed class ArrayOrSingleConverter<T> : JsonConverter<IReadOnlyList<T>> where T: struct, Enum
{
    //a JSON null must reach Read so it maps to an empty list instead of a null reference
    public override bool HandleNull => true;

    public override IReadOnlyList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Null => new List<T>(),

            //deserialize List<T>, NOT IReadOnlyList<T>: this converter handles IReadOnlyList<T>, so deserializing
            //that type would re-enter it; List<T> is a distinct type that resolves the default collection converter
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>(),
            _ => new List<T>
            {
                JsonSerializer.Deserialize<T>(ref reader, options)
            }
        };

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}