using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(AttributedObjectConverter<CharacterData>))]
    public record CharacterData : Character
    {
        [JsonProperty("hitchhikers")]
        public JArray[] ExtraEvents { get; init; }
    }
}