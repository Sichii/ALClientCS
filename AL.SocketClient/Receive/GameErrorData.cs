using AL.Core.Json.Converters;
using AL.Core.Json.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    [JsonConverter(typeof(StringOrObjectConverter<GameErrorData>), nameof(Message))]
    public record GameErrorData : IOptionalObject
    {
        public bool ContainsData { get; init; }
        public string Message { get; set; }
    }
}