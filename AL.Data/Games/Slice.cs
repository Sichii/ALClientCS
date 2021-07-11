using AL.Data.Json.Converters;
using Newtonsoft.Json;

#nullable disable

namespace AL.Data.Games
{
    [JsonConverter(typeof(ArrayToSliceConverter))]
    public record Slice
    {
        public int Amount { get; init; } = 1;
        public string Description { get; init; }
        public string RewardName { get; init; }
        public string RewardType { get; init; }
        public string SliceName { get; init; }
    }
}