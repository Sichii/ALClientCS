using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Receive
{
    public record NewMapData : ILocation, IOriented
    {
        public Direction Direction { get; init; }
        public DisappearEffect Effect { get; init; }
        public EntitiesData Entities { get; init; }
        public string In { get; init; }
        public JObject Info { get; init; }
        public float M { get; init; }
        [JsonProperty("name")]
        public string Map { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
    }
}