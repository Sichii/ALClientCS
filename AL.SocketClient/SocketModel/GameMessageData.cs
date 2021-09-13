using AL.Core.Json.Converters;
using AL.Core.Json.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when a game error occurs.
    /// </summary>
    /// <seealso cref="IOptionalObject" />
    [JsonConverter(typeof(StringOrObjectConverter<GameMessageData>), nameof(Message))]
    public record GameMessageData : IOptionalObject
    {
        public bool ContainsData { get; init; }

        /// <summary>
        ///     The game error message.
        /// </summary>
        public string Message { get; init; } = null!;
    }
}