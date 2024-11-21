#region
using System;
using Newtonsoft.Json;
#endregion

namespace AL.Core.Json.Converters;

/// <summary>
///     Provides conversion logic for objects that can be represented as either an array or single instance.
/// </summary>
/// <typeparam name="T">
/// </typeparam>
/// <seealso cref="JsonConverter" />
public sealed class ArrayOrSingleConverter<T> : JsonConverter<T[]> where T: struct, Enum
{
    public override T[] ReadJson(
        JsonReader reader,
        Type objectType,
        T[]? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
        => reader.TokenType switch
        {
            JsonToken.Null       => [],
            JsonToken.StartArray => serializer.Deserialize<T[]>(reader) ?? [],
            _                    => [serializer.Deserialize<T>(reader)]
        };

    public override void WriteJson(JsonWriter writer, T[]? value, JsonSerializer serializer) => throw new NotImplementedException();
}