using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
    public interface ICraftResponse : INameResponse
    {
        [JsonIgnore]
        string ItemName { get; }
    }
}