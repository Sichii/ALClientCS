#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     When a server runs its scheduled events. Sent whole inside the server info snapshot, on login and on every change.
/// </summary>
/// <remarks>
///     The server publishes seven days ahead in <see cref="Events" />, and keeps that lineup across restarts.
/// </remarks>
public sealed record EventSchedule
{
    /// <summary>The hours a daily event starts at.</summary>
    public IReadOnlyList<int> Dailies { get; init; } = [];

    /// <summary>
    ///     The dated events of the next seven days, plus finished and missed ones for a few hours, earliest first.
    /// </summary>
    [JsonPropertyName("events")]
    public IReadOnlyList<ScheduledEvent> Events { get; init; } = [];

    /// <summary>
    ///     Whether the server clock currently reads night, which is what gates the nightly monsters.
    /// </summary>
    public bool Night { get; init; }

    /// <summary>The hours a nightly event starts at.</summary>
    public IReadOnlyList<int> Nightlies { get; init; } = [];

    /// <summary>
    ///     If populated, the realm the lineup is for: region and server name, such as <c>EUI</c>.
    /// </summary>
    [JsonPropertyName("realm")]
    public string? Realm { get; init; }

    /// <summary>
    ///     The events that come round several times a day, one entry each: the live occurrence, or else the next one.
    /// </summary>
    [JsonPropertyName("repeats")]
    public IReadOnlyList<ScheduledEvent> Repeats { get; init; } = [];

    /// <summary>
    ///     The server's clock when it published the lineup, in UTC unix milliseconds.
    /// </summary>
    [JsonPropertyName("server_time")]
    public long ServerTime { get; init; }

    /// <summary>
    ///     Whole hours the server's own clock runs ahead of UTC: +1 EU, -5 US, +7 ASIA. Every hour above is on that clock.
    /// </summary>
    [JsonPropertyName("time_offset")]
    public int TimeOffset { get; init; }
}
