using Newtonsoft.Json;

namespace AL.SocketClient.Send
{
    public class LoadedData
    {
        [JsonProperty("height")]
        public int Height { get; init; }

        [JsonProperty("scale")]
        public string Scale { get; init; }

        [JsonProperty("success")]
        public virtual int Success { get; init; }

        [JsonProperty("width")]
        public int Width { get; init; }
    }
}