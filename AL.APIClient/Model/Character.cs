using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    public record Character : IInstancedLocation
    {
        public string Id { get; init; }

        public string In { get; init; }
        public int Level { get; init; }
        public string Map { get; init; }
        public string Name { get; init; }
        public bool Online { get; init; }
        public string Secret { get; init; }
        [JsonProperty("server")]
        public string ServerKey { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
    }
}