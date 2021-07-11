using AL.Core.Json.Converters;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
    public interface IGameResponse
    {
        [JsonProperty("response")]
        GameResponseType ResponseType { get; }
    }
}