using System;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic for <see cref="DateTime" /> values.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter{T}" />
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(
            JsonReader reader,
            Type objectType,
            DateTime existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if ((reader.TokenType != JsonToken.String) && (reader.TokenType != JsonToken.Date))
                return default;

            var str = serializer.Deserialize<string>(reader);

            if (str == null)
                return default;

            str = str.Replace("-", " ");

            return DateTime.Parse(str);
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}