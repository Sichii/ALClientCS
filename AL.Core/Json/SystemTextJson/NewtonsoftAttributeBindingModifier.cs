#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using NJson = Newtonsoft.Json;
using NsConverters = AL.Core.Json.Converters;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     A <see cref="DefaultJsonTypeInfoResolver" /> modifier that drives System.Text.Json binding from the models'
///     existing Newtonsoft <c>[JsonProperty]</c> / <c>[JsonIgnore]</c> attributes, so the migration needs no
///     per-member System.Text.Json attributes while both engines run side by side. It applies exactly what
///     Newtonsoft binds — renames, non-public setters, private <c>[JsonProperty]</c> members — and nothing it
///     does not (a member without <c>[JsonProperty]</c> and no public setter, e.g. <c>EntityBase.In</c>, stays
///     unbound). Transitional: Phase 6 replaces this with native <c>[JsonPropertyName]</c>/<c>[JsonInclude]</c>.
/// </summary>
public static class NewtonsoftAttributeBindingModifier
{
    public static void Modify(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        var type = typeInfo.Type;
        var toRemove = new List<JsonPropertyInfo>();

        // (1) members System.Text.Json already surfaced (public properties): apply rename, enable a non-public
        //     setter, or drop the member if Newtonsoft ignores it.
        foreach (var jsonProperty in typeInfo.Properties)
        {
            if (jsonProperty.AttributeProvider is not MemberInfo member)
                continue;

            if (member.GetCustomAttribute<NJson.JsonIgnoreAttribute>() is not null)
            {
                toRemove.Add(jsonProperty);

                continue;
            }

            if (member.GetCustomAttribute<NJson.JsonPropertyAttribute>()?.PropertyName is { } wireName)
                jsonProperty.Name = wireName;

            //a public property with a non-public setter is get-only to System.Text.Json until Set is supplied
            if ((jsonProperty.Set is null) && member is PropertyInfo property && (property.GetSetMethod(nonPublic: true) is { } setter))
                jsonProperty.Set = (target, value) => setter.Invoke(target, [value]);

            ApplyPropertyConverter(jsonProperty, member);
        }

        foreach (var jsonProperty in toRemove)
            typeInfo.Properties.Remove(jsonProperty);

        // (2) [JsonProperty] members System.Text.Json never surfaces (non-public properties, any field): create
        //     a JsonPropertyInfo with reflection accessors so they bind the way Newtonsoft binds them.
        foreach (var member in HiddenJsonPropertyMembers(type))
        {
            if (typeInfo.Properties.Any(existing => ReferenceEquals(existing.AttributeProvider, member)))
                continue;

            var attribute = member.GetCustomAttribute<NJson.JsonPropertyAttribute>()!;
            var name = attribute.PropertyName ?? member.Name;

            JsonPropertyInfo created;

            if (member is PropertyInfo property)
            {
                created = typeInfo.CreateJsonPropertyInfo(property.PropertyType, name);

                if (property.GetGetMethod(nonPublic: true) is { } getter)
                    created.Get = target => getter.Invoke(target, null);

                if (property.GetSetMethod(nonPublic: true) is { } setter)
                    created.Set = (target, value) => setter.Invoke(target, [value]);
            }
            else
            {
                var field = (FieldInfo)member;
                created = typeInfo.CreateJsonPropertyInfo(field.FieldType, name);
                created.Get = field.GetValue;
                created.Set = field.SetValue;
            }

            ApplyPropertyConverter(created, member);

            typeInfo.Properties.Add(created);
        }

        DropDerivedAccessorCollisions(typeInfo);
    }

    // A model idiom pairs a wire-bound member with a derived accessor: `public X => _x ?? …;` next to
    // `[JsonProperty("x")] private _x`. Newtonsoft binds the annotated member and treats the accessor as
    // serialize-only, but System.Text.Json rejects the case-insensitive duplicate name. Drop the get-only,
    // unannotated accessor so the annotated member is the sole binding for that wire key, matching Newtonsoft.
    private static void DropDerivedAccessorCollisions(JsonTypeInfo typeInfo)
    {
        var derivedAccessors = typeInfo.Properties
                                       .GroupBy(jsonProperty => jsonProperty.Name, StringComparer.OrdinalIgnoreCase)
                                       .Where(group => group.Count() > 1)
                                       .SelectMany(group => group)
                                       .Where(jsonProperty => (jsonProperty.Set is null)
                                                              && ((jsonProperty.AttributeProvider as MemberInfo)?.GetCustomAttribute<NJson.JsonPropertyAttribute>()
                                                                  is null))
                                       .ToList();

        foreach (var jsonProperty in derivedAccessors)
            typeInfo.Properties.Remove(jsonProperty);
    }

