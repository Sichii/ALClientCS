using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.SocketClient.Interfaces;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    /// <summary>
    ///     Represents a player. (ymyself or others)
    /// </summary>
    /// <seealso cref="EntityBase" />
    /// <seealso cref="ISimplePlayer" />
    /// <seealso cref="IEquatable{T}" />
    [JsonConverter(typeof(AttributedObjectConverter<Player>))]
    public class Player : EntityBase, ISimplePlayer, IEquatable<Player>
    {
        [JsonProperty, JsonConverter(typeof(AfkConverter))]
        public bool AFK { get; protected set; }

        [JsonProperty]
        public int Age { get; protected set; }

        /// <summary>
        ///     <b>NULLABLE.</b> If populated, you are channeling. <br />
        ///     Some channeling abilities will be canceled if you move. Check the skill's <c>CanMove</c> property.
        /// </summary>
        [JsonProperty("c")]
        public IReadOnlyDictionary<string, ChannelingInfo>? Channeling { get; protected set; }

        /// <summary>
        ///     The class of the player.
        /// </summary>
        [JsonProperty("ctype")]
        public ALClass Class { get; protected set; }

        /// <summary>
        ///     Whether or not the player is running code.
        /// </summary>
        [JsonProperty]
        public bool Code { get; protected set; }

        /// <summary>
        ///     If populated, this character is using the steam client's frame-in-frame multi logging. <br />
        ///     This is the name of the character who's iframe is hosting this character.
        /// </summary>
        [JsonProperty]
        public string? Controller { get; protected set; }

        /// <summary>
        ///     Appearanc information about the character.
        /// </summary>
        [JsonProperty("cx")]
        public CosmeticInfo Cosmetics { get; protected set; } = null!;

        /// <summary>
        ///     If populated, this player is targeting another player. <br />
        ///     This is the player's name they are targeting.
        /// </summary>
        [JsonProperty]
        public string? Focus { get; protected set; }

        [JsonProperty("max_mp")]
        public float MaxMP { get; protected set; }

        /// <summary>
        ///     If populated, this player is actually an NPC. <br />
        ///     You can also check <see cref="IsNPC" />.
        /// </summary>
        [JsonProperty("npc")]
        public string? NPCName { get; protected set; }

        /// <summary>
        ///     If populated, this player is an actual person. <br />
        ///     This is the ID of the account this player belongs to.
        ///     This
        /// </summary>
        [JsonProperty]
        public string? Owner { get; protected set; }

        public string? PartyLeader { get; protected set; }

        /// <summary>
        ///     This is a value indicating the overall contribution a player is making towards his party. (higher is better)
        /// </summary>
        [JsonProperty]
        public float PDPS { get; protected set; }

        /// <summary>
        ///     <b>NULLABLE.</b> If populated, this player is performing a queued action. <br />
        ///     Queued actions are actions that take some time to complete. This object lets you keep track of their progress.
        /// </summary>
        [JsonProperty("q")]
        public QueuedActionInfo? QueuedActions { get; protected set; }

        /// <summary>
        ///     Whether or not you are dead.
        /// </summary>
        [JsonProperty]
        public bool RIP { get; protected set; }

        /// <summary>
        ///     If populated, this is the skin applied to the character. (appearance stuff)
        /// </summary>
        [JsonProperty]
        public string? Skin { get; protected set; }

        /// <summary>
        ///     A collection of equipment the player is wearing, and items they are selling/buying. <br />
        ///     All slots should have keys in the dictionary, but the items may be null.
        /// </summary>
        /// <remarks>Enriched by converter</remarks>
        [JsonProperty]
        public IReadOnlyDictionary<Slot, SlotItem?> Slots { get; protected set; } = new Dictionary<Slot, SlotItem?>();

        /// <summary>
        ///     The type of stand this player is using.
        /// </summary>
        [JsonProperty]
        public Stand Stand { get; protected set; }

        /// <summary>
        ///     Whether or not this player is currently teleporting.
        /// </summary>
        [JsonProperty("tp")]
        public bool Teleporting { get; protected set; }

        /// <summary>
        ///     Checks if this player is actually an NPC.
        /// </summary>
        [JsonIgnore]
        public bool IsNPC => (NPCName != null) || (Class == ALClass.NPC);

        /// <summary>
        ///     The name of the player.
        /// </summary>
        [JsonIgnore]
        public string Name => Id;

        public virtual bool Equals(Player? other) =>
            other is not null && base.Equals(other);

        public override bool Equals(object? obj) => Equals(obj as Player);

        public override int GetHashCode() => base.GetHashCode();

        public void Update(QueuedActionInfo queuedActionInfo) => QueuedActions = queuedActionInfo;

        public void Update(Player other)
        {
            if (Id != other.Id)
                throw new InvalidOperationException($"Attempting to update player with ID: {Id}, with data for entity with ID: {other.Id}");

            Channeling = other.Channeling;
            Cosmetics = other.Cosmetics;
            QueuedActions = other.QueuedActions;
            Skin = other.Skin;

            if (!IsNPC)
            {
                Range = other.Range;
                AFK = other.AFK;
                Age = other.Age;
                Code = other.Code;
                PDPS = other.PDPS;
                RIP = other.RIP;
                Slots = other.Slots;
                Stand = other.Stand;
            }

            base.Update(other);
        }
    }
}