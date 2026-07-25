#region
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Events
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class EventsDatum : DatumBase<GEvent>
    {
        [JsonProperty("abtesting")]
        [JsonPropertyName("abtesting")]
        public GEvent Abtesting { get; init; } = null!;

        [JsonProperty("crabxx")]
        [JsonPropertyName("crabxx")]
        public GEvent Crabxx { get; init; } = null!;

        [JsonProperty("egghunt")]
        [JsonPropertyName("egghunt")]
        public GEvent Egghunt { get; init; } = null!;

        [JsonProperty("franky")]
        [JsonPropertyName("franky")]
        public GEvent Franky { get; init; } = null!;

        [JsonProperty("goobrawl")]
        [JsonPropertyName("goobrawl")]
        public GEvent Goobrawl { get; init; } = null!;

        [JsonProperty("halloween")]
        [JsonPropertyName("halloween")]
        public GEvent Halloween { get; init; } = null!;

        [JsonProperty("holidayseason")]
        [JsonPropertyName("holidayseason")]
        public GEvent Holidayseason { get; init; } = null!;

        [JsonProperty("icegolem")]
        [JsonPropertyName("icegolem")]
        public GEvent Icegolem { get; init; } = null!;

        [JsonProperty("lunarnewyear")]
        [JsonPropertyName("lunarnewyear")]
        public GEvent Lunarnewyear { get; init; } = null!;

        [JsonProperty("valentines")]
        [JsonPropertyName("valentines")]
        public GEvent Valentines { get; init; } = null!;
    }
}