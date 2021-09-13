using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when a chest is dropped from an entity dying.
    /// </summary>
    /// <seealso cref="ILocation" />
    public record DropData : ILocation
    {
        /// <summary>
        ///     The type of chest that dropped. Fancier chest types drop more gold/better stuff.
        /// </summary>
        [JsonProperty("chest")]
        public ChestType ChestType { get; set; }

        /// <summary>
        ///     The id of the chest. This will be needed when opening the chest.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        ///     The number of items the chest contains.
        /// </summary>
        [JsonProperty("items")]
        public int ItemCount { get; set; }

        public string Map { get; set; } = null!;

        /// <summary>
        ///     If populated, this is the party this chest dropped for.
        /// </summary>
        public string? Party { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);
    }
}