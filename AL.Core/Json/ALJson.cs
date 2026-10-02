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
///     Provides the System.Text.Json options shared by the socket transport, the REST client and the game-data loader.
/// </summary>
/// <remarks>
///     Tolerant enums carry <see cref="TolerantStringEnumConverterFactory" /> on the enum itself, since registering it here
///     would make every enum tolerant. Converters for socket and REST types register from their own assemblies.
/// </remarks>
public static class ALJson
{
    /// <summary>
    ///     The shared options instance, built once and frozen on first use.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,

            //the server sends some numbers as JSON strings
            NumberHandling = JsonNumberHandling.AllowReadingFromString,

            //some frames carry a trailing comma before a closing ] or }
            AllowTrailingCommas = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        //order matters: the first converter whose CanConvert matches wins
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

        //last, so every more specific converter claims its type first
        options.Converters.Add(new ForcedObjectConverterFactory());

        return options;
    }
}