using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when switching maps.
    /// </summary>
    /// <seealso cref="IInstancedLocation" />
    /// <seealso cref="IOriented" />
    public record NewMapData : IInstancedLocation, IOriented
    {
        public Direction Direction { get; init; }

        /// <summary>
        ///     GUI related. The effect that plays to make your character disappear from the previous map.
        /// </summary>
        public DisappearEffect Effect { get; init; }

        /// <summary>
        ///     A full entities update for the entities on the new map.
        /// </summary>
        public EntitiesData Entities { get; init; } = null!;
        public string In { get; init; } = null!;

        /// <summary>
        ///     TODO: unknown, always empty? maybe map specific info?
        /// </summary>
        public JObject? Info { get; init; }

        [JsonProperty("name")]
        public string Map { get; init; } = null!;

        /// <summary>
        ///     The map change count value for this map change.
        /// </summary>
        [JsonProperty("m")]
        public ulong MapChangeCount { get; init; }

        public float X { get; init; }
        public float Y { get; init; }
        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
    }
}