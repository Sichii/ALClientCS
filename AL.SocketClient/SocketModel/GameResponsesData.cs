using AL.Core.Json.Converters;
using AL.Core.Json.Interfaces;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    /// <summary>
    ///     Represents the data recieved when the game server responds.
    /// </summary>
    /// <seealso cref="IOptionalObject" />
    [JsonConverter(typeof(StringOrObjectConverter<GameResponseData>), nameof(ResponseType))]
    public record GameResponseData : IOptionalObject
    {
        /// <summary>
        ///     The chance of the item to be upgraded/compounded successfully.
        /// </summary>
        public float Chance { get; init; }

        [JsonIgnore]
        public bool ContainsData { get; init; }

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
        public int Distance { get; init; }

        /// <summary>
        ///     The amount of gold sent or received.
        /// </summary>
        [JsonProperty("gold")]
        public int Gold { get; init; }

        /// <summary>
        ///     The grace of the item to be upgrade/compounded successfully.
        /// </summary>
        public float Grace { get; init; }

        /// <summary>
        ///     The item you calculated chance for.
        /// </summary>
        [JsonConverter(typeof(StringOrObjectConverter<ResponseItem>), nameof(ResponseItem.Name))]
        public ResponseItem? Item { get; init; } = null!;

        /// <summary>
        ///     The name of the monster that defeated the player.
        /// </summary>
        [JsonProperty("monster")]
        public string? MonsterName { get; set; }
        /// <summary>
        ///     The name of the character already in the bank. <br />
        ///     The name of the item bought, crafted, or sent <br />
        ///     The name of the condition expiring <br />
        ///     The name of the person you sent gold or items to <br />
        ///     The name of the skill that succeeded or failed <br />
        ///     The name of person you received gold or items from
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; init; }

        /// <summary>
        ///     TODO: no rhyme or reson, it's just extra info?
        /// </summary>
        public string? Place { get; init; }

        /// <summary>
        ///     The quantity of the item bought or sent.
        /// </summary>
        [JsonProperty("q")]
        public int Quantity { get; init; }

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
        ///     TODO: something to do with seashells
        /// </summary>
        public string? Suffix { get; init; }

        /// <summary>
        ///     The ID of the target the skill was used on <br />
        ///     The ID of the player the magiport offer was sent to <br />
        ///     The ID of the target you tried to attack, but are too far away from
        /// </summary>
        [JsonProperty("id")]
        public string? TargetID { get; init; }

        /// <summary>
        ///     The amount of XP lost from being defeated by a monster.
        /// </summary>
        [JsonProperty("xp")]
        public int XPLost { get; init; }
    }
}