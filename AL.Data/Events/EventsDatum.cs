#region
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
        public GEvent Abtesting { get; init; } = null!;

        [JsonProperty("crabxx")]
        public GEvent Crabxx { get; init; } = null!;

        [JsonProperty("egghunt")]
        public GEvent Egghunt { get; init; } = null!;

        [JsonProperty("franky")]
        public GEvent Franky { get; init; } = null!;

        [JsonProperty("goobrawl")]
        public GEvent Goobrawl { get; init; } = null!;

        [JsonProperty("halloween")]
        public GEvent Halloween { get; init; } = null!;

        [JsonProperty("holidayseason")]
        public GEvent Holidayseason { get; init; } = null!;

        [JsonProperty("icegolem")]
        public GEvent Icegolem { get; init; } = null!;

        [JsonProperty("lunarnewyear")]
        public GEvent Lunarnewyear { get; init; } = null!;

        [JsonProperty("valentines")]
        public GEvent Valentines { get; init; } = null!;
    }
}