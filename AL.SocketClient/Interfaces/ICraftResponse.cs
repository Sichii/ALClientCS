using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ICraftResponse : INameResponse
    {
        [JsonIgnore]
        string ItemName { get; }
    }
}