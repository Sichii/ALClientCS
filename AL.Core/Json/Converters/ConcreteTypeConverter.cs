using System;
using Newtonsoft.Json;

namespace AL.Core.Json.Converters
{
    /// <summary>
    ///     Provides conversion logic for interfaces.
    /// </summary>
    /// <typeparam name="T">A concrete type that implements the interface you want to deserialize.</typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter{T}" />
    public class ConcreteTypeConverter<T> : JsonConverter<T?>
    {
        public override T? ReadJson(
            JsonReader reader,
            Type objectType,
            T? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer) =>
            serializer.Deserialize<T>(reader);

        public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}