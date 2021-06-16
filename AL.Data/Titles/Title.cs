using Newtonsoft.Json;

namespace AL.Data.Titles
{
    public record Title
    {
        public string Achievement { get; init; }
        [JsonProperty("type")]
        public string AffectsItem { get; init; }
        [JsonProperty("title")]
        public string Name { get; init; }
        public string Source { get; init; }
    }
}