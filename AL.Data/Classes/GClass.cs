#region
using System.Collections.Generic;
using AL.Core.Definitions;
using Newtonsoft.Json;
#endregion

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace AL.Data.Classes;

/// <summary>
///     Provides information about a class(archetype) in adventure land.
/// </summary>
public sealed record GClass
{
    /// <summary>
    ///     Base armor.
    /// </summary>
    public int Armor { get; init; }

    /// <summary>
    ///     Base attack.
    /// </summary>
    public int Attack { get; init; }

    /// <summary>
    ///     The stat values this class starts out with at level 1.
    /// </summary>
    [JsonProperty("stats")]
    public Stats BaseStats { get; init; } = null!;

    /// <summary>
    ///     If true, this class can be the target of any number of monsters without slowing down
    /// </summary>
    public bool Brave { get; init; }

    /// <summary>
    ///     The default number of enemies that deal <see cref="AL.Core.Definitions.DamageType.Pure" /> damage that you can
    ///     engage before getting scared.
    ///     <br />
    ///     TODO: figure out details of scared
    /// </summary>
    public int Courage { get; init; }

    /// <summary>
    ///     The type of damage this character deals.
    /// </summary>
    [JsonProperty("damage_type")]
    public DamageType DamageType { get; init; }

    /// <summary>
    ///     The 2handed weapons this class can use, and the stat modifications that apply when wielding them.
    /// </summary>
    public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Doublehand { get; init; }
        = new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();

    /// <summary>
    ///     The default number of attacks this class can do per second. (AttackSpeed/100)
    /// </summary>
    public float Frequency { get; init; }

    /// <summary>
    ///     The default number of enemies that deal <see cref="AL.Core.Definitions.DamageType.Magical" /> damage that you can
    ///     engage before getting scared.
    ///     <br />
    ///     TODO: figure out details of scared
    /// </summary>
    [JsonProperty("mcourage")]
    public int MagicCourage { get; init; }

    /// <summary>
    ///     The mainhand weapons this class can use, and the stat modifications that apply when wielding them.
    /// </summary>
    public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Mainhand { get; init; }
        = new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();

    /// <summary>
    ///     The main stat this class uses for damage calculations.
    /// </summary>
    [JsonProperty("main_stat")]
    public ALAttribute MainStat { get; init; }

    /// <summary>
    ///     Base max hp.
    /// </summary>
    [JsonProperty("hp")]
    public int MaxHp { get; init; }

    /// <summary>
    ///     Base max mp.
    /// </summary>
    [JsonProperty("mp")]
    public int MaxMp { get; init; }

    /// <summary>
    ///     The amount of mp this class's basic attack costs by default.
    /// </summary>
    public int MpCost { get; init; }

    /// <summary>
    ///     The offhand weapons this class can use, and the stat modifications that apply when wielding them.
    /// </summary>
    public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Offhand { get; init; }
        = new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();

    /// <summary>
    ///     The default number of enemies that deal <see cref="AL.Core.Definitions.DamageType.Physical" /> damage that you can
    ///     engage before getting scared.
    ///     <br />
    ///     TODO: figure out details of scared
    /// </summary>
    [JsonProperty("pcourage")]
    public int PhysicalCourage { get; init; }

    /// <summary>
    ///     The name of the projectile this class's base attack uses by default.
    /// </summary>
    public string Projectile { get; init; } = null!;

    /// <summary>
    ///     Base range.
    /// </summary>
    public int Range { get; init; }

    /// <summary>
    ///     Base resistance.
    /// </summary>
    public int Resistance { get; init; }

    /// <summary>
    ///     Base speed.
    /// </summary>
    public int Speed { get; init; }

    /// <summary>
    ///     The amount of each stat this class receives when leveling up.
    /// </summary>
    [JsonProperty("lstats")]
    public Stats StatGrowth { get; init; } = null!;

    /// <summary>
    ///     Checks if this class can wield the given 2handed weapon.
    /// </summary>
    /// <param name="weaponType">
    ///     A 2handed weapon.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if this class can wield it, otherwise
    ///     <c>
    ///         false
    ///     </c>
    /// </returns>
    public bool Can2Hand(WeaponType weaponType) => Doublehand.ContainsKey(weaponType);

    /// <summary>
    ///     Checks if this class can wield the given mainhand weapon.
    /// </summary>
    /// <param name="weaponType">
    ///     A mainhand weapon.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if this class can wield it, otherwise
    ///     <c>
    ///         false
    ///     </c>
    /// </returns>
    public bool CanMainHand(WeaponType weaponType) => Mainhand.ContainsKey(weaponType);

    /// <summary>
    ///     Checks if this class can wield the given offhand weapon.
    /// </summary>
    /// <param name="weaponType">
    ///     An offhand.
    /// </param>
    /// <returns>
    ///     <c>
    ///         true
    ///     </c>
    ///     if this class can wield it, otherwise
    ///     <c>
    ///         false
    ///     </c>
    /// </returns>
    public bool CanOffHand(WeaponType weaponType) => Offhand.ContainsKey(weaponType);
}