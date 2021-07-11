using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
    public interface ITargetedResponse : IGameResponse
    {
        [JsonProperty("id")]
        string TargetID { get; }
    }
}