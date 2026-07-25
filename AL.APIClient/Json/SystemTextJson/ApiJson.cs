#region
using System.Text.Json;
using AL.Core.Json;
#endregion

namespace AL.APIClient.Json.SystemTextJson;

/// <summary>
///     The REST client's System.Text.Json configuration: the canonical <see cref="ALJson.Options" /> layered
///     with the API-only converters that AL.Core cannot reference (they target types in this assembly). Used by
///     request-body serialization and every response deserialize.
/// </summary>
/// <remarks>
///     The API converters are inserted at the front so they win the first-CanConvert-wins resolution over the
///     base factories. <see cref="LoginResponseConverter" /> and <see cref="StringOrObjectMailItemConverter" />
///     each target a single type used only in this shape, so global registration matches the Newtonsoft
///     per-property <c>[JsonConverter]</c> they replace.
/// </remarks>
public static class ApiJson
{
    /// <summary>
    ///     The REST options instance. Built once from a copy of the shared options.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(ALJson.Options);

        options.Converters.Insert(0, new LoginResponseConverter());
        options.Converters.Insert(1, new StringOrObjectMailItemConverter());

        return options;
    }
}
