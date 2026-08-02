#region
using System.Runtime.Serialization;
using StjConverters = AL.Core.Json.SystemTextJson;
using StjJson = System.Text.Json.Serialization;
#endregion

namespace AL.SocketClient.Definitions;

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
public enum GameResponseType
{
    Unknown,

    [EnumMember(Value = "buy_cant_npc")]
    BuyCantNPC,

    [EnumMember(Value = "buy_cant_space")]
    BuyCantSpace,

    [EnumMember(Value = "buy_cost")]
    BuyCost,

    [EnumMember(Value = "cant_escape")]
    CantEscape,

    //explicit, so attaching a naming strategy to this converter could never silently unbind it
    [EnumMember(Value = "data")]
    Data,

    [EnumMember(Value = "dismantle_cant")]
    DismantleCant,

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

    [EnumMember(Value = "send_no_space")]
    SendNoSpace,

    [EnumMember(Value = "send_no_item")]
    SendNoItem,

    [EnumMember(Value = "skill_cant_incapacitated")]
    SkillCantIncapacitated,

    [EnumMember(Value = "skill_cant_wtype")]
    SkillCantWType,

    [EnumMember(Value = "trade_bspace")]
    TradeBSpace,

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

    [EnumMember(Value = "transport_failed")]
    TransportFailed,

    //explicit, so attaching a naming strategy to this converter could never silently unbind it
    [EnumMember(Value = "invalid")]
    Invalid,

    [EnumMember(Value = "non_friendly_target")]
    NonFriendlyTarget,

    [EnumMember(Value = "no_item")]
    NoItem,

    [EnumMember(Value = "slot_occuppied")]
    SlotOccupied,

    //the single code that replaced every *_get_closer. Nine of its emit sites send a bare string rather
    //than an object, so it must be matched without requiring Failed, and Place must stay optional.
    [EnumMember(Value = "distance")]
    Distance,

    [EnumMember(Value = "cant_in_bank")]
    CantInBank,

    [EnumMember(Value = "condition")]
    Condition,

    [EnumMember(Value = "gold_not_enough")]
    GoldNotEnough,

    [EnumMember(Value = "not_ready")]
    NotReady,

    [EnumMember(Value = "cant_equip")]
    CantEquip,

    [EnumMember(Value = "cant_respawn")]
    CantRespawn,

    [EnumMember(Value = "item_locked")]
    ItemLocked,

    [EnumMember(Value = "item_blocked")]
    ItemBlocked,

    [EnumMember(Value = "inventory_full")]
    InventoryFull,

    [EnumMember(Value = "no_space")]
    NoSpace,

    [EnumMember(Value = "inv_size")]
    InvSize,

    [EnumMember(Value = "max_level")]
    MaxLevel,

    [EnumMember(Value = "dismantle")]
    Dismantle,

    [EnumMember(Value = "upgrade_offering_success")]
    UpgradeOfferingSuccess,

    [EnumMember(Value = "upgrade_success_stat")]
    UpgradeSuccessStat,

    [EnumMember(Value = "upgrade_scroll_q")]
    UpgradeScrollQ,

    [EnumMember(Value = "compound_no_scroll")]
    CompoundNoScroll,

    [EnumMember(Value = "skill_immune")]
    SkillImmune,

    [EnumMember(Value = "invalid_target")]
    InvalidTarget,

    [EnumMember(Value = "target_invincible")]
    TargetInvincible,

    [EnumMember(Value = "target_alive")]
    TargetAlive,

    //revive's rejection for a gravestone below full hp. Built by hand rather than by fail_response, so it carries
    //neither place nor failed - the response code is the only discriminator it has.
    [EnumMember(Value = "revive_failed")]
    ReviveFailed,

    [EnumMember(Value = "not_in_pvp")]
    NotInPvP,

    [EnumMember(Value = "friendly")]
    Friendly,

    [EnumMember(Value = "no_skill")]
    NoSkill,

    [EnumMember(Value = "skill_cant_use")]
    SkillCantUse,

