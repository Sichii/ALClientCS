#region
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.Attributes;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Binds a type marked <see cref="JsonForcedObjectAttribute" /> as a named object, which System.Text.Json would
///     otherwise treat as a collection because it implements <see cref="IEnumerable" />.
/// </summary>
/// <remarks>
///     Members bind from their own <c>[JsonPropertyName]</c>, <c>[JsonIgnore]</c> and <c>[JsonInclude]</c> attributes, and
///     each value deserializes through the options so nested converters still apply.
/// </remarks>
public sealed class ForcedObjectConverter<T> : JsonConverter<T> where T: new()
{
    private static readonly (string WireName, Type MemberType, JsonConverter? Converter, Action<T, object?> Set)[] Members = BuildMembers();

    /// <summary>
    ///     The options per outer options instance and member converter, each carrying that one converter so it applies to its
    ///     own member only.
    /// </summary>

    // ReSharper disable StaticMemberInGenericType
    private static readonly ConditionalWeakTable<JsonSerializerOptions, ConcurrentDictionary<JsonConverter, JsonSerializerOptions>>
        MemberOptionsCache = new();

    // ReSharper restore StaticMemberInGenericType

    public override bool HandleNull => true;

    /// <summary>
    ///     Collects the members an object contract would surface: settable public properties and
    ///     <see cref="JsonIncludeAttribute" /> fields, skipping <see cref="JsonIgnoreAttribute" /> and computed get-only
    ///     accessors.
    /// </summary>
    /// <remarks>
    ///     Each member's <see cref="JsonConverterAttribute" /> is applied here too, since the resolver never gets to apply it
    ///     to these <see cref="IEnumerable" /> types.
    /// </remarks>
    /// <returns>
    ///     The wire name, type, converter and setter of each member.
    /// </returns>
    private static (string, Type, JsonConverter?, Action<T, object?>)[] BuildMembers()
    {
        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        var members = new List<(string, Type, JsonConverter?, Action<T, object?>)>();

        //a derived `new` member (Character.Code shadows Player.Code) hides the base one; the walk runs most-derived
        //first, so the first declaration of a name wins
        var shadowed = new HashSet<string>(StringComparer.Ordinal);

        for (var type = typeof(T); type is not null && (type != typeof(object)); type = type.BaseType)
        {
            foreach (var property in type.GetProperties(FLAGS))
            {
                if (!shadowed.Add(property.Name))
                    continue;

                if (property.GetCustomAttribute<JsonIgnoreAttribute>() is not null)
                    continue;

                if (property.GetSetMethod(true) is not { } setter)
                    continue;

                //a non-public setter binds only with [JsonInclude], so EntityBase.In stays unbound
                if (!setter.IsPublic && property.GetCustomAttribute<JsonIncludeAttribute>() is null)
                    continue;

                var wireName = property.GetCustomAttribute<JsonPropertyNameAttribute>()
                                       ?.Name
                               ?? property.Name;
                var converter = CreateMemberConverter(property);
                members.Add((wireName, property.PropertyType, converter, (target, value) => setter.Invoke(target, [value])));
            }

            //a field is never surfaced on its own, so [JsonInclude] is the whole opt-in
            foreach (var field in type.GetFields(FLAGS))
                if (field.GetCustomAttribute<JsonIncludeAttribute>() is not null)
                    members.Add(
                        (field.GetCustomAttribute<JsonPropertyNameAttribute>()
                              ?.Name
                         ?? field.Name, field.FieldType, CreateMemberConverter(field), (target, value) => field.SetValue(target, value)));
        }

        return members.ToArray();
    }

