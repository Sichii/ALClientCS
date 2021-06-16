using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface IBankOpxResponse : INameResponse
    {
        string Reason { get; }
        [JsonIgnore]
        string NameInBank { get; }
    }
}