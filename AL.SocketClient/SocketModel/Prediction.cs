using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record Prediction
    {
        public float Chance { get; init; }
        public int Level { get; init; }
        public string Name { get; init; }
        //TODO: What's this?
        public int[] Nums { get; init; }
        [JsonProperty("scroll")]
        public string ScrollName { get; init; }
        public bool Success { get; init; }
    }
}