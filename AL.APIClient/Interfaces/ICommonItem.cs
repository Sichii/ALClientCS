using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.APIClient.Interfaces
{
    /// <summary>
    ///     Represents the information that is common between most instances of an item.
    /// </summary>
    /// <seealso cref="ISimpleItem" />
    public interface ICommonItem : ISimpleItem
    {
        /// <summary>
        ///     If populated, the name of the achievement earned on this item.
        /// </summary>
        [JsonProperty("ach")]
        string? AchievementName { get; }

        /// <summary>
        ///     The grace value of this item.
        /// </summary>
        [JsonProperty]
        float Grace { get; }

        /// <summary>
        ///     The level of this item;
        /// </summary>
        [JsonProperty]
        int Level { get; }

        /// <summary>
        ///     The type of stat attributed to this item.
        /// </summary>
        [JsonProperty("stat_type")]
        ALAttribute StatType { get; }
    }
}