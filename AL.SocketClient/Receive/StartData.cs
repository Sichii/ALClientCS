using System.Collections.Generic;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(CharacterConverter<StartData>))]
    public record StartData : CharacterData
    {
        [JsonProperty("base_gold")]
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> BaseGold { get; init; } =
            new Dictionary<string, IReadOnlyDictionary<string, int>>();

        [JsonProperty]
        public EntitiesData Entities { get; init; }

        [JsonProperty("s_info"), JsonConverter(typeof(EventAndBossDataConverter))]
        public EventAndBossInfo EventAndBossInfo { get; init; }
    }
}