using System.Collections.Generic;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.SocketClient.Json.Converters;
using AL.SocketClient.Receive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AL.SocketClient.Model
{
    [JsonConverter(typeof(AttributedObjectConverter<Character>))]
    public record Character : Player
    {
        [JsonProperty("acx")]
        //something to do with achievements?
        public JObject ACX { get; init; }

        [JsonProperty("targets")]
        public int AggroTargets { get; init; }

        [JsonProperty("user"), JsonConverter(typeof(BankDataConverter))]
        public BankInfo Bank { get; init; }

        [JsonProperty]
        public int Cache { get; init; }

        [JsonProperty]
        public new string Code { get; init; }

        [JsonProperty("cc")]
        public float CodeCost { get; init; }

        [JsonProperty("emx")]
        public IReadOnlyDictionary<Emotion, float> Emotion { get; init; }

        [JsonProperty("esize")]
        public int EmptySlots { get; init; }

        [JsonProperty("hitchhikers")]
        public IReadOnlyDictionary<string, JObject> ExtraEvents { get; init; }

        [JsonProperty("xrange")]
        public float ExtraRange { get; init; }
        [JsonProperty]
        public float Fear { get; init; }

        //TODO: what's this?
        [JsonProperty]
        public dynamic Friends { get; init; }

        [JsonProperty("isize")]
        public int InventorySize { get; init; }

        // ReSharper disable once InconsistentNaming
        [JsonProperty]
        public string IPass { get; init; }

        [JsonProperty]
        public Item[] Items { get; init; }

        [JsonProperty]
        public float M { get; init; }

        [JsonProperty]
        public float Tax { get; init; }

        [JsonProperty]
        // ReSharper disable once InconsistentNaming
        public string[] XCX { get; init; }
        
        public virtual bool Equals(Character other) => base.Equals(other);
        public override int GetHashCode() => base.GetHashCode();
    }
}