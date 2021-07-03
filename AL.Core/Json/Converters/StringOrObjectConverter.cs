using System;
using AL.Core.Json.Interfaces;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    public class StringOrObjectConverter<T> : JsonConverter<T> where T: IOptionalObject, new()
    {
        public string PropertyForString { get; set; }

        public StringOrObjectConverter(string propertyForString) => PropertyForString = propertyForString;

        public override T ReadJson(
            JsonReader reader,
            Type objectType,
            T existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            T result;

            switch (reader.TokenType)
            {
                case JsonToken.String:
                    result = new T();
                    var prop = typeof(T).GetProperty(PropertyForString);
                    var propType = prop?.PropertyType;
                    prop?.SetValue(result, serializer.Deserialize(reader, propType));

                    break;
                case JsonToken.StartObject:
                {
                    result = new T
                    {
                        ContainsData = true
                    };

                    serializer.Populate(reader, result);
                    break;
                }
                default:
                    return default;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}