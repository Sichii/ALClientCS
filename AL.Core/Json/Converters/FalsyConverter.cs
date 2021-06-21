using System;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    public class ObjOrFalseConverter<T> : JsonConverter<T>
    {
        private readonly T Default;

        public ObjOrFalseConverter(T @default = default) => Default = @default;

        public override T ReadJson(
            JsonReader reader,
            Type objectType,
            T existingValue,
            bool hasExistingValue,
            JsonSerializer serializer) =>
            reader.TokenType switch
            {
                JsonToken.Null    => Default,
                JsonToken.Boolean => Default,
                _                 => serializer.Deserialize<T>(reader)
            };

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}