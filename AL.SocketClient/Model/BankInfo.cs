using System.Collections.Generic;
using AL.Core.Definitions;
using Newtonsoft.Json;

namespace AL.SocketClient.Model
{
    public record BankInfo
    {
        [JsonIgnore]
        public long Gold { get; init; }

        [JsonIgnore]
        public IReadOnlyDictionary<BankPack, IReadOnlyList<Item>> Items { get; init; } =
            new Dictionary<BankPack, IReadOnlyList<Item>>();
    }
}