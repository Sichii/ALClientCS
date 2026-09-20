#region
using System.Text.Json.Serialization;
using AL.APIClient.Interfaces;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents an item received via <see cref="AL.SocketClient.SocketModel.GameResponseData" />.
/// </summary>
/// <remarks>
///     The server may send <c>item</c> as a bare name rather than an object.
/// </remarks>
[JsonStringOrObject(nameof(Name))]
public sealed record ResponseItem : ISimpleItem, IOptionalObject
{
    /// <summary>The chance of upgrading/compounding the item.</summary>
    public float? Chance { get; init; }

    public bool ContainsData { get; set; }

    /// <summary>The current grace of the item.</summary>
    public float? Grace { get; init; }

    /// <summary>The current level of the item.</summary>
    public int? Level { get; init; }

    public string Name { get; init; } = null!;

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    [JsonPropertyName("q")]
    public int Quantity { get; } = 1;
}