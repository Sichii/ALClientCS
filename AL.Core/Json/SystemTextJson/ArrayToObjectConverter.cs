#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Json.Attributes;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Converts an object whose wire form is a positional array, mapping array index &lt;-&gt; member via
///     <see cref="JsonArrayIndexAttribute" />. The System.Text.Json replacement for the Newtonsoft
///     <c>
///         ArrayToObjectConverter
///     </c>
///     . Register in the shared options (or reuse <see cref="FromArray" />) rather than as a type-level attribute, so the
///     inner declared-member deserialize can drop this converter and avoid re-entering itself.
/// </summary>
public sealed class ArrayToObjectConverter<T> : JsonConverter<T>
{
    // members carrying [JsonArrayIndex], ordered by index — drives both the array->object read and object->array write
    private static readonly (int Index, MemberInfo Member)[] Indexed = typeof(T)
                                                                       .GetProperties(
                                                                           BindingFlags.Public
                                                                           | BindingFlags.NonPublic
                                                                           | BindingFlags.Instance)
                                                                       .Cast<MemberInfo>()
                                                                       .Concat(
                                                                           typeof(T).GetFields(
                                                                               BindingFlags.Public
                                                                               | BindingFlags.NonPublic
                                                                               | BindingFlags.Instance))
                                                                       .Select(member => (member,
                                                                           attribute: member.GetCustomAttribute<JsonArrayIndexAttribute>()))
                                                                       .Where(set => set.attribute is not null)
                                                                       .Select(set => (set.attribute!.Index, set.member))
                                                                       .OrderBy(set => set.Index)
                                                                       .ToArray();

    // the deserialization constructor: the public instance ctor with the most parameters (a record's non-public
    // copy ctor is excluded). Non-IEnumerable positional types are built through it, not System.Text.Json's
    // stricter parameterized-object binding (which requires every ctor parameter to map to an included member).
    private static readonly ConstructorInfo Constructor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                                                   .OrderByDescending(ctor => ctor.GetParameters()
                                                                       .Length)
                                                                   .First();

    // [JsonArrayIndex] member name (case-insensitive, matched against a ctor parameter name) -> array index
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Dictionary<string, int> IndexByParameterName = Indexed.ToDictionary(
        set => set.Member.Name,
        set => set.Index,
        StringComparer.OrdinalIgnoreCase);

    //a JSON null (or any non-array token) yields default, matching Newtonsoft; HandleNull routes null here too
    public override bool HandleNull => true;

    //object binding for the IEnumerable case: create T and set each [JsonArrayIndex] member from its array slot,
    //deserializing through the options so nested converters (tolerant enums for lock/key types) still apply
    private static T BindIndexedMembers(JsonArray array, JsonSerializerOptions options)
    {
        var instance = Activator.CreateInstance<T>();

        foreach ((var index, var member) in Indexed)
        {
            if ((index >= array.Count) || array[index] is not { } node)
                continue;

            switch (member)
            {
                case PropertyInfo property when property.GetSetMethod(true) is { } setter:
                    setter.Invoke(instance, [node.Deserialize(property.PropertyType, options)]);

                    break;
                case FieldInfo field:
                    field.SetValue(instance, node.Deserialize(field.FieldType, options));

                    break;
            }
        }

        return instance;
    }

    //A non-IEnumerable positional type is built through its constructor, as Newtonsoft's JToken.ToObject did.
    //Each parameter is sourced from the [JsonArrayIndex] member of the same name, deserialized through the
    //options so nested converters still apply; a parameter with no indexed member (StraightLine.isVertical,
    //which enrichment sets later) gets its default. This sidesteps System.Text.Json's rule that every ctor
    //parameter must bind to an included property - which a [JsonIgnore] member like StraightLine.IsVertical breaks.
    private static T ConstructFromArray(JsonArray array, JsonSerializerOptions options)
    {
        var parameters = Constructor.GetParameters();
        var args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (IndexByParameterName.TryGetValue(parameter.Name!, out var index) && (index < array.Count) && array[index] is { } node)
                args[i] = node.Deserialize(parameter.ParameterType, options);
            else if (parameter.HasDefaultValue)
                args[i] = parameter.DefaultValue;
            else if (parameter.ParameterType.IsValueType)
                args[i] = Activator.CreateInstance(parameter.ParameterType);
        }

        return (T)Constructor.Invoke(args);
    }

    /// <summary>
    ///     Maps a positional <see cref="JsonArray" /> to <typeparamref name="T" /> by index. Exposed so other converters (e.g.
    ///     the disappear-data converter's spawn orientation) can reuse it without a reader.
    /// </summary>
    public static T FromArray(JsonArray array, JsonSerializerOptions options)
    {
        // System.Text.Json classifies an IEnumerable type as a collection, so binding it from a reconstructed
        // JsonObject would route to the enumerable converter and fail. Newtonsoft forces these (IRectangle
        // implementers, which are IEnumerable<IPoint>) to an object contract with [JsonObject], which
        // System.Text.Json ignores; bind their indexed members directly. They expose a parameterless ctor and
        // settable (init/private) members, so there is no re-deserialize of T — which also cannot recurse.
        if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            return BindIndexedMembers(array, options);

        return ConstructFromArray(array, options);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            //consume an unexpected scalar/object/null so the reader stays aligned; a no-op on a scalar
            reader.Skip();

            return default;
        }

        return FromArray(JsonNode.Parse(ref reader)!.AsArray(), options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        //index-ordered member values as a dense array (Newtonsoft ordered by index and ignored gaps)
        var array = Indexed.Select(set => set.Member switch
                           {
                               PropertyInfo property => property.GetValue(value),
                               FieldInfo field       => field.GetValue(value),
                               _                     => null
                           })
                           .ToArray();

        JsonSerializer.Serialize(writer, array, options);
    }
}

/// <summary>
///     Applies <see cref="ArrayToObjectConverter{T}" /> to any type that carries <see cref="JsonArrayIndexAttribute" />
///     members — such a type is positional by design and is therefore always array-shaped on the wire. Registered in the
///     shared options so a single factory covers every
///     <c>
///         ItemConverterType = ArrayToObjectConverter&lt;…&gt;
///     </c>
///     element type (doors, spawns, tiles, orientations, …) without a per-type registration.
/// </summary>
public sealed class ArrayToObjectConverterFactory : JsonConverterFactory, IExcludingConverterFactory
{
    private const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private readonly Type? Excluded;

    public ArrayToObjectConverterFactory() { }

    private ArrayToObjectConverterFactory(Type excluded) => Excluded = excluded;

    // the inner declared-member fill runs under a copy of the options where this factory declines T, so T resolves
    // the default object converter and cannot re-enter its own converter, while nested positional types still match.
    public JsonConverterFactory Excluding(Type type) => new ArrayToObjectConverterFactory(type);

    public override bool CanConvert(Type typeToConvert)
        => (typeToConvert != Excluded)
           && !typeToConvert.IsAbstract
           && !typeToConvert.IsPrimitive
           && (typeToConvert.GetProperties(FLAGS)
                            .Any(HasIndex)
               || typeToConvert.GetFields(FLAGS)
                               .Any(HasIndex));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(ArrayToObjectConverter<>).MakeGenericType(typeToConvert))!;

    private static bool HasIndex(MemberInfo member) => member.GetCustomAttribute<JsonArrayIndexAttribute>() is not null;
}