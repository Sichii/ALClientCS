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
    Heal,

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