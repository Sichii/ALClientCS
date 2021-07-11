using AL.Core.Definitions;
using AL.Core.Json.Attributes;

namespace AL.Data.Monsters
{
    /// <summary>
    ///     Represents an achievement you get from killing a certain amount of a monster.
    /// </summary>
    /// <param name="RequiredPoints">The amount of points required to get this achievement.</param>
    /// <param name="RewardType">The type of achievement. (stats or nothing)</param>
    /// <param name="Attribute">The attribute this achievement grants, if any.</param>
    /// <param name="Amount">The amount of the attribute this achievement grants.</param>
    public record KillAchievement(
        [property: JsonArrayIndex(0)] float RequiredPoints,
        [property: JsonArrayIndex(1)] AchievementRewardType RewardType,
        [property: JsonArrayIndex(2)] ALAttribute Attribute,
        [property: JsonArrayIndex(3)] float Amount);
}