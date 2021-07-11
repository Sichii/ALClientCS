using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Interfaces;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.SocketModel;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    [JsonConverter(typeof(CharacterConverter<Character>))]
    public record Character : Player, IMutable<ILocation>, IMutable<NewMapData>
    {
        [JsonProperty("targets")]
        public int AggroTargets { get; init; }

        [JsonProperty("user"), JsonConverter(typeof(BankDataConverter))]
        public BankInfo Bank { get; init; }

        //TODO: what's this?
        [JsonProperty]
        public int Cache { get; init; }

        [JsonProperty]
        public new string Code { get; init; }

        [JsonProperty("cc")]
        public float CodeCost { get; init; }

        [JsonProperty("emx")]
        public IReadOnlyDictionary<Emotion, float> Emotion { get; init; } = new Dictionary<Emotion, float>();

        [JsonProperty("esize")]
        public int EmptySlots { get; init; }

        [JsonProperty("xrange")]
        public float ExtraRange { get; init; }
        [JsonProperty]
        public float Fear { get; init; }

        //TODO: what's this?
        [JsonProperty]
        public List<string> Friends { get; init; }

        [JsonProperty("items")]
        public IReadOnlyList<Item> Inventory { get; internal set; }

        [JsonProperty("isize")]
        public int InventorySize { get; init; }

        // ReSharper disable once InconsistentNaming
        //TODO: what's this?
        [JsonProperty]
        public string IPass { get; init; }

        //TODO: what's this?
        [JsonProperty("m")]
        public int MapChangeCount { get; protected set; }
        [JsonProperty("acx")]
        public IReadOnlyDictionary<string, int> OwnedCosmetics { get; init; }

        [JsonProperty]
        public float Tax { get; init; }

        [JsonProperty]
        // ReSharper disable once InconsistentNaming
        public string[] XCX { get; init; }

        public virtual bool Equals(Character other) => base.Equals(other);

        public override int GetHashCode() => base.GetHashCode();

        public void Mutate(NewMapData mutator)
        {
            X = mutator.X;
            Y = mutator.Y;
            Map = mutator.Map;
            MapChangeCount = mutator.MapChangeCount;
        }

        public void Mutate(ILocation mutator)
        {
            X = mutator.X;
            Y = mutator.Y;
            Map = mutator.Map;
        }
    }
}