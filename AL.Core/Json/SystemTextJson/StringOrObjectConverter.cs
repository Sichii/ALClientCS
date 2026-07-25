#region
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.Interfaces;
using NJson = Newtonsoft.Json;
using NsConverters = AL.Core.Json.Converters;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Handles a field the server sends as either a bare string or a full object. On a string, constructs T and
///     assigns the string to the property named at construction; on an object, populates T normally and marks
///     <see cref="IOptionalObject.ContainsData" />. The System.Text.Json replacement for the Newtonsoft
///     <c>StringOrObjectConverter</c>. Register in the shared options (its property name cannot be passed through
///     a System.Text.Json <c>[JsonConverter]</c> attribute), so the object branch drops this converter to avoid
///     re-entering itself.
/// </summary>
public sealed class StringOrObjectConverter<T> : JsonConverter<T?> where T: class, IOptionalObject, new()
{
    private readonly PropertyInfo? StringProperty;

    public StringOrObjectConverter(string propertyForString) => StringProperty = typeof(T).GetProperty(propertyForString);

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
///     Registers <see cref="StringOrObjectConverter{T}" /> for the types the models mark with the Newtonsoft
///     <c>[JsonConverter(typeof(StringOrObjectConverter&lt;T&gt;), nameof(SomeProperty))]</c> attribute, reading the
///     string-property name from the attribute's second argument. The factory lives in AL.Core yet still covers the
///     socket/API types (it inspects the attribute by reflection and closes the generic at runtime), so no cross-project
///     reference is needed. Transitional: reads Newtonsoft attributes so the models need no per-type wiring before Phase 6.
/// </summary>
public sealed class NewtonsoftStringOrObjectConverterFactory : JsonConverterFactory, IExcludingConverterFactory
{
    private readonly Type? Excluded;

    public NewtonsoftStringOrObjectConverterFactory() { }

    private NewtonsoftStringOrObjectConverterFactory(Type excluded) => Excluded = excluded;

    public override bool CanConvert(Type typeToConvert) => (typeToConvert != Excluded) && (StringOrObjectAttribute(typeToConvert) is not null);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var attribute = StringOrObjectAttribute(typeToConvert)!;
        var propertyName = (string)attribute.ConverterParameters![0];

        return (JsonConverter)Activator.CreateInstance(typeof(StringOrObjectConverter<>).MakeGenericType(typeToConvert), new object[] { propertyName })!;
    }

    // the object branch's inner fill runs under a copy of the options where this factory declines T, so T resolves
    // the default object converter instead of re-entering this converter and recursing until the stack overflows.
    public JsonConverterFactory Excluding(Type type) => new NewtonsoftStringOrObjectConverterFactory(type);

    private static NJson.JsonConverterAttribute? StringOrObjectAttribute(Type type)
    {
        var attribute = type.GetCustomAttribute<NJson.JsonConverterAttribute>();
        var converterType = attribute?.ConverterType;

        return (converterType is not null)
               && converterType.IsGenericType
               && (converterType.GetGenericTypeDefinition() == typeof(NsConverters.StringOrObjectConverter<>))
            ? attribute
            : null;
    }
}
