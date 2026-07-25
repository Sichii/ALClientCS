using System;
using System.Collections.Generic;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents a monster entity.
    /// </summary>
    /// <seealso cref="EntityBase" />
    [JsonConverter(typeof(AttributedObjectConverter<Monster>))]
    public class Monster : EntityBase, IEquatable<Monster>
    {
        /// <summary>
        ///     The name of the monster. (bee, cutebee, mole, etc...)
        /// </summary>
        [JsonProperty("type")]
        public string Name { get; init; } = null!;

        /// <summary>
        ///     <b>NULLABLE.</b> If this monster is a summoned pet, its given display name. Distinct from
        ///     <see cref="Name" />, which carries the monster <c>type</c>.
        /// </summary>
        [JsonProperty("name")]
        public string? PetName { get; init; }

        /// <summary>
        ///     Whether this monster is a summoned pet.
        /// </summary>
        [JsonProperty("pet")]
        public bool Pet { get; init; }

        /// <summary>
        ///     Whether this monster is a placed trap.
        /// </summary>
        [JsonProperty("trap")]
        public bool Trap { get; init; }

        /// <summary>
        ///     <b>NULLABLE.</b> For a pet or trap, the id of the character that owns it.
        /// </summary>
        [JsonProperty("owner")]
        public string? Owner { get; init; }

        /// <summary>
        ///     Whether kills on this monster count for every attacker, not just the tag holder.
        /// </summary>
        [JsonProperty("cooperative")]
        public bool Cooperative { get; init; }

        /// <summary>
        ///     Whether this monster dies in a single hit regardless of damage dealt.
        /// </summary>
        [JsonProperty("1hp")]
        public bool OneHP { get; init; }

        /// <summary>
        ///     <b>NULLABLE.</b> If populated, the appearance skin overriding this monster's default sprite.
        /// </summary>
        [JsonProperty("skin")]
        public string? Skin { get; init; }

        /// <summary>
        ///     <b>NULLABLE.</b> If populated, a per-monster drop table as raw <c>[chance, item]</c> tuples,
        ///     overriding the type's defaults. Not interpreted by this client.
        /// </summary>
        [JsonProperty("drops")]
        public IReadOnlyList<object>? Drops { get; init; }

        public virtual bool Equals(Monster? other) => Name.Equals(other?.Name) && base.Equals(other);

        public override bool Equals(object? obj) => Equals(obj as Monster);

        public override int GetHashCode() =>
            HashCode.Combine(Name.GetHashCode(), base.GetHashCode());
    }
}