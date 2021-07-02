using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    [JsonConverter(typeof(AttributedObjectConverter<Player>))]
    public record Player : EntityBase, IMutable<Player>
    {
        [JsonProperty, JsonConverter(typeof(AfkConverter))]
        public bool AFK { get; protected set; }

        [JsonProperty]
        public float Age { get; protected set; }

        [JsonProperty]
        public bool Allow { get; init; }

        [JsonProperty("c")]
        public IReadOnlyDictionary<string, ChannelingInfo> Channeling { get; protected set; } =
            new Dictionary<string, ChannelingInfo>();

        [JsonProperty("ctype")]
        public ALClass Class { get; init; }

        [JsonProperty]
        public bool Code { get; protected set; }

        [JsonProperty]
        public string Controller { get; init; }

        [JsonProperty("cx")]
        public CosmeticInfo Cosmetics { get; protected set; }

        [JsonProperty]
        public string Focus { get; init; }

        [JsonProperty("max_mp")]
        public float MaxMP { get; protected set; }

        [JsonProperty("npc")]
        public string NPCName { get; init; }

        [JsonProperty]
        public string Owner { get; init; }

        [JsonProperty("party")]
        public string PartyLeader { get; init; }

        [JsonProperty]
        public float PDPS { get; protected set; }

        [JsonProperty("q")]
        public QueuedActionInfo QueuedActions { get; protected set; }

        [JsonProperty]
        public bool RIP { get; protected set; }

        [JsonProperty]
        public string Skin { get; protected set; }

        [JsonProperty]
        public IReadOnlyDictionary<Slot, SlotItem> Slots { get; protected set; } = new Dictionary<Slot, SlotItem>();

        [JsonProperty]
        public Stand Stand { get; protected set; }

        [JsonProperty("tp")]
        public bool Teleporting { get; init; }

        [JsonIgnore]
        public bool IsNPC => NPCName != null || Class == ALClass.NPC;

        [JsonIgnore]
        public string Name => Id;

        public virtual bool Equals(Player other) => CID == other?.CID && base.Equals(other);

        public override int GetHashCode() => HashCode.Combine(CID.GetHashCode(), base.GetHashCode());

        public void Mutate(Player other)
        {
            //TODO: CID is always +1 from previous CID?
            if (Id != other.Id)
                throw new InvalidOperationException(
                    $"Attempting to update player with ID: {Id}, with data for entity with ID: {other.Id}");
            
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

            base.Mutate(other);
        }
    }
}