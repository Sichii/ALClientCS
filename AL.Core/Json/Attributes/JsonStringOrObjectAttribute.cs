namespace AL.Core.Json.Attributes;

/// <summary>
///     Marks a type the server may send either as a full object or as a bare string, where the string is shorthand for the
///     single property named by <see cref="PropertyName" />.
/// </summary>
/// <remarks>
///     A <c>[JsonConverter]</c> attribute cannot carry the property name, so
///     <see cref="Json.SystemTextJson.StringOrObjectConverterFactory" /> reads it from this marker.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class JsonStringOrObjectAttribute(string propertyName) : Attribute
{
    /// <summary>The property a bare string fills.</summary>
    public string PropertyName { get; } = propertyName;
}