using Newtonsoft.Json.Linq;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when disappearing text appears on the UI.
    /// </summary>
    public record DisappearingTextData
    {
        /// <summary>
        ///     If populated, contains various UI datas
        /// </summary>
        public JToken? Args { get; init; }

        /// <summary>
        ///     The id of the entity this text appears over.
        /// </summary>
        public string Id { get; init; } = null!;

        /// <summary>
        ///     The raw text that appears.
        /// </summary>
        public string Message { get; init; } = null!;

        public float X { get; init; }
        public float Y { get; init; }
    }
}