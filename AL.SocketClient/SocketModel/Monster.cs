using System;
using System.Collections.Generic;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    [JsonConverter(typeof(AttributedObjectConverter<Monster>))]
    public record Monster : EntityBase
    {
        public IReadOnlyDictionary<string, Ability> Abilities { get; init; } = new Dictionary<string, Ability>();
        [JsonProperty("type")]
        public string Name { get; init; }

        public virtual bool Equals(Monster other) => Name.Equals(other?.Name) && base.Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Name.GetHashCode(), base.GetHashCode());

        public void Update(Monster other)
        {
            ABS = other.ABS;
            Angle = other.Angle;
            Armor = other.Armor;
            Resistance = other.Resistance;
            Attack = other.Attack;
            Frequency = other.Frequency;
            GoingX = other.GoingX;
            GoingY = other.GoingY;
            HP = other.HP;
            MaxHP = other.MaxHP;
            MP = other.MP;
            Level = other.Level;
            StepCount = other.StepCount;
            Moving = other.Moving;
            Conditions = other.Conditions;
            Speed = other.Speed;
            X = other.X;
            Y = other.Y;
            XP = other.XP;
        }
    }
}