using System;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic specifically for Player.AFK
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter{T}" />
    public class AfkConverter : JsonConverter<bool>
    {
        public override bool ReadJson(
            JsonReader reader,
            Type objectType,
            bool existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return false;

            return (reader.TokenType == JsonToken.String) || serializer.Deserialize<bool>(reader);
        }

        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}