using AL.Core.Definitions;
using AL.Core.Geometry;
using AL.SocketClient.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when an entity disappears.
    /// </summary>
    [JsonConverter(typeof(DisappearDataConverter))]
    public record DisappearData
    {
        /// <summary>
        ///     GUI related. The effect that plays to make the entity disappear.
        /// </summary>
        public DisappearEffect Effect { get; init; }

        /// <summary>
        ///     The ID of the entity disappearing.
        /// </summary>
        public string Id { get; init; } = null!;

        /// <summary>
        ///     TODO: unknown
        /// </summary>
        [JsonProperty("invis")]
        public bool Invisible { get; init; }

        /// <summary>
        ///     The reason the entity disappeared.
        /// </summary>
        public string Reason { get; init; } = string.Empty;

        /// <summary>
        ///     If populated, the entity transported to another map. <br />
        ///     This is the accessor of the map they transported to.
        /// </summary>
        [JsonProperty("to")]
        public string? ToMap { get; init; }

        /// <summary>
        ///     If populated, the entity transported to another map. <br />
        ///     This is the orientation they had as they appeared in the new map.
        /// </summary>
        [JsonIgnore]
        public Orientation? ToOrientation { get; internal set; }

        /// <summary>
        ///     If populated, the entity transported to another map. <br />
        ///     This is id of the spawn they teleported to.
        /// </summary>
        [JsonIgnore]
        public int? ToSpawnId { get; internal set; }
    }
}