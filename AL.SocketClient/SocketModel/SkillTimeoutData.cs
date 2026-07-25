using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel;

public class SkillTimeoutData
{
    //the attack_ms correction sends attack_ms minus mssince, which is routinely negative and fractional
    [JsonProperty("ms")]
    public float TimeoutMs { get; set; }
    [JsonProperty("name")]
    public string SkillName { get; set; } = null!;
    public float Penalty { get; set; }
    /// <summary>
    ///     If populated, why the timeout was sent. Only <c>calculate_player_stats</c> sets it, as "attack_ms".
    /// </summary>
    [JsonProperty("reason")]
    public string? Reason { get; set; }
}