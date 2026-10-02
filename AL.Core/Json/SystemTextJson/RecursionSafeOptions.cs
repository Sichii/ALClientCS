#region
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Represents a converter factory that can copy itself to decline one type, so a converter it produced can run an
///     inner member fill without re-entering itself while nested types still match.
/// </summary>
/// <remarks>
///     Options cannot drop a factory by the produced converter's type, so the factory is swapped for such a copy instead.
/// </remarks>
public interface IExcludingConverterFactory
{
    /// <summary>
    ///     Returns a copy of this factory that declines <paramref name="type" />.
    /// </summary>
    /// <param name="type">The type the copy declines.</param>
    /// <returns>The excluding copy.</returns>
    JsonConverterFactory Excluding(Type type);
}

/// <summary>
///     Provides cached copies of <see cref="JsonSerializerOptions" /> with one converter neutralized, so a converter that
///     deserializes its own handled type can run the inner member fill without re-entering itself.
/// </summary>
/// <remarks>
///     A concrete converter instance is dropped; an <see cref="IExcludingConverterFactory" /> is replaced by a copy that
///     declines the handled type.
/// </remarks>
public static class RecursionSafeOptions
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, ConcurrentDictionary<Type, JsonSerializerOptions>> Cache = new();

    private static JsonSerializerOptions Build(JsonSerializerOptions outer, Type converterType)
    {
        var inner = new JsonSerializerOptions(outer);

        //a generic converter handles its sole type argument; a non-generic one is registered as a concrete instance
        var handledType = converterType.IsGenericType ? converterType.GetGenericArguments()[0] : null;

        for (var i = inner.Converters.Count - 1; i >= 0; i--)
        {
            var converter = inner.Converters[i];

            //assignability, not exact type, so FalsyStackSizeConverter is dropped for typeof(FalsyConverter<int>)
            if (converterType.IsInstanceOfType(converter))
                inner.Converters.RemoveAt(i);

            //a factory is swapped for a copy that declines the handled type
            else if (handledType is not null && converter is IExcludingConverterFactory factory)
                inner.Converters[i] = factory.Excluding(handledType);
        }

        return inner;
    }

    /// <summary>
    ///     Gets a cached copy of <paramref name="outer" /> with <paramref name="converterType" /> neutralized.
    /// </summary>
    /// <param name="outer">
    ///     The options the running converter was called with.
    /// </param>
    /// <param name="converterType">The type of the running converter.</param>
    /// <returns>The options for the inner deserialize.</returns>
    /// <exception cref="System.ArgumentNullException">outer</exception>
    /// <exception cref="System.ArgumentNullException">converterType</exception>
    public static JsonSerializerOptions Without(JsonSerializerOptions outer, Type converterType)
    {
        ArgumentNullException.ThrowIfNull(outer);

        ArgumentNullException.ThrowIfNull(converterType);

        return Cache.GetValue(outer, static _ => new ConcurrentDictionary<Type, JsonSerializerOptions>())
                    .GetOrAdd(converterType, static (ct, from) => Build(from, ct), outer);
    }
}