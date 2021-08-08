using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents information about a boss monster.
    /// </summary>
    /// <seealso cref="ILocation" />
    /// <seealso cref="IMutable{TMutator}" />
    /// <remarks>Mutated by <see cref="Mutation" /></remarks>
    public record BossInfo : ILocation, IMutable<Mutation>
    {
        /// <summary>
        ///     If populated, the remaining HP of the boss.
        /// </summary>
        [JsonProperty]
        public float? HP { get; protected set; }

        /// <summary>
        ///     The accessor of this boss.
        /// </summary>
        [JsonIgnore]
        public string Id { get; init; } = null!;

        /// <summary>
        ///     Whether or not this boss is alive.
        /// </summary>
        [JsonProperty]
        public bool Live { get; init; }

        /// <summary>
        ///     If populated, the name of the map this boss is on.
        /// </summary>
        [JsonProperty]
 #pragma warning disable 8766
        public string? Map { get; init; }
 #pragma warning restore 8766

        /// <summary>
        ///     If populated, the maximum hp of the boss.
        /// </summary>
        [JsonProperty("max_hp")]
        public float? MaxHP { get; protected set; }

        /// <summary>
        ///     If populated, the current target of the boss.
        /// </summary>
        [JsonProperty]
        public string? Target { get; init; }

        /// <summary>
        ///     The X coordinate of the boss if it's alive.
        /// </summary>
        [JsonProperty]
        public float X { get; init; }

        /// <summary>
        ///     The Y coordinate of the boss if it's alive.
        /// </summary>
        [JsonProperty]
        public float Y { get; init; }

        public virtual bool Equals(IPoint? other) => IPoint.Comparer.Equals(this, other);

        public virtual bool Equals(ILocation? other) => ILocation.Comparer.Equals(this, other);

        public void Mutate(Mutation mutator)
        {
            if (mutator.Attribute == ALAttribute.Hp)
            {
                HP = mutator.Mutator;
                MaxHP = mutator.Mutator;
            }
        }
    }
}