using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Receive
{
    public record DisappearingTextData
    {
        public JToken Args { get; init; }
        public string Id { get; init; }
        public string Message { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
    }
}