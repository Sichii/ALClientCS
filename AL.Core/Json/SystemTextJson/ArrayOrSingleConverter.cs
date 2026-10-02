#region
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads a field the server sends as either a single enum value or an array of them into a list.
/// </summary>
/// <remarks>
///     Targets <see cref="IReadOnlyList{T}" /> rather than <c>T[]</c>, because a property-level converter must match the
///     declared property type exactly.
/// </remarks>
public sealed class ArrayOrSingleConverter<T> : JsonConverter<IReadOnlyList<T>> where T: struct, Enum
{
    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null maps to an empty list.
    /// </summary>
    public override bool HandleNull => true;

    public override IReadOnlyList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Null => new List<T>(),

            //List<T>, not IReadOnlyList<T>, which would re-enter this converter
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>(),
            _ => new List<T>
            {
                JsonSerializer.Deserialize<T>(ref reader, options)
            }
        };

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<T> value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}