#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents all of the data for ongoing events and bosses for a server.
/// </summary>
public record EventAndBossInfo
{
    public bool EggHunt { get; init; }

    public bool HolidaySeason { get; init; }

    public bool LunaryNewYear { get; init; }

    public bool Valentines { get; init; }

    /// <summary>
    ///     Contains information about bosses on this server.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, BossInfo> BossInfo { get; } = new Dictionary<string, BossInfo>();
}