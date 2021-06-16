using Newtonsoft.Json;

namespace AL.SocketClient.Send
{
    public class AuthData : LoadedData
    {
        [JsonProperty("auth")]
        public string Auth { get; init; }

        [JsonProperty("character")]
        public string Character { get; init; }

        [JsonProperty("code_slot")]
        public string CodeSlot { get; init; }

        [JsonProperty("no_graphics")]
        public string NoGraphics { get; init; }

        [JsonProperty("no_html")]
        public string NoHtml { get; init; }

        [JsonProperty("passphrase")]
        public string Passphrase { get; init; }

        [JsonIgnore]
        public override int Success { get; init; } = 1;
        [JsonProperty("user")]
        public string User { get; init; }
    }
}