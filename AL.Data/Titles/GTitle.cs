using AL.Data.Achievements;
using Newtonsoft.Json;

namespace AL.Data.Titles
{
    /// <summary>
    ///     Represents a title that can be applied to an item.
    /// </summary>
    public record GTitle
    {
        /// <summary>
        ///     If populated, the <see cref="GAchievement" /> this title is associated with.
        /// </summary>
        public string? Achievement { get; init; }

        /// <summary>
        ///     If populated, the type of item this title can affect.
        /// </summary>
        [JsonProperty("type")]
        public string? AffectsItemType { get; init; }

        /// <summary>
        ///     The name of this title.
        /// </summary>
        [JsonProperty("title")]
        public string Name { get; init; } = null!;

        /// <summary>
        ///     If populated, the source of the title.
        /// </summary>
        public string? Source { get; init; }
    }
}