using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IBankOpxResponse : INameResponse
    {
        [JsonIgnore]
        string NameInBank { get; }
        string Reason { get; }
    }
}