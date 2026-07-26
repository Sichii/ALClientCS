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
///     The single canonical System.Text.Json configuration, shared by the socket transport, the REST client, and the
///     game-data loader. It reproduces the Newtonsoft.Json defaults this codebase relied on: case-insensitive property
///     matching, number-from-string coercion, and non-HTML-escaping output.
/// </summary>
/// <remarks>
///     Binding comes entirely from the models' own System.Text.Json attributes —
///     <c>
///         [JsonPropertyName]
///     </c>
///     for renames,
///     <c>
///         [JsonInclude]
///     </c>
///     for non-public setters and fields,
///     <c>
///         [JsonIgnore]
///     </c>
///     for exclusions. A member with a non-public setter and no
///     <c>
///         [JsonInclude]
///     </c>
///     (e.g.
///     <c>
///         EntityBase.In
///     </c>
///     ) stays unbound by construction, which is why the transitional resolver modifier could be removed outright rather
///     than reproduced. The converters are registered here as type-matched factories/instances —
///     <see cref="AttributedObjectConverterFactory" /> for every <see cref="AL.Core.Interfaces.IAttributed" />, and factories
///     keyed on the AL-local markers (
///     <c>
///         [JsonStringOrObject]
///     </c>
///     ,
///     <c>
///         [JsonArrayIndex]
///     </c>
///     ,
///     <c>
///         [JsonForcedObject]
///     </c>
///     ) that carry a parameter a System.Text.Json
///     <c>
///         [JsonConverter]
///     </c>
///     attribute cannot. Tolerant enums need no marker — they carry
///     <c>
///         [JsonConverter(typeof(TolerantStringEnumConverterFactory))]
///     </c>
///     directly, and registering that factory here would make every enum tolerant. The self-recursive socket/REST
///     converters (event/boss, disappear, trade history, bank, login) register from their own assemblies at the transport
///     cutover, since AL.Core cannot reference their types.
/// </remarks>
public static class ALJson
{
    /// <summary>
    ///     The shared options instance. Built once and cached — constructing options is expensive and the instance is frozen
    ///     on first use.
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

            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        // ordering matters: System.Text.Json takes the first converter whose CanConvert returns true. The
        // specific factories precede the type instances; none of their CanConvert predicates overlap.
        options.Converters.Add(new AttributedObjectConverterFactory());
        options.Converters.Add(new ArrayToObjectConverterFactory());
        options.Converters.Add(new StringOrObjectConverterFactory());
        options.Converters.Add(new TupleConverterFactory());
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