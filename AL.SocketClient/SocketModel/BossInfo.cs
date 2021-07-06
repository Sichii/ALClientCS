using System;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public record BossInfo : ILocation, IMutable<Mutation>
    {
        [JsonProperty]
        public float HP { get; protected set; }

        [JsonProperty]
        public bool Live { get; init; }

        [JsonProperty]
        public string Map { get; init; }
        [JsonProperty("max_hp")]
        public float MaxHP { get; protected set; }

        [JsonProperty]
        public string Target { get; init; }

        [JsonProperty]
        public float X { get; init; }

        [JsonProperty]
        public float Y { get; init; }
        
        [JsonIgnore]
        public string Id { get; init; }

        public void Mutate(Mutation mutator)
        {
            if (mutator.Attribute == ALAttribute.Hp)
            {
                HP = mutator.Mutator;
                MaxHP = mutator.Mutator;
            }
        }

        public void Mutate(object mutator) => throw new NotImplementedException();

        public virtual bool Equals(BossInfo other) => other is not null && Id == other.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}