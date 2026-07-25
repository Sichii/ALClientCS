using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel;

/// <summary>
///     The server's rate-limit telemetry, emitted immediately before it drops the connection with a
///     <c>disconnect_reason</c> of <c>"limitdc"</c> (node/server.js:4366, 4379). Informational only -
///     logged, never acted on.
/// </summary>
public sealed record LimitDcReportData
{
    /// <summary>
    ///     The accrued call-cost breakdown for the 4-second window, keyed by call name (mixed value types,
    ///     so kept as a raw object).
    /// </summary>
    [JsonProperty("calls")]
    public JsonObject? Calls { get; init; }

    /// <summary>
    ///     The call-cost ceiling that was exceeded.
    /// </summary>
    [JsonProperty("climit")]
    public double CallLimit { get; init; }

    /// <summary>
    ///     Total calls made over the lifetime of the connection.
    /// </summary>
    [JsonProperty("total")]
    public long TotalCalls { get; init; }

    /// <summary>
    ///     The offending method - present only on the exception-path variant (node/server.js:4383).
    /// </summary>
    [JsonProperty("method")]
    public string? Method { get; init; }
}
