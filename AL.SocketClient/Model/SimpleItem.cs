#region
using System.Text.Json.Serialization;
using AL.APIClient.Interfaces;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.SocketClient.Model;

/// <inheritdoc cref="ISimpleItem" />
/// <remarks>
///     The server sends <c>item</c> as a bare name on some frames, such as the item a rogue's <c>throw</c> threw.
/// </remarks>
/// <seealso cref="ISimpleItem" />
[JsonStringOrObject(nameof(Name))]
public sealed record SimpleItem : ISimpleItem, IOptionalObject
{
    public bool ContainsData { get; set; }

    public string Name { get; init; } = null!;

    [JsonPropertyName("q")]
    public int Quantity { get; init; } = 1;
}