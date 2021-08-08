using AL.Core.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data received when an action is taken.
    /// </summary>
    /// <seealso cref="IPoint" />
    public record ActionData : IPoint
    {
        /// <summary>
        ///     The id of the entity who attacked.
        /// </summary>
        [JsonProperty("attacker")]
        public string AttackerId { get; init; } = null!;

        /// <summary>
        ///     The amount of damage this action did/will result in.
        /// </summary>
        [JsonProperty]
        public float Damage { get; init; }

        /// <summary>
        ///     The amount of time it will take the projectile to reach the target, in milliseconds.
        /// </summary>
        [JsonProperty]
        public float ETA { get; init; }

        /// <summary>
        ///     The amount of healing this action did/will result in.
        /// </summary>
        [JsonProperty]
        public float Heal { get; init; }

        /// <summary>
        ///     TODO: unknown
        /// </summary>
        [JsonProperty]
        public float M { get; init; }

        /// <summary>
        ///     If populated, the name of the projectile.
        /// </summary>
        [JsonProperty]
        public string? Projectile { get; init; }

        /// <summary>
        ///     If populated, a unique id for the projectile.
        /// </summary>
        [JsonProperty("pid")]
        public string? ProjectileId { get; init; }

        /// <summary>
        ///     Describes the source of the action. "attack", "3shot", etc
        /// </summary>
        [JsonProperty]
        public string Source { get; init; } = null!;

        /// <summary>
        ///     The id of the entity that is the target of this action.
        /// </summary>
        [JsonProperty]
        public string Target { get; init; } = null!;

        /// <summary>
        ///     The type of action. Generally the same as <see cref="Source" />
        /// </summary>
        [JsonProperty]
        public string Type { get; init; } = null!;

        [JsonProperty]
        public float X { get; init; }

        [JsonProperty]
        public float Y { get; init; }
        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);
    }
}