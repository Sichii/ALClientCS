using System.Collections.Generic;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Skills
{
    public record Skill : AttributedRecordBase
    {
        public string Action { get; init; }

        [JsonProperty("party")]
        public bool AffectsParty { get; init; }

        [JsonProperty("requirements")]
        public IReadOnlyDictionary<ALAttribute, float> AttributeRequirements { get; init; } =
            new Dictionary<ALAttribute, float>();
        public bool Aura { get; init; }
        public ALClass[] Class { get; init; }
        public string Complementary { get; init; }
        public Condition Condition { get; init; }
        public string Consume { get; init; }

        [JsonProperty("cooldown")]
        public float CooldownMS { get; private set; }

        [JsonProperty("cooldown_multiplier")]
        public float CooldownMultiplier { get; init; }

        [JsonProperty("damage_type")]
        public DamageType DamageType { get; init; }

        public float Duration { get; init; }
        public bool Hostile { get; init; }
        public float Level { get; init; }

        [JsonProperty("levels", ItemConverterType = typeof(ArrayToTupleConverter<float, float>))]
        public (float Level, float Modifier)[] LevelMods { get; init; }

        public float Max { get; init; }

        [JsonProperty("targets")]
        public bool MultiTargeted { get; private set; }

        public string Name { get; init; }
        public bool Passive { get; init; }
        public bool Persistent { get; init; }

        [JsonProperty("range_bonus")]
        public float RangeBonus { get; init; }

        [JsonProperty("range_multiplier")]
        public float RangeMultiplier { get; init; }

        public float Ratio { get; init; }

        [JsonProperty("inventory")]
        public string[] RequiredItems { get; init; }

        [JsonProperty("slots", ItemConverterType = typeof(ArrayToTupleConverter<EquipmentSlot, string>))]
        public (EquipmentSlot Slot, string ItemName)[] RequiredSlots { get; init; }

        [JsonProperty("share")]
        public string SharedCooldown { get; init; }

        [JsonProperty("target")]
        public bool SingleTarget { get; init; }

        public bool Toggle { get; init; }
        public SkillType Type { get; init; }

        [JsonProperty("monsters")]
        public bool UseableOnMonsters { get; init; }

        public float Variance { get; init; }

        [JsonProperty("wtype"), JsonConverter(typeof(ArrayOrSingleConverter<WeaponType>))]
        public WeaponType[] WeaponTypes { get; init; }

        [JsonProperty("list")]
        private bool ListTargets
        {
            get => MultiTargeted;
            set => MultiTargeted = value;
        }

        [JsonProperty("reuse_cooldown")]
        private float ReuseCooldown
        {
            get => CooldownMS;
            set => CooldownMS = value;
        }
    }
}