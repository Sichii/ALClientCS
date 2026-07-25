#region
using System.Text.Json.Serialization;
using Newtonsoft.Json;
#endregion

namespace AL.Data.Achievements
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <seealso cref="DatumBase{T}" />
    public class AchievementsDatum : DatumBase<GAchievement>
    {
        [JsonProperty("1000boss")]
        [JsonPropertyName("1000boss")]
        public GAchievement _1000Boss { get; init; } = null!;

        [JsonProperty("100boss")]
        [JsonPropertyName("100boss")]
        public GAchievement _100Boss { get; init; } = null!;

        [JsonProperty("abtesting")]
        [JsonPropertyName("abtesting")]
        public GAchievement Abtesting { get; init; } = null!;

        [JsonProperty("discoverlair")]
        [JsonPropertyName("discoverlair")]
        public GAchievement Discoverlair { get; init; } = null!;

        [JsonProperty("festive")]
        [JsonPropertyName("festive")]
        public GAchievement Festive { get; init; } = null!;

        [JsonProperty("firehazard")]
        [JsonPropertyName("firehazard")]
        public GAchievement Firehazard { get; init; } = null!;

        [JsonProperty("gooped")]
        [JsonPropertyName("gooped")]
        public GAchievement Gooped { get; init; } = null!;

        [JsonProperty("lucky")]
        [JsonPropertyName("lucky")]
        public GAchievement Lucky { get; init; } = null!;

        [JsonProperty("monsterhunter")]
        [JsonPropertyName("monsterhunter")]
        public GAchievement Monsterhunter { get; init; } = null!;

        /// <summary>
        ///     Defeat 1,000 Bosses.
        /// </summary>
        [JsonProperty("reach40")]
        [JsonPropertyName("reach40")]
        public GAchievement Reach40 { get; init; } = null!;

        [JsonProperty("reach50")]
        [JsonPropertyName("reach50")]
        public GAchievement Reach50 { get; init; } = null!;

        [JsonProperty("reach60")]
        [JsonPropertyName("reach60")]
        public GAchievement Reach60 { get; init; } = null!;

        [JsonProperty("reach70")]
        [JsonPropertyName("reach70")]
        public GAchievement Reach70 { get; init; } = null!;

        [JsonProperty("reach80")]
        [JsonPropertyName("reach80")]
        public GAchievement Reach80 { get; init; } = null!;

        [JsonProperty("reach90")]
        [JsonPropertyName("reach90")]
        public GAchievement Reach90 { get; init; } = null!;

        [JsonProperty("stomped")]
        [JsonPropertyName("stomped")]
        public GAchievement Stomped { get; init; } = null!;

        [JsonProperty("upgrade10")]
        [JsonPropertyName("upgrade10")]
        public GAchievement Upgrade10 { get; init; } = null!;
    }
}