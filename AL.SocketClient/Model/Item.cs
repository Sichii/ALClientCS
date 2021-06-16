using System;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.SocketClient.Model
{
    public record Item : IItem
    {
        [JsonProperty("ach")]
        public string AchievementName { get; init; }

        [JsonProperty("acc")]
        public float AchievementProgress { get; init; }

        [JsonProperty, JsonConverter(typeof(IsoDateTimeConverter))] //TODO: FIX THIS
        public DateTime Expires { get; init; }

        [JsonProperty]
        public float Extra { get; init; }

        [JsonProperty]
        //TODO: this is a number?
        public float Gift { get; init; }

        [JsonProperty("gf")]
        public string GiveawayFrom { get; init; }

        [JsonProperty]
        public float Grave { get; init; }

        [JsonProperty("ex")]
        public bool IsElixir { get; init; }

        [JsonProperty]
        public int Level { get; init; }

        [JsonProperty("l")]
        public LockType LockType { get; init; }

        [JsonProperty]
        public string Name { get; init; }

        [JsonProperty("ps")]
        public string[] PossiblePrefixes { get; init; }

        [JsonProperty("p")]
        public string Prefix { get; init; }

        [JsonProperty("q")]
        public int Quantity { get; init; }

        [JsonProperty("stat_type")]
        public ALAttribute StatType { get; init; }

        [JsonProperty("v")]
        public string Volatile { get; init; }
    }
}