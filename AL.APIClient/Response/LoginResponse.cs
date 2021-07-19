using AL.APIClient.Json.Converters;
using Newtonsoft.Json;

namespace AL.APIClient.Response
{
    /// <summary>
    ///     Represents the data received when trying to log in.
    /// </summary>
    [JsonConverter(typeof(LoginResponseConverter))]
    public record LoginResponse
    {
        /// <summary>
        ///     The game, even on the electron client, is basically a website. <br />
        ///     If this is populated, you successfully logged in and this is the html response sent back.
        /// </summary>
        [JsonProperty("html")]
        public string? Html { get; init; }

        /// <summary>
        ///     If something went wrong when trying to log in, this is the error message.
        /// </summary>
        [JsonProperty("message")]
        public string? Message { get; init; }

        /// <summary>
        ///     TODO: unknown
        /// </summary>
        [JsonProperty("type")]
        public string? Type { get; init; }
    }
}