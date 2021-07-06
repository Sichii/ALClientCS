using System;
using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.Core.Interfaces
{
    public interface IItem
    {
        [JsonProperty("acc")]
        string AchievementName { get; init; }

        [JsonProperty("acc")]
        float AchievementProgress { get; init; }

        [JsonProperty]
        DateTime Expires { get; init; }

        [JsonProperty]
        float Extra { get; init; }

        [JsonProperty]
        //TODO: this is a number?
        float Gift { get; init; }

        [JsonProperty("gf")]
        string GiveawayFrom { get; init; }

        [JsonProperty]
        float Grave { get; init; }

        [JsonProperty("ex")]
        bool IsElixir { get; init; }

        [JsonProperty]
        int Level { get; init; }

        [JsonProperty("l")]
        LockType LockType { get; init; }

        [JsonProperty]
        string Name { get; init; }

        [JsonProperty("ps")]
        string[] PossiblePrefixes { get; init; }

        [JsonProperty("p")]
        string Prefix { get; init; }

        [JsonProperty("q")]
        int Quantity { get; init; }

        [JsonProperty("stat_type")]
        ALAttribute StatType { get; init; }

        [JsonProperty("v")]
        string Volatile { get; init; }
    }
}