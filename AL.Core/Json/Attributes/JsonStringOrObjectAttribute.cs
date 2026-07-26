#region
using System;
#endregion

namespace AL.Core.Json.Attributes;

/// <summary>
///     Marks a type the server may send either as a full object or as a bare string, where the string is shorthand for the
///     single property named by <see cref="PropertyName" />.
/// </summary>
/// <remarks>
///     Replaces the Newtonsoft
///     <c>
///         [JsonConverter(typeof(StringOrObjectConverter&lt;T&gt;), nameof(Prop))]
///     </c>
///     form. System.Text.Json's
///     <c>
///         [JsonConverter]
///     </c>
///     attribute cannot carry the property name - it requires a public parameterless constructor - so the parameter has to
///     travel on a marker the factory reads instead.
/// </remarks>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class JsonStringOrObjectAttribute(string propertyName) : Attribute
{
    /// <summary>
    ///     The property a bare string fills.
    /// </summary>
    public string PropertyName { get; } = propertyName;
}