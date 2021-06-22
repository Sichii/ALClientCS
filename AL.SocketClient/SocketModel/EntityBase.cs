using AL.Core.Abstractions;
using AL.Core.Interfaces;
using Chaos.Core.Collections.Synchronized.Awaitable;
using Newtonsoft.Json;

namespace AL.SocketClient.SocketModel
{
    public abstract record EntityBase : AttributedRecordBase, ILocation
    {
        //TODO: what's this?
        [JsonProperty]
        public bool ABS { get; protected set; }

        [JsonProperty]
        public float Angle { get; protected set; }

        [JsonProperty]
        public int CID { get; init; }

        [JsonProperty("s")]
        public AwaitableDictionary<Core.Definitions.Condition, Condition> Conditions { get; protected set; } = new();

        [JsonProperty("going_x")]
        public float GoingX { get; protected set; }

        [JsonProperty("going_y")]
        public float GoingY { get; protected set; }

        [JsonProperty]
        public string Id { get; init; }

        [JsonProperty]
        public int Level { get; protected set; }

        [JsonProperty("map")]
        public string Map { get; init; }

        [JsonProperty("max_hp")]
        public int MaxHP { get; protected set; }

        [JsonProperty]
        public bool Moving { get; protected set; }

        [JsonProperty("move_num")]
        public ulong StepCount { get; protected set; }

        [JsonProperty]
        public string Target { get; init; }

        [JsonProperty]
        public float X { get; protected set; }

        [JsonProperty]
        public float Y { get; protected set; }

        public virtual bool Equals(EntityBase other) => Id.Equals(other?.Id);

        public override int GetHashCode() => Id.GetHashCode();
    }
}