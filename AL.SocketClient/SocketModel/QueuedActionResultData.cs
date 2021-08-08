using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when a queued action completes.
    /// </summary>
    public record QueuedActionResultData
    {
        /// <summary>
        ///     The type of the queued action this is the result for.
        /// </summary>
        [JsonProperty("type")]
        public QueuedActionType QueuedActionType { get; init; }
        /// <summary>
        ///     Whether or not the upgrade succeeded.
        /// </summary>
        public bool Success { get; init; }
    }
}