    /// <summary>
    ///     Creates the converter named by the member's own <see cref="JsonConverterAttribute" />.
    /// </summary>
    /// <remarks>
    ///     A converter without a public parameterless constructor throws <see cref="MissingMethodException" /> out of the
    ///     static <see cref="Members" /> initializer.
    /// </remarks>
    /// <param name="member">The property or field.</param>
    /// <returns>
    ///     The converter, or <c>null</c> if the member names none.
    /// </returns>
    private static JsonConverter? CreateMemberConverter(MemberInfo member)
        => member.GetCustomAttribute<JsonConverterAttribute>() is { ConverterType: { } converterType }
            ? (JsonConverter)Activator.CreateInstance(converterType)!
            : null;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonNode.Parse(ref reader) is not JsonObject obj)
            return default;

        var instance = new T();

        foreach ((var wireName, var memberType, var converter, var set) in Members)
            if (TryGet(obj, wireName, out var node) && node is not null)
                set(instance, node.Deserialize(memberType, converter is null ? options : WithConverter(options, converter)));

        //System.Text.Json fires this from its own object converter, which these types never reach; it runs before the
        //attributed converter's harvest, so no OnDeserialized may read Attributes or PresentFields
        (instance as IJsonOnDeserialized)?.OnDeserialized();

        return instance;
    }

    /// <summary>
    ///     Gets the node under <paramref name="wireName" />, matching keys case-insensitively as the shared options do.
    /// </summary>
    /// <param name="obj">The object to search.</param>
    /// <param name="wireName">The key to find.</param>
    /// <param name="node">The node under the key, if found.</param>
    /// <returns>
    ///     <c>true</c> if the key is present; otherwise, <c>false</c>.
    /// </returns>
    private static bool TryGet(JsonObject obj, string wireName, out JsonNode? node)
    {
        foreach ((var key, var value) in obj)
            if (string.Equals(key, wireName, StringComparison.OrdinalIgnoreCase))
            {
                node = value;

                return true;
            }

        node = null;

        return false;
    }

    private static JsonSerializerOptions WithConverter(JsonSerializerOptions ambient, JsonConverter converter)
        => MemberOptionsCache.GetValue(ambient, static _ => new ConcurrentDictionary<JsonConverter, JsonSerializerOptions>())
                             .GetOrAdd(
                                 converter,
                                 static (toApply, from) =>
                                 {
                                     var options = new JsonSerializerOptions(from);
                                     options.Converters.Insert(0, toApply);

                                     return options;
                                 },
                                 ambient);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotSupportedException();
}

/// <summary>
///     Applies <see cref="ForcedObjectConverter{T}" /> to a type that carries <see cref="JsonForcedObjectAttribute" /> (on
///     itself or an implemented interface such as <c>IRectangle</c>), implements <see cref="IEnumerable" />, exposes a
///     parameterless constructor, and is not a positional <c>[JsonArrayIndex]</c> type.
/// </summary>
/// <remarks>
///     Registered last, so the more specific converters claim their types first.
/// </remarks>
public sealed class ForcedObjectConverterFactory : JsonConverterFactory
{
    /// <summary>
    ///     The cached answer to <see cref="IsConvertible" /> per type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> Convertible = new();

    public override bool CanConvert(Type typeToConvert) => Convertible.GetOrAdd(typeToConvert, IsConvertible);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(ForcedObjectConverter<>).MakeGenericType(typeToConvert))!;

    private static bool HasArrayIndex(Type type)
    {
        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        return type.GetMembers(FLAGS)
                   .Any(member => member.GetCustomAttribute<JsonArrayIndexAttribute>() is not null);
    }

    private static bool IsConvertible(Type typeToConvert)
        => !typeToConvert.IsAbstract
           && typeof(IEnumerable).IsAssignableFrom(typeToConvert)
           && typeToConvert.GetConstructor(Type.EmptyTypes) is not null
           && !HasArrayIndex(typeToConvert)
           && IsForcedToObject(typeToConvert);

    /// <summary>
    ///     Determines whether <see cref="JsonForcedObjectAttribute" /> sits on the type or on an interface it implements.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <returns>
    ///     <c>true</c> if the type or one of its interfaces carries the attribute; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsForcedToObject(Type type)
        => type.GetCustomAttribute<JsonForcedObjectAttribute>() is not null
           || type.GetInterfaces()
                  .Any(contract => contract.GetCustomAttribute<JsonForcedObjectAttribute>() is not null);
}