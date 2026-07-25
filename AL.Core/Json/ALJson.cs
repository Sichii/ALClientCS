#region
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AL.Core.Definitions;
using AL.Core.Json.SystemTextJson;
#endregion

namespace AL.Core.Json;

/// <summary>
///     The single canonical System.Text.Json configuration, shared by the socket transport, the REST
///     client, and the game-data loader. It reproduces the Newtonsoft.Json defaults this codebase relied
///     on: case-insensitive property matching, number-from-string coercion, and non-HTML-escaping output.
/// </summary>
/// <remarks>
///     Binding is driven from the models' existing Newtonsoft attributes by
///     <see cref="NewtonsoftAttributeBindingModifier" /> (renames, non-public setters, private
///     <c>[JsonProperty]</c> members) rather than by per-member System.Text.Json attributes, so the migration
///     touches no members while both engines run side by side. The converters are registered here as
///     type-matched factories/instances — <see cref="AttributedObjectConverterFactory" /> for every
///     <see cref="Interfaces.IAttributed" />, and factories that read the Newtonsoft
///     <c>[JsonConverter]</c>/<c>ItemConverterType</c> attributes for tolerant enums, string-or-object types,
///     positional arrays, and value tuples — so the models need no System.Text.Json wiring before Phase 6.
///     The self-recursive socket/REST converters (event/boss, disappear, trade history, bank, login) register
///     from their own assemblies at the transport cutover, since AL.Core cannot reference their types.
/// </remarks>
public static class ALJson
{
    /// <summary>
    ///     The shared options instance. Built once and cached — constructing options is expensive and the
    ///     instance is frozen on first use.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            // Newtonsoft matched wire keys to members case-insensitively; STJ defaults to case-sensitive.
            PropertyNameCaseInsensitive = true,

            // the server sends numbers as JSON strings (and the reverse); Newtonsoft coerced by default.
            NumberHandling = JsonNumberHandling.AllowReadingFromString,

            // Newtonsoft tolerates a trailing comma before a closing ] or } by default; some frames carry one.
            AllowTrailingCommas = true,

            // Newtonsoft does not \uXXXX-escape < > & +; keep emitted payloads byte-identical for parity.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            // bind members from the models' Newtonsoft attributes without touching a single member.
            TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { NewtonsoftAttributeBindingModifier.Modify } }
        };

        // ordering matters: System.Text.Json takes the first converter whose CanConvert returns true. The
        // specific factories precede the type instances; none of their CanConvert predicates overlap.
        options.Converters.Add(new AttributedObjectConverterFactory());
        options.Converters.Add(new ArrayToObjectConverterFactory());
        options.Converters.Add(new NewtonsoftTolerantEnumConverterFactory());
        options.Converters.Add(new NewtonsoftStringOrObjectConverterFactory());
        options.Converters.Add(new NewtonsoftTupleConverterFactory());
        options.Converters.Add(new ALClassConverter());
        options.Converters.Add(new LenientBooleanConverter());
        options.Converters.Add(new LenientStringConverter());
        options.Converters.Add(new FalsyConverter<Stand>(Stand.None));
        options.Converters.Add(new ArrayToPointConverter());
        options.Converters.Add(new MapRectangleConverter());
        options.Converters.Add(new PolygonConverter());

        // last: claims [JsonObject] types System.Text.Json would otherwise treat as collections (IRectangle
        // containers like GGeometry), after every more specific converter has had its turn.
        options.Converters.Add(new ForcedObjectConverterFactory());

        return options;
    }
}
