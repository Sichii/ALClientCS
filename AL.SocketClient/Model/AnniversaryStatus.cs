#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     The server's own answer on whether this character's next anniversary kiss pays, carried on the character frame
///     while a round is running. The game's kiss button is enabled on exactly this, so it is the whole of what a client
///     needs to ask before casting.
/// </summary>
public sealed record AnniversaryStatus
{
    /// <summary>
    ///     The reason that says the kiss pays.
    /// </summary>
    public const string READY = "ready";

    /// <summary>
    ///     The realm (region and server name) the answer is for.
    /// </summary>
    [JsonPropertyName("realm")]
    public string? Realm { get; init; }

    /// <summary>
    ///     Why the kiss will not pay, or <see cref="READY" /> when it will. The other reasons the server writes: claimed,
    ///     no_visit, no_round, host, target_unavailable, wrong_target, merchant_home, realmfatigue, hopsickness.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>
    ///     The round the answer is for. An answer for another round is stale.
    /// </summary>
    [JsonPropertyName("round")]
    public long? Round { get; init; }
}
