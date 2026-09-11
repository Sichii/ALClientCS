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

    /// <summary>
    ///     Whether the Halloween event is running.
    /// </summary>
    /// <remarks>
    ///     While it is,
    ///     <c>
    ///         mrpumpkin
    ///     </c>
    ///     and
    ///     <c>
    ///         mrgreen
    ///     </c>
    ///     are also keys on <see cref="BossInfo" /> - live, or carrying the time they next spawn. Off it, they are absent
    ///     entirely.
    /// </remarks>
    public bool Halloween { get; init; }

    public bool HolidaySeason { get; init; }

    public bool LunarNewYear { get; init; }

    /// <summary>
    ///     When this server runs its scheduled events. Null only before the first snapshot arrives.
    /// </summary>
    public EventSchedule? Schedule { get; init; }

    public bool Valentines { get; init; }

    /// <summary>
    ///     Contains information about bosses on this server.
    /// </summary>
    /// <remarks>
    ///     Every object-valued field of the snapshot lands here, so
    ///     <c>
    ///         schedule
    ///     </c>
    ///     and
    ///     <c>
    ///         duels
    ///     </c>
    ///     are keys too. Filter against
    ///     <c>
    ///         GameData.Events
    ///     </c>
    ///     to keep only the ones that are events.
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyDictionary<string, BossInfo> BossInfo { get; } = new Dictionary<string, BossInfo>();
}