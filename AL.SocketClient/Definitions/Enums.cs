using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AL.SocketClient.Definitions
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameResponseType
    {
        Unknown,
        [EnumMember(Value = "bank_restrictions")]
        BankRestrictions,
        [EnumMember(Value = "buy_cant_npc")]
        BuyCantNPC,
        [EnumMember(Value = "buy_cant_space")]
        BuyCantSpace,
        [EnumMember(Value = "buy_cost")]
        BuyCost,
        [EnumMember(Value = "buy_get_closer")]
        BuyGetCloser,
        [EnumMember(Value = "cant_escape")]
        CantEscape,
        [EnumMember(Value = "upgrade_incompatible_scroll")]
        UpgradeIncompatibleScroll,
        [EnumMember(Value = "upgrade_in_progress")]
        UpgradeInProgress,
        [EnumMember(Value = "upgrade_chance")]
        UpgradeChance, //Emit calculate: true
        [EnumMember(Value = "upgrade_success")]
        UpgradeSuccess,
        [EnumMember(Value = "upgrade_fail")]
        UpgradeFail,
        [EnumMember(Value = "upgrade_no_item")]
        UpgradeNoItem,
        [EnumMember(Value = "upgrade_mismatch")]
        UpgradeMismatch,
        [EnumMember(Value = "upgrade_cant")]
        UpgradeCant,
        [EnumMember(Value = "upgrade_invalid_offering")]
        UpgradeInvalidOffering,
        //The compound equivalent to this is exception (omegalul)
        [EnumMember(Value = "upgrade_no_scroll")]
        UpgradeNoScroll,
        [EnumMember(Value = "compound_incompatible_scroll")]
        CompoundIncompatibleScroll,
        [EnumMember(Value = "compound_in_progress")]
        CompoundInProgress,
        [EnumMember(Value = "compound_chance")]
        CompoundChance, //EmitAsync calculate: true
        [EnumMember(Value = "compound_success")]
        CompoundSuccess,
        [EnumMember(Value = "compound_fail")]
        CompoundFail,
        [EnumMember(Value = "compound_no_item")]
        CompoundNoItem,
        [EnumMember(Value = "compound_mismatch")]
        CompoundMismatch,
        [EnumMember(Value = "compound_cant")]
        CompoundCant,
        [EnumMember(Value = "compound_invalid_offering")]
        CompoundInvalidOffering,
        [EnumMember(Value = "misc_fail")]
        MiscFail,
        [EnumMember(Value = "exception")]
        Exception,
        [EnumMember(Value = "ecu_get_closer")]
        ECUGetCloser,
        [EnumMember(Value = "emotion_cant")]
        EmotionCant,
        [EnumMember(Value = "emotion_cooldown")]
        EmotionCooldown,
        [EnumMember(Value = "exchange_existing")]
        ExchangeExisting,
        [EnumMember(Value = "exchange_notenough")]
        ExchangeNotEnough,
        [EnumMember(Value = "monsterhunt_merchant")]
        MonsterHuntMerchant,
        [EnumMember(Value = "monsterhunt_started")]
        MonsterHuntStarted,
        [EnumMember(Value = "no_level")]
        NoLevel,
        [EnumMember(Value = "no_target")]
        NoTarget,
        [EnumMember(Value = "resolve_skill")]
        ResolveSkill,
        [EnumMember(Value = "send_no_space")]
        SendNoSpace,
        [EnumMember(Value = "send_no_item")]
        SendNoItem,
        [EnumMember(Value = "skill_cant_incapacitated")]
        SkillCantIncapacitated,
        [EnumMember(Value = "skill_cant_wtype")]
        SkillCantWType,
        [EnumMember(Value = "skill_too_far")]
        SkillTooFar,
        [EnumMember(Value = "trade_bspace")]
        TradeBSpace,
        [EnumMember(Value = "trade_get_closer")]
        TradeGetCloser,
        [EnumMember(Value = "attack_failed")]
        AttackFailed,
        [EnumMember(Value = "bank_opx")]
        BankOperation,
        [EnumMember(Value = "buy_success")]
        BuySuccess,
        [EnumMember(Value = "cooldown")]
        Cooldown,
        [EnumMember(Value = "craft")]
        Craft,
        [EnumMember(Value = "defeated_by_a_monster")]
        DefeatedByAMonster,
        [EnumMember(Value = "disabled")]
        Disabled,
        [EnumMember(Value = "ex_condition")]
        ConditionExpired,
        [EnumMember(Value = "gold_sent")]
        GoldSent,
        [EnumMember(Value = "item_sent")]
        ItemSent,
        [EnumMember(Value = "mail_item_taken")]
        MailItemTaken,
        [EnumMember(Value = "magiport_failed")]
        MagiportFailed,
        [EnumMember(Value = "magiport_sent")]
        MagiportSent,
        [EnumMember(Value = "no_mp")]
        NoMP,
        [EnumMember(Value = "seashell_success")]
        SeashellSuccess,
        [EnumMember(Value = "skill_fail")]
        SkillFail,
        [EnumMember(Value = "skill_success")]
        SkillSuccess,
        [EnumMember(Value = "too_far")]
        TooFar,
        [EnumMember(Value = "gold_received")]
        GoldReceived,
        [EnumMember(Value = "item_placeholder")]
        ItemPlaceholder,
        [EnumMember(Value = "item_received")]
        ItemReceived,
        [EnumMember(Value = "transport_cant_reach")]
        TransportCantReach,
        Invalid,
        [EnumMember(Value = "non_friendly_target")]
        NonFriendlyTarget,
        [EnumMember(Value = "op_unavailable")]
        OperationUnavailable,
        [EnumMember(Value = "no_item")]
        NoItem,
        [EnumMember(Value = "log_gold_not_enough")]
        NotEnoughGold,
        [EnumMember(Value = "slot_occuppied")]
        SlotOccupied
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ALSocketMessageType
    {
        Unknown,
        Invite,
        Welcome,
        [EnumMember(Value = "player")]
        Character,
        Correction,
        Players,
        [EnumMember(Value = "server_info")]
        ServerInfo,
        Entities,
        [EnumMember(Value = "ping_ack")]
        PingAck,
        Action,
        [EnumMember(Value = "chat_log")]
        ChatLog,
        Hit,
        SecondHands,
        Disappear,
        [EnumMember(Value = "new_map")]
        NewMap,
        [EnumMember(Value = "chest_opened")]
        ChestOpened,
        [EnumMember(Value = "achievement_progress")]
        AchievementProgress,
        Drop,
        Eval,
        [EnumMember(Value = "game_error")]
        GameError,
        [EnumMember(Value = "game_response")]
        GameResponse,
        Start,
        [EnumMember(Value = "party_update")]
        PartyUpdate,
        [EnumMember(Value = "q_data")]
        QueuedActionData,
        [EnumMember(Value = "upgrade")]
        QueuedActionResult,
        [EnumMember(Value = "game_log")]
        GameLog,
        Death,
        [EnumMember(Value = "disappearing_text")]
        DisappearingText,
        UI,
        NotThere
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ALSocketEmitType
    {
        Unknown,
        Attack,
        Auth,
        Bank,
        Booster,
        Buy,
        [EnumMember(Value = "cm")]
        Command,
        Compound,
        Craft,
        Emotion,
        Equip,
        Exchange,
        [EnumMember(Value = "imove")]
        InventoryMove,
        [EnumMember(Value = "leave")]
        LeaveMap,
        Loaded,
        Magiport,
        MonsterHunt,
        Move,
        [EnumMember(Value = "open_chest")]
        OpenChest,
        Party,
        [EnumMember(Value = "ping_trig")]
        Ping,
        Players,
        Property,
        [EnumMember(Value = "town")]
        ReturnToTown,
        Respawn,
        SecondHands,
        [EnumMember(Value = "sbuy")]
        SecondHandsBuy,
        Sell,
        Send,
        [EnumMember(Value = "send_updates")]
        SendUpdates,
        Skill,
        [EnumMember(Value = "stop")]
        Stop,
        Swap,
        [EnumMember(Value = "mail_take_item")]
        TakeMailItem,
        Tracker,
        [EnumMember(Value = "trade_buy")]
        TradeBuy,
        [EnumMember(Value = "trade_wishlist")]
        TradeWishlist,
        Transport,
        Unequip,
        Upgrade,
        Use,
        Merchant
    }
}