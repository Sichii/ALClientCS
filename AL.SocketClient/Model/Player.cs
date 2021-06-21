using System;
using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.Core.Json.Converters;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    [JsonConverter(typeof(AttributedObjectConverter<Player>))]
    public record Player : EntityBase, IUpdateable<Player>
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

        public void Update(Player other)
        {
            if (CID != other.CID || Id != other.Id)
                throw new InvalidOperationException(
                    $"Attempting to update entity with ID: {Id} CID: {CID} with data for entity with ID: {other.Id}, CID: {other.CID}");

            ABS = other.ABS;
            Angle = other.Angle;
            Armor = other.Armor;
            Channeling = other.Channeling;
            Cosmetics = other.Cosmetics;
            GoingX = other.GoingX;
            GoingY = other.GoingY;
            HP = other.HP;
            MaxHP = other.MaxHP;
            Level = other.Level;
            StepCount = other.StepCount;
            Moving = other.Moving;
            QueuedActions = other.QueuedActions;
            Conditions = other.Conditions;
            Skin = other.Skin;
            Speed = other.Speed;
            X = other.X;
            Y = other.Y;
            XP = other.XP;

            if (!IsNPC)
            {
                AFK = other.AFK;
                Age = other.Age;
                Attack = other.Attack;
                Code = other.Code;
                Frequency = other.Frequency;
                MP = other.MP;
                MaxMP = other.MaxMP;
                PDPS = other.PDPS;
                Range = other.Range;
                Resistance = other.Resistance;
                RIP = other.RIP;
                Slots = other.Slots;
                Stand = other.Stand;
            }
        }
    }
}