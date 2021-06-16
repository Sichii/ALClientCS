using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record DropData : ILocation
    {
        [JsonProperty("chest")]
        public string ChestType { get; set; }
        public string Id { get; set; }
        public string Map { get; set; }
        [JsonProperty("items")]
        public int NumberOfItems { get; set; }
        public bool Party { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}