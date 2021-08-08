using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic for values that can optionally be "false".
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter{T}" />
    public class FalsyConverter<T> : JsonConverter<T?>
    {
        private readonly T? Default;

        public FalsyConverter(T? @default = default) => Default = @default;

        public override T? ReadJson(
            JsonReader reader,
            Type objectType,
            T? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                case JsonToken.Boolean:
                    return Default;
                case JsonToken.String:
                    if (objectType.IsEnum)
                    {
                        var strEnumConverter = new StringEnumConverter();
                        var value = strEnumConverter.ReadJson(reader, objectType, existingValue, serializer);

                        return (T?)value;
                    } else
                        return (T?)serializer.Deserialize(reader, typeof(string));
                case JsonToken.Integer:
                    if (objectType.IsEnum)
                    {
                        var num = serializer.Deserialize<int>(reader);
                        return (T?)Enum.ToObject(typeof(T?), num);
                    } else
                        return (T?)serializer.Deserialize(reader, typeof(int));
                case JsonToken.Float:
                    return (T?)serializer.Deserialize(reader, typeof(double));
                default:
                {
                    var instance = Activator.CreateInstance<T?>();

                    if (instance == null)
                        return Default;

                    serializer.Populate(reader, instance);
                    return instance;
                }
            }
        }

        public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}