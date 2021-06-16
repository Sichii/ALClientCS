using AL.SocketClient.Definitions;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IGameResponse
    {
        [JsonProperty("response")]
        GameResponseType ResponseType { get; }
    }
}