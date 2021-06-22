using System.Collections.Generic;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(ServerInfoDataConverter))]
    public record ServerInfoData
    {
        [JsonIgnore]
        public IReadOnlyDictionary<string, BossInfo> BossInfo { get; init; } = new Dictionary<string, BossInfo>();

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