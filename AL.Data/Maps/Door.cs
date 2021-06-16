using AL.Core.Geometry;
using AL.Core.Json.Attributes;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    public record Door : Rectangle
    {
        [JsonProperty, JsonArrayIndex(4)]
        public string DestinationMap { get; }
        [JsonProperty, JsonArrayIndex(5)]
        public int DestinationSpawnId { get; }

        [JsonProperty, JsonArrayIndex(3)]
        public new float Height => base.Height;
        [JsonProperty, JsonArrayIndex(6)]
        public float NearbySpawn { get; }

        [JsonProperty, JsonArrayIndex(2)]
        public new float Width => base.Width;
        [JsonProperty, JsonArrayIndex(0)]
        public new float X => base.X;

        [JsonProperty, JsonArrayIndex(1)]
        public new float Y => base.Y;

        [JsonConstructor]
        public Door(float x, float y, float width, float height, string destinationMap, int destinationSpawnId, float nearbySpawn)
            : base(x, y, width, height)
        {
            DestinationMap = destinationMap;
            DestinationSpawnId = destinationSpawnId;
            NearbySpawn = nearbySpawn;
        }
    }
}