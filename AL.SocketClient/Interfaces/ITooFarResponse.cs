using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ITooFarResponse : ITargetedResponse, IPlaceResponse
    {
        [JsonProperty("dist")]
        int Distance { get; }
    }
}