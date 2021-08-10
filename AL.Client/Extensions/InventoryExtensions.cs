using System;
using System.Collections.Generic;
using System.Linq;
using AL.Client.Model;
using AL.Core.Definitions;
using AL.SocketClient.Model;
using Chaos.Core.Extensions;

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace AL.Client.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for the <see cref="Character" />'s <see cref="Inventory" />.
    /// </summary>
    public static class InventoryExtensions
    {
        /// <summary>
        ///     Lazily enumerates all inventory in the inventory, providing a way to keep track of what slot the item was in.
        /// </summary>
        /// <param name="inventory">The inventory to index.</param>
        /// <returns><see cref="IEnumerable{T}" /> of <see cref="IndexedInventoryItem" /> <br /></returns>
        /// <exception cref="ArgumentNullException">inventory</exception>
        public static IEnumerable<IndexedInventoryItem> AsIndexed(this Inventory inventory)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            return inventory.Select((item, index) => item == null
                    ? null
                    : new IndexedInventoryItem
                    {
                        Index = index,
                        Item = item
                    })
                .Where(indexed => indexed != null)!;
        }

        /// <summary>
        ///     Checks the inventory to see if it contains an item with the given name.
        /// </summary>
        /// <param name="inventory">The character's <see cref="Inventory" />.</param>
        /// <param name="itemName">The name of the item to search for.</param>
        /// <returns>
        ///     <see cref="bool" /> <br />
        ///     <c>true</c> if an item with the name was found, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">inventory</exception>
        /// <exception cref="ArgumentNullException">itemName</exception>
        public static bool ContainsItem(this Inventory inventory, string itemName)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            if (string.IsNullOrEmpty(itemName))
                throw new ArgumentNullException(nameof(inventory));

            return inventory.Any(item => (item != null) && item.Name.EqualsI(itemName));
        }

        /// <summary>
        ///     Checks the inventory for all inventory with the given name and totals them up.
        /// </summary>
        /// <param name="inventory">The character's <see cref="Inventory" />.</param>
        /// <param name="itemName">The name of the item to search for.</param>
        /// <returns>
        ///     <see cref="int" /> <br />
        ///     The number of inventory with the given name. (counts inventory in stacks)
        /// </returns>
        /// <exception cref="ArgumentNullException">inventory</exception>
        /// <exception cref="ArgumentNullException">itemName</exception>
        public static int CountOf(this Inventory inventory, string itemName)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            if (string.IsNullOrEmpty(itemName))
                throw new ArgumentNullException(nameof(itemName));

            return inventory.Where(item => (item != null) && item.Name.EqualsI(itemName)).Sum(item => item!.Quantity);
        }

        /// <summary>
        ///     Finds the first item in the inventory that is not null, and meets the predicate conditions.
        /// </summary>
        /// <param name="inventory">The character's <see cref="Inventory"/>.</param>
        /// <param name="predicate">A function that returns true or false for a given <see cref="InventoryItem"/>.</param>
        /// <returns>
        ///     <see cref="IndexedInventoryItem" /> <br />
        ///     The item, and informaiton about what slot it is in, or <c>null</c> if no item was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">inventory</exception>
        /// <exception cref="ArgumentNullException">predicate</exception>
        public static IndexedInventoryItem? FindItem(this Inventory inventory, Func<InventoryItem, bool> predicate)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));
            
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            
            var index = inventory.FindIndex(item => (item != null) && predicate(item));

            if (index == -1)
                return null;

            return new IndexedInventoryItem
            {
                Index = index,
                Item = inventory[index]!
            };
        }

        /// <summary>
        ///     Finds the first item in the inventory that meets the conditions.
        /// </summary>
        /// <param name="inventory">The character's <see cref="Inventory" />.</param>
        /// <param name="itemName">The name of the item to search for. Leave null to ignore name.</param>
        /// <param name="level">Overrides levelMin/levelMax. Specifies exact level to look for.</param>
        /// <param name="quantity">Overrides quantityMin/quantityMax. Specifies exact quantity to look for.</param>
        /// <param name="levelMin">The item must have at least this level.</param>
        /// <param name="levelMax">The item must have at most this level.</param>
        /// <param name="quantityMin">The item must have a minimum of this quantity.</param>
        /// <param name="quantityMax">The item must have a maximum of this quantity.</param>
        /// <param name="itemType">The item must be of this type.</param>
        /// <returns>
        ///     <see cref="IndexedInventoryItem" /> <br />
        ///     The item, and informaiton about what slot it is in, or <c>null</c> if no item was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">inventory</exception>
        public static IndexedInventoryItem? FindItem(
            this Inventory inventory,
            string? itemName = null,
            int? level = null,
            int? quantity = null,
            int levelMin = int.MinValue,
            int levelMax = int.MaxValue,
            int quantityMin = int.MinValue,
            int quantityMax = int.MaxValue)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

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

            var index = inventory.FindIndex(item =>
                (item != null)
                && (item.Level >= levelMin)
                && (item.Level <= levelMax)
                && (item.Quantity >= quantityMin)
                && (item.Quantity <= quantityMax)
                && ((itemName == null) || item.Name.EqualsI(itemName)));

            if (index == -1)
                return null;

            return new IndexedInventoryItem
            {
                Index = index,
                Item = inventory[index]!
            };
        }
    }
}