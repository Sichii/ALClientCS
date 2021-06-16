using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ITargetedResponse : IGameResponse
    {
        [JsonProperty("id")]
        string TargetID { get; }
    }
}