#region
using AL.APIClient.Interfaces;
using Newtonsoft.Json;
#endregion

namespace AL.SocketClient.Model;

/// <inheritdoc cref="ISimpleItem" />
/// <seealso cref="ISimpleItem" />
public sealed record SimpleItem : ISimpleItem
{
    public string Name { get; init; } = null!;

    [JsonProperty("q")]
    public int Quantity { get; init; } = 1;
}