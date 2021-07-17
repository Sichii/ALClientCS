using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.SocketClient.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    [JsonConverter(typeof(DisappearDataConverter))]
    public record DisappearData
    {
        public DisappearEffect Effect { get; init; }
        public string Id { get; init; }
        [JsonProperty("invis")]
        public bool Invisible { get; init; }
        public string Reason { get; init; }
        [JsonProperty("to")]
        public string ToMap { get; init; }
        [JsonIgnore]
        public Orientation? ToOrientation { get; internal set; }
        [JsonIgnore]
        public int? ToSpawnId { get; internal set; }
    }
}