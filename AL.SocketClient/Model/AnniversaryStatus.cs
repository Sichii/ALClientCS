#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     The server's answer on whether this character's next anniversary kiss pays, carried on the character frame while a
///     round is running. The game's kiss button is enabled on exactly this.
/// </summary>
public sealed record AnniversaryStatus
{
    /// <summary>The reason that says the kiss pays.</summary>
    public const string READY = "ready";

    /// <summary>
    ///     The realm (region and server name) the answer is for.
    /// </summary>
    [JsonPropertyName("realm")]
    public string? Realm { get; init; }

    /// <summary>
    ///     Why the kiss will not pay, or <see cref="READY" /> when it will. The other reasons the server writes:
    ///     <c>claimed</c>, <c>no_visit</c>, <c>no_round</c>, <c>host</c>, <c>target_unavailable</c>, <c>wrong_target</c>,
    ///     <c>merchant_home</c>, <c>realmfatigue</c>, <c>hopsickness</c>.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>
    ///     The round the answer is for. An answer for another round is stale.
    /// </summary>
    [JsonPropertyName("round")]
    public long? Round { get; init; }
}