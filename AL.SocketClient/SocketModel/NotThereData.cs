using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when a target is "not there".
    /// </summary>
    public record NotThereData
    {
        /// <summary>
        ///     The cause of this data.
        /// </summary>
        [JsonProperty("place")]
        public string Source { get; init; } = null!;
    }
}