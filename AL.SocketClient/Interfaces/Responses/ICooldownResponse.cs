using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
    public interface ICooldownResponse : IPlaceResponse, ITargetedResponse
    {
        [JsonProperty("ms")]
        float CooldownMS { get; }
        [JsonProperty("skill")]
        string SkillName { get; }
    }
}