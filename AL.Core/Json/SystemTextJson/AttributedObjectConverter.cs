#region
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.Core.Interfaces;
#endregion

namespace AL.Core.Json.SystemTextJson;

/// <summary>
///     Produces the converter for every <see cref="IAttributed" /> type.
/// </summary>
/// <remarks>
///     Registered in the shared options rather than as an attribute, so the inner member fill can exclude only the type
///     being filled while nested <see cref="IAttributed" /> members still harvest.
/// </remarks>
public sealed class AttributedObjectConverterFactory : JsonConverterFactory
{
    /// <summary>
    ///     The cached answer to <see cref="IsHarvestable" /> per type, shared across every <see cref="Excluding" /> copy.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> Harvestable = new();

    private readonly Type? Excluded;

    public AttributedObjectConverterFactory() { }

    private AttributedObjectConverterFactory(Type excluded) => Excluded = excluded;

    public override bool CanConvert(Type typeToConvert)
        => (typeToConvert != Excluded) && Harvestable.GetOrAdd(typeToConvert, IsHarvestable);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(typeof(AttributedObjectConverter<>).MakeGenericType(typeToConvert), options)!;

    /// <summary>
    ///     Returns a copy that declines <paramref name="type" />, so the type being filled cannot re-enter its own converter
    ///     while nested <see cref="IAttributed" /> members still match.
    /// </summary>
    /// <param name="type">
    ///     The type the copy declines.
    /// </param>
    /// <returns>
    ///     The excluding copy.
    /// </returns>
    internal AttributedObjectConverterFactory Excluding(Type type) => new(type);

    private static bool IsHarvestable(Type typeToConvert)
        => typeof(IAttributed).IsAssignableFrom(typeToConvert)
           && !typeToConvert.IsAbstract
           && typeToConvert.GetConstructor(Type.EmptyTypes) is not null;
}

/// <summary>
///     Reads an <see cref="IAttributed" /> type: fills its declared members, then marks each top-level wire key present
///     (<see cref="IKeyPresenceCapturable" />) and harvests numeric <see cref="ALAttribute" /> keys into
///     <see cref="IAttributed.Attributes" />.
/// </summary>
public sealed class AttributedObjectConverter<T> : JsonConverter<T> where T: class, IAttributed, new()
{
    /// <summary>
    ///     The inner options per outer options instance. Kept per <typeparamref name="T" />, because each one excludes
    ///     <typeparamref name="T" /> from the attributed factory.
    /// </summary>

    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> InnerCache = new();

    private static readonly bool RecoversScrollStat = typeof(IScrollStatRecoverable).IsAssignableFrom(typeof(T));

    private readonly JsonSerializerOptions InnerOptions;

    public AttributedObjectConverter(JsonSerializerOptions options) => InnerOptions = InnerCache.GetValue(options, BuildInner);

    private static JsonSerializerOptions BuildInner(JsonSerializerOptions outer)
    {
        var inner = new JsonSerializerOptions(outer);

        //exclude T so the inner deserialize cannot re-enter this converter; nested IAttributed members still match
        for (var i = 0; i < inner.Converters.Count; i++)
            if (inner.Converters[i] is AttributedObjectConverterFactory factory)
                inner.Converters[i] = factory.Excluding(typeof(T));

        //rounds a fractional int (Grade 3.6 -> 4) here only; the socket path must still throw on one
        inner.Converters.Add(new LenientInt32Converter());

        return inner;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //a non-object token, null included, yields null
        if (JsonNode.Parse(ref reader) is not JsonObject obj)
            return null;

        //GItem sends a scroll-stat name in the numeric `stat` slot, which would abort binding the float Stat; strip it
        //here and recover it after binding
        string? scrollStatName = null;

        if (RecoversScrollStat && obj.TryGetPropertyValue("stat", out var statNode) && (statNode?.GetValueKind() == JsonValueKind.String))
        {
            var raw = statNode.GetValue<string>();

            //a numeric string ("5") still binds to Stat via coercion; only a name ("int") needs recovery
            if (!float.TryParse(
                    raw,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out _))
            {
                scrollStatName = raw;
                obj.Remove("stat");
            }
        }

        var value = obj.Deserialize<T>(InnerOptions) ?? throw new JsonException($"Failed to deserialize {typeof(T).Name}.");

        if (scrollStatName is not null && value is IScrollStatRecoverable recoverable)
            recoverable.RecoverScrollStat(scrollStatName);

        //the base ctor allocated a concrete Dictionary behind the IReadOnlyDictionary facade; harvest into it
        var attributes = value.Attributes as IDictionary<ALAttribute, float>;
        var presence = value as IKeyPresenceCapturable;

        foreach ((var key, var child) in obj)
        {
            presence?.MarkPresent(key);

            //a non-numeric ALAttribute-named key (heal=true, courage=[...]) is skipped
            if (attributes is not null
                && (child?.GetValueKind() == JsonValueKind.Number)
                && EnumHelper.TryParse<ALAttribute>(key, out var attribute))
                attributes[attribute] = child.GetValue<float>();
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotSupportedException();
}