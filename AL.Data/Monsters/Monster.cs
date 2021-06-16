using System.Collections.Generic;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Monsters
{
    public record Monster : AttributedRecordBase
    {
        [JsonProperty("1hp")]
        public bool _1hp { get; init; }

        [JsonProperty(ItemConverterType = typeof(AttributedObjectConverter<Ability>))]
        public IReadOnlyDictionary<string, Ability> Abilities { get; init; }

        [JsonProperty(ItemConverterType = typeof(ArrayToTupleConverter<float, AchievementRewardType, ALAttribute, float>))]
        public (float RequiredPoints, AchievementRewardType RewardType, ALAttribute Attribute, float Amount)[] Achievements { get; init; }

        public float Aggro { get; init; }

        [JsonProperty("charge")]
        public float ChargeSpeed { get; init; }

        public bool Cooperative { get; init; }
        public bool Cute { get; init; }

        [JsonProperty("damage_type")]
        public DamageType DamageType { get; init; }

        public float Difficulty { get; init; }
        public bool Escapist { get; init; }
        public bool Global { get; init; }
        public bool Humanoid { get; init; }
        public bool Immune { get; init; }

        [JsonProperty("s")]
        public IReadOnlyDictionary<Condition, InitialCondition> InitialConditions { get; init; }

        [JsonProperty("orientation")]
        public Direction InitialDirection { get; init; }

        public float Lucrativeness { get; init; }
        public string Name { get; init; }
        public bool Passive { get; init; }
        public bool Poisonous { get; init; }
        public string Projectile { get; init; }
        public float Rage { get; init; }

        [JsonProperty("respawn")]
        public float RespawnMS { get; init; }

        [JsonProperty("rbuff")]
        public Condition RewardBuff { get; init; }

        public bool Roam { get; init; }
        public float Size { get; init; }

        [JsonProperty(ItemConverterType = typeof(ArrayToTupleConverter<float, string>))]
        public (float SpawnDelay, string MonsterName)[] Spawns { get; init; }

        public bool Special { get; init; }
        public bool Stationary { get; init; }
        public bool Supporter { get; init; }
        public bool Trap { get; init; }
        public bool Unlist { get; init; }
    }
}