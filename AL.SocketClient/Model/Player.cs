using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    [JsonConverter(typeof(AttributedObjectConverter<Player>))]
    public record Player : EntityBase
    {
        [JsonProperty, JsonConverter(typeof(AfkConverter))]
        public bool AFK { get; init; }

        [JsonProperty]
        public float Age { get; init; }

        [JsonProperty]
        public bool Allow { get; init; }

        [JsonProperty("c")]
        public IReadOnlyDictionary<string, ChannelingInfo> Channeling { get; init; }

        [JsonProperty("ctype")]
        public ALClass Class { get; init; }

        [JsonProperty]
        public bool Code { get; init; }

        [JsonProperty("s")]
        public IReadOnlyDictionary<Core.Definitions.Condition, Condition> Conditions { get; init; }

        [JsonProperty]
        public string Controller { get; init; }

        [JsonProperty("cx")]
        public CosmeticInfo Cosmetics { get; init; }

        [JsonProperty]
        public string Focus { get; init; }

        [JsonProperty("max_mp")]
        public float MaxMP { get; init; }

        [JsonProperty("npc")]
        public string NPCName { get; init; }

        [JsonProperty]
        public string Owner { get; init; }

        [JsonProperty("party")]
        public string PartyLeader { get; init; }

        [JsonProperty]
        public float PDPS { get; init; }

        [JsonProperty("q")]
        public QueuedActionInfo QueuedActions { get; init; }

        [JsonProperty]
        public bool RIP { get; init; }

        [JsonProperty]
        public string Skin { get; init; }

        [JsonProperty]
        public IReadOnlyDictionary<Slot, SlotItem> Slots { get; init; }

        [JsonProperty]
        public Stand Stand { get; init; }

        [JsonProperty("tp")]
        public bool Teleporting { get; init; }

        [JsonIgnore]
        public bool IsNPC => NPCName != null || Class == ALClass.NPC;

        [JsonIgnore]
        public string Name => Id;

        public virtual bool Equals(Player other) => base.Equals(other);

        public override int GetHashCode() => base.GetHashCode();
    }
}