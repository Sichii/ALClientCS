using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface INameResponse : IGameResponse
    {
        [JsonIgnore]
        internal string Name { get; }
    }
}