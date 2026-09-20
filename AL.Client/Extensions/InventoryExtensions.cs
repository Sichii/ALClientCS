#region
using AL.Client.Model;
using AL.Core.Extensions;
using AL.SocketClient.Model;
using Chaos.Extensions.Common;
#endregion

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace AL.Client.Extensions;

/// <summary>
///     Provides a set of extensions for the <see cref="Character" />'s <see cref="Inventory" />.
/// </summary>
public static class InventoryExtensions
{
    /// <summary>
    ///     Lazily enumerates all inventory in the inventory, providing a way to keep track of what slot the item was in.
    /// </summary>
    /// <param name="inventory">The inventory to index.</param>
    /// <returns>
    ///     <see cref="IEnumerable{T}" /> of <see cref="InventoryIndexer" />
    ///     <br />
    /// </returns>
    /// <exception cref="ArgumentNullException">inventory</exception>
    public static IEnumerable<InventoryIndexer> AsIndexed(this Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        return inventory.Select((item, index) => item == null
                            ? null
                            : new InventoryIndexer
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
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if an item with the name was found, otherwise <c>false</c> .
    /// </returns>
    /// <exception cref="ArgumentNullException">inventory</exception>
    /// <exception cref="ArgumentNullException">itemName</exception>
    public static bool ContainsItem(this Inventory inventory, string itemName)
    {
        ArgumentNullException.ThrowIfNull(inventory);

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
    ///     <see cref="int" />
    ///     <br />
    ///     The number of inventory with the given name. (counts inventory in stacks)
    /// </returns>
    /// <exception cref="ArgumentNullException">inventory</exception>
    /// <exception cref="ArgumentNullException">itemName</exception>
    public static int CountOf(this Inventory inventory, string itemName)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        if (string.IsNullOrEmpty(itemName))
            throw new ArgumentNullException(nameof(itemName));

        return inventory.Where(item => (item != null) && item.Name.EqualsI(itemName))
                        .Sum(item => item!.Quantity);
    }

    /// <summary>
    ///     Finds the inventory slot an exchange prize landed in, and reports only the units gained.
    /// </summary>
    /// <param name="after">The inventory after the exchange finished.</param>
    /// <param name="before">
    ///     The inventory as it stood when the exchange was emitted.
    /// </param>
    /// <param name="consumedSlot">The slot the exchange took items from.</param>
    /// <param name="consumedCount">How many units that slot lost to the exchange.</param>
    /// <returns>
    ///     The prize slot with <see cref="Item.Quantity" /> set to the gained count, or <c>null</c> when nothing was added
    ///     (gold-only / empty table).
    /// </returns>
    /// <remarks>
    ///     Name/level/quantity are the durable facts: a character frame replaces every <see cref="Item" /> , so set-difference
    ///     on <see cref="InventoryIndexer" /> identity subtracts nothing and <c>.First()</c> hands back the lowest occupied
    ///     slot — the leftover input on a stack of gifts. Newly occupied slots win over a pile that only gained quantity; the
    ///     consumed slot's expected leftover is <c>before − consumedCount</c> so a prize stacking onto that same pile still
    ///     reads as a net gain.
    /// </remarks>
    public static InventoryIndexer? FindExchangePrize(
        this Inventory after,
        IReadOnlyList<Item?> before,
        int consumedSlot,
        int consumedCount)
    {
        ArgumentNullException.ThrowIfNull(after);
        ArgumentNullException.ThrowIfNull(before);

        InventoryIndexer? best = null;
        var bestIsNewSlot = false;

        for (var index = 0; index < after.Count; index++)
        {
            var afterItem = after[index];

            if (afterItem is null)
                continue;

            var beforeItem = index < before.Count ? before[index] : null;
            int gained;
            var isNewSlot = beforeItem is null;

            if (beforeItem is null)
                gained = Math.Max(1, afterItem.Quantity);
            else if (!beforeItem.Name.EqualsI(afterItem.Name) || (beforeItem.Level != afterItem.Level))
            {
                gained = Math.Max(1, afterItem.Quantity);
                isNewSlot = true;
            } else
            {
                var baseline = beforeItem.Quantity;

                if (index == consumedSlot)
                    baseline = Math.Max(0, baseline - consumedCount);

                if (afterItem.Quantity <= baseline)
                    continue;

                gained = afterItem.Quantity - baseline;
            }

            //a free-slot landing beats a merge onto an existing pile when both somehow move
            if (best is not null && ((!isNewSlot && bestIsNewSlot) || ((isNewSlot == bestIsNewSlot) && (index >= best.Index))))
                continue;

            bestIsNewSlot = isNewSlot;

            best = new InventoryIndexer
            {
                Index = index,
                Item = afterItem with
                {
                    Quantity = gained
                }
            };
        }

        return best;
    }

    /// <summary>
    ///     Finds the first item in the inventory that is not null, and meets the predicate conditions.
    /// </summary>
    /// <param name="inventory">The character's <see cref="Inventory" />.</param>
    /// <param name="predicate">
    ///     A function that returns true or false for a given <see cref="Item" />.
    /// </param>
    /// <returns>
    ///     <see cref="InventoryIndexer" />
    ///     <br />
    ///     The item, and informaiton about what slot it is in, or <c>null</c> if no item was found.
    /// </returns>
    /// <exception cref="ArgumentNullException">inventory</exception>
    /// <exception cref="ArgumentNullException">predicate</exception>
    public static InventoryIndexer? FindItem(this Inventory inventory, Func<Item, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        ArgumentNullException.ThrowIfNull(predicate);

        var index = inventory.FindIndex(item => (item != null) && predicate(item));

        if (index == -1)
            return null;

        return new InventoryIndexer
        {
            Index = index,
            Item = inventory[index]!
        };
    }

    /// <summary>
    ///     Finds the first item in the inventory that meets the conditions.
    /// </summary>
    /// <param name="inventory">The character's <see cref="Inventory" />.</param>
    /// <param name="itemName">
    ///     The name of the item to search for. Leave null to ignore name.
    /// </param>
    /// <param name="level">
    ///     Overrides levelMin/levelMax. Specifies exact level to look for.
    /// </param>
    /// <param name="quantity">
    ///     Overrides quantityMin/quantityMax. Specifies exact quantity to look for.
    /// </param>
    /// <param name="levelMin">The item must have at least this level.</param>
    /// <param name="levelMax">The item must have at most this level.</param>
    /// <param name="quantityMin">The item must have a minimum of this quantity.</param>
    /// <param name="quantityMax">The item must have a maximum of this quantity.</param>
    /// <returns>
    ///     <see cref="InventoryIndexer" />
    ///     <br />
    ///     The item, and informaiton about what slot it is in, or <c>null</c> if no item was found.
    /// </returns>
    /// <exception cref="ArgumentNullException">inventory</exception>
    public static InventoryIndexer? FindItem(
        this Inventory inventory,
        string? itemName = null,
        int? level = null,
        int? quantity = null,
        int levelMin = int.MinValue,
        int levelMax = int.MaxValue,
        int quantityMin = int.MinValue,
        int quantityMax = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(inventory);

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

        var index = inventory.FindIndex(item
            => (item != null)
               && (item.Level >= levelMin)
               && (item.Level <= levelMax)
               && (item.Quantity >= quantityMin)
               && (item.Quantity <= quantityMax)
               && ((itemName == null) || item.Name.EqualsI(itemName)));

        if (index == -1)
            return null;

        return new InventoryIndexer
        {
            Index = index,
            Item = inventory[index]!
        };
    }

    /// <summary>
    ///     Finds the inventory slot an unequipped item landed in, preferring a slot that was empty when the unequip was
    ///     emitted.
    /// </summary>
    /// <param name="inventory">The inventory to search.</param>
    /// <param name="occupiedBefore">
    ///     The indexes of the slots that were occupied when the unequip was emitted.
    /// </param>
    /// <param name="itemName">The name of the unequipped item.</param>
    /// <param name="level">The level of the unequipped item.</param>
    /// <returns>
    ///     <see cref="InventoryIndexer" />
    ///     <br />
    ///     The slot the item landed in, or <c>null</c> when no matching item is on a candidate slot yet.
    /// </returns>
    /// <remarks>
    ///     The server's <c>add_item</c> places a non-stackable unequip in the first free slot, while an equip in flight on the
    ///     same socket swaps in place - the item it displaces lands in the occupied slot the replacement vacated. Requiring a
    ///     previously-empty slot is what tells those apart when both hands wear the same weapon at the same level, and it also
    ///     skips a spare copy already sitting in a lower slot. The fallback answers for a stackable merged onto an existing
    ///     pile, which lands on no empty slot at all.
    /// </remarks>
    /// <exception cref="ArgumentNullException">inventory</exception>
    /// <exception cref="ArgumentNullException">occupiedBefore</exception>
    public static InventoryIndexer? FindLandedItem(
        this Inventory inventory,
        IReadOnlySet<int> occupiedBefore,
        string itemName,
        int level)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        ArgumentNullException.ThrowIfNull(occupiedBefore);

        var candidates = inventory.AsIndexed()
                                  .Where(indexed => indexed.Item.Name.EqualsI(itemName) && (indexed.Item.Level == level))
                                  .ToList();

        return candidates.FirstOrDefault(indexed => !occupiedBefore.Contains(indexed.Index)) ?? candidates.FirstOrDefault();
    }
}