    [EnumMember(Value = "skill_cant_safe")]
    SkillCantSafe,

    [EnumMember(Value = "skill_cant_slot")]
    SkillCantSlot,

    [EnumMember(Value = "skill_cant_charges")]
    SkillCantCharges,

    [EnumMember(Value = "skill_cant_item")]
    SkillCantItem,

    [EnumMember(Value = "skill_cant_requirements")]
    SkillCantRequirements,

    [EnumMember(Value = "skill_cant_pve")]
    SkillCantPvE,

    [EnumMember(Value = "skill_no_item")]
    SkillNoItem,

    [EnumMember(Value = "loot_failed")]
    LootFailed,

    [EnumMember(Value = "loot_no_space")]
    LootNoSpace,

    [EnumMember(Value = "party_full")]
    PartyFull,

    [EnumMember(Value = "player_gone")]
    PlayerGone,

    //sent by success_response, so this arrives with Success set even though nothing happened
    [EnumMember(Value = "already_in_party")]
    AlreadyInParty,

    [EnumMember(Value = "invitation_expired")]
    InvitationExpired,

    [EnumMember(Value = "request_expired")]
    RequestExpired,

    [EnumMember(Value = "receiver_unavailable")]
    ReceiverUnavailable,

    [EnumMember(Value = "seller_gone")]
    SellerGone,

    [EnumMember(Value = "insufficient_q")]
    InsufficientQuantity,

    [EnumMember(Value = "item_gone")]
    ItemGone,

    [EnumMember(Value = "cant_enter")]
    CantEnter,

    [EnumMember(Value = "monsterhunt_already")]
    MonsterHuntAlready,

    [EnumMember(Value = "magiport_gone")]
    MagiportGone,

    [EnumMember(Value = "inviter_gone")]
    InviterGone,

    //the code, distinct from the GameResponseData.InProgress flag - blessing is already running
    [EnumMember(Value = "in_progress")]
    InProgressResponse
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
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

    [EnumMember(Value = "skill_timeout")]
    SkillTimeout,

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

    [EnumMember(Value = "disconnect_reason")]
    DisconnectReason,

    [EnumMember(Value = "limitdcreport")]
    LimitDcReport,

    //Phase 10 - inbound event coverage (tier 1). Members whose wire name equals the lowercased C# name
    //carry no [EnumMember]; only underscore wire names (kill_credit, trade_history, game_event) do.
    Cm,
    Magiport,
    Request,
    Track,
    Tracker,
    LostAndFound,

    [EnumMember(Value = "kill_credit")]
    KillCredit,

    [EnumMember(Value = "trade_history")]
    TradeHistory,

    [EnumMember(Value = "game_event")]
    GameEvent
}

[StjJson.JsonConverter(typeof(StjConverters.TolerantStringEnumConverterFactory))]
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
    Dismantle,
    Emotion,
    Equip,
    Exchange,
    Heal,

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
    Merchant,

    //Phase 9 - emit surface coverage (tier 1). Members whose wire name is just the lowercased C# name
    //carry no [EnumMember]; only underscore names do (EmitAsync lowercases via EnumHelper.ToString).
    //New members go here and not in the alphabetical run above: no member carries an explicit value, so
    //inserting up there renumbers every later ordinal, and enum-tolerance-matrix.json pins what raw
    //ordinal 17 resolves to.
    Bet,
    Cruise,
    Destroy,
    Donate,
    Enter,

    [EnumMember(Value = "equip_batch")]
    EquipBatch,

    [EnumMember(Value = "exchange_buy")]
    ExchangeBuy,
    Friend,
    Interaction,
    Join,

    [EnumMember(Value = "join_giveaway")]
    JoinGiveaway,
    LostAndFound,
    Mail,
    Pet,
    Pets,
    Say,

    [EnumMember(Value = "set_home")]
    SetHome,
    Split,

    [EnumMember(Value = "trade_history")]
    TradeHistory,

    [EnumMember(Value = "trade_sell")]
    TradeSell,
    Whistle
}