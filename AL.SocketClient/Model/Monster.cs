﻿using System;
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

        public virtual bool Equals(Monster? other) => Name.Equals(other?.Name) && base.Equals(other);

        public override bool Equals(object? obj) => Equals(obj as Monster);

        public override int GetHashCode() =>
            HashCode.Combine(Name.GetHashCode(), base.GetHashCode());
    }
}