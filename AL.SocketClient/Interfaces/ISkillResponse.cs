using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ISkillResponse : INameResponse
    {
        [JsonIgnore]
        string SkillName { get; }
    }
}