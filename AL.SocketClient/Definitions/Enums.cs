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

    [EnumMember(Value = "transport_cant_item")]
    TransportCantItem,

    [EnumMember(Value = "transport_cant_invalid")]
    TransportCantInvalid,

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
    InProgressResponse,

    //the lost-and-found gold reserve read (node/server.js:7334); needs no prior donation and no distance check
    [EnumMember(Value = "lostandfound_info")]
    LostAndFoundInfo,

    //the lost-and-found listing's refusal for an account that has not donated on this connection
    //(node/server.js:6891). The uncorrelated branch is built by hand and carries neither place nor failed; the
    //correlated one goes through fail_response and carries both, plus reason "donation_required"
    [EnumMember(Value = "lostandfound_donate")]
    LostAndFoundDonate,

    //hopsickness refusing a lost-and-found buy (node/server.js:7134). Ponty is served the same handler and is
    //deliberately not refused, so this arrives only from the lost-and-found branch
    [EnumMember(Value = "cant_when_sick")]
    CantWhenSick,

    //Endpoint coverage. Five of the locksmith's nine arrive as a BARE STRING rather than an object
    //(node/server.js:6309, :6314, :6325, :6337, :6344), the shape Distance above already warns about, so nothing
    //here may require Place or Failed to be present
    [EnumMember(Value = "locksmith_cant")]
    LocksmithCant,

    [EnumMember(Value = "locksmith_locked")]
    LocksmithLocked,

    [EnumMember(Value = "locksmith_sealed")]
    LocksmithSealed,

    [EnumMember(Value = "locksmith_unlocked")]
    LocksmithUnlocked,

    [EnumMember(Value = "locksmith_unsealed")]
    LocksmithUnsealed,

    [EnumMember(Value = "locksmith_unseal_complete")]
    LocksmithUnsealComplete,

    //sent through success_response with Success FALSE and InProgress true (node/server.js:6317), so it is a
    //"nothing happened yet" rather than a completion - Hours is how long is left
    [EnumMember(Value = "locksmith_unsealing")]
    LocksmithUnsealing,

    //fail_response with reason "already_unlocked" (node/server.js:6300)
    [EnumMember(Value = "locksmith_aunlocked")]
    LocksmithAlreadyUnlocked,

    //fail_response with reason "already_locked" (node/server.js:6330)
    [EnumMember(Value = "locksmith_alocked")]
    LocksmithAlreadyLocked,

    [EnumMember(Value = "scrollsmith_cant")]
    ScrollsmithCant,

    //carries Gold: the amount actually spent, and the only place the destat cost is reported (node/server.js:6274)
    [EnumMember(Value = "scrollsmith_success")]
    ScrollsmithSuccess,

    [EnumMember(Value = "cx_not_found")]
    CosmeticNotFound,

    //each of these three carries a whole replacement acx dictionary rather than a delta
    //(node/server.js:7243, :7981, :7988)
    [EnumMember(Value = "cx_new")]
    CosmeticNew,

    [EnumMember(Value = "cx_sent")]
    CosmeticSent,

    [EnumMember(Value = "cx_received")]
    CosmeticReceived,

    [EnumMember(Value = "send_no_cx")]
    SendNoCosmetic,

    [EnumMember(Value = "tavern_not_yet")]
    TavernNotYet,

    [EnumMember(Value = "tavern_too_late")]
    TavernTooLate,

    [EnumMember(Value = "tavern_dice_exist")]
    TavernDiceExist,

    [EnumMember(Value = "tavern_gold_not_enough")]
    TavernGoldNotEnough,

    [EnumMember(Value = "bet_xshot")]
    BetXShot,

    [EnumMember(Value = "gold_use")]
    GoldUse,

    [EnumMember(Value = "slots_success")]
    SlotsSuccess,

    [EnumMember(Value = "slots_fail")]
    SlotsFail,

    [EnumMember(Value = "door_unlocked")]
    DoorUnlocked,

    [EnumMember(Value = "bank_pack_unlocked")]
    BankPackUnlocked,

    [EnumMember(Value = "only_in_bank")]
    OnlyInBank,

    //activate's own already-unlocked refusal, distinct from locksmith_aunlocked's reason string of the same
    //text (node/server.js:8871, :8884)
    [EnumMember(Value = "already_unlocked")]
    AlreadyUnlocked,

    //activate's refusal when the class or level requirement for a cosmetic toggle is not met
    //(node/server.js:8820)
    [EnumMember(Value = "nothing")]
    Nothing,

    //the only answer the signup emit gets (node/server.js:11335). No method consumes it; the member exists so the
    //frame resolves to something other than Unknown
    [EnumMember(Value = "signed_up")]
    SignedUp
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
    GameEvent,

    //private chat. Appended rather than filed beside ChatLog because this enum has its own row in
    //enum-tolerance-matrix.json pinning what raw ordinal 17 resolves to, and no member here carries an explicit
    //value - so inserting above renumbers every later ordinal and fails that snapshot
    Pm,

    //the tavern's info reply and its bet/won/lost broadcasts all share this one event name, so the frame's own
    //"event" field is what tells them apart. Appended for the same ordinal reason as Pm
    Tavern,

    //one transition of the tavern's dice round. Appended for the same ordinal reason as Pm
    Dice
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
    Convert,
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

    //Phase 9 - emit surface coverage (tier 1). Members whose wire name is just the lowercased C# name carry no
    //[EnumMember]; only underscore names do. New members go here and not in the alphabetical run above: no member
    //carries an explicit value, so inserting up there renumbers every later ordinal
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
    Throw,

    [EnumMember(Value = "trade_history")]
    TradeHistory,

    [EnumMember(Value = "trade_sell")]
    TradeSell,
    Whistle,

    //Endpoint coverage - the locksmith, scrollsmith, activate, cosmetics and tavern surfaces. Appended for the
    //reason the block above is: no member here carries an explicit value, so inserting into the alphabetical run
    //renumbers every later ordinal and rebinds enum-tolerance-matrix.json's pinned rows. Each of these eight wire
    //names is already the lowercased C# name, so none needs an [EnumMember]
    Activate,
    Blend,
    Cx,
    Destat,
    Harakiri,
    Locksmith,
    Signup,
    Tavern
}