using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public record BossInfo : ILocation, IMutable<Mutation>
    {
        [JsonProperty]
        public float HP { get; protected set; }

        [JsonIgnore]
        public string Id { get; init; }

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