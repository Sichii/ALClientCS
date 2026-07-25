#region
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.Core.Json.Interfaces;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data recieved when the game server responds.
/// </summary>
/// <seealso cref="IOptionalObject" />
[JsonConverter(typeof(StringOrObjectConverter<GameResponseData>), nameof(ResponseType))]
public sealed record GameResponseData : IOptionalObject
{
    /// <summary>
    ///     A client-event tag. The server sends either <c>true</c> or a name, so this is a string either way.
    /// </summary>
    [JsonProperty("cevent")]
    public string? CEvent { get; init; }

    /// <summary>
    ///     The chance of the item to be upgraded/compounded successfully.
    /// </summary>
    public float Chance { get; init; }

    /// <summary>
    ///     Whether the monster hunt was turned in.
    /// </summary>
    [JsonProperty("completed")]
    public bool Completed { get; init; }

    [JsonIgnore]
    public bool ContainsData { get; set; }

    /// <summary>
    ///     If populated, contains the cooldown of the skill used.
    /// </summary>
    [JsonProperty("ms")]
    public float? CooldownMS { get; init; }

    /// <summary>
    ///     The cost of the item bought.
    /// </summary>
    public int Cost { get; init; }

    /// <summary>
    ///     The distance to the entity you are too far away from.
    /// </summary>
    [JsonProperty("dist")]
    public float Distance { get; init; }

    /// <summary>
    ///     The duration of the condition being applied, in milliseconds.
    /// </summary>
    [JsonProperty("duration")]
    public float Duration { get; init; }

    /// <summary>
    ///     Whether the operation failed. Set by every <c>fail_response</c>, so it is the one universal failure
    ///     discriminator; the failing operation is named by <see cref="Place" />.
    /// </summary>
    [JsonProperty("failed")]
    public bool Failed { get; init; }

    /// <summary>
    ///     The amount of gold sent or received. Fractional for alchemy, which scales gold by a rate.
    /// </summary>
    [JsonProperty("gold")]
    public float Gold { get; init; }

    /// <summary>
    ///     The grace of the item to be upgrade/compounded successfully.
    /// </summary>
    public float Grace { get; init; }

    /// <summary>
    ///     The ids of the entities affected by an area skill.
    /// </summary>
    [JsonProperty("ids")]
    public string[]? Ids { get; init; }

    /// <summary>
    ///     Whether the operation has started and will be finished by a later frame.
    /// </summary>
    [JsonProperty("in_progress")]
    public bool InProgress { get; init; }

    /// <summary>
    ///     The item you calculated chance for.
    /// </summary>
    [JsonConverter(typeof(StringOrObjectConverter<ResponseItem>), nameof(ResponseItem.Name))]
    public ResponseItem? Item { get; init; } = null!;

    /// <summary>
    ///     The level of the item that was upgraded, compounded, or dismantled.
    /// </summary>
    [JsonProperty("level")]
    public int Level { get; init; }

    /// <summary>
    ///     The name of the monster that defeated the player.
    /// </summary>
    [JsonProperty("monster")]
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
    [JsonProperty("name")]
    public string? Name { get; init; }

    /// <summary>
    ///     The projectile ids of a multi-target skill, one per target, in the same order as <see cref="Targets" />.
    /// </summary>
    [JsonProperty("pids")]
    public string[]? Pids { get; init; }

    /// <summary>
    ///     Extra information about the response. Often the name of a skill or action.
    /// </summary>
    public string? Place { get; init; }

    /// <summary>
    ///     The quantity of the item bought or sent.
    /// </summary>
    [JsonProperty("q")]
    public int Quantity { get; init; } = 1;

    /// <summary>
    ///     The reason you are unable to enter the bank.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    ///     The type of the response.
    /// </summary>
    [JsonProperty("response")]
    public GameResponseType ResponseType { get; init; }

    /// <summary>
    ///     The name of the skill the cooldown is for.
    /// </summary>
    [JsonProperty("skill")]
    public string? SkillName { get; init; }

    /// <summary>
    ///     The slot the bought item went into.
    /// </summary>
    [JsonProperty("num")]
    public int SlotNum { get; init; }

    /// <summary>
    ///     Whether this frame is a replay of an already-settled operation. A stale frame must not complete an await.
    /// </summary>
    [JsonProperty("stale")]
    public bool Stale { get; init; }

    /// <summary>
    ///     The attribute a stat scroll granted.
    /// </summary>
    [JsonProperty("stat_type")]
    public ALAttribute StatType { get; init; }

    /// <summary>
    ///     Whether the operation succeeded. Not set by the skills that answer with a collapsed action frame -
    ///     see <see cref="Place" />.
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; init; }

    /// <summary>
    ///     TODO: something to do with seashells
    /// </summary>
    public string? Suffix { get; init; }

    /// <summary>
    ///     The ID of the target the skill was used on
    ///     <br />
    ///     The ID of the player the magiport offer was sent to
    ///     <br />
    ///     The ID of the target you tried to attack, but are too far away from
    /// </summary>
    [JsonProperty("id")]
    public string? TargetId { get; init; }

    /// <summary>
    ///     The ids of the entities a multi-target skill actually hit.
    /// </summary>
    [JsonProperty("targets")]
    public string[]? Targets { get; init; }

    /// <summary>
    ///     The amount of XP lost from being defeated by a monster.
    /// </summary>
    [JsonProperty("xp")]
    public int XPLost { get; init; }
}