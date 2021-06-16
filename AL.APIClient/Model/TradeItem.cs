using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    public record TradeItem
    {
        [JsonProperty("ach")]
        public string AchievementName { get; init; }

        [JsonProperty("b")]
        public bool Buying { get; init; }

        [JsonProperty("giveaway")]
        public float GiveawayMins { get; init; }

        [JsonProperty("list")]
        public string[] GiveawayParticipants { get; init; }
        public float Grace { get; init; }

        [JsonProperty("rid")]
        public string Id { get; init; }
        public int Level { get; init; }
        public string Name { get; init; }

        [JsonProperty("p")]
        public string Prefix { get; init; }
        public long Price { get; init; }
        [JsonProperty("q")]
        public int Quantity { get; init; }

        [JsonProperty("stat_type")]
        public ALAttribute StatType { get; init; }
    }
}