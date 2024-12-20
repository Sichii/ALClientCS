﻿#region
using System;
using System.Runtime.Serialization;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endregion

namespace AL.Core.Definitions;

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
public enum ALAttribute
{
    None,
    Armor,
    Courage,

    [EnumMember(Value = "fzresistance")]
    FreezeResistance,

    [EnumMember(Value = "firesistance")]
    FireResistance,

    [EnumMember(Value = "pnresistance")]
    PoisonResistance,

    [EnumMember(Value = "stresistance")]
    StunResistance,

    [EnumMember(Value = "phresistance")]
    PhysicalResistance,
    Resistance,
    Reflection,
    DReturn,
    Evasion,
    Miss,
    Hp,
    Mp,
    APiercing,
    RPiercing,
    Crit,
    CritDamage,
    Attack,
    Range,
    Frequency,
    Lifesteal,
    ManaSteal,
    GoldSteal,
    Speed,
    Stat,
    Str,
    Int,
    Dex,
    Vit,
    For,
    Luck,
    Output,
    Blast,
    Explosion,

    [EnumMember(Value = "stun")]
    StunChance,
    XP,
    Gold,

    [EnumMember(Value = "healm")]
    HealMod,

    [EnumMember(Value = "frequencym")]
    FrequencyMod,

    [EnumMember(Value = "potionsm")]
    PotionsMod,
    Cuteness,
    Charisma,

    [EnumMember(Value = "mp_reduction")]
    MPReduction,
    Bling,
    Awesomeness,

    //needed because AL.Data.Classes. doublehands/mainhand/offhand use it as a mod
    [EnumMember(Value = "mp_cost")]
    MpCost,

    [Obsolete("No idea what this is.")]
    Attr0,

    [Obsolete("No idea what this is.")]
    Attr1,

    [Obsolete("Data bug, don't use.")]
    Breaks
}

[JsonConverter(typeof(StringEnumConverter))]
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

public enum KeyType
{
    None,

    [EnumMember(Value = "frozenkey")]
    FrozenKey,

    [EnumMember(Value = "tombkey")]
    TombKey,

    [EnumMember(Value = "cryptkey")]
    CryptKey,

    [EnumMember(Value = "complicated")]
    Complicated
}

