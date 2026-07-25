#region
using System.Text.Json.Serialization;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Titles
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    [JsonObject(ItemConverterType = typeof(AttributedObjectConverter<GTitle>))]
    public class TitlesDatum : DatumBase<GTitle>
    {
        [JsonProperty("abtesting")]
        [JsonPropertyName("abtesting")]
        public GTitle Abtesting { get; init; } = null!;

        [JsonProperty("critmonger")]
        [JsonPropertyName("critmonger")]
        public GTitle Critmonger { get; init; } = null!;

        [JsonProperty("fast")]
        [JsonPropertyName("fast")]
        public GTitle Fast { get; init; } = null!;

        [JsonProperty("festive")]
        [JsonPropertyName("festive")]
        public GTitle Festive { get; init; } = null!;

        [JsonProperty("firehazard")]
        [JsonPropertyName("firehazard")]
        public GTitle Firehazard { get; init; } = null!;

        [JsonProperty("glitched")]
        [JsonPropertyName("glitched")]
        public GTitle Glitched { get; init; } = null!;

        [JsonProperty("gooped")]
        [JsonPropertyName("gooped")]
        public GTitle Gooped { get; init; } = null!;

        [JsonProperty("legacy")]
        [JsonPropertyName("legacy")]
        public GTitle Legacy { get; init; } = null!;

        [JsonProperty("lucky")]
        [JsonPropertyName("lucky")]
        public GTitle Lucky { get; init; } = null!;

        [JsonProperty("shiny")]
        [JsonPropertyName("shiny")]
        public GTitle Shiny { get; init; } = null!;

        [JsonProperty("sniper")]
        [JsonPropertyName("sniper")]
        public GTitle Sniper { get; init; } = null!;

        [JsonProperty("stomped")]
        [JsonPropertyName("stomped")]
        public GTitle Stomped { get; init; } = null!;

        [JsonProperty("superfast")]
        [JsonPropertyName("superfast")]
        public GTitle Superfast { get; init; } = null!;
    }
}