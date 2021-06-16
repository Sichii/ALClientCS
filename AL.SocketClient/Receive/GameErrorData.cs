using Newtonsoft.Json;

namespace AL.SocketClient.Receive
{
    public record GameErrorData
    {
        public string Message { get; set; }

        [JsonProperty("data")]
        private string Data
        {
            set => Message = value;
        }
    }
}