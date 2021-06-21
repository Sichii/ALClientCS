using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ISkillNameResponse : INameResponse
    {
        [JsonIgnore]
        string SkillName { get; }
    }
}