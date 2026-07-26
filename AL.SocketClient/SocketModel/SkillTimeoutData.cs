#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

public class SkillTimeoutData
{
    public float Penalty { get; set; }

    /// <summary>
    ///     If populated, why the timeout was sent. Only
    ///     <c>
    ///         calculate_player_stats
    ///     </c>
    ///     sets it, as "attack_ms".
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("name")]
    public string SkillName { get; set; } = null!;

    //the attack_ms correction sends attack_ms minus mssince, which is routinely negative and fractional
    [JsonPropertyName("ms")]
    public float TimeoutMs { get; set; }
}