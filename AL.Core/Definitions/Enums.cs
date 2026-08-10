#region
using System.Runtime.Serialization;
using StjConverters = AL.Core.Json.SystemTextJson;
using StjJson = System.Text.Json.Serialization;
#endregion

namespace AL.Core.Definitions;

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum RayType
{
    None,

    [EnumMember(Value = "tiling_burst")]
    TilingBurst,

    [EnumMember(Value = "tiling_burstj")]
    TilingBurstJ,

    [EnumMember(Value = "tiling_burst_g")]
    TilingBurstG
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum TargetType
{
    NotSingleTarget,

    [EnumMember(Value = "player")]
    PlayersOnly,

    [EnumMember(Value = "monster")]
    MonstersOnly,

    [EnumMember(Value = "true")]
    Any
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ALAttribute
{
    /// <summary>
    ///     No attribute. The default for an unset value; the server never sends a key that maps here.
    /// </summary>
    None,

    /// <summary>
    ///     Physical defense, fed to the server's diminishing-returns curve. The first 100 points cut incoming
    ///     physical damage by 0.1% each, tapering to 0.04% per point past 800.
    /// </summary>
    Armor,

    /// <summary>
    ///     How many physically-attacking monsters may target you before fear sets in, which cuts speed and attack
    ///     sharply. Magical and pure attackers count against MagicalCourage and PureCourage instead.
    /// </summary>
    Courage,

    /// <summary>
    ///     Percent chance to shrug off frozen and deepfreezed outright. An all-or-nothing roll as the condition
    ///     lands, not damage reduction.
    /// </summary>
    [EnumMember(Value = "fzresistance")]
    FreezeResistance,

    /// <summary>
    ///     Percent chance to shrug off burned outright, rolled as the condition lands.
    /// </summary>
    [EnumMember(Value = "firesistance")]
    FireResistance,

    /// <summary>
    ///     Percent chance to shrug off poisoned outright, rolled as the condition lands.
    /// </summary>
    [EnumMember(Value = "pnresistance")]
    PoisonResistance,

    /// <summary>
    ///     The game's own notes call this status resistance and intend it to shorten debuffs, but no live server
    ///     code reads it. Stuns are resisted by PhysicalResistance instead, despite this member's name.
    /// </summary>
    [EnumMember(Value = "stresistance")]
    StunResistance,

    /// <summary>
    ///     Percent chance to resist the stunned condition outright. The game's notes call it impact resistance; it
    ///     does not reduce physical damage, which is Armor's job.
    /// </summary>
    [EnumMember(Value = "phresistance")]
    PhysicalResistance,

    /// <summary>
    ///     Magic defense, run through the same diminishing-returns curve as Armor. The per-element resistances are
    ///     unrelated: those roll to block a condition rather than reducing damage.
    /// </summary>
    Resistance,

    /// <summary>
    ///     Percent chance to bounce an incoming magical attack back at its caster and take nothing. Capped at 30,
    ///     or 50 while the reflection buff is up.
    /// </summary>
    Reflection,

    /// <summary>
    ///     Percent of an incoming physical hit dealt straight back to a melee attacker, one whose range is under
    ///     75. Measured against the raw hit, before your armor reduced it.
    /// </summary>
    DReturn,

    /// <summary>
    ///     Percent chance to dodge an incoming physical attack entirely. Capped at 50, and it does nothing against
    ///     magical damage.
    /// </summary>
    Evasion,

    /// <summary>
    ///     Percent chance that your own attacks miss. A self-inflicted penalty carried by alcohol and by two-handed
    ///     bows; nothing on a target makes an incoming attack miss.
    /// </summary>
    Miss,

    /// <summary>
    ///     On equipment, points added to maximum hp. On a monster or character record it is the hp pool itself.
    /// </summary>
    Hp,

    /// <summary>
    ///     On equipment, points added to maximum mp. On a monster or character record it is the mp pool itself.
    /// </summary>
    Mp,

    /// <summary>
    ///     Armor piercing, subtracted from the target's armor before the damage curve. Driving the total negative
    ///     is worth up to 32% extra damage, where the curve stops.
    /// </summary>
    APiercing,

    /// <summary>
    ///     Resistance piercing, subtracted from the target's resistance before the damage curve. The magical
    ///     counterpart of APiercing.
    /// </summary>
    RPiercing,

    /// <summary>
    ///     Percent chance for a hit to crit. A crit deals double damage before CritDamage adds to it.
    /// </summary>
    Crit,

    /// <summary>
    ///     Percentage points added to the 2x crit multiplier, so 60 makes a crit hit for 2.6x. Worth nothing on its
    ///     own without Crit.
    /// </summary>
    CritDamage,

    /// <summary>
    ///     Base damage per hit, before the server's 10% random spread, Output and the target's defenses. A weapon's
    ///     attack is also scaled by the wielder's main stat.
    /// </summary>
    Attack,

    /// <summary>
    ///     Attack range in game units. Ranges are measured edge-to-edge between hit boxes, not centre to centre.
    /// </summary>
    Range,

    /// <summary>
    ///     Attacks per second, on two scales. A class's base and a monster's own value are the rate itself, roughly
    ///     0.35 to 1.6; anything layered on by gear or a condition is hundredths, so 60 means +0.6.
    /// </summary>
    Frequency,

    /// <summary>
    ///     Percent of the damage dealt returned to the attacker as hp.
    /// </summary>
    Lifesteal,

    /// <summary>
    ///     Percent of the damage dealt drained from the target and given to the attacker as mp.
    /// </summary>
    ManaSteal,

    /// <summary>
    ///     Marks a monster that steals gold when it hits you. A flag rather than a magnitude: the amount taken is 1
    ///     to 12 gold whatever the value says.
    /// </summary>
    GoldSteal,

    /// <summary>
    ///     Movement speed in game units per second. A character sits near 45 to 55; the server floors it at 5.
    /// </summary>
    Speed,

    /// <summary>
    ///     Generic stat points on scrollable gear rather than a stat of its own. They convert wholesale into
    ///     whichever stat the item was scrolled to, scaled by that stat's own rate.
    /// </summary>
    Stat,

    /// <summary>
    ///     Strength. Adds armor, maximum hp and a little speed, and scales weapon damage for warriors and paladins.
    /// </summary>
    Str,

    /// <summary>
    ///     Intelligence. Adds resistance, maximum mp and a little attack speed, and scales weapon damage for mages,
    ///     priests and merchants.
    /// </summary>
    Int,

    /// <summary>
    ///     Dexterity. Adds movement speed and attack speed, and scales weapon damage for rangers and rogues.
    /// </summary>
    Dex,

    /// <summary>
    ///     Vitality. Adds maximum hp and nothing else, giving more per point the higher your level.
    /// </summary>
    Vit,

    /// <summary>
    ///     Fortitude. Reduces damage taken from other players only, each point counting as 5 defense. Nothing to do
    ///     with Str, despite reading like an abbreviation of it.
    /// </summary>
    For,

    /// <summary>
    ///     Percentage points added to the drop-luck multiplier, so 20 makes luckm 1.2.
    /// </summary>
    Luck,

    /// <summary>
    ///     Percent multiplier on your attack. Most classes start at 100 and a priest at 40; the server floors the
    ///     total at 5.
    /// </summary>
    Output,

    /// <summary>
    ///     Splash intensity for magical hits. The splash reaches intensity/3.6 units and lands for that percent of
    ///     a full hit, still reduced by each victim's resistance.
    /// </summary>
    Blast,

    /// <summary>
    ///     Splash intensity for physical hits, worked out exactly as Blast is. The pairing runs the opposite way
    ///     round from what the two names suggest.
    /// </summary>
    Explosion,

    /// <summary>
    ///     Percent chance a physical hit stuns its target. Only rolled on attacks that proc effects.
    /// </summary>
    [EnumMember(Value = "stun")]
    StunChance,

    /// <summary>
    ///     On equipment, percentage points added to the xp multiplier. On a monster it is the xp the kill awards.
    /// </summary>
    XP,

    /// <summary>
    ///     Percentage points added to the gold multiplier. Not an item's price, which is a separate field.
    /// </summary>
    Gold,

    /// <summary>
    ///     Healing power. A priest's is set equal to their attack; on a condition or a monster ability it is the hp
    ///     restored on each tick.
    /// </summary>
    Heal,

    /// <summary>
    ///     The fraction of healing a condition lets through, 0.25 on poison. The server hard-codes the same number
    ///     rather than reading this key, so treat it as descriptive.
    /// </summary>
    [EnumMember(Value = "healm")]
    HealMod,

    /// <summary>
    ///     A condition's multiplier on attack speed, as a fraction, where Frequency is the rate itself. The server
    ///     hard-codes the multiplier rather than reading this key.
    /// </summary>
    [EnumMember(Value = "frequencym")]
    FrequencyMod,

    /// <summary>
    ///     The fraction of a potion's effect a condition lets through, 0.5 on poison. The server hard-codes the
    ///     halving rather than reading this key.
    /// </summary>
    [EnumMember(Value = "potionsm")]
    PotionsMod,

    /// <summary>
    ///     Lowers the chance a monster picks you as its target. Only the gap to Bling counts: the server rolls
    ///     against (bling - cuteness)/100.
    /// </summary>
    Cuteness,

    /// <summary>
    ///     Carried by a handful of joke items and shown in the client's tooltip, but no live server code reads it.
    /// </summary>
    Charisma,

    /// <summary>
    ///     Percent off the mp a skill costs, applied both to the affordability check and to the deduction. Separate
    ///     from MpCost, which is the mp a normal attack spends.
    /// </summary>
    [EnumMember(Value = "mp_reduction")]
    MPReduction,

    /// <summary>
    ///     Raises the chance a monster picks you as its target, the inverse of Cuteness. The server rolls against
    ///     (bling - cuteness)/100.
    /// </summary>
    Bling,

    /// <summary>
    ///     Carried by a single item and shown in the client's tooltip, but no live server code reads it.
    /// </summary>
    Awesomeness,

    //needed because AL.Data.Classes. doublehands/mainhand/offhand use it as a mod
    /// <summary>
    ///     Mp spent per normal attack. The class sets the base and gear adjusts it, then it grows with level and
    ///     with your crit, lifesteal and piercing.
    /// </summary>
    [EnumMember(Value = "mp_cost")]
    MpCost,

    /// <summary>
    ///     The first of two free-form numbers an item's ability or aura carries. Its meaning belongs to that
    ///     ability, and is most often a percent proc chance.
    /// </summary>
    [Obsolete("No idea what this is.")]
    Attr0,

    /// <summary>
    ///     The second free-form number an item's ability or aura carries. Rarely populated, and its meaning
    ///     likewise belongs to the ability.
    /// </summary>
    [Obsolete("No idea what this is.")]
    Attr1,

    /// <summary>
    ///     Percent chance a fishing rod or pickaxe breaks on a successful gather. Upgrading the tool lowers it.
    /// </summary>
    [Obsolete("Data bug, don't use.")]
    Breaks,

    //appended rather than placed beside Courage: a numeric wire value is parsed as an ordinal, so inserting a
    //member mid-enum silently repoints every member after it
    /// <summary>
    ///     How many magical attackers this can be engaged by before fear sets in. The server counts magical
    ///     attackers separately from physical and pure ones, and compares each count against its own limit.
    /// </summary>
    [EnumMember(Value = "mcourage")]
    MagicalCourage,

    /// <summary>
    ///     How many pure-damage attackers this can be engaged by before fear sets in. The rarest of the three, and
    ///     the one paladins carry most of.
    /// </summary>
    [EnumMember(Value = "pcourage")]
    PureCourage
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum LockType
{
    None,

    [EnumMember(Value = "l")]
    Locked,

    [EnumMember(Value = "s")]
    Sealed,

    [EnumMember(Value = "u")]
    Unlocked,

    [EnumMember(Value = "protected")]
    Protected,

    [EnumMember(Value = "key")]
    Key,

    [EnumMember(Value = "ulocked")]
    AlsoLocked = Locked
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum KeyType
{
    None,

    [EnumMember(Value = "frozenkey")]
    FrozenKey,

    [EnumMember(Value = "tombkey")]
    TombKey,

    [EnumMember(Value = "cryptkey")]
    CryptKey,

    [EnumMember(Value = "spiderkey")]
    SpiderKey,

    [EnumMember(Value = "complicated")]
    Complicated
}

public enum ALClass
{
    None,
    Mage,
    Merchant,
    Paladin,
    Priest,
    Ranger,
    Rogue,
    Warrior,
    NPC
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum Projectile
{
    None,
    Burst,
    Pinky,

    [EnumMember(Value = "stone_k")]
    StoneK,
    Plight,
    Acid,
    FireArrow,
    FrostBall,
    Curse,
    BigMagic,
    Cupid,
    SuperShot,

    [EnumMember(Value = "magic_divine")]
    MagicDivine,
    PMagic,

    [EnumMember(Value = "magic_purple")]
    MagicPurple,
    GArrow,
    SnowBall,
    FrostArrow,
    Pouch,
    PoisonArrow,
    Stone,
    Magic,
    CrossBowArrow,
    MMagic,
    Momentum,
    Arrow,
    Wandy,
    FireBall,
    WMomentum
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ArmorSet
{
    None,
    Bunny,
    WT3,
    WT4,
    Vampires,
    Easter,
    Fury,
    MRanger,
    MRogue,
    MMage,
    Holidays,
    Wanderers,
    MMerchant,
    MPriest,
    MWarrior,
    Rugged,
    Swift,
    Tiger,
    MPX,
    Legends
}

//the server keys bank packs as "items0", "items1", ... - the PascalCase name matches nothing
[StjJson.JsonConverter(typeof(StjConverters.LowerCaseTolerantStringEnumConverterFactory))]
public enum BankPack
{
    None,
    Items0,
    Items1,
    Items2,
    Items3,
    Items4,
    Items5,
    Items6,
    Items7,
    Items8,
    Items9,
    Items10,
    Items11,
    Items12,
    Items13,
    Items14,
    Items15,
    Items16,
    Items17,
    Items18,
    Items19,
    Items20,
    Items21,
    Items22,
    Items23,
    Items24,
    Items25,
    Items26,
    Items27,
    Items28,
    Items29,
    Items30,
    Items31,
    Items32,
    Items33,
    Items34,
    Items35,
    Items36,
    Items37,
    Items38,
    Items39,
    Items40,
    Items41,
    Items42,
    Items43,
    Items44,
    Items45,
    Items46,
    Items47
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum WorldType
{
    None,
    Dungeon
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum Condition
{
    None,
    Anger,
    Authfail,
    Blink,
    Block,
    Burned,
    Charging,
    Charmed,
    Citizen0Aura,
    Citizen4Aura,
    CoOp,

    [EnumMember(Value = "curse_aura")]
    CurseAura,
    Cursed,

    [EnumMember(Value = "damage_received")]
    DamageReceived,
    Dampened,

    [EnumMember(Value = "dampening_aura")]
    DampeningAura,
    DarkBlessing,
    Dash,
    DeepFreeze,
    Deepfreezed,
    Degen,
    EasterLuck,
    EBurn,
    EHeal,
    Energized,
    Ethereal,
    Filter,
    Fingered,
    Fishing,
    Frozen,
    FullGuard,
    FullguardX,
    Halloween0,
    Halloween1,
    Halloween2,
    HardShell,
    Heal,
    Healed,
    Healing,
    HolidaySpirit,
    HopSickness,
    Invincible,
    Invis,

    [EnumMember(Value = "licenced")]
    Licensed,
    Magiport,
    Marked,
    MassExchange,
    MassExchangePP,
    MassProduction,
    MassProductionPP,
    MCourage,
    MFrenzy,
    MLifeSteal,
    MLight,
    MLuck,
    Mining,
    MonsterHunt,
    MShield,
    MTangle,

    [EnumMember(Value = "multi_burn")]
    MultiBurn,

    [EnumMember(Value = "multi_freeze")]
    MultiFreeze,
    Mute,
    NewcomersBlessing,
    NotVerified,
    PatronsGrace,

    [EnumMember(Value = "penalty_cd")]
    PenaltyCooldown,
    PhasedOut,
    PickPocket,
    Poisoned,
    Poisonous,
    Power,
    Purifier,
    Reflection,
    RSpeed,
    Sanguine,

    [EnumMember(Value = "self_healing")]
    SelfHealing,
    Shocked,
    Sleeping,
    Slowness,

    [EnumMember(Value = "stack")]
    Stacked,
    Stone,
    Stoned,
    Stunned,
    SugarRush,
    Tangle,
    Tangled,
    Town,
    Typing,
    WarCry,
    WarpStomp,
    Weakness,

    [EnumMember(Value = "weakness_aura")]
    WeaknessAura,
    Withdrawal,
    Woven,
    XPower,
    XShotted,
    Young,
    Zap
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum DamageType
{
    None,
    Magical,
    Physical,
    Pure,
    Heal
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ItemType
{
    Activator,
    Amulet,

    [EnumMember(Value = "bank_key")]
    BankKey,
    Belt,
    Booster,
    Box,
    Cape,
    Chest,
    Chrysalis,
    Computer,
    Container,
    Cosmetics,
    Cscroll,

    [EnumMember(Value = "dungeon_key")]
    DungeonKey,
    Earring,
    Elixir,
    Flute,
    Gem,
    Gloves,
    Helmet,
    Jar,
    Licence,
    Material,
    Misc,

    [EnumMember(Value = "misc_offhand")]
    MiscOffhand,
    Offering,
    Orb,
    Pants,
    Petlicence,
    Placeholder,
    Pot,
    PScroll,
    Qubics,
    Quest,
    Quiver,
    Ring,
    Shield,
    Shoes,

    [EnumMember(Value = "skill_item")]
    SkillItem,
    Source,
    Spawner,
    Stand,
    Stone,
    Test,
    Throw,
    Token,
    Tome,
    Tool,
    Tracker,
    UScroll,
    Weapon,
    XP
}

[Flags]
[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum WeaponType : ulong
{
    None = 0,
    Axe = 1,
    Basher = 1 << 1,
    Bow = 1 << 2,
    Crossbow = 1 << 3,
    Dagger = 1 << 4,
    DartGun = 1 << 5,
    Fist = 1 << 6,

    [EnumMember(Value = "great_staff")]
    GreatStaff = 1 << 7,

    [EnumMember(Value = "great_sword")]
    GreatSword = 1 << 8,
    Mace = 1 << 9,

    [EnumMember(Value = "misc_offhand")]
    MiscOffhand = 1 << 10,
    PMace = 1 << 11,
    Quiver = 1 << 12,
    Rapier = 1 << 13,
    Rod = 1 << 14,
    Scythe = 1 << 15,
    Shield = 1 << 16,

    [EnumMember(Value = "short_sword")]
    ShortSword = 1 << 17,
    Source = 1 << 18,
    Spear = 1 << 19,
    Staff = 1 << 20,
    Stars = 1 << 21,
    Sword = 1 << 22,
    Wand = 1 << 23,
    WBlade = 1 << 24,
    PickAxe = 1 << 25
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum EquipmentSlot
{
    None,
    Amulet,
    Belt,
    Cape,
    Chest,
    Earring1,
    Earring2,
    Elixir,
    Gloves,
    Helmet,
    MainHand,
    OffHand,
    Orb,
    Pants,
    Ring1,
    Ring2,
    Shoes
}

//the server keys trade slots as "trade1".."trade30"
[StjJson.JsonConverter(typeof(StjConverters.LowerCaseTolerantStringEnumConverterFactory))]
public enum TradeSlot
{
    None,
    Trade1 = 17,
    Trade2,
    Trade3,
    Trade4,
    Trade5,
    Trade6,
    Trade7,
    Trade8,
    Trade9,
    Trade10,
    Trade11,
    Trade12,
    Trade13,
    Trade14,
    Trade15,
    Trade16,
    Trade17,
    Trade18,
    Trade19,
    Trade20,
    Trade21,
    Trade22,
    Trade23,
    Trade24,
    Trade25,
    Trade26,
    Trade27,
    Trade28,
    Trade29,
    Trade30
}

//the server keys equipment slots as "mainhand", "offhand", "ring1", ...
[StjJson.JsonConverter(typeof(StjConverters.LowerCaseTolerantStringEnumConverterFactory))]
public enum Slot
{
    None,
    Amulet,
    Belt,
    Cape,
    Chest,
    Earring1,
    Earring2,
    Elixir,
    Gloves,
    Helmet,
    MainHand,
    OffHand,
    Orb,
    Pants,
    Ring1,
    Ring2,
    Shoes,

    Trade1,
    Trade2,
    Trade3,
    Trade4,
    Trade5,
    Trade6,
    Trade7,
    Trade8,
    Trade9,
    Trade10,
    Trade11,
    Trade12,
    Trade13,
    Trade14,
    Trade15,
    Trade16,
    Trade17,
    Trade18,
    Trade19,
    Trade20,
    Trade21,
    Trade22,
    Trade23,
    Trade24,
    Trade25,
    Trade26,
    Trade27,
    Trade28,
    Trade29,
    Trade30
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum DisappearEffect
{
    None,
    Town,
    Blink,
    MagiPort
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum Direction
{
    Down,
    Left,
    Right,
    Up,
    Invalid
}

public enum Stand
{
    None,
    Stand,
    CStand,
    Stand0,
    Stand1
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum DropType
{
    None,

    [EnumMember(Value = "m1")]
    Mining1,

    [EnumMember(Value = "m2")]
    Mining2,

    [EnumMember(Value = "f1")]
    Fishing1
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum SpawnType
{
    Normal,

    [EnumMember(Value = "randomrespawn")]
    Random
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum TrapType
{
    None,
    Debuff,
    Spikes
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ZoneType
{
    None,
    Fishing,
    Mining
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum AchievementRewardType
{
    None,
    Stat
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum Quest
{
    None,
    MCollector,
    Witch,
    CX,
    GemFragment,
    Glitch,
    Leather,
    LostEarring,
    Seashell,
    Ornament,
    Candycane,
    Mistletoe
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum Token
{
    None,
    FriendToken,
    FunToken,
    MonsterToken,
    PvPToken
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum NPCRole
{
    None,
    Announcer,
    Blocker,
    Bouncer,
    Citizen,
    Companion,
    Compound,
    Craftsman,
    CX,

    [EnumMember(Value = "daily_events")]
    DailyEvents,
    Events,
    Exchange,
    FriendTokens,
    FunTokens,
    Gold,
    Guard,
    Items,
    Jailer,
    Locksmith,
    LostAndFound,
    Lottery,
    MCollector,
    Merchant,
    MonsterTokens,
    NewUpgrade,

    [EnumMember(Value = "newyear_tree")]
    NewYearTree,
    PetKeeper,
    Premium,

    [EnumMember(Value = "pvp_announcer")]
    PvPAnnouncer,
    PvPTokens,
    Quest,
    Repeater,
    Rewards,
    Resort,
    Santa,
    SecondHands,
    Shells,
    Ship,
    Shrine,
    StandMerchant,
    Tavern,
    Tease,
    TheSearch,
    Transport,
    Witch,

    //appended rather than sorted in: the implicit numbering is pinned by the tolerance matrix, and the
    //numeric-string fallback resolves against it
    Favors,
    Scrollsmith
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum SkillType
{
    None,
    Ability,
    GM,
    Monster,
    Passive,
    Skill,
    Utility
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum Emotion
{
    None,

    [EnumMember(Value = "drop_egg")]
    DropEgg,

    [EnumMember(Value = "hearts_single")]
    HeartsSingle
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ExitType
{
    None,
    Door,
    Transporter
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum EntitiesUpdateType
{
    None,
    All,

    [EnumMember(Value = "xy")]
    Partial
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum QueuedActionType
{
    Unknown,
    Compound,
    Upgrade,
    Exchange,
    FunTokens,
    PvPTokens,
    FriendTokens,
    MonsterTokens,
    Poof
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum UIDataType
{
    Unknown,

    [EnumMember(Value = "fishing_fail")]
    FishingFail,

    [EnumMember(Value = "fishing_none")]
    FishingNone,

    [EnumMember(Value = "fishing_start")]
    FishingStart,
    MassProduction,

    [EnumMember(Value = "mining_fail")]
    MiningFail,

    [EnumMember(Value = "mining_none")]
    MiningNone,

    [EnumMember(Value = "mining_start")]
    MiningStart,
    MLuck,
    Scare,

    [EnumMember(Value = "+$")]
    GainMoney,

    [EnumMember(Value = "-$")]
    LoseMoney,

    //appended rather than sorted in: the implicit numbering is pinned by the tolerance matrix, and the
    //numeric-string fallback resolves against it
    Energize,

    [EnumMember(Value = "restore_mp")]
    RestoreMP,

    //the two "draw a line between the two players" texts; sender/receiver rather than a single anchor
    [EnumMember(Value = "gold_sent")]
    GoldSent,

    [EnumMember(Value = "item_sent")]
    ItemSent
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ObtainType
{
    Unknown,
    Craft,
    Exchange,
    Buy,
    Quest
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum ChestType
{
    Unknown,
    Chest1,
    Chest2,
    Chest3,
    Chest4,
    Chest5,
    Chest6,
    Chest7,
    Chest8,

    [EnumMember(Value = "chestp")]
    ChestP
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum GamePlayMode
{
    Unknown,
    Normal,
    Hardcore,
    Test,
    Dungeon
}