using AL.Core.Abstractions;
using AL.Core.Definitions;

namespace AL.Data.Monsters
{
    public record Ability : AttributedRecordBase
    {
        public float Amount { get; init; }
        public bool Aura { get; init; }
        public Condition Condition { get; init; }
        public float Cooldown { get; init; }
        public bool Curse { get; init; }
        public bool Poison { get; init; }
        public float Radius { get; init; }
        public bool Unlimited { get; init; }
    }
}