[JsonConverter(typeof(ALClassConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
public enum BankPack
{
    None,

    [JsonProperty("items0")]
    Items0,

    [JsonProperty("items1")]
    Items1,

    [JsonProperty("items2")]
    Items2,

    [JsonProperty("items3")]
    Items3,

    [JsonProperty("items4")]
    Items4,

    [JsonProperty("items5")]
    Items5,

    [JsonProperty("items6")]
    Items6,

    [JsonProperty("items7")]
    Items7,

    [JsonProperty("items8")]
    Items8,

    [JsonProperty("items9")]
    Items9,

    [JsonProperty("items10")]
    Items10,

    [JsonProperty("items11")]
    Items11,

    [JsonProperty("items12")]
    Items12,

    [JsonProperty("items13")]
    Items13,

    [JsonProperty("items14")]
    Items14,

    [JsonProperty("items15")]
    Items15,

    [JsonProperty("items16")]
    Items16,

    [JsonProperty("items17")]
    Items17,

    [JsonProperty("items18")]
    Items18,

    [JsonProperty("items19")]
    Items19,

    [JsonProperty("items20")]
    Items20,

    [JsonProperty("items21")]
    Items21,

    [JsonProperty("items22")]
    Items22,

    [JsonProperty("items23")]
    Items23,

    [JsonProperty("items24")]
    Items24,

    [JsonProperty("items25")]
    Items25,

    [JsonProperty("items26")]
    Items26,

    [JsonProperty("items27")]
    Items27,

    [JsonProperty("items28")]
    Items28,

    [JsonProperty("items29")]
    Items29,

    [JsonProperty("items30")]
    Items30,

    [JsonProperty("items31")]
    Items31,

    [JsonProperty("items32")]
    Items32,

    [JsonProperty("items33")]
    Items33,

    [JsonProperty("items34")]
    Items34,

    [JsonProperty("items35")]
    Items35,

    [JsonProperty("items36")]
    Items36,

    [JsonProperty("items37")]
    Items37,

    [JsonProperty("items38")]
    Items38,

    [JsonProperty("items39")]
    Items39,

    [JsonProperty("items40")]
    Items40,

    [JsonProperty("items41")]
    Items41,

    [JsonProperty("items42")]
    Items42,

    [JsonProperty("items43")]
    Items43,

    [JsonProperty("items44")]
    Items44,

    [JsonProperty("items45")]
    Items45,

    [JsonProperty("items46")]
    Items46,

    [JsonProperty("items47")]
    Items47
}

[JsonConverter(typeof(StringEnumConverter))]
public enum WorldType
{
    None,
    Dungeon
}

[JsonConverter(typeof(StringEnumConverter))]
public enum Condition
{
    None,
    Authfail,
    Blink,
    Burned,
    Charging,
    Charmed,
    Citizen0Aura,
    Citizen4Aura,
    CoOp,
    Cursed,
    Dampened,
    DarkBlessing,
    EasterLuck,
    EBurn,
    EHeal,
    Energized,
    Fingered,
    Fishing,
    Frozen,
    FullGuard,
    FullguardX,
    HardShell,
    HolidaySpirit,
    Heal,
    Invincible,
    Invis,

    [EnumMember(Value = "licenced")]
    Licensed,
    Magiport,
    Marked,
    MassProduction,
    MassProductionPP,
    MCourage,
    MLifeSteal,
    MLuck,
    MFrenzy,
    MonsterHunt,
    MShield,
    MTangle,

    [EnumMember(Value = "multi_freeze")]
    MultiFreeze,
    NewcomersBlessing,
    NotVerified,

    [EnumMember(Value = "penalty_cd")]
    PenaltyCooldown,
    PhasedOut,
    Poisoned,
    Poisonous,
    Power,
    Reflection,
    RSpeed,
    Sanguine,

    [EnumMember(Value = "self_healing")]
    SelfHealing,
    Shocked,
    Slowness,

    [EnumMember(Value = "stack")]
    Stacked,
    Stoned,
    Stunned,
    SugarRush,
    Tangled,
    Town,
    Typing,
    WarCry,
    Weakness,

    [EnumMember(Value = "weakness_aura")]
    WeaknessAura,
    Withdrawal,
    XPower,
    XShotted,
    Young
}

[JsonConverter(typeof(StringEnumConverter))]
public enum DamageType
{
    None,
    Magical,
    Physical,
    Pure,
    Heal
}

[JsonConverter(typeof(StringEnumConverter))]
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

[Flags, JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
public enum DisappearEffect
{
    None,
    Town,
    Blink,
    MagiPort
}

[JsonConverter(typeof(StringEnumConverter))]
public enum Direction
{
    Down,
    Left,
    Right,
    Up,
    Invalid
}

[JsonConverter(typeof(FalsyConverter<Stand>), None)]
public enum Stand
{
    None,
    Stand,
    CStand,
    Stand0
}

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

[JsonConverter(typeof(StringEnumConverter))]
public enum SpawnType
{
    Normal,

    [EnumMember(Value = "randomrespawn")]
    Random
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TrapType
{
    None,
    Debuff,
    Spikes
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ZoneType
{
    None,
    Fishing,
    Mining
}

[JsonConverter(typeof(StringEnumConverter))]
public enum AchievementRewardType
{
    None,
    Stat
}

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
public enum Token
{
    None,
    FriendToken,
    FunToken,
    MonsterToken,
    PvPToken
}

[JsonConverter(typeof(StringEnumConverter))]
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
    Witch
}

[JsonConverter(typeof(StringEnumConverter))]
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

[JsonConverter(typeof(StringEnumConverter))]
public enum Emotion
{
    None,

    [EnumMember(Value = "drop_egg")]
    DropEgg,

    [EnumMember(Value = "hearts_single")]
    HeartsSingle
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ExitType
{
    None,
    Door,
    Transporter
}

[JsonConverter(typeof(StringEnumConverter))]
public enum EntitiesUpdateType
{
    None,
    All,

    [EnumMember(Value = "xy")]
    Partial
}

[JsonConverter(typeof(StringEnumConverter))]
public enum QueuedActionType
{
    Unknown,
    Compound,
    Upgrade,
    Exchange
}

[JsonConverter(typeof(StringEnumConverter))]
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
    LoseMoney
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ObtainType
{
    Unknown,
    Craft,
    Exchange,
    Buy,
    Quest
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ChestType
{
    Unknown,
    Chest1,
    Chest2,
    Chest3,
    Chest4,
    Chest5,
    Chest6
}

[JsonConverter(typeof(StringEnumConverter))]
public enum GamePlayMode
{
    Unknown,
    Normal,
    Hardcore
}