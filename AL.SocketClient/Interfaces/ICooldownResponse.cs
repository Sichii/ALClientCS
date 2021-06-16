using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ICooldownResponse : IPlaceResponse, ITargetedResponse
    {
        [JsonProperty("ms")]
        string CooldownMS { get; }
        [JsonProperty("skill")]
        string SkillName { get; }
    }
}