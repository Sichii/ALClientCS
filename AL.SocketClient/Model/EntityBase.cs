using System;
using AL.Core.Abstractions;
using AL.Core.Interfaces;
using Chaos.Core.Extensions;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public abstract record EntityBase : AttributedRecordBase, ILocation
    {
        [JsonProperty]
        public bool ABS { get; init; }

        [JsonProperty]
        public float Angle { get; init; }

        [JsonProperty]
        public float CID { get; init; }

        [JsonProperty("going_x")]
        public float GoingX { get; init; }

        [JsonProperty("going_y")]
        public float GoingY { get; init; }

        [JsonProperty]
        public string Id { get; init; }

        [JsonProperty]
        public float Level { get; init; }

        [JsonProperty("map")]
        public string Map { get; init; }

        [JsonProperty("max_hp")]
        public float MaxHP { get; init; }

        [JsonProperty("move_num")]
        public float MoveNum { get; init; }

        [JsonProperty]
        public bool Moving { get; init; }

        [JsonProperty]
        public string Target { get; init; }

        [JsonProperty]
        public float X { get; init; }

        [JsonProperty]
        public float Y { get; init; }

        public virtual bool Equals(EntityBase other) => Id.Equals(other?.Id);

        public override int GetHashCode() => Id.GetHashCode();
    }
}