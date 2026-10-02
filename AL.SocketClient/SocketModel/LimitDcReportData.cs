#region
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the server's rate-limit telemetry, sent just before it drops the connection with a
///     <c>disconnect_reason</c> of <c>"limitdc"</c>. Logged, never acted on.
/// </summary>
public sealed record LimitDcReportData
{
    /// <summary>The call-cost ceiling that was exceeded.</summary>
    [JsonPropertyName("climit")]
    public double CallLimit { get; init; }

    /// <summary>
    ///     The accrued call-cost breakdown for the window: an array of <c>[timestamp, method, cost]</c> triples, one per run
    ///     of consecutive same-method calls. Mixed element types, so kept raw.
    /// </summary>
    [JsonPropertyName("calls")]
    public JsonArray? Calls { get; init; }

    /// <summary>
    ///     If populated, the offending method. Present only on the exception-path variant.
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; init; }

    /// <summary>
    ///     Total calls made over the lifetime of the connection.
    /// </summary>
    [JsonPropertyName("total")]
    public long TotalCalls { get; init; }
}