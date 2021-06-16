using AL.Core.Abstractions;
using Newtonsoft.Json;

namespace AL.Data.Conditions
{
    public record Condition : AttributedRecordBase
    {
        [JsonProperty("can_move")]
        private bool _canMove;
        public bool Aura { get; init; }
        public bool Bad { get; init; }
        public bool Blocked { get; init; }
        public bool Buff { get; init; }

        [JsonProperty("cap_reflection")]
        public int CapReflection { get; init; }

        public bool Channel { get; init; }

        [JsonProperty("duration")]
        public int DurationMs { get; init; }

        public int Heal { get; init; }
        public string Intensity { get; init; }
        public int Interval { get; init; }

        public string Name { get; init; }
        public bool Persistent { get; init; }

        [JsonProperty("set_speed")]
        public int SetSpeed { get; init; }

        public bool Technical { get; init; }
        public bool UI { get; init; }

        [JsonIgnore]
        public bool CanMove => !Channel || _canMove;
    }
}