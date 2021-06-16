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
        public IReadOnlyDictionary<BankPack, Item> Items { get; init; }
    }
}