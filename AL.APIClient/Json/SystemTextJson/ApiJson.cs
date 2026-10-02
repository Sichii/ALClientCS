#region
using System.Text.Json;
using AL.Core.Json;
#endregion

namespace AL.APIClient.Json.SystemTextJson;

/// <summary>
///     Provides the REST client's serializer options: <see cref="ALJson.Options" /> plus the converters for this
///     assembly's types, which AL.Core cannot reference.
/// </summary>
/// <remarks>
///     The API converters go first, so they win over the base factories.
/// </remarks>
public static class ApiJson
{
    /// <summary>
    ///     The REST serializer options, built once from a copy of the shared options.
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