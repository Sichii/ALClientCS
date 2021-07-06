using System.Collections.Generic;
using AL.Core.Definitions;
using Newtonsoft.Json;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace AL.Data.Classes
{
    public record Class
    {
        public int Armor { get; init; }
        public int Attack { get; init; }

        [JsonProperty("stats")]
        public Stats BaseStats { get; init; }

        public int Courage { get; init; }

        [JsonProperty("damage_type")]
        public string DamageType { get; init; }

        public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Doublehands { get; init; } =
            new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();
        public float Frequency { get; init; }
        public int Hp { get; init; }
        public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Mainhands { get; init; } =
            new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();
        public string MainStat { get; init; }
        public int Mcourage { get; init; }
        public int Mp { get; init; }
        public int MpCost { get; init; }
        public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Offhands { get; init; } =
            new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();
        public int Pcourage { get; init; }
        public string Projectile { get; init; }
        public int Range { get; init; }
        public int Resistance { get; init; }
        public int Speed { get; init; }

        [JsonProperty("lstats")]
        public Stats StatGrowth { get; init; }

        public bool CanDualWield(WeaponType weaponType) => Doublehands.ContainsKey(weaponType);
        public bool CanMainHand(WeaponType weaponType) => Mainhands.ContainsKey(weaponType);
        public bool CanOffHand(WeaponType weaponType) => Offhands.ContainsKey(weaponType);
    }
}