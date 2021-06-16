using System.Collections.Generic;
using AL.Core.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(AttributedObjectConverter<StartData>))]
    public record StartData : Character
    {
        [JsonProperty("base_gold")]
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> BaseGold { get; init; }

        [JsonProperty]
        public EntitiesData Entities { get; init; }

        [JsonProperty("s_info")]
        public ServerInfoData ServerInfo { get; init; }
    }
}