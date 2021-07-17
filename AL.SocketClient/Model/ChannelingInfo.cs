using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Provides information about the channeling of a skill.
    /// </summary>
    public record ChannelingInfo
    {
        /// <summary>
        ///     The remaining MS needed to complete channeling.
        /// </summary>
        [JsonProperty("ms")]
        public float RemainingMS { get; init; }
    }
}