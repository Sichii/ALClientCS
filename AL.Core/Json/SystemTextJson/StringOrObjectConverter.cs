#region
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Reads a field the server sends as either a bare string or a full object. A string fills the property named at
///     construction; an object binds normally and sets <see cref="IOptionalObject.ContainsData" />.
/// </summary>
/// <remarks>
///     Register it through <see cref="StringOrObjectConverterFactory" />, so the object branch can exclude it.
/// </remarks>
public sealed class StringOrObjectConverter<T> : JsonConverter<T?> where T: class, IOptionalObject, new()
{
    private readonly PropertyInfo? StringProperty;

    public StringOrObjectConverter(string stringPropertyName) => StringProperty = typeof(T).GetProperty(stringPropertyName);

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var scalar = new T();

                if (StringProperty is not null)
                    StringProperty.SetValue(scalar, JsonSerializer.Deserialize(ref reader, StringProperty.PropertyType, options));
                else
                    reader.Skip();

                return scalar;

            case JsonTokenType.StartObject:
                var node = JsonNode.Parse(ref reader);
                var value = node?.Deserialize<T>(RecursionSafeOptions.Without(options, typeof(StringOrObjectConverter<T>))) ?? new T();
                value.ContainsData = true;

                return value;

            default:
                reader.Skip();

                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) => throw new NotSupportedException();
}

/// <summary>
///     Registers <see cref="StringOrObjectConverter{T}" /> for every type marked
///     <see cref="JsonStringOrObjectAttribute" />, taking the string property's name from it.
/// </summary>
/// <remarks>
///     It closes the generic at runtime, so it covers socket and API types without referencing them.
/// </remarks>
public sealed class StringOrObjectConverterFactory : JsonConverterFactory, IExcludingConverterFactory
{
    /// <summary>
    ///     The cached answer to whether a type carries <see cref="JsonStringOrObjectAttribute" />, shared across every
    ///     <see cref="Excluding" /> copy.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> Marked = new();

    private readonly Type? Excluded;

    public StringOrObjectConverterFactory() { }

    private StringOrObjectConverterFactory(Type excluded) => Excluded = excluded;

    /// <inheritdoc />
    public JsonConverterFactory Excluding(Type type) => new StringOrObjectConverterFactory(type);

    public override bool CanConvert(Type typeToConvert)
        => (typeToConvert != Excluded)
           && Marked.GetOrAdd(typeToConvert, type => type.GetCustomAttribute<JsonStringOrObjectAttribute>() is not null);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var attribute = typeToConvert.GetCustomAttribute<JsonStringOrObjectAttribute>()!;

        return (JsonConverter)Activator.CreateInstance(
            typeof(StringOrObjectConverter<>).MakeGenericType(typeToConvert),
            attribute.PropertyName)!;
    }
}