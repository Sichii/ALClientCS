#region
using AL.APIClient.Interfaces;
using AL.Client.Definitions;
using AL.Client.Helpers;
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
    ///     Determines whether the server would merge <paramref name="item" /> onto <paramref name="other" />.
    /// </summary>
    /// <remarks>
    ///     Requires a stackable name, quantities fitting under the stack size, the same title ignoring stackable titles, the
    ///     same data, the PvP mark on both or neither, and no lock on either.
    /// </remarks>
    /// <param name="item">The item to merge.</param>
    /// <param name="other">The pile to merge it onto.</param>
    /// <param name="ignorePvp">
    ///     Specifies whether the PvP mark is left out of the comparison, as for a bank deposit, which strips <c>v</c>.
    /// </param>
    /// <returns>
    ///     true if the server would merge the two; otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    /// <exception cref="ArgumentNullException">other</exception>
    public static bool CanStackWith(this IInventoryItem item, IInventoryItem other, bool ignorePvp = false)
    {
        ArgumentNullException.ThrowIfNull(item);

        ArgumentNullException.ThrowIfNull(other);

        var stackSize = item.GetData()
                            ?.StackSize
                        ?? 1;

        if ((stackSize <= 1) || !item.Name.EqualsI(other.Name) || ((item.Quantity + other.Quantity) > stackSize))
            return false;

        if (item.GetStackingTitle() != other.GetStackingTitle())
            return false;

        if (item.Data != other.Data)
            return false;

        if (!ignorePvp && (string.IsNullOrEmpty(item.Volatile) != string.IsNullOrEmpty(other.Volatile)))
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

    /// <summary>
    ///     Calculates the grade of the item by the server's rule,
    ///     <see cref="UpgradeMath.CalculateGrade(IReadOnlyList{int}, int)" />.
    /// </summary>
    /// <param name="item">The item to calculate the grade for.</param>
    /// <returns>
    ///     The grade of the item, or <see cref="Grade.None" /> when it has no upgrade or compound track.
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public static Grade GetGrade(this ICommonItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!UpgradeMath.TryGetGradeThresholds(item.GetData(), out var thresholds))
            return Grade.None;

        return (Grade)UpgradeMath.CalculateGrade(thresholds, item.Level);
    }

    /// <summary>
    ///     Gets the item's title as the server compares it for stacking.
    /// </summary>
    /// <param name="item">The item to read.</param>
    /// <returns>
    ///     The title, or null when the item has none or its title is one the game marks stackable ("Cave-found").
    /// </returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public static string? GetStackingTitle(this IInventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var title = item.Prediction?.Title;

        if (string.IsNullOrEmpty(title) || GameData.Titles[title] is { Stackable: true })
            return null;

        return title;
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