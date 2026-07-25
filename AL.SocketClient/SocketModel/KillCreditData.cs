using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents authoritative kill credit for a monster (node/server.js:2545). More reliable than inferring
///     a kill from the 'death' event.
/// </summary>
public sealed record KillCreditData
{
    /// <summary>
    ///     The monster type id that was credited (e.g. "goo", "crab", "bee").
    /// </summary>
    [JsonProperty("mtype")]
    public string MonsterType { get; init; } = null!;
}
