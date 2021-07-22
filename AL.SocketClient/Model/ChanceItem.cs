using AL.APIClient.Interfaces;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents an item, and the calculated details for upgrading/compounding it.
    /// </summary>
    public record ChanceItem : ISimpleItem
    {
        /// <summary>
        ///     The chance of upgrading/compounding the item.
        /// </summary>
        public float Chance { get; init; }

        /// <summary>
        ///     The current grace of the item.
        /// </summary>
        public float Grace { get; init; }

        /// <summary>
        ///     The current level of the item.
        /// </summary>
        public int Level { get; init; }
        public string Name { get; init; } = null!;

        // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
        public int Quantity { get; } = 1;
    }
}