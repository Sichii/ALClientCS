using System.Collections.Generic;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    public record MerchantList
    {
        [JsonProperty("chars")]
        public IEnumerable<Merchant> Merchants { get; init; } = new List<Merchant>();
    }
}