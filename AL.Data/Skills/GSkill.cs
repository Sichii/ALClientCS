using System.Collections.Generic;
using AL.Core.Abstractions;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.Data.Skills
{
    /// <summary>
    ///     <inheritdoc cref="AttributedRecordBase" /> <br />
    ///     Represents the static data of a skill.
    /// </summary>
    /// <seealso cref="AttributedRecordBase" />
    public record GSkill : AttributedRecordBase
    {
        /// <summary>
        ///     If populated, when this skill is used this action text will appear on the GUI.
        /// </summary>
        public string? Action { get; init; }

        /// <summary>
        ///     If true, this skill affects the casters party.
        /// </summary>
        [JsonProperty("party")]
        public bool AffectsParty { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this skill requires certain attributed to be used. <br />
        ///     This dictionary contains the attributes and the required values to meet this condition.
        /// </summary>
        [JsonProperty("requirements")]
        public IReadOnlyDictionary<ALAttribute, float>? AttributeRequirements { get; init; }

        /// <summary>
        ///     If true, this skill Emits an aura.
        /// </summary>
        public bool Aura { get; init; }

        /// <summary>
        ///     <b>NULlABLE</b>. If populated, this skill is only usable by certain classes. <br />
        ///     This list contains the classes this skill can be used by.
        /// </summary>
        [JsonProperty("class")]
        public IReadOnlyList<ALClass>? Classes { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. Extra information about the skill. Currently only used by magiport.
        /// </summary>
        public string? Complementary { get; init; }

        /// <summary>
        ///     The condition this skill causes on the target.
        /// </summary>
        public Condition Condition { get; init; }

        /// <summary>
        ///     If populated, the name of the item this skill consumes when used.
        /// </summary>
        public string? Consume { get; init; }

        /// <summary>
        ///     The cooldown of this ability in milliseconds.
        /// </summary>
        [JsonProperty("cooldown")]
        public int CooldownMS { get; private set; }

        /// <summary>
        ///     Used with <see cref="SharedCooldown" />. This is the multiplier applied to the shared cooldown to get this skill's
        ///     cooldown.
        /// </summary>
        [JsonProperty("cooldown_multiplier")]
        public float? CooldownMultiplier { get; init; }

        /// <summary>
        ///     The type of damage this skill deals.
        /// </summary>
        [JsonProperty("damage_type")]
        public DamageType DamageType { get; init; }

        /// <summary>
        ///     The duration of this skill.
        /// </summary>
        public float Duration { get; init; }

        /// <summary>
        ///     Whether or not this skill has a hostile effect.
        /// </summary>
        public bool Hostile { get; init; }

        /// <summary>
        ///     If populated, this skill has a level requirement. <br />
        ///     This is the level requirement to use this skill.
        /// </summary>
        public int? Level { get; init; }

        /// <summary>
        ///     <b>NULlABLE</b>. If populated, this skill changes depending on the level of the caster. <br />
        ///     This list contains the levels in which the effect of this skill changes, and the amount it changes to.
        /// </summary>
        [JsonProperty("levels", ItemConverterType = typeof(ArrayToTupleConverter<float, float>))]
        public IReadOnlyList<(float Level, float NewValue)>? LevelMods { get; init; }

        /// <summary>
        ///     Currently only used for stack. This is the maximum amount of damage you can stack.
        /// </summary>
        public float Max { get; init; }

        /// <summary>
        ///     If true, this skill is multitargeted, and requires you to specify each target.
        /// </summary>
        [JsonProperty("targets")]
        public bool MultiTargeted { get; private set; }

        /// <summary>
        ///     The name of the skill as seen on the GUI.
        /// </summary>
        public string Name { get; init; } = null!;

        /// <summary>
        ///     If true, this skill is not casted; it's always one.
        /// </summary>
        public bool Passive { get; init; }

        /// <summary>
        ///     Whether or not this skill will persist after logging out.
        /// </summary>
        public bool Persistent { get; init; }

        /// <summary>
        ///     A bonus to range(character range) applied for this skill. (applied after <see cref="RangeMultiplier" /> if present)
        /// </summary>
        [JsonProperty("range_bonus")]
        public float RangeBonus { get; init; }

        /// <summary>
        ///     A multiplier applied to range(character range) for this skill.
        /// </summary>
        [JsonProperty("range_multiplier")]
        public float? RangeMultiplier { get; init; }

        /// <summary>
        ///     The ratio at which manage is converted to damage for burst and cburst.
        /// </summary>
        public float Ratio { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this skill requires items to be present in your inventory. <br />
        ///     This list contains the items required to have in your inventory to use this skill. <br />
        ///     They are not necessarily consumed, look at <see cref="Consume" /> for that.
        /// </summary>
        [JsonProperty("inventory")]
        public IReadOnlyList<string>? RequiredInventoryItems { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this skill requires an item to be equipped. <br />
        ///     This list contains an item name that need to be equipped, and all the slots that item can go in.
        /// </summary>
        [JsonProperty("slot", ItemConverterType = typeof(ArrayToTupleConverter<EquipmentSlot, string>))]
        public IReadOnlyList<(EquipmentSlot Slot, string ItemName)>? RequiredSlotItems { get; init; }

        /// <summary>
        ///     If populated, this is the name of the skill this skill shares a cooldown with. <br />
        ///     Check <see cref="CooldownMultiplier" /> for a cooldown multiplier.
        /// </summary>
        [JsonProperty("share")]
        public string? SharedCooldown { get; init; }

        /// <summary>
        ///     If true, this skill is single target, and only requires you to specify 1 target.
        /// </summary>
        [JsonProperty("target")]
        public bool SingleTarget { get; init; }

        /// <summary>
        ///     If true, this skill is toggleable.
        /// </summary>
        public bool Toggle { get; init; }

        /// <summary>
        ///     The type of skill.
        /// </summary>
        public SkillType Type { get; init; }

        /// <summary>
        ///     If true, this skill is useable on monsters.
        /// </summary>
        [JsonProperty("monsters")]
        public bool UseableOnMonsters { get; init; }

        /// <summary>
        ///     If populated, there is a variance (+/-) applied to the effect of this skill. <br />
        ///     Currently only used on alchemy.
        /// </summary>
        public float? Variance { get; init; }

        /// <summary>
        ///     <b>NULLABLE</b>. If populated, this skill is only usable if you have a certain weapon type equipped.
        ///     This list contains the weapon types that enable this skill to be used.
        /// </summary>
        [JsonProperty("wtype"), JsonConverter(typeof(ArrayOrSingleConverter<WeaponType>))]
        public IReadOnlyList<WeaponType>? WeaponTypes { get; init; }

        [JsonProperty("list")]
        private bool ListTargets
        {
            get => MultiTargeted;
            set => MultiTargeted = value;
        }

        [JsonProperty("reuse_cooldown")]
        private int ReuseCooldown
        {
            get => CooldownMS;
            set => CooldownMS = value;
        }
    }
}