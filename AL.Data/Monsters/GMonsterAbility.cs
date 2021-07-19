using AL.Core.Abstractions;
using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.Data.Monsters
{
    /// <summary>
    ///     <inheritdoc cref="AttributedRecordBase" /> <br />
    ///     Represents an ability used by a monster.
    /// </summary>
    /// <seealso cref="AttributedRecordBase" />
    public record GMonsterAbility : AttributedRecordBase
    {
        [JsonProperty("amount")]
        private float? _amount;
        [JsonProperty("damage")]
        private float? _damage;
        [JsonProperty("heal")]
        private float? _heal;

        /// <summary>
        ///     Whether or not this is an aura ability.
        /// </summary>
        public bool Aura { get; init; }

        /// <summary>
        ///     The condition this ability applies.
        /// </summary>
        public Condition Condition { get; init; }

        /// <summary>
        ///     The cooldown of this ability in milliseconds.
        /// </summary>
        [JsonProperty("cooldown")]
        public float CooldownMS { get; init; }

        /// <summary>
        ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Cursed" /> <br />
        ///     This will not show up as the <see cref="Condition" /> for this ability.
        /// </summary>
        public bool Curse { get; init; }

        /// <summary>
        ///     Whether or not this ability applies <see cref="AL.Core.Definitions.Condition.Poisoned" />. <br />
        ///     This will not show up as the <see cref="Condition" /> for this ability.
        /// </summary>
        public bool Poison { get; init; }

        /// <summary>
        ///     Whether or not this ability does pure damage.
        /// </summary>
        [JsonProperty("pure")]
        public bool PureDamage { get; set; }

        /// <summary>
        ///     If this is an aura ability, this is the radius of the aura. <br />
        ///     If this is not an aura, this is the radius of ability effect.
        /// </summary>
        public float Radius { get; init; }

        /// <summary>
        ///     Whether or not this ability's condition stacks infinitely. <br />
        ///     Currently only used for <see cref="AL.Core.Definitions.Condition.Burned" />,
        ///     which will not show up as the <see cref="Condition" /> for this ability.
        /// </summary>
        public bool Unlimited { get; init; }

        /// <summary>
        ///     The amount of heal/damage/amount for this ability.
        /// </summary>
        public float Amount => _amount ?? _damage ?? _heal ?? 0f;
    }
}