using AL.APIClient.Interfaces;

namespace AL.Client.Interfaces
{
    /// <summary>
    ///     Represents an item at a specific index within a collection of items.
    /// </summary>
    public interface IIndexer<out T> where T: ISimpleItem
    {
        /// <summary>
        ///     The index the item is at within it's collection.
        /// </summary>
        int Index { get; }

        /// <summary>
        ///     The item itself.
        /// </summary>
        T Item { get; }
    }
}