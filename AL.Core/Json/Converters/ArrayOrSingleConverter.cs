using System;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    public class ArrayOrSingleConverter<T> : JsonConverter<T[]> where T: struct, Enum
    {
        public override T[] ReadJson(
            JsonReader reader,
            Type objectType,
            T[] existingValue,
            bool hasExistingValue,
            JsonSerializer serializer) =>
            reader.TokenType switch
            {
                JsonToken.Null       => default,
                JsonToken.StartArray => serializer.Deserialize<T[]>(reader),
                _                    => new[] { serializer.Deserialize<T>(reader) }
            };

        public override void WriteJson(JsonWriter writer, T[] value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}