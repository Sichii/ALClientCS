#region
using System;
using AL.Core.Json.Interfaces;
using Newtonsoft.Json;
#endregion

namespace AL.Core.Json.Converters;

/// <summary>
///     Provides conversion logic for objects that can optionally be strings. Implements <see cref="JsonConverter" />
/// </summary>
/// <typeparam name="T">
/// </typeparam>
/// <seealso cref="JsonConverter" />
public sealed class StringOrObjectConverter<T> : JsonConverter<T?> where T: IOptionalObject, new()
{
    public string PropertyForString { get; }

    public StringOrObjectConverter(string propertyForString) => PropertyForString = propertyForString;

    public override T? ReadJson(
        JsonReader reader,
        Type objectType,
        T? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        T? result;

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

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer) => throw new NotImplementedException();
}