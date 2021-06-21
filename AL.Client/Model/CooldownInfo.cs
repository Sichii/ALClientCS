using System;

namespace ALClientCS.Model
{
    public record CooldownInfo
    {
        public DateTime LocalLastUse { get; init; }
        public float ServerCooldownMS { get; init; }
        public string SkillName { get; init; }

        public bool CanUse(int globalJitterMS) =>
            DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(ServerCooldownMS + globalJitterMS)) > LocalLastUse;
    }
}