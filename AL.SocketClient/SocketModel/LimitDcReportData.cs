#region
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     The server's rate-limit telemetry, emitted immediately before it drops the connection with a
///     <c>
///         disconnect_reason
///     </c>
///     of
///     <c>
///         "limitdc"
///     </c>
///     (node/server.js:4366, 4379). Informational only - logged, never acted on.
/// </summary>
public sealed record LimitDcReportData
{
    /// <summary>
    ///     The call-cost ceiling that was exceeded.
    /// </summary>
    [JsonPropertyName("climit")]
    public double CallLimit { get; init; }

    /// <summary>
    ///     The accrued call-cost breakdown for the 4-second window: an
    ///     <b>
    ///         array
    ///     </b>
    ///     of
    ///     <c>
    ///         [timestamp, method, cost]
    ///     </c>
    ///     triples, one per run of consecutive same-method calls (add_call_cost, node/server_functions.js:4621, folds a
    ///     repeat into the previous entry rather than appending). Mixed element types, so kept raw.
    /// </summary>
    [JsonPropertyName("calls")]
    public JsonArray? Calls { get; init; }

    /// <summary>
    ///     The offending method - present only on the exception-path variant (node/server.js:4383).
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; init; }

    /// <summary>
    ///     Total calls made over the lifetime of the connection.
    /// </summary>
    [JsonPropertyName("total")]
    public long TotalCalls { get; init; }
}