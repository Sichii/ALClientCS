#region
using AL.Core.Definitions;
using AL.Core.Json.Attributes;
#endregion

namespace AL.Data.Monsters;

/// <summary>
///     Represents an achievement you get from killing a certain amount of a monster.
/// </summary>
/// <param name="RequiredPoints">
///     How many of this monster you must have killed. Tiers are cumulative - every one at or below your kill
///     count is granted (node/server.js:1334).
/// </param>
/// <param name="RewardType">
///     What the tier hands out. The server only acts on "stat".
/// </param>
/// <param name="Attribute">
///     The attribute this achievement grants, if any.
/// </param>
/// <param name="Amount">
///     How much of <paramref name="Attribute" /> is added, and it only counts while the monster tracker is
///     equipped.
/// </param>
public record GKillAchievement(
    [property: JsonArrayIndex(0)]
    float RequiredPoints,
    [property: JsonArrayIndex(1)]
    AchievementRewardType RewardType,
    [property: JsonArrayIndex(2)]
    ALAttribute Attribute,
    [property: JsonArrayIndex(3)]
    float Amount);