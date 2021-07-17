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
        ///     TODO: Create an enum for this field (chest1, chest2, chest3, chest4, chest5, chest6)
        /// </summary>
        [JsonProperty("chest")]
        public string ChestType { get; set; } = null!;

        /// <summary>
        ///     The id of the chest. This will be needed when opening the chest.
        /// </summary>
        public string Id { get; set; } = null!;

        public string Map { get; set; } = null!;

        /// <summary>
        ///     The number of items the chest contains.
        /// </summary>
        [JsonProperty("items")]
        public int NumberOfItems { get; set; }

        /// <summary>
        ///     Whether or not the contents of this chest will be distributed to the party.
        /// </summary>
        public bool Party { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}