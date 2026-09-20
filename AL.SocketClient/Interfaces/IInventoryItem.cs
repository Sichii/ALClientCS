#region
using AL.APIClient.Interfaces;
using AL.Core.Definitions;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.Interfaces;

/// <summary>Represents an item in the inventory.</summary>
/// <seealso cref="ICommonItem" />
public interface IInventoryItem : ICommonItem
{
    /// <summary>
    ///     The progress this item has toward's it's achievement.
    /// </summary>
    float AchievementProgress { get; init; }

    /// <summary>
    ///     Item-specific payload: on a cxjar, the appearance inside it. Two cxjars with different data never stack.
    /// </summary>
    string? Data { get; init; }

    /// <summary>
    ///     The date/time this item expires. (it will disappear)
    /// </summary>
    DateTime? Expires { get; init; }

    /// <summary>
    ///     On a booster, how many bonus levels an offering procced when it was compounded (node/server.js:6582-6590).
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

    /// <summary>The type of lock on the item.</summary>
    ItemLockType LockType { get; init; }

    /// <summary>A list of possible prefixes for this item.</summary>
    IReadOnlyList<string> PossiblePrefixes { get; init; }

    /// <summary>
    ///     The title (shiny, glitched, ...) on the item, if any. Two piles with different titles never stack.
    /// </summary>
    Prediction? Prediction { get; init; }

    /// <summary>
    ///     This item is volatile until this date, if you die to another player you may lose it.
    /// </summary>
    string? Volatile { get; init; }
}