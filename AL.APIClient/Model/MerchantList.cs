using System.Collections.Generic;
using Newtonsoft.Json;

namespace AL.APIClient.Model
{
    /// <summary>
    ///     Represents all of the merchants with a stand open in any server.
    /// </summary>
    public record MerchantList
    {
        /// <summary>
        ///     A list of merchants with a stand open in any server.
        /// </summary>
        [JsonProperty("chars")]
        public IReadOnlyList<MerchantInfo> Merchants { get; init; } = new List<MerchantInfo>();
    }
}