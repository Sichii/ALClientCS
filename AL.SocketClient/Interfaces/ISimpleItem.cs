using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    /// <summary>
    ///     Represents the simplest information possible about an item.
    /// </summary>
    public interface ISimpleItem
    {
        /// <summary>
        ///     The name of the item.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The quantity of the item.
        /// </summary>
        [JsonProperty("q")]
        int Quantity { get; }
    }
}