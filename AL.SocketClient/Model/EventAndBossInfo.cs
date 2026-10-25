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
    ///     If populated, when each running season with a set end time ends, in UTC unix milliseconds, keyed by season.
    /// </summary>
    [JsonPropertyName("event_ends")]
    public IReadOnlyDictionary<string, long>? EventEnds { get; init; }

    /// <summary>
    ///     If populated, which seasons are on, keyed by season. A season turns false here at its end time.
    /// </summary>
    [JsonPropertyName("event_flags")]
    public IReadOnlyDictionary<string, bool>? EventFlags { get; init; }

    /// <summary>Whether the Halloween event is running.</summary>
    /// <remarks>
    ///     While it is, <c>mrpumpkin</c> and <c>mrgreen</c> are also keys on <see cref="BossInfo" /> - live, or carrying the
    ///     time they next spawn. Off it, they are absent entirely.
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
    ///     If populated, the Witch's Cipher round. Also a key on <see cref="BossInfo" />, like every object-valued field.
    /// </summary>
    [JsonPropertyName("witchcipher")]
    public WitchCipherRound? WitchCipher { get; init; }

    /// <summary>
    ///     Contains information about bosses on this server.
    /// </summary>
    /// <remarks>
    ///     Every object-valued field of the snapshot lands here, so <c>schedule</c>, <c>duels</c>, <c>event_flags</c> and
    ///     <c>event_ends</c> are keys too. Filter
    ///     against <c>GameData.Events</c> to keep only the ones that are events.
    /// </remarks>
    [JsonIgnore]
    public IReadOnlyDictionary<string, BossInfo> BossInfo { get; } = new Dictionary<string, BossInfo>();

    /// <summary>
    ///     Builds the <c>G.events</c> keys of the seasonal events running on this server.
    /// </summary>
    /// <returns>
    ///     The running events' keys, compared case-insensitively.
    /// </returns>
    public IReadOnlySet<string> GetSeasonalEvents()
    {
        var events = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (EggHunt)
            events.Add("egghunt");

        if (Halloween)
            events.Add("halloween");

        if (HolidaySeason)
            events.Add("holidayseason");

        if (LunarNewYear)
            events.Add("lunarnewyear");

        if (Valentines)
            events.Add("valentines");

        //the anniversary carries its round state rather than a flag, so it arrives among the bosses
        if (BossInfo.ContainsKey("anniversary"))
            events.Add("anniversary");

        return events;
    }
}