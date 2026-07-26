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

//Newtonsoft applies string-or-object at the one member that holds a ResponseItem
//(GameResponseData.Item); System.Text.Json's factory reads a type-level marker, and without this a bare-string
//"item" would throw on the System.Text.Json path while Newtonsoft bound it to Name.
[JsonStringOrObject(nameof(Name))]
public sealed record ResponseItem : ISimpleItem, IOptionalObject
{
    /// <summary>
    ///     The chance of upgrading/compounding the item.
    /// </summary>
    public float? Chance { get; init; }

    public bool ContainsData { get; set; }

    /// <summary>
    ///     The current grace of the item.
    /// </summary>
    public float? Grace { get; init; }

    /// <summary>
    ///     The current level of the item.
    /// </summary>
    public int? Level { get; init; }

    public string Name { get; init; } = null!;

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    [JsonPropertyName("q")]
    public int Quantity { get; } = 1;
}