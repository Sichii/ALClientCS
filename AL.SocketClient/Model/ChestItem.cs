using AL.SocketClient.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents an item looted out of a chest.
    /// </summary>
    public class ChestItem : ISimpleItem
    {
        public int Level { get; set; }

        /// <summary>
        ///     The name of the character that looted the item.
        /// </summary>
        [JsonProperty("looter")]
        public string LooterName { get; set; } = null!;

        public string Name { get; set; } = null!;

        [JsonProperty("q")]
        public int Quantity { get; set; } = 1;
    }
}