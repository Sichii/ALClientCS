#region
using System.Text.Json.Serialization;
using AL.APIClient.Interfaces;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.SocketClient.Model;

/// <inheritdoc cref="ISimpleItem" />
/// <remarks>
///     The server sends <c>item</c> as a bare name on some frames - a rogue's <c>throw</c> names the item it threw as a
///     string (node/server.js:9754) - and a string landing on the object shape throws, which discards the whole frame.
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