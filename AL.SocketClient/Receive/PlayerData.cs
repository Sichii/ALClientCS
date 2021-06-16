using AL.Core.Json.Converters;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(AttributedObjectConverter<PlayerData>))]
    public record PlayerData : Player;
}