    // (3) property-level [JsonConverter] for the converters that cannot be registered globally by type, because
    //     their target type (bool, IReadOnlyList<string>, an enum-keyed dictionary) is too common to claim in the
    //     shared options. Every other Newtonsoft converter is applied by a type-matched factory/instance there.
    private static void ApplyPropertyConverter(JsonPropertyInfo jsonProperty, MemberInfo member)
    {
        if (member.GetCustomAttribute<NJson.JsonConverterAttribute>() is not { } attribute)
            return;

        if (MapPropertyConverter(attribute) is { } converter)
            jsonProperty.CustomConverter = converter;
    }

    /// <summary>
    ///     The property-level converter a member's Newtonsoft <c>[JsonConverter]</c> maps to, or null if it has
    ///     none or maps to one registered globally. Exposed so the forced-object binder (which hand-binds
    ///     <c>[JsonObject]</c> IEnumerable types the modifier's object path never sees) applies the same
    ///     property-level converters this modifier would.
    /// </summary>
    internal static JsonConverter? MapPropertyConverter(MemberInfo member)
        => member.GetCustomAttribute<NJson.JsonConverterAttribute>() is { } attribute ? MapPropertyConverter(attribute) : null;

    private static JsonConverter? MapPropertyConverter(NJson.JsonConverterAttribute attribute)
    {
        var converterType = attribute.ConverterType;

        if (converterType == typeof(NsConverters.AfkConverter))
            return new AfkConverter();

        if (!converterType.IsGenericType)
            return null;

        var definition = converterType.GetGenericTypeDefinition();
        var arguments = converterType.GetGenericArguments();

        if (definition == typeof(NsConverters.FalsyListConverter<>))
            return (JsonConverter)Activator.CreateInstance(typeof(FalsyListConverter<>).MakeGenericType(arguments))!;

        if (definition == typeof(NsConverters.TolerantEnumKeyDictionaryConverter<,>))
            return (JsonConverter)Activator.CreateInstance(typeof(TolerantEnumKeyDictionaryConverter<,>).MakeGenericType(arguments))!;

        if (definition == typeof(NsConverters.ArrayOrSingleConverter<>))
            return (JsonConverter)Activator.CreateInstance(typeof(ArrayOrSingleConverter<>).MakeGenericType(arguments))!;

        //FalsyConverter carries its fallback value as the Newtonsoft attribute's converter parameter (e.g. `1` on
        //GItem.StackSize); pass it through so a falsy `s` degrades to that value instead of throwing
        if (definition == typeof(NsConverters.FalsyConverter<>))
            return (JsonConverter)Activator.CreateInstance(typeof(FalsyConverter<>).MakeGenericType(arguments), attribute.ConverterParameters ?? [])!;

        return null;
    }

    //non-public properties and any fields carrying [JsonProperty] — the members STJ's default resolver omits
    private static IEnumerable<MemberInfo> HiddenJsonPropertyMembers(System.Type type)
    {
        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        for (var current = type; (current is not null) && (current != typeof(object)); current = current.BaseType)
        {
            foreach (var property in current.GetProperties(FLAGS))
            {
                var getter = property.GetGetMethod(nonPublic: true);
                var setter = property.GetSetMethod(nonPublic: true);
                var isPublic = (getter?.IsPublic ?? false) || (setter?.IsPublic ?? false);

                if (!isPublic && (property.GetCustomAttribute<NJson.JsonPropertyAttribute>() is not null))
                    yield return property;
            }

            foreach (var field in current.GetFields(FLAGS))
                if (field.GetCustomAttribute<NJson.JsonPropertyAttribute>() is not null)
                    yield return field;
        }
    }
}
