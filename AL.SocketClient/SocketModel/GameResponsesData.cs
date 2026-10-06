#region
using System.Text.Json;
using System.Text.Json.Serialization;
using AL.APIClient.Model;
using AL.Core.Definitions;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
using AL.SocketClient.Definitions;
using AL.SocketClient.Json.SystemTextJson;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data recieved when the game server responds.
/// </summary>
/// <seealso cref="IOptionalObject" />
[JsonStringOrObject(nameof(ResponseType))]
public sealed record GameResponseData : IOptionalObject
{
    /// <summary>
    ///     The account's full cosmetics unlock dictionary, sent by <c>cx_new</c>, <c>cx_sent</c> and <c>cx_received</c> as a
    ///     replacement rather than a delta; a send deletes the key at zero, so merging would restore a traded-away cosmetic.
    /// </summary>
    [JsonPropertyName("acx")]
    public Dictionary<string, int>? Acx { get; init; }

    /// <summary>
    ///     The bank slot a bank item operation moved an item out of or into, echoed back from the emit. Null on the gold
    ///     operations, which name no slot.
    /// </summary>
    [JsonPropertyName("str")]
    public int? BankSlot { get; init; }

    /// <summary>
    ///     A client-event tag. The server sends either <c>true</c> or a name, so this is a string either way.
    /// </summary>
    [JsonPropertyName("cevent")]
    public string? CEvent { get; init; }

    /// <summary>
    ///     The chance of the item to be upgraded/compounded successfully.
    /// </summary>
    public float Chance { get; init; }

    /// <summary>A traveler's reply to a cave talk request.</summary>
    [JsonPropertyName("chat")]
    public CaveChat? Chat { get; init; }

    /// <summary>Whether the monster hunt was turned in.</summary>
    [JsonPropertyName("completed")]
    public bool Completed { get; init; }

    [JsonIgnore]
    public bool ContainsData { get; set; }

    /// <summary>
    ///     If populated, contains the cooldown of the skill used.
    /// </summary>
    [JsonPropertyName("ms")]
    public float? CooldownMS { get; init; }

    /// <summary>
    ///     The cost of the item bought, or on a correlated slots settlement the machine's fixed price for that pull.
    /// </summary>
    public int Cost { get; init; }

    /// <summary>
    ///     The distance to the entity you are too far away from.
    /// </summary>
    [JsonPropertyName("dist")]
    public float Distance { get; init; }

    /// <summary>
    ///     The duration of the condition being applied, in milliseconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public float Duration { get; init; }

    /// <summary>
    ///     On a correlated tavern info reply, the percentage of a win's profit the house keeps (<see cref="TavernData.Edge" />
    ///     ). On a correlated dice settlement, the gold the house took out of that bet, already subtracted from the payout.
    ///     Zero on every other frame.
    /// </summary>
    [JsonPropertyName("edge")]
    public float Edge { get; init; }

    /// <summary>
    ///     What an <c>equip_batch</c> did with each of the equips it was given, in the order the emit listed them. Null on
    ///     every other frame.
    /// </summary>
    /// <remarks>
    ///     <c>equip_batch</c> echoes no <see cref="RequestId" />, so this is the only way to tell one batch's answer from
    ///     another's.
    /// </remarks>
    [JsonPropertyName("slots")]
    public EquipBatchEntry[]? EquipBatchEntries { get; init; }

    /// <summary>
    ///     Whether the operation failed. Set by every <c>fail_response</c>; <see cref="Place" /> names the failing operation.
    /// </summary>
    [JsonPropertyName("failed")]
    public bool Failed { get; init; }

    /// <summary>
    ///     The amount of gold sent or received. Fractional for alchemy, which scales gold by a rate.
    /// </summary>
    [JsonPropertyName("gold")]
    public float Gold { get; init; }

    /// <summary>
    ///     The grace of the item to be upgrade/compounded successfully.
    /// </summary>
    public float Grace { get; init; }

    /// <summary>
    ///     The server this character now calls home, sent on <c>home_set</c> as <c>region + server_name</c>. Null on every
    ///     other frame.
    /// </summary>
    [JsonPropertyName("home")]
    public string? Home { get; init; }

    /// <summary>
    ///     The hours a pending operation still has to run, on <c>locksmith_unsealing</c> and on the set-home cooldown's
    ///     <c>sh_time</c> (<see cref="GameResponseType.SetHomeCooldown" />). Zero on every other frame.
    /// </summary>
    [JsonPropertyName("hours")]
    public float Hours { get; init; }

    /// <summary>
    ///     The ids of the entities affected by an area skill.
    /// </summary>
    [JsonPropertyName("ids")]
    public string[]? Ids { get; init; }

    /// <summary>
    ///     Whether the operation has started and will be finished by a later frame.
    /// </summary>
    [JsonPropertyName("in_progress")]
    public bool InProgress { get; init; }

    /// <summary>
    ///     The inventory slot a bank item operation moved an item into, or null when it moved no item. The server picks the
    ///     slot when the emit names none, which is how a withdraw folds into a stack the inventory already holds.
    /// </summary>
    [JsonPropertyName("inv")]
    public int? InventorySlot { get; init; }

    /// <summary>The item you calculated chance for.</summary>
    public ResponseItem? Item { get; init; } = null!;

    /// <summary>
    ///     The listing a correlated secondhands or lostandfound request asked for. Null on every other frame. The server
    ///     reverses it on the way out, so this is newest-first where the bare secondhands event is oldest-first.
    /// </summary>
    [JsonIgnore]
    public TradeItem[]? Items { get; init; }

