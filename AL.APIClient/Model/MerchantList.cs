#region
using System.Collections.Generic;
using Newtonsoft.Json;
#endregion

namespace AL.APIClient.Model;

/// <summary>
///     Represents all of the merchants with a stand open in any server.
/// </summary>
public sealed record MerchantList
{
    /// <summary>
    ///     A list of merchants with a stand open in any server.
    /// </summary>
    [JsonProperty("chars")]
    public IReadOnlyList<MerchantInfo> Merchants { get; init; } = new List<MerchantInfo>();
}