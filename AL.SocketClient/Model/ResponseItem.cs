using AL.APIClient.Interfaces;
using AL.Core.Json.Interfaces;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents an item received via <see cref="AL.SocketClient.SocketModel.GameResponseData" />.
    /// </summary>
    public record ResponseItem : ISimpleItem, IOptionalObject
    {
        /// <summary>
        ///     The chance of upgrading/compounding the item.
        /// </summary>
        public float? Chance { get; init; }

        public bool ContainsData { get; init; }

        /// <summary>
        ///     The current grace of the item.
        /// </summary>
        public float? Grace { get; init; }

        /// <summary>
        ///     The current level of the item.
        /// </summary>
        public int? Level { get; init; }
        public string Name { get; init; } = null!;

        // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
        public int Quantity { get; } = 1;
    }
}