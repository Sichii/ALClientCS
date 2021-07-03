using AL.APIClient.Definitions;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    public record Server
    {
        [JsonProperty("name")]
        public ServerId Identifier { get; set; }
        [JsonProperty("addr")]
        public string IPAddress { get; init; }
        public string Key { get; init; }
        public int Players { get; init; }
        public int Port { get; init; }
        public ServerRegion Region { get; set; }
    }
}