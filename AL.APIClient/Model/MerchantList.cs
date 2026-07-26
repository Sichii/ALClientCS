#region
using System.Collections.Generic;
using System.Text.Json.Serialization;
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
    [JsonPropertyName("chars")]
    public IReadOnlyList<MerchantInfo> Merchants { get; init; } = new List<MerchantInfo>();
}