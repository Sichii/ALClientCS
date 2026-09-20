#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.SocketModel;

public class SkillTimeoutData
{
    public float Penalty { get; set; }

    /// <summary>
    ///     If populated, why the timeout was sent. Only <c>calculate_player_stats</c> sets it, as "attack_ms".
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("name")]
    public string SkillName { get; set; } = null!;

    /// <summary>
    ///     The <c>attack_ms</c> correction sends <c>attack_ms</c> minus <c>mssince</c> , which is routinely negative and
    ///     fractional.
    /// </summary>
    [JsonPropertyName("ms")]
    public float TimeoutMs { get; set; }
}