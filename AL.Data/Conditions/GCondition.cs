using AL.Core.Abstractions;
using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.Data.Conditions
{
    /// <summary>
    ///     Represents a buff or detriment. <br />
    ///     <inheritdoc cref="AttributedRecordBase" />
    /// </summary>
    /// <seealso cref="AttributedRecordBase" />
    public record GCondition : AttributedRecordBase
    {
        #pragma warning disable 0649
        [JsonProperty("can_move")]
        private bool _canMove;
        #pragma warning restore 0649

        /// <summary>
        ///     Whether or not this condition is associated with an aura.
        /// </summary>
        public bool Aura { get; init; }

        /// <summary>
        ///     Whether or not this condition is bad.
        /// </summary>
        public bool Bad { get; init; }

        /// <summary>
        ///     Whether or not you are blocked from taking actions.
        /// </summary>
        public bool Blocked { get; init; }

        /// <summary>
        ///     Whether or not this condition is a buff.
        /// </summary>
        public bool Buff { get; init; }

        /// <summary>
        ///     For the condition "Reflection", the maximum amount of reflection you can have.
        /// </summary>
        [JsonProperty("cap_reflection")]
        public int CapReflection { get; init; }

        /// <summary>
        ///     Whether or not you are channeling with this condition.
        /// </summary>
        public bool Channel { get; init; }

        /// <summary>
        ///     The duration of this condition in milliseconds.
        /// </summary>
        [JsonProperty("duration")]
        public int DurationMs { get; init; }

        /// <summary>
        ///     The amount this condition heals. (per interval)
        /// </summary>
        public int Heal { get; init; }

        /// <summary>
        ///     TODO: Unknown
        /// </summary>
        public string? Intensity { get; init; }

        /// <summary>
        ///     The time between ticks of this condition in milliseconds.
        /// </summary>
        [JsonProperty("interval")]
        public int IntervalMS { get; init; }

        /// <summary>
        ///     The name of this condition. (can be different than the key)
        /// </summary>
        public string Name { get; init; } = null!;

        /// <summary>
        ///     Whether or not this condition will persist after logging out.
        /// </summary>
        public bool Persistent { get; init; }

        /// <summary>
        ///     <see cref="ALAttribute.Speed" /> will be set to this value.
        /// </summary>
        [JsonProperty("set_speed")]
        public int SetSpeed { get; init; }

        /// <summary>
        ///     Whether or not this condition is related to things outside of the game.
        /// </summary>
        public bool Technical { get; init; }

        /// <summary>
        ///     Whether or not there is a GUI element related to this condition.
        /// </summary>
        public bool UI { get; init; }

        /// <summary>
        ///     Whether or not you can move and channel this ability at the same time.
        /// </summary>
        [JsonIgnore]
        public bool CanMove => !Channel || _canMove;
    }
}