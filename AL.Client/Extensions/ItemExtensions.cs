#region
using AL.APIClient.Interfaces;
using AL.Client.Definitions;
using AL.Core.Definitions;
using AL.Data;
using AL.Data.Items;
using AL.SocketClient.Interfaces;
using Chaos.Extensions.Common;
#endregion

namespace AL.Client.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="ISimpleItem" />s and <see cref="ICommonItem" />s.
/// </summary>
public static class ItemExtensions
{
    /// <summary>
    ///     Whether the server would merge <paramref name="item" /> onto <paramref name="other" />.
    /// </summary>
    /// <remarks>
    ///     Restates <c>can_stack</c> (js/old_common_functions.js:391): a stackable name, the two quantities fitting under the
    ///     stack size, the same title, the same data, the PvP mark on both or neither, and no lock on either. The server only
    ///     reads data on a cxjar; it is compared on everything here, which never offers a merge the server refuses. A merge
    ///     asked for against a pile that fails this lands as <c>storage_full</c> whenever the pack has no empty slot
    ///     (node/server.js:8919), whatever the rest of the vault holds.
    /// </remarks>
    public static bool CanStackWith(this IInventoryItem item, IInventoryItem other)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(other);

        var stackSize = item.GetData()
                            ?.StackSize
                        ?? 1;

        if ((stackSize <= 1) || !item.Name.EqualsI(other.Name) || ((item.Quantity + other.Quantity) > stackSize))
            return false;

        if ((item.Prediction?.Title ?? "") != (other.Prediction?.Title ?? ""))
            return false;

        if (item.Data != other.Data)
            return false;

        if (string.IsNullOrEmpty(item.Volatile) != string.IsNullOrEmpty(other.Volatile))
            return false;

        return (item.LockType == ItemLockType.None) && (other.LockType == ItemLockType.None);
    }

    /// <summary>Gets the "G" data for this item.</summary>
    /// <param name="item">The item to get the data for.</param>
    /// <returns>
    ///     <see cref="GItem" />
    ///     <br />
    ///     The "G" data for this item from <see cref="GameData" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public static GItem? GetData(this ISimpleItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return GameData.Items[item.Name];
    }

    /// <summary>Calculates the grade of the item.</summary>
    /// <param name="item">The item to calculate the grade for.</param>
    /// <returns>
    ///     <see cref="Grade" />
    ///     <br />
    ///     The grade of the item.
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public static Grade GetGrade(this ICommonItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var data = item.GetData();

        if (data?.Grades == null)
            return Grade.None;

        var grade = 0;

        foreach (var level in data.Grades)
            if (item.Level < level)
                break;
            else
                grade++;

        return (Grade)grade;
    }

    /// <summary>Checks if the item is compoundable.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if the item is compoundable, otherwise <c>false</c> .
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public static bool IsCompoundable(this ISimpleItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.GetData()
                   ?.CompoundModifiers
               != null;
    }

    /// <summary>Checks if the item is stackable.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if the item is stackable, otherwise <c>false</c> .
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public static bool IsStackable(this ISimpleItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.GetData()
                   ?.StackSize
               > 1;
    }

    /// <summary>Checks if the item is upgradeable.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>
    ///     <see cref="bool" />
    ///     <br />
    ///     <c>true</c> if the item is upgradeable, otherwise <c>false</c> .
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public static bool IsUpgradeable(this ISimpleItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.GetData()
                   ?.UpgradeModifiers
               != null;
    }
}