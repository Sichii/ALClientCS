#region
using System.Text.Json.Serialization;
using AL.Core.Definitions;
using AL.SocketClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents the data received for UI related things. This data is generally unimportant.
/// </summary>
public sealed class UIData
{
    /// <summary>The mana a <c>restore_mp</c> proc gave back.</summary>
    public float? Amount { get; init; }

    /// <summary>
    ///     The paying side of a player-to-player sale (<c>+$$</c> ).
    /// </summary>
    public string? Buyer { get; init; }

    /// <summary>
    ///     Why a monster dropped its target on a <c>disengage</c> - <c>taunt redirect</c> , <c>bored</c> , <c>scare</c> and
    ///     the rest. The client picks the monster's shout off it.
    /// </summary>
    public string? Cause { get; init; }

    public Direction? Direction { get; init; }
    public string? From { get; init; }

    /// <summary>
    ///     How much health a monster regained on an <c>mheal</c> .
    /// </summary>
    public float? Heal { get; init; }

    public string? Id { get; init; }
    public IReadOnlyList<string>? Ids { get; init; }
    public SimpleItem? Item { get; init; }

    /// <summary>
    ///     Which way a monster's level moved on an <c>mlevel</c> : -1 down, anything else up.
    /// </summary>
    /// <remarks>
    ///     Read as a float although the server only ever sends ±1: a fractional value would fail to parse and take the whole
    ///     frame with it.
    /// </remarks>
    public float? Mult { get; init; }

    public string? Name { get; init; }

    /// <summary>
    ///     Player-to-player transfers name their two ends here rather than in <c>From</c> / <c>To</c> .
    /// </summary>
    public string? Receiver { get; init; }

    /// <summary>
    ///     The receiving side of a player-to-player sale (<c>+$$</c> ).
    /// </summary>
    public string? Seller { get; init; }

    public string? Sender { get; init; }
    public string? To { get; init; }

    [JsonPropertyName("type")]
    public UIDataType UIDataType { get; init; }
}