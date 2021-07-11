using System;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic for objects that can be represented as either an array or single instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter{T}" />
    public class ArrayOrSingleConverter<T> : JsonConverter<T[]> where T: struct, Enum
    {
        public override T[] ReadJson(
            JsonReader reader,
            Type objectType,
            T[]? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer) =>
            reader.TokenType switch
            {
                JsonToken.Null       => Array.Empty<T>(),
                JsonToken.StartArray => serializer.Deserialize<T[]>(reader) ?? Array.Empty<T>(),
                _                    => new[] { serializer.Deserialize<T>(reader) }
            };

        public override void WriteJson(JsonWriter writer, T[]? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}