#region
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     A converter factory that can produce a copy of itself which declines one specific type, so a converter registered
///     VIA this factory can run its inner declared-member fill without the factory re-producing the very converter that is
///     running (nested types still match). Mirrors
///     <c>
///         AttributedObjectConverterFactory.Excluding
///     </c>
///     ; needed because a factory cannot be removed by concrete converter type.
/// </summary>
public interface IExcludingConverterFactory
{
    JsonConverterFactory Excluding(Type type);
}

/// <summary>
///     Returns a cached copy of a <see cref="JsonSerializerOptions" /> with a specific converter neutralized for the inner
///     deserialize, so a converter that deserializes its OWN handled type (the array-to-object, string-or-object,
///     event/boss and disappear converters) can run the inner declared-member fill without re-entering itself. A converter
///     registered as a concrete instance is dropped by exact type; one registered via an
///     <see cref="IExcludingConverterFactory" /> is instead replaced by a copy that excludes the handled type (removing by
///     type would miss the factory, since the factory's own type never equals the produced converter's type — the defect
///     that made every positional-array type bind to null / string-or-object recurse).
/// </summary>
public static class RecursionSafeOptions
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, ConcurrentDictionary<Type, JsonSerializerOptions>> Cache = new();

    private static JsonSerializerOptions Build(JsonSerializerOptions outer, Type converterType)
    {
        var inner = new JsonSerializerOptions(outer);

        // the type the converter handles is its sole generic argument (ArrayToObjectConverter<T>,
        // StringOrObjectConverter<T>, FalsyConverter<T>); a non-generic converter (event/boss, disappear) is
        // registered as a concrete instance and needs no factory exclusion.
        var handledType = converterType.IsGenericType ? converterType.GetGenericArguments()[0] : null;

        for (var i = inner.Converters.Count - 1; i >= 0; i--)
        {
            var converter = inner.Converters[i];

            // registered as a concrete instance (the disappear/event-boss path, direct-instance tests) -> drop it.
            // Assignability, not exact type: FalsyConverter<T> is subclassed (FalsyStackSizeConverter) to carry a
            // fallback an attribute cannot, and it passes typeof(FalsyConverter<T>) - the BASE - to Without.
            if (converterType.IsInstanceOfType(converter))
                inner.Converters.RemoveAt(i);

            // registered via a factory (the shared options) -> replace it with a copy that declines the handled
            // type, so the inner deserialize resolves the default converter for T while nested types still match
            else if (handledType is not null && converter is IExcludingConverterFactory factory)
                inner.Converters[i] = factory.Excluding(handledType);
        }

        return inner;
    }

    public static JsonSerializerOptions Without(JsonSerializerOptions outer, Type converterType)
        => Cache.GetValue(outer, static _ => new ConcurrentDictionary<Type, JsonSerializerOptions>())
                .GetOrAdd(converterType, static (ct, from) => Build(from, ct), outer);
}