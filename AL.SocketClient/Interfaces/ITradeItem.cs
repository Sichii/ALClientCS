using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.SocketClient.Interfaces
{
    public interface ITradeItem
    {
        [JsonProperty("ach")]
        public string AchievementName { get; init; }

        [JsonProperty("b")]
        public bool Buying { get; init; }

        [JsonProperty("giveaway")]
        public float GiveawayMins { get; init; }

        [JsonProperty("list")]
        public string[] GiveawayParticipants { get; init; }

        [JsonProperty]
        public float Grace { get; init; }

        [JsonProperty("rid")]
        public string Id { get; init; }

        [JsonProperty]
        public int Level { get; init; }

        [JsonProperty]
        public string Name { get; init; }

        [JsonProperty("p")]
        public string Prefix { get; init; }

        [JsonProperty]
        public long Price { get; init; }

        [JsonProperty("q")]
        public int Quantity { get; init; }

        [JsonProperty("stat_type")]
        public ALAttribute StatType { get; init; }
    }
}