    /// <summary>
    ///     If populated, the items a door that costs items still needs, by item name, off <c>transport_cant_item</c>.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<string, int>? NeededItems { get; init; }

    /// <summary>
    ///     The <c>items</c> key: an array on a listing reply, filling <see cref="Items" />, and an object of item name to
    ///     count on <c>transport_cant_item</c>, filling <see cref="NeededItems" />.
    /// </summary>
    [JsonPropertyName("items")]
    [JsonInclude]
    private JsonElement ItemsJson
    {
        init
        {
            if (value.ValueKind == JsonValueKind.Array)
                Items = value.Deserialize<TradeItem[]>(SocketJson.Options);
            else if (value.ValueKind == JsonValueKind.Object)
                NeededItems = value.Deserialize<Dictionary<string, int>>(SocketJson.Options);
        }
    }

    /// <summary>
    ///     The level of the item that was upgraded, compounded, or dismantled.
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; init; }

    /// <summary>
    ///     On a correlated tavern info reply, the largest net win the bank will cover on one dice bet. Zero on every other
    ///     frame; see <see cref="TavernData.Max" />, which is the same number off the uncorrelated tavern event.
    /// </summary>
    [JsonPropertyName("max")]
    public long MaxPayout { get; init; }

    /// <summary>
    ///     The name of the monster that defeated the player.
    /// </summary>
    [JsonPropertyName("monster")]
    public string? MonsterName { get; set; }

    /// <summary>
    ///     The name of the character already in the bank.
    ///     <br />
    ///     The name of the item bought, crafted, or sent
    ///     <br />
    ///     The name of the condition expiring
    ///     <br />
    ///     The name of the person you sent gold or items to
    ///     <br />
    ///     The name of the skill that succeeded or failed
    ///     <br />
    ///     The name of person you received gold or items from
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    ///     The change a settled wager made to the character's gold, on a correlated slots settlement: <see cref="Payout" />
    ///     less <see cref="Cost" />, negative on a loss.
    /// </summary>
    /// <remarks>
    ///     The server applies the payout before it sends the settlement, so reading this and watching the balance counts the
    ///     same win twice.
    /// </remarks>
    [JsonPropertyName("net")]
    public long Net { get; init; }

    /// <summary>
    ///     The bank pack key echoed by an unlock success (<c>bank_new_pack</c>), e.g. <c>items3</c>.
    /// </summary>
    [JsonPropertyName("pack")]
    public string? Pack { get; init; }

    /// <summary>
    ///     The gold a settled wager paid, on a correlated slots settlement. Zero on a loss.
    /// </summary>
    [JsonPropertyName("payout")]
    public long Payout { get; init; }

    /// <summary>
    ///     The projectile ids of a multi-target skill, one per target, in the same order as <see cref="Targets" />.
    /// </summary>
    [JsonPropertyName("pids")]
    public string[]? Pids { get; init; }

    /// <summary>
    ///     Extra information about the response. Often the name of a skill or action.
    /// </summary>
    public string? Place { get; init; }

    /// <summary>The quantity of the item bought or sent.</summary>
    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;

    /// <summary>The reason you are unable to enter the bank.</summary>
    public string? Reason { get; init; }

    /// <summary>
    ///     The correlation token this frame's emit supplied, echoed back verbatim. Null on every frame the server produced on
    ///     its own, and on every handler that does not support correlation.
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; init; }

    /// <summary>The type of the response.</summary>
    [JsonPropertyName("response")]
    public GameResponseType ResponseType { get; init; }

    /// <summary>The name of the skill the cooldown is for.</summary>
    [JsonPropertyName("skill")]
    public string? SkillName { get; init; }

    /// <summary>The slot the bought item went into.</summary>
    [JsonPropertyName("num")]
    public int SlotNum { get; init; }

    /// <summary>
    ///     Whether this frame is a replay of an already-settled operation. A stale frame must not complete an await.
    /// </summary>
    [JsonPropertyName("stale")]
    public bool Stale { get; init; }

    /// <summary>The attribute a stat scroll granted.</summary>
    [JsonPropertyName("stat_type")]
    public ALAttribute StatType { get; init; }

    /// <summary>
    ///     Whether the operation succeeded. Not set by the skills that answer with a collapsed action frame; see
    ///     <see cref="Place" />.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>TODO: something to do with seashells</summary>
    public string? Suffix { get; init; }

    /// <summary>
    ///     The ID of the target the skill was used on
    ///     <br />
    ///     The ID of the player the magiport offer was sent to
    ///     <br />
    ///     The ID of the target you tried to attack, but are too far away from
    /// </summary>
    [JsonPropertyName("id")]
    public string? TargetId { get; init; }

    /// <summary>
    ///     The ids of the entities a multi-target skill actually hit.
    /// </summary>
    [JsonPropertyName("targets")]
    public string[]? Targets { get; init; }

    /// <summary>
    ///     The account's standing with the daily dungeon, on the reply to a cave info request.
    /// </summary>
    [JsonPropertyName("visit")]
    public CaveVisit? Visit { get; init; }

    /// <summary>
    ///     Whether a settled wager won, on a correlated slots settlement. <see cref="Success" /> only says the pull was
    ///     accepted.
    /// </summary>
    [JsonPropertyName("won")]
    public bool Won { get; init; }

    /// <summary>
    ///     The amount of XP lost from being defeated by a monster.
    /// </summary>
    [JsonPropertyName("xp")]
    public int XPLost { get; init; }
}