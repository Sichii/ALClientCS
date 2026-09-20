#region
using System.Collections;
using System.Collections.Concurrent;
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
    /// <summary>
    ///     The members carrying <see cref="JsonArrayIndexAttribute" />, ordered by index. Drives both the array-to-object read
    ///     and the object-to-array write.
    /// </summary>
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

    /// <summary>
    ///     The deserialization constructor: the public instance constructor with the most parameters, so a record's non-public
    ///     copy constructor is excluded.
    /// </summary>
    /// <remarks>
    ///     Non-<see cref="IEnumerable" /> positional types are built through it rather than System.Text.Json's stricter
    ///     parameterized-object binding, which requires every constructor parameter to map to an included member.
    /// </remarks>
    private static readonly ConstructorInfo Constructor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                                                   .OrderByDescending(ctor => ctor.GetParameters()
                                                                                                  .Length)
                                                                   .First();

    /// <summary>
    ///     <see cref="JsonArrayIndexAttribute" /> member name to array index, matched case-insensitively against a constructor
    ///     parameter name.
    /// </summary>

    // ReSharper disable once StaticMemberInGenericType
    private static readonly Dictionary<string, int> IndexByParameterName = Indexed.ToDictionary(
        set => set.Member.Name,
        set => set.Index,
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null, like any non-array token, yields default,
    ///     matching Newtonsoft.
    /// </summary>
    public override bool HandleNull => true;

    /// <summary>
    ///     Binds the <see cref="IEnumerable" /> case: creates T and sets each <see cref="JsonArrayIndexAttribute" /> member
    ///     from its array slot, deserializing through the options so nested converters (tolerant enums for lock and key types)
    ///     still apply.
    /// </summary>
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

    /// <summary>
    ///     Builds a non-<see cref="IEnumerable" /> positional type through its constructor, as Newtonsoft's JToken.ToObject
    ///     did. Each parameter is sourced from the <see cref="JsonArrayIndexAttribute" /> member of the same name; one with no
    ///     indexed member gets its default.
    /// </summary>
    /// <remarks>
    ///     Sidesteps the rule that every constructor parameter must bind to an included property.
    /// </remarks>
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
        // JsonObject would route to the enumerable converter and fail. Bind their indexed members directly: they
        // expose a parameterless ctor and settable members, so there is no re-deserialize of T
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

    /// <summary>
    ///     Shared across every <see cref="Excluding" /> copy: the exclusion is a reference compare on the way in, and
    ///     everything after it is a property of the type alone.
    /// </summary>
    /// <remarks>
    ///     The question is asked again for every <see cref="JsonSerializerOptions" /> instance the nesting converters mint,
    ///     and each miss walks every member reading attributes.
    /// </remarks>
    private static readonly ConcurrentDictionary<Type, bool> Positional = new();

    private readonly Type? Excluded;

    public ArrayToObjectConverterFactory() { }

    private ArrayToObjectConverterFactory(Type excluded) => Excluded = excluded;

    /// <summary>
    ///     Returns a copy that declines <paramref name="type" />, so the inner declared-member fill resolves it with the
    ///     default object converter and cannot re-enter its own converter, while nested positional types still match.
    /// </summary>
    public JsonConverterFactory Excluding(Type type) => new ArrayToObjectConverterFactory(type);

    public override bool CanConvert(Type typeToConvert) => (typeToConvert != Excluded) && Positional.GetOrAdd(typeToConvert, IsPositional);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(ArrayToObjectConverter<>).MakeGenericType(typeToConvert))!;

    private static bool HasIndex(MemberInfo member) => member.GetCustomAttribute<JsonArrayIndexAttribute>() is not null;

    private static bool IsPositional(Type typeToConvert)
        => typeToConvert is { IsAbstract: false, IsPrimitive: false }
           && (typeToConvert.GetProperties(FLAGS)
                            .Any(HasIndex)
               || typeToConvert.GetFields(FLAGS)
                               .Any(HasIndex));
}