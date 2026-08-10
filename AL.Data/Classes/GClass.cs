#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
#endregion

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace AL.Data.Classes;

/// <summary>
///     Provides information about a class(archetype) in adventure land.
/// </summary>
public sealed record GClass
{
    /// <summary>
    ///     Base armor, before stats and gear. Armor cuts physical damage taken.
    /// </summary>
    public int Armor { get; init; }

    /// <summary>
    ///     Base attack, before a weapon, stats and gear.
    /// </summary>
    public int Attack { get; init; }

    /// <summary>
    ///     The stat values this class starts out with at level 1.
    /// </summary>
    [JsonPropertyName("stats")]
    public Stats BaseStats { get; init; } = null!;

    /// <summary>
    ///     Base black-magic resistance, meant for curse and stone. Both conditions have their defense line commented
    ///     out on the server, so nothing reads this today.
    /// </summary>
    [JsonPropertyName("bmresistance")]
    public int BlackMagicResistance { get; init; }

    /// <summary>
    ///     Set on the warrior alone. Nothing in the published server or browser client reads it, so what it would do
    ///     is unsettled.
    /// </summary>
    public bool Brave { get; init; }

    /// <summary>
    ///     How many <see cref="AL.Core.Definitions.DamageType.Physical" /> attackers this class tolerates before fear
    ///     sets in. Fear slows the character and drops attack to 60%, then 40%, then 20%.
    /// </summary>
    public int Courage { get; init; }

    /// <summary>
    ///     The type of damage this character deals.
    /// </summary>
    [JsonPropertyName("damage_type")]
    public DamageType DamageType { get; init; }

    /// <summary>
    ///     The 2handed weapons this class can use, and the stat modifications that apply when wielding them.
    /// </summary>
    public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Doublehand { get; init; }
        = new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();

    /// <summary>
    ///     Percent chance to shrug off being frozen or deep-frozen outright.
    /// </summary>
    [JsonPropertyName("fzresistance")]
    public int FreezeResistance { get; init; }

    /// <summary>
    ///     The class's base attacks per second, before level, stats and gear add to it. Gear states frequency as a
    ///     percentage and contributes a hundredth of it.
    /// </summary>
    public float Frequency { get; init; }

    /// <summary>
    ///     How many <see cref="AL.Core.Definitions.DamageType.Magical" /> attackers this class tolerates before fear
    ///     sets in. Priests get the highest.
    /// </summary>
    [JsonPropertyName("mcourage")]
    public int MagicCourage { get; init; }

    /// <summary>
    ///     The mainhand weapons this class can use, and the stat modifications that apply when wielding them.
    /// </summary>
    public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Mainhand { get; init; }
        = new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();

    /// <summary>
    ///     The stat that scales weapon damage - each point adds a twentieth of the weapon's attack.
    /// </summary>
    [JsonPropertyName("main_stat")]
    public ALAttribute MainStat { get; init; }

    /// <summary>
    ///     Base max hp, before stats and gear. Strength and vitality both add to it.
    /// </summary>
    [JsonPropertyName("hp")]
    public int MaxHp { get; init; }

    /// <summary>
    ///     Base max mp, before stats and gear. Intelligence and level both add to it.
    /// </summary>
    [JsonPropertyName("mp")]
    public int MaxMp { get; init; }

    /// <summary>
    ///     The amount of mp this class's basic attack costs by default.
    /// </summary>
    [JsonPropertyName("mp_cost")]
    public int MpCost { get; init; }

    /// <summary>
    ///     The offhand weapons this class can use, and the stat modifications that apply when wielding them.
    /// </summary>
    public IReadOnlyDictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>> Offhand { get; init; }
        = new Dictionary<WeaponType, IReadOnlyDictionary<ALAttribute, float>>();

    /// <summary>
    ///     The base damage output of this class as a percentage. (attack is scaled by output/100)
    /// </summary>
    [JsonPropertyName("output")]
    public int Output { get; init; }

    /// <summary>
    ///     How many <see cref="AL.Core.Definitions.DamageType.Pure" /> attackers this class tolerates before fear sets
    ///     in. This is the pure-damage limit, not a physical one; paladins get the highest.
    /// </summary>
    [JsonPropertyName("pcourage")]
    public int PureCourage { get; init; }

    /// <summary>
    ///     Percent chance to shrug off a stun outright. Stun is the only condition that names it.
    /// </summary>
    [JsonPropertyName("phresistance")]
    public int PhysicalResistance { get; init; }

    /// <summary>
    ///     Percent chance to shrug off poison outright, and the percent by which a poison that does land is shortened.
    /// </summary>
    [JsonPropertyName("pnresistance")]
    public int PoisonResistance { get; init; }

    /// <summary>
    ///     The key of the projectile this class's basic attack uses when the weapon names none.
    /// </summary>
    public string Projectile { get; init; } = null!;

    /// <summary>
    ///     Base attack range, before a weapon and gear add to it.
    /// </summary>
    public int Range { get; init; }

    /// <summary>
    ///     Base resistance, before stats and gear. Resistance cuts magical damage taken.
    /// </summary>
    public int Resistance { get; init; }

    /// <summary>
    ///     Base movement speed, before stats and gear. Strength and dexterity both add to it.
    /// </summary>
    public int Speed { get; init; }

    /// <summary>
    ///     The amount of each stat this class receives per level. Fractional amounts are normal.
    /// </summary>
    [JsonPropertyName("lstats")]
    public Stats StatGrowth { get; init; } = null!;

    /// <summary>
    ///     Percent by which a debuff's duration is cut. It applies to every condition flagged as a debuff, not just
    ///     stuns.
    /// </summary>
    [JsonPropertyName("stresistance")]
    public int StunResistance { get; init; }

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