namespace AL.Core.Json.Attributes;

/// <summary>
///     Marks a type that serializes as a named object even though it implements
///     <see cref="System.Collections.IEnumerable" />, so <see cref="Json.SystemTextJson.ForcedObjectConverterFactory" />
///     claims it before System.Text.Json can classify it as a collection.
/// </summary>
/// <remarks>
///     On an interface, every implementer inherits it; <see cref="AL.Core.Interfaces.IRectangle" /> carries it for the
///     geometry containers that enumerate their vertices.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class JsonForcedObjectAttribute : Attribute;