using AL.Core.Interfaces;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;

namespace AL.Data.Geometry
{
    public record Tile : IPoint
    {
        [JsonProperty, JsonArrayIndex(3)]
        public float Size { get; init; }
        [JsonProperty, JsonArrayIndex(0)]
        public string TileSet { get; init; }
        [JsonProperty, JsonArrayIndex(4)]
        public float Unknown { get; init; }
        [JsonProperty, JsonArrayIndex(1)]
        public float X { get; init; }
        [JsonProperty, JsonArrayIndex(2)]
        public float Y { get; init; }
    }
}