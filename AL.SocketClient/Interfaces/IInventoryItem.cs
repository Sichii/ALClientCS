#region
using AL.APIClient.Interfaces;
using AL.Core.Definitions;
#endregion

namespace AL.SocketClient.Interfaces;

/// <summary>
///     Represents an item in the inventory.
/// </summary>
/// <seealso cref="ICommonItem" />
public interface IInventoryItem : ICommonItem
{
    /// <summary>
    ///     The progress this item has toward's it's achievement.
    /// </summary>
    float AchievementProgress { get; init; }

    /// <summary>
    ///     The date/time this item expires. (it will disappear)
    /// </summary>
    DateTime? Expires { get; init; }

    /// <summary>
    ///     TODO: Something to do with boosters
    /// </summary>
    float Extra { get; init; }

    /// <summary>
    ///     This item was a gift, and is only worth 1 gold.
    ///     <br />
    ///     TODO: Is this a number?
    /// </summary>
    float Gift { get; init; }

    /// <summary>
    ///     If populated, the name of the player that held a giveaway that gave out this item.
    /// </summary>
    string? GiveawayFrom { get; init; }

    /// <summary>
    ///     The type of lock on the item.
    /// </summary>
    LockType LockType { get; init; }

    /// <summary>
    ///     A list of possible prefixes for this item.
    /// </summary>
    IReadOnlyList<string> PossiblePrefixes { get; init; }

    /// <summary>
    ///     This item is volatile until this date, if you die to another player you may lose it.
    /// </summary>
    string? Volatile { get; init; }
}