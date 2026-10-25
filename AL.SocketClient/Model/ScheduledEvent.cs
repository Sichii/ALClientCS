#region
using System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents one dated event in a server's published lineup. Times are UTC unix milliseconds.
/// </summary>
public sealed record ScheduledEvent
{
    /// <summary>
    ///     When the event starts, or for a repeating event its next or current start.
    /// </summary>
    [JsonPropertyName("at")]
    public long At { get; init; }

    /// <summary>If populated, when the event ended.</summary>
    [JsonPropertyName("ended")]
    public long? Ended { get; init; }

    /// <summary>
    ///     The event's <c>G.events</c> key, such as <c>crabxx</c> or <c>horseman</c>.
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; init; } = null!;

    /// <summary>
    ///     If populated, how often a repeating event comes round, in milliseconds. Set only on
    ///     <see cref="EventSchedule.Repeats" />.
    /// </summary>
    [JsonPropertyName("every")]
    public long? Every { get; init; }

    /// <summary>The occurrence's id, unique within the lineup.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    /// <summary>
    ///     If populated, the season the event only runs in, such as <c>halloween</c>.
    /// </summary>
    [JsonPropertyName("season")]
    public string? Season { get; init; }

    /// <summary>If populated, when the event actually started.</summary>
    [JsonPropertyName("started")]
    public long? Started { get; init; }

    /// <summary>
    ///     One of <c>scheduled</c>, <c>live</c>, <c>finished</c> or <c>missed</c>.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = null!;
}