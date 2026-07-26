#region
using System;
using System.Text.Json;
using AL.APIClient.Json.SystemTextJson;
using AL.Core.Json;
using AL.SocketClient.Json.SystemTextJson;
#endregion

namespace AL.Tests;

/// <summary>
///     Deserializes test payloads through the production System.Text.Json options, so the characterization and
///     DTO-contract tests exercise the same facade production uses. Replaces the
///     <c>
///         DeserializeObject&lt;T&gt;(json)
///     </c>
///     call the pre-migration tests made — same shape, so a call site changed only its prefix.
/// </summary>
internal static class TestJson
{
    /// <summary>
    ///     REST payloads, through <see cref="ApiJson.Options" />.
    /// </summary>
    public static T? Api<T>(string json) => JsonSerializer.Deserialize<T>(json, ApiJson.Options);

    /// <summary>
    ///     G-data / core-model payloads, through <see cref="ALJson.Options" />.
    /// </summary>
    public static T? Data<T>(string json) => JsonSerializer.Deserialize<T>(json, ALJson.Options);

    /// <summary>
    ///     Runtime-typed payloads, for the reflection-driven matrices that have no compile-time
    ///     <c>
    ///         T
    ///     </c>
    ///     .
    /// </summary>
    public static object? Data(string json, Type type) => JsonSerializer.Deserialize(json, type, ALJson.Options);

    /// <summary>
    ///     Outbound wire form, through the options the socket transport actually emits with (
    ///     <c>
    ///         ALSocketClient
    ///     </c>
    ///     installs <see cref="SocketJson.Options" />).
    /// </summary>
    /// <remarks>
    ///     Always go through this rather than calling
    ///     <c>
    ///         JsonSerializer.Serialize
    ///     </c>
    ///     inline: with default options
    ///     <c>
    ///         TradeSlot.Trade1
    ///     </c>
    ///     emits as
    ///     <c>
    ///         17
    ///     </c>
    ///     rather than
    ///     <c>
    ///         "trade1"
    ///     </c>
    ///     , and the server acts on it — the exact slip that once destroyed real inventory. One forgotten options argument is
    ///     all it takes.
    /// </remarks>
    public static string Emit<T>(T value) => JsonSerializer.Serialize(value, SocketJson.Options);

    /// <summary>
    ///     Socket-frame payloads, through <see cref="SocketJson.Options" />.
    /// </summary>
    public static T? Socket<T>(string json) => JsonSerializer.Deserialize<T>(json, SocketJson.Options);
}