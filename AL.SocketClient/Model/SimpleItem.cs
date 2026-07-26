#region
using System.Text.Json.Serialization;
using AL.APIClient.Interfaces;
#endregion

namespace AL.SocketClient.Model;

/// <inheritdoc cref="ISimpleItem" />
/// <seealso cref="ISimpleItem" />
public sealed record SimpleItem : ISimpleItem
{
    public string Name { get; init; } = null!;

    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;
}