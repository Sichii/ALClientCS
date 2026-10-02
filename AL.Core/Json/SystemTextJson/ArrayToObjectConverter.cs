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
///     Converts an object whose wire form is a positional array, mapping each array index to the member that carries it in
///     <see cref="JsonArrayIndexAttribute" />.
/// </summary>
/// <remarks>
///     Register it through <see cref="ArrayToObjectConverterFactory" /> rather than as a type-level attribute, so the inner
///     member fill can exclude it.
/// </remarks>
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
    private static readonly ConstructorInfo Constructor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                                                   .OrderByDescending(ctor => ctor.GetParameters()
                                                                                                  .Length)
                                                                   .First();

    /// <summary>
    ///     The array index per <see cref="JsonArrayIndexAttribute" /> member name, matched case-insensitively against a
    ///     constructor parameter name.
    /// </summary>

    // ReSharper disable once StaticMemberInGenericType
    private static readonly Dictionary<string, int> IndexByParameterName = Indexed.ToDictionary(
        set => set.Member.Name,
        set => set.Index,
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Whether a JSON null reaches <see cref="Read" />. It does, so that a null, like any non-array token, yields default.
    /// </summary>
    public override bool HandleNull => true;

    /// <summary>
    ///     Creates an <see cref="IEnumerable" /> <typeparamref name="T" /> and sets each indexed member from its array slot.
    /// </summary>
    /// <param name="array">
    ///     The positional array.
    /// </param>
    /// <param name="options">
    ///     The options each member value deserializes through, so nested converters still apply.
    /// </param>
    /// <returns>
    ///     The bound instance.
    /// </returns>
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
    ///     Builds a non-<see cref="IEnumerable" /> <typeparamref name="T" /> through its constructor, sourcing each parameter
    ///     from the indexed member of the same name; one with no indexed member gets its default.
    /// </summary>
    /// <remarks>
    ///     Sidesteps System.Text.Json's rule that every constructor parameter must bind to an included member.
    /// </remarks>
    /// <param name="array">
    ///     The positional array.
    /// </param>
    /// <param name="options">
    ///     The options each argument deserializes through.
    /// </param>
    /// <returns>
    ///     The constructed instance.
    /// </returns>
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
    ///     Maps a positional <see cref="JsonArray" /> to <typeparamref name="T" /> by index, for converters that hold a parsed
    ///     node rather than a reader.
    /// </summary>
    /// <param name="array">
    ///     The positional array.
    /// </param>
    /// <param name="options">
    ///     The options each member value deserializes through.
    /// </param>
    /// <returns>
    ///     The mapped instance.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     array
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     options
    /// </exception>
    public static T FromArray(JsonArray array, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(array);

        ArgumentNullException.ThrowIfNull(options);

        //System.Text.Json treats an IEnumerable type as a collection, so its indexed members are bound directly
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
        //index-ordered member values as a dense array, ignoring gaps in the indices
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
///     Applies <see cref="ArrayToObjectConverter{T}" /> to any type with <see cref="JsonArrayIndexAttribute" /> members, which
///     is always array-shaped on the wire.
/// </summary>
public sealed class ArrayToObjectConverterFactory : JsonConverterFactory, IExcludingConverterFactory
{
    /// <summary>
    ///     The cached answer to <see cref="IsPositional" /> per type, shared across every <see cref="Excluding" /> copy.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> Positional = new();

    private readonly Type? Excluded;

    public ArrayToObjectConverterFactory() { }

    private ArrayToObjectConverterFactory(Type excluded) => Excluded = excluded;

    /// <inheritdoc />
    public JsonConverterFactory Excluding(Type type) => new ArrayToObjectConverterFactory(type);

    public override bool CanConvert(Type typeToConvert) => (typeToConvert != Excluded) && Positional.GetOrAdd(typeToConvert, IsPositional);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(ArrayToObjectConverter<>).MakeGenericType(typeToConvert))!;

    private static bool HasIndex(MemberInfo member) => member.GetCustomAttribute<JsonArrayIndexAttribute>() is not null;

    private static bool IsPositional(Type typeToConvert)
    {
        const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        return typeToConvert is { IsAbstract: false, IsPrimitive: false }
               && (typeToConvert.GetProperties(FLAGS)
                                .Any(HasIndex)
                   || typeToConvert.GetFields(FLAGS)
                                   .Any(HasIndex));
    }
}