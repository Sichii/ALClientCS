using System;
using System.Collections.Generic;
using System.Linq;
using AL.Client.Model;
using AL.SocketClient.Model;
using Chaos.Core.Extensions;

namespace AL.Client.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for the <see cref="ALClient" />'s <see cref="ALClient.Bank" />
    /// </summary>
    public static class BankExtensions
    {
        /// <summary>
        ///     Lazily enumerates all items in all banks, providing a way to keep track of what bank the item was in, and the slot
        ///     within the bank.
        /// </summary>
        /// <param name="bank">The client's <see cref="BankInfo" />.</param>
        /// <returns>
        ///     <see cref="IEnumerable{T}" /> of <see cref="BankIndexer" /> <br />
        ///     A lazy enumeration of banked items.
        /// </returns>
        /// <exception cref="ArgumentNullException">bank</exception>
        public static IEnumerable<BankIndexer> AsIndexed(this BankInfo bank)
        {
            if (bank == null)
                throw new ArgumentNullException(nameof(bank));

            return bank.SelectMany(kvp => kvp.Value.Select((item, index) => item == null
                    ? null
                    : new BankIndexer
                    {
                        BankPack = kvp.Key,
                        Index = index,
                        Item = item
                    }))
                .Where(bankedItem => bankedItem != null)!;
        }

        /// <summary>
        ///     Checks the bank to see if it contains an item with the given name.
        /// </summary>
        /// <param name="bank">The client's <see cref="BankInfo" />.</param>
        /// <param name="itemName">The name of the item to search for.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if an item with the name was found, otherwise <c>false</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">bank</exception>
        /// <exception cref="ArgumentNullException">itemName</exception>
        public static bool ContainsItem(this BankInfo bank, string itemName)
        {
            if (bank == null)
                throw new ArgumentNullException(nameof(bank));

            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentNullException(nameof(itemName));

            return bank.Values.Any(items => items.Any(item => (item != null) && item.Name.EqualsI(itemName)));
        }

        /// <summary>
        ///     Checks the bank for all items with the given name and totals them up.
        /// </summary>
        /// <param name="bank">The client's <see cref="BankInfo" />.</param>
        /// <param name="itemName">The name of the item to search for.</param>
        /// <returns>
        ///     <see cref="int" /> <br />
        ///     The number of items with the given name. (counts items in stacks)
        /// </returns>
        /// <exception cref="ArgumentNullException">bank</exception>
        /// <exception cref="ArgumentNullException">itemName</exception>
        public static int CountOf(this BankInfo bank, string itemName)
        {
            if (bank == null)
                throw new ArgumentNullException(nameof(bank));

            if (string.IsNullOrEmpty(itemName))
                throw new ArgumentNullException(nameof(itemName));

            return bank.Values.SelectMany(items => items)
                .Where(item => (item != null) && item.Name.EqualsI(itemName))
                .Sum(item => item!.Quantity);
        }

        /// <summary>
        ///     Finds the first item in the bank that is not null, and meets the predicate conditions.
        /// </summary>
        /// <param name="bank">The client's <see cref="BankInfo" />.</param>
        /// <param name="predicate">A function that returns true or false for a given <see cref="Item" />.</param>
        /// <returns>
        ///     <see cref="InventoryIndexer" /> <br />
        ///     The item, and informaiton about what slot it is in, or <c>null</c> if no item was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">bank</exception>
        /// <exception cref="ArgumentNullException">predicate</exception>
        public static BankIndexer? FindItem(this BankInfo bank, Func<Item, bool> predicate)
        {
            if (bank == null)
                throw new ArgumentNullException(nameof(bank));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            foreach ((var bankPack, var items) in bank)
            {
                var index = items.FindIndex(item => (item != null) && predicate(item));

                if (index == -1)
                    continue;

                return new BankIndexer
                {
                    BankPack = bankPack,
                    Index = index,
                    Item = items[index]!
                };
            }

            return null;
        }

        /// <summary>
        ///     Finds the first item in the bank that meets the conditions.
        /// </summary>
        /// <param name="bank">The client's <see cref="BankInfo" />.</param>
        /// <param name="itemName">The name of the item to search for. Leave null to ignore name.</param>
        /// <param name="level">Overrides levelMin/levelMax. Specifies exact level to look for.</param>
        /// <param name="quantity">Overrides quantityMin/quantityMax. Specifies exact quantity to look for.</param>
        /// <param name="levelMin">The item must have at least this level.</param>
        /// <param name="levelMax">The item must have at most this level.</param>
        /// <param name="quantityMin">The item must have a minimum of this quantity.</param>
        /// <param name="quantityMax">The item must have a maximum of this quantity.</param>
        /// <returns>
        ///     <see cref="BankIndexer" /> <br />
        ///     The item, and information about what bank and slot it is in, or <c>null</c> if no item was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">bank</exception>
        public static BankIndexer? FindItem(
            this BankInfo bank,
            string? itemName = null,
            int? level = null,
            int? quantity = null,
            int levelMin = int.MinValue,
            int levelMax = int.MaxValue,
            int quantityMin = int.MinValue,
            int quantityMax = int.MaxValue)
        {
            if (bank == null)
                throw new ArgumentNullException(nameof(bank));

            if (level.HasValue)
            {
                levelMin = level.Value;
                levelMax = level.Value;
            }

            if (quantity.HasValue)
            {
                quantityMin = quantity.Value;
                quantityMax = quantity.Value;
            }

            foreach ((var bankPack, var items) in bank)
            {
                var index = items.FindIndex(item =>
                    (item != null)
                    && (item.Level >= levelMin)
                    && (item.Level <= levelMax)
                    && (item.Quantity >= quantityMin)
                    && (item.Quantity <= quantityMax)
                    && ((itemName == null) || item.Name.EqualsI(itemName)));

                if (index == -1)
                    continue;

                return new BankIndexer
                {
                    BankPack = bankPack,
                    Index = index,
                    Item = items[index]!
                };
            }

            return null;
        }
    }
}