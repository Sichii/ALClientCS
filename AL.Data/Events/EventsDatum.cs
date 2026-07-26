#region
using System.Text.Json.Serialization;
#endregion

namespace AL.Data.Events;

/// <summary>
///     <inheritdoc />
/// </summary>
/// <seealso cref="DatumBase{T}" />
public class EventsDatum : DatumBase<GEvent>
{
    [JsonPropertyName("abtesting")]
    public GEvent Abtesting { get; init; } = null!;

    [JsonPropertyName("crabxx")]
    public GEvent Crabxx { get; init; } = null!;

    [JsonPropertyName("egghunt")]
    public GEvent Egghunt { get; init; } = null!;

    [JsonPropertyName("franky")]
    public GEvent Franky { get; init; } = null!;

    [JsonPropertyName("goobrawl")]
    public GEvent Goobrawl { get; init; } = null!;

    [JsonPropertyName("halloween")]
    public GEvent Halloween { get; init; } = null!;

    [JsonPropertyName("holidayseason")]
    public GEvent Holidayseason { get; init; } = null!;

    [JsonPropertyName("icegolem")]
    public GEvent Icegolem { get; init; } = null!;

    [JsonPropertyName("lunarnewyear")]
    public GEvent Lunarnewyear { get; init; } = null!;

    [JsonPropertyName("valentines")]
    public GEvent Valentines { get; init; } = null!;
}