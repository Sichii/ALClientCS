#region
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.Attributes;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Binds a type marked <see cref="JsonForcedObjectAttribute" /> which System.Text.Json would otherwise treat as a
///     collection because it also implements <see cref="IEnumerable" /> — the geometry containers (e.g.
///     <c>
///         GGeometry
///     </c>
///     ) that implement
///     <c>
///         IRectangle : IEnumerable&lt;IPoint&gt;
///     </c>
///     for bounding-box convenience yet serialize as a named object. It reads the named object and sets each member from
///     the native attributes (
///     <c>
///         [JsonPropertyName]
///     </c>
///     renames,
///     <c>
///         [JsonIgnore]
///     </c>
///     ,
///     <c>
///         [JsonInclude]
///     </c>
///     for non-public setters and fields), deserializing each value through the options so nested converters still apply.
///     The positional
///     <c>
///         [JsonArrayIndex]
///     </c>
///     shapes (GDoor/GTile) are handled by <see cref="ArrayToObjectConverter{T}" /> instead.
/// </summary>
public sealed class ForcedObjectConverter<T> : JsonConverter<T> where T: new()
{
    private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static readonly (string WireName, Type MemberType, JsonConverter? Converter, Action<T, object?> Set)[] Members = BuildMembers();

    // per (ambient options, member converter) cached options carrying that one converter, so a member's
    // property-level converter (AfkConverter on RIP/AFK, the tolerant condition-dict converter) is applied to
    // that member only - never leaked onto a same-typed sibling. Per-T costs nothing: BuildMembers instantiates a
    // fresh converter per member, so the keys never collide across T anyway.
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConditionalWeakTable<JsonSerializerOptions, ConcurrentDictionary<JsonConverter, JsonSerializerOptions>>
        MemberOptionsCache = new();

    public override bool HandleNull => true;

    //settable public properties and any [JsonInclude] fields — the members System.Text.Json's own resolver would
    //surface for an object contract; [JsonIgnore] and computed get-only accessors are skipped. Each member's
    //[JsonConverter] is applied here too, so RIP/AFK (AfkConverter) and Conditions (the tolerant condition-dict
    //converter) bind identically even though the resolver never gets to apply it to these IEnumerable types.
    private static (string, Type, JsonConverter?, Action<T, object?>)[] BuildMembers()
    {
        var members = new List<(string, Type, JsonConverter?, Action<T, object?>)>();

        //a derived `new` member (Character.Code : string shadows Player.Code : bool; likewise MPCost) hides the
        //base one, so only the most-derived declaration binds - matching System.Text.Json's default resolver.
        //The walk runs most-derived first, so the first declaration of a name wins and later (base) ones are skipped.
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

                //a public setter binds by default; a non-public one only when the member opts in with
                //[JsonInclude]. EntityBase.In (public getter, protected setter, no opt-in) therefore stays
                //unbound - the map/in stamp is applied later by ALClient, not at deserialize.
                if (!setter.IsPublic && property.GetCustomAttribute<JsonIncludeAttribute>() is null)
                    continue;

                var wireName = property.GetCustomAttribute<JsonPropertyNameAttribute>()
                                       ?.Name
                               ?? property.Name;
                var converter = MemberConverter(property);
                members.Add((wireName, property.PropertyType, converter, (target, value) => setter.Invoke(target, [value])));
            }

            //a field is never surfaced on its own, so [JsonInclude] is the whole opt-in
            foreach (var field in type.GetFields(FLAGS))
                if (field.GetCustomAttribute<JsonIncludeAttribute>() is not null)
                    members.Add(
                        (field.GetCustomAttribute<JsonPropertyNameAttribute>()
                              ?.Name
                         ?? field.Name, field.FieldType, MemberConverter(field), (target, value) => field.SetValue(target, value)));
        }

        return members.ToArray();
    }

    //the member's own [JsonConverter]. These types never reach the resolver, which would normally apply it, so the
    //binder instantiates it directly. That requires a public parameterless constructor - the same constraint
    //System.Text.Json's own attribute path imposes, so a converter that satisfies [JsonConverter] satisfies this.
    //A converter type that does not (StringOrObjectConverter<T> takes the property name) throws
    //MissingMethodException out of the static Members initializer, which the CLR then caches for the process.
    private static JsonConverter? MemberConverter(MemberInfo member)
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

        //System.Text.Json fires this callback from its own object converter, which these types never reach - a
        //custom converter has to invoke it itself. Every entity binds here, and Player's missing-slot back-fill
        //and Character's inventory sizing both hang off it, so skipping it leaves Slots missing every trade slot
        //the wire omitted (an indexer read then throws) and the inventory unsized.
        //It must run AFTER the member loop (it reads bound members) and it runs BEFORE the outer attributed
        //converter's key-presence marking and attribute harvest, which is the reverse of Newtonsoft's order.
        //Safe only because no OnDeserialized implementation reads Attributes or PresentFields - keep it that way.
        (instance as IJsonOnDeserialized)?.OnDeserialized();

        return instance;
    }

    //match keys case-insensitively, as Newtonsoft did and the shared options request
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
        => MemberOptionsCache.GetValue(ambient, _ => new ConcurrentDictionary<JsonConverter, JsonSerializerOptions>())
                             .GetOrAdd(
                                 converter,
                                 toApply =>
                                 {
                                     var options = new JsonSerializerOptions(ambient);
                                     options.Converters.Insert(0, toApply);

                                     return options;
                                 });

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotSupportedException();
}

/// <summary>
///     Applies <see cref="ForcedObjectConverter{T}" /> to a type that carries <see cref="JsonForcedObjectAttribute" /> (on
///     itself or an implemented interface such as
///     <c>
///         IRectangle
///     </c>
///     ), implements <see cref="IEnumerable" />, exposes a parameterless constructor, and is not a positional
///     <c>
///         [JsonArrayIndex]
///     </c>
///     type. Registered last, so the specific converters (attributed, array-to-object, map-rectangle, …) claim their types
///     first.
/// </summary>
public sealed class ForcedObjectConverterFactory : JsonConverterFactory
{
    private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public override bool CanConvert(Type typeToConvert)
        => !typeToConvert.IsAbstract
           && typeof(IEnumerable).IsAssignableFrom(typeToConvert)
           && typeToConvert.GetConstructor(Type.EmptyTypes) is not null
           && !HasArrayIndex(typeToConvert)
           && ForcedToObject(typeToConvert);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(ForcedObjectConverter<>).MakeGenericType(typeToConvert))!;

    //[JsonForcedObject] on the type or an implemented interface (e.g. IRectangle) forces an object contract
    private static bool ForcedToObject(Type type)
        => type.GetCustomAttribute<JsonForcedObjectAttribute>() is not null
           || type.GetInterfaces()
                  .Any(contract => contract.GetCustomAttribute<JsonForcedObjectAttribute>() is not null);

    private static bool HasArrayIndex(Type type)
        => type.GetMembers(FLAGS)
               .Any(member => member.GetCustomAttribute<JsonArrayIndexAttribute>() is not null);
}