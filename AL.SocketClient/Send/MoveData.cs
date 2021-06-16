using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Send
{
    public class MoveData : IPoint
    {
        [JsonProperty("going_x")]
        public float GoingX { get; init; }
        [JsonProperty("going_y")]
        public float GoingY { get; init; }
        public int M { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
    }
}