#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.Core.Json.Attributes;
using AL.Core.Json.Interfaces;
#endregion

namespace AL.SocketClient.Model;

/// <summary>
///     Represents what an <c>equip_batch</c> did with one of the equips it was given (node/server.js:7316).
/// </summary>
/// <remarks>
///     An entry the server applied is an object echoing the emit's own inventory slot; one it refused is a bare reason
///     string, and is always the last entry, since a refusal stops the batch where it stands.
/// </remarks>
[JsonStringOrObject(nameof(Refusal))]
public sealed record EquipBatchEntry : IOptionalObject
{
    /// <summary>
    ///     Whether the server applied this entry. False on a refusal, which arrives as a bare string.
    /// </summary>
    public bool ContainsData { get; set; }

    /// <summary>
    ///     The inventory slot the item came out of, echoed back from the emit. Null on a refused entry.
    /// </summary>
    [JsonPropertyName("num")]
    public int? InventorySlot { get; init; }

    /// <summary>
    ///     Why the server refused this entry, or null where it applied it. One of <c>invalid</c> , <c>no_item</c> ,
    ///     <c>item_blocked</c> , <c>item_placeholder</c> or <c>cant_equip</c> .
    /// </summary>
    public string? Refusal { get; init; }

    /// <summary>
    ///     The slot the item actually went into, which the server picks itself and need not be the one the emit named. Null on
    ///     a refused entry.
    /// </summary>
    [JsonPropertyName("slot")]
    public Slot? Slot { get; init; }
}