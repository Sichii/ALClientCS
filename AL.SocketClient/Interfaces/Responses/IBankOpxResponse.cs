using AL.Core.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces.Responses
{
    [JsonConverter(typeof(ConcreteTypeConverter<GameResponseData>))]
    public interface IBankOpxResponse : INameResponse
    {
        [JsonIgnore]
        string NameInBank { get; }
        string Reason { get; }
    }
}