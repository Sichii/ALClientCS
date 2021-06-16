using System.Collections.Generic;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(ServerInfoDataConverter))]
    public record ServerInfoData
    {
        [JsonIgnore]
        public Dictionary<string, BossInfo> BossInfo { get; init; }

        [JsonProperty]
        public bool EggHunt { get; init; }

        [JsonProperty]
        public bool HolidaySeason { get; init; }

        [JsonProperty]
        public bool LunaryNewYear { get; init; }

        [JsonProperty]
        public bool Valentines { get; init; }
    }
}