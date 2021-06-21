using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ICooldownResponse : IPlaceResponse, ITargetedResponse
    {
        [JsonProperty("ms")]
        float CooldownMS { get; }
        [JsonProperty("skill")]
        string SkillName { get; }
    }
}