#region
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Shared positional-element reader: null-safe and forwards <see cref="JsonSerializerOptions" /> so nested converters
///     and number-from-string coercion apply, mirroring Newtonsoft's
///     <c>
///         JToken.ToObject&lt;T&gt;(serializer)
///     </c>
///     .
/// </summary>
internal static class TupleElement
{
    public static T? Read<T>(JsonArray arr, int index, JsonSerializerOptions options)
        => arr[index] is { } node ? node.Deserialize<T>(options) : default;
}

public sealed class ArrayToTupleConverter<T1, T2> : JsonConverter<ValueTuple<T1?, T2?>>
{
    //a JSON null must reach Read so it maps to the default tuple instead of throwing (value type)
    public override bool HandleNull => true;

    public override (T1?, T2?) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonNode.Parse(ref reader)
                          ?.AsArray();

        if (arr is null)
            return default;

        return arr.Count switch
        {
            1 => (TupleElement.Read<T1>(arr, 0, options), default),
            2 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options)),
            _ => default
        };
    }

    public override void Write(Utf8JsonWriter writer, (T1?, T2?) value, JsonSerializerOptions options) => throw new NotSupportedException();
}

public sealed class ArrayToTupleConverter<T1, T2, T3> : JsonConverter<ValueTuple<T1?, T2?, T3?>>
{
    public override bool HandleNull => true;

    public override (T1?, T2?, T3?) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonNode.Parse(ref reader)
                          ?.AsArray();

        if (arr is null)
            return default;

        return arr.Count switch
        {
            1 => (TupleElement.Read<T1>(arr, 0, options), default, default),
            2 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), default),
            3 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options)),
            _ => default
        };
    }

    public override void Write(Utf8JsonWriter writer, (T1?, T2?, T3?) value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}

public class ArrayToTupleConverter<T1, T2, T3, T4> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?>>
{
    public override bool HandleNull => true;

    public override (T1?, T2?, T3?, T4?) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonNode.Parse(ref reader)
                          ?.AsArray();

        if (arr is null)
            return default;

        return arr.Count switch
        {
            1 => (TupleElement.Read<T1>(arr, 0, options), default, default, default),
            2 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), default, default),
            3 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                default),
            4 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options)),
            _ => default
        };
    }

    public override void Write(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?) value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}

public sealed class ArrayToTupleConverter<T1, T2, T3, T4, T5> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
{
    public override bool HandleNull => true;

    public override (T1?, T2?, T3?, T4?, T5?) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonNode.Parse(ref reader)
                          ?.AsArray();

        if (arr is null)
            return default;

        return arr.Count switch
        {
            1 => (TupleElement.Read<T1>(arr, 0, options), default, default, default, default),
            2 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), default, default, default),
            3 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                default, default),
            4 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), default),
            5 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), TupleElement.Read<T5>(arr, 4, options)),
            _ => default
        };
    }

    public override void Write(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?) value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}

public class ArrayToTupleConverter<T1, T2, T3, T4, T5, T6> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    public override bool HandleNull => true;

    public override (T1?, T2?, T3?, T4?, T5?, T6?) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonNode.Parse(ref reader)
                          ?.AsArray();

        if (arr is null)
            return default;

        return arr.Count switch
        {
            1 => (TupleElement.Read<T1>(arr, 0, options), default, default, default, default, default),
            2 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), default, default, default, default),
            3 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                default, default, default),
            4 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), default, default),
            5 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), TupleElement.Read<T5>(arr, 4, options), default),
            6 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), TupleElement.Read<T5>(arr, 4, options), TupleElement.Read<T6>(arr, 5, options)),
            _ => default
        };
    }

    public override void Write(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?, T6?) value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}

public class ArrayToTupleConverter<T1, T2, T3, T4, T5, T6, T7> : JsonConverter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    public override bool HandleNull => true;

    public override (T1?, T2?, T3?, T4?, T5?, T6?, T7?) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var arr = JsonNode.Parse(ref reader)
                          ?.AsArray();

        if (arr is null)
            return default;

        return arr.Count switch
        {
            1 => (TupleElement.Read<T1>(arr, 0, options), default, default, default, default, default, default),
            2 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), default, default, default, default,
                default),
            3 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                default, default, default, default),
            4 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), default, default, default),
            5 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), TupleElement.Read<T5>(arr, 4, options), default, default),
            6 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), TupleElement.Read<T5>(arr, 4, options), TupleElement.Read<T6>(arr, 5, options),
                default),

            // ponytail: the 7th element deserializes WITHOUT options, faithfully preserving the Newtonsoft quirk
            // (arr[6].ToObject<T7>() omitted the serializer). Pinned by characterization; revisit only if it moves.
            7 => (TupleElement.Read<T1>(arr, 0, options), TupleElement.Read<T2>(arr, 1, options), TupleElement.Read<T3>(arr, 2, options),
                TupleElement.Read<T4>(arr, 3, options), TupleElement.Read<T5>(arr, 4, options), TupleElement.Read<T6>(arr, 5, options),
                arr[6] is { } n7 ? n7.Deserialize<T7>() : default),
            _ => default
        };
    }

    public override void Write(Utf8JsonWriter writer, (T1?, T2?, T3?, T4?, T5?, T6?, T7?) value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}

/// <summary>
///     Applies the arity-matched <see cref="ArrayToTupleConverter{T1,T2}" /> to any <see cref="ValueTuple" />. Every
///     serialized tuple in the model is a positional array (both the single-tuple
///     <c>
///         [JsonConverter]
///     </c>
///     sites and the
///     <c>
///         ItemConverterType
///     </c>
///     list-element sites), so one global factory replaces both. The
///     <c>
///         ?
///     </c>
///     on the converter's unconstrained parameters is annotation-only, so the produced converter's runtime target type is
///     the exact declared tuple (e.g.
///     <c>
///         ValueTuple&lt;float, float&gt;
///     </c>
///     ), which is what System.Text.Json matches on.
/// </summary>
public sealed class TupleConverterFactory : JsonConverterFactory
{
    private static readonly Dictionary<Type, Type> ByArity = new()
    {
        [typeof(ValueTuple<,>)] = typeof(ArrayToTupleConverter<,>),
        [typeof(ValueTuple<,,>)] = typeof(ArrayToTupleConverter<,,>),
        [typeof(ValueTuple<,,,>)] = typeof(ArrayToTupleConverter<,,,>),
        [typeof(ValueTuple<,,,,>)] = typeof(ArrayToTupleConverter<,,,,>),
        [typeof(ValueTuple<,,,,,>)] = typeof(ArrayToTupleConverter<,,,,,>),
        [typeof(ValueTuple<,,,,,,>)] = typeof(ArrayToTupleConverter<,,,,,,>)
    };

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType && ByArity.ContainsKey(typeToConvert.GetGenericTypeDefinition());

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterDefinition = ByArity[typeToConvert.GetGenericTypeDefinition()];

        return (JsonConverter)Activator.CreateInstance(converterDefinition.MakeGenericType(typeToConvert.GetGenericArguments()))!;
    }
}