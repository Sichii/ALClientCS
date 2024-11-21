#region
using Newtonsoft.Json;
#endregion

namespace AL.Data.Titles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class TitlesDatum : DatumBase<GTitle>
    {
        [JsonProperty("abtesting")]
        public GTitle Abtesting { get; init; } = null!;

        [JsonProperty("critmonger")]
        public GTitle Critmonger { get; init; } = null!;

        [JsonProperty("fast")]
        public GTitle Fast { get; init; } = null!;

        [JsonProperty("festive")]
        public GTitle Festive { get; init; } = null!;

        [JsonProperty("firehazard")]
        public GTitle Firehazard { get; init; } = null!;

        [JsonProperty("glitched")]
        public GTitle Glitched { get; init; } = null!;

        [JsonProperty("gooped")]
        public GTitle Gooped { get; init; } = null!;

        [JsonProperty("legacy")]
        public GTitle Legacy { get; init; } = null!;

        [JsonProperty("lucky")]
        public GTitle Lucky { get; init; } = null!;

        [JsonProperty("shiny")]
        public GTitle Shiny { get; init; } = null!;

        [JsonProperty("sniper")]
        public GTitle Sniper { get; init; } = null!;

        [JsonProperty("stomped")]
        public GTitle Stomped { get; init; } = null!;

        [JsonProperty("superfast")]
        public GTitle Superfast { get; init; } = null!;
    }
}