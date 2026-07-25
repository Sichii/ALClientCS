#region
using System;
using System.Collections.Generic;
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
///     Produces the System.Text.Json converter for every <see cref="IAttributed" /> type. Registered in the
///     shared options (NOT applied as a <c>[JsonConverter]</c> attribute) so the inner declared-member fill can
///     run under a copy of the options whose factory excludes only the type being filled — that type cannot
///     re-enter its own converter, yet a NESTED <see cref="IAttributed" /> member is still routed through the
///     factory and gets its attribute harvest (matching Newtonsoft's per-element <c>ItemConverterType</c>).
/// </summary>
public sealed class AttributedObjectConverterFactory : JsonConverterFactory
{
    private readonly Type? Excluded;

    public AttributedObjectConverterFactory() { }

    private AttributedObjectConverterFactory(Type excluded) => Excluded = excluded;

    public override bool CanConvert(Type typeToConvert)
        => (typeToConvert != Excluded)
           && typeof(IAttributed).IsAssignableFrom(typeToConvert)
           && !typeToConvert.IsAbstract
           && (typeToConvert.GetConstructor(Type.EmptyTypes) is not null);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(
            typeof(AttributedObjectStjConverter<>).MakeGenericType(typeToConvert),
            new object[] { options })!;

    /// <summary>
    ///     A copy that also excludes <paramref name="type" /> — used by the inner options so the type being
    ///     filled cannot re-enter its own converter, while nested <see cref="IAttributed" /> members still match.
    /// </summary>
    internal AttributedObjectConverterFactory Excluding(Type type) => new(type);
}

/// <summary>
///     The System.Text.Json replacement for the Newtonsoft <c>AttributedObjectConverter</c>. Fills T's declared
///     members via a recursion-safe inner deserialize, then walks every top-level wire key to (1) mark it present
///     (<see cref="IKeyPresenceCapturable" />) and (2) harvest numeric <see cref="ALAttribute" /> keys into the
///     <see cref="IAttributed.Attributes" /> dictionary. Read-only.
/// </summary>
public sealed class AttributedObjectStjConverter<T> : JsonConverter<T> where T: class, IAttributed, new()
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> InnerCache = new();

    private static readonly bool RecoversScrollStat = typeof(IScrollStatRecoverable).IsAssignableFrom(typeof(T));

    private readonly JsonSerializerOptions InnerOptions;

    public AttributedObjectStjConverter(JsonSerializerOptions options) => InnerOptions = InnerCache.GetValue(options, BuildInner);

    private static JsonSerializerOptions BuildInner(JsonSerializerOptions outer)
    {
        var inner = new JsonSerializerOptions(outer);

        //keep the factory for NESTED IAttributed members (so they still harvest), but exclude T so the inner
        //Deserialize<T> resolves the default object converter and cannot re-enter this converter
        for (var i = 0; i < inner.Converters.Count; i++)
            if (inner.Converters[i] is AttributedObjectConverterFactory factory)
                inner.Converters[i] = factory.Excluding(typeof(T));

        //Newtonsoft's JToken->int populate path rounds a fractional number (Grade 3.6->4); the text-reader
        //socket path throws instead, so this leniency belongs only here, never in the shared options.
        //(number/bool->string leniency, by contrast, is universal in Newtonsoft, so LenientStringConverter is in
        //the shared options and copied in here automatically.)
        inner.Converters.Add(new LenientInt32Converter());

        return inner;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //a non-object token (including JSON null) yields null, matching Newtonsoft's null short-circuit before
        //the instance is ever constructed
        if (JsonNode.Parse(ref reader) is not JsonObject obj)
            return null;

        //GItem sends a scroll-stat NAME in the numeric `stat` slot; strip it before binding (the float Stat
        //target would throw and abort the whole object) and recover it after. Newtonsoft used [OnError] here,
        //which resumes binding; System.Text.Json cannot, so the wire key is removed and re-handled explicitly.
        string? scrollStatName = null;

        if (RecoversScrollStat
            && obj.TryGetPropertyValue("stat", out var statNode)
            && (statNode?.GetValueKind() == JsonValueKind.String))
        {
            var raw = statNode.GetValue<string>();

            //a numeric string ("5") still binds to Stat via coercion; only a name ("int") needs recovery
            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                scrollStatName = raw;
                obj.Remove("stat");
            }
        }

        var value = obj.Deserialize<T>(InnerOptions)
                    ?? throw new JsonException($"Failed to deserialize {typeof(T).Name}.");

        if ((scrollStatName is not null) && value is IScrollStatRecoverable recoverable)
            recoverable.RecoverScrollStat(scrollStatName);

        //the base ctor allocated a concrete Dictionary behind the IReadOnlyDictionary facade; harvest into it
        var attributes = value.Attributes as IDictionary<ALAttribute, float>;
        var presence = value as IKeyPresenceCapturable;

        foreach ((var key, var child) in obj)
        {
            presence?.MarkPresent(key);

            //Number covers Newtonsoft's Integer|Float; a non-numeric ALAttribute-named G key (heal=true,
            //courage=[...]) falls through the guard and never calls GetValue<float>, so it does not throw
            if ((attributes is not null)
                && (child?.GetValueKind() == JsonValueKind.Number)
                && EnumHelper.TryParse<ALAttribute>(key, out var attribute))
                attributes[attribute] = child.GetValue<float>();
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotSupportedException();
}
