using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic for <see cref="IAttributed" /> objects.
    ///     Implements <see cref="Newtonsoft.Json.JsonConverter{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter{T}" />
    public class AttributedObjectConverter<T> : JsonConverter<T?> where T: IAttributed, new()
    {
        public override T? ReadJson(
            JsonReader reader,
            Type objectType,
            T? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var obj = serializer.Deserialize<JObject>(reader);

            if (obj == null)
                return default;

            var attribs = new Dictionary<ALAttribute, float>();
            var value = new T { Attributes = attribs };

            serializer.Populate(obj.CreateReader(), value);

            foreach ((var key, var jToken) in obj)
                if (jToken is { Type: JTokenType.Integer or JTokenType.Float } && EnumHelper.TryParse<ALAttribute>(key, out var attribute))
                    attribs[attribute] = jToken.Value<float>();

            return value;
        }

        public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}