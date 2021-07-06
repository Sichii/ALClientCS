using System;
using System.Runtime.Serialization;
using AL.Core.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.Core.Definitions
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ALAttribute
    {
        None,
        Armor,

        [EnumMember(Value = "fzresistance")]
        FreezeResistance,

        [EnumMember(Value = "firesistance")]
        FireResistance,

        [EnumMember(Value = "pnresistance")]
        PoisonResistance,
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

        [EnumMember(Value = "mp_cost")]
        MpCost,
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
        Unlocked
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
        MPX,
        Legends
    }

    [JsonConverter(typeof(StringEnumConverter))]
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
        HardShell,
        HolidaySpirit,
        Heal,
        Invincible,
        Invis,

        [EnumMember(Value = "licenced")]
        Licensed,
        Marked,
        MassProduction,
        MassProductionPP,
        MCourage,
        MLifeSteal,
        MLuck,
        MonsterHunt,
        MShield,
        MTangle,
        NotVerified,
        PhasedOut,
        Poisoned,
        Poisonous,
        Power,
        Reflection,
        RSpeed,
        Sanguine,
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
        Withdrawal,
        XPower,
        XShotted,
        Young,

        //MONSTER ABILITIES
        [EnumMember(Value = "self_healing")]
        SelfHealing
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
    public enum WeaponType
    {
        None,
        Axe,
        Basher,
        Bow,
        Crossbow,
        Dagger,
        DartGun,
        Fist,

        [EnumMember(Value = "great_staff")]
        GreatStaff,

        [EnumMember(Value = "great_sword")]
        GreatSword,
        Mace,

        [EnumMember(Value = "misc_offhand")]
        MiscOffhand,
        PMace,
        Quiver,
        Rapier,
        Rod,
        Scythe,
        Shield,

        [EnumMember(Value = "short_sword")]
        ShortSword,
        Source,
        Spear,
        Staff,
        Stars,
        Sword,
        Wand,
        WBlade,
        PickAxe
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
        Exchange,
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
        FunToken,
        MonsterToken,
        PvPToken
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
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
        Exchange,
        FunTokens,
        Gold,
        Guard,
        Items,
        Jailer,
        LockSmith,
        LostAndFound,
        LotteryLady,
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
        Resort,
        Rewards,
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
        NPC
    }
}