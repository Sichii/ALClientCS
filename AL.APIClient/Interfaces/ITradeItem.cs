using System.Collections.Generic;

namespace AL.APIClient.Interfaces
{
    /// <summary>
    ///     Represents an item for trade or sale.
    /// </summary>
    /// <seealso cref="ICommonItem" />
    public interface ITradeItem : ICommonItem
    {
        /// <summary>
        ///     Whether or not the item is being bought by a merchant.
        /// </summary>
        bool Buying { get; init; }

        /// <summary>
        ///     If populated, the remaining number of minutes left in the giveaway for this item.
        /// </summary>
        float? GiveawayMins { get; init; }

        /// <summary>
        ///     If populated, a list of names of the participants in the giveaway for this item.
        /// </summary>
        IReadOnlyList<string>? GiveawayParticipants { get; init; }

        /// <summary>
        ///     A unique id, required to be sent if buying or entering the giveaway for this item.
        /// </summary>
        string Id { get; init; }

        /// <summary>
        ///     The price of this item to buy.
        /// </summary>
        long Price { get; init; }
    }
}