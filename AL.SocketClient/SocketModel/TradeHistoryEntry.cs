#region
using AL.APIClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents one entry in a merchant's trade history. On the wire it is a positional array
///     <c>[event, name, item, price]</c>, with a fifth element on a swap; see
///     <see cref="AL.SocketClient.Json.SystemTextJson.TradeHistoryEntryConverter" />.
/// </summary>
public sealed record TradeHistoryEntry
{
    /// <summary>
    ///     The trade type: "sell", "buy", "giveaway" or "swap".
    /// </summary>
    public string Event { get; init; } = null!;

    /// <summary>
    ///     The item traded. On a swap, the item this merchant gave.
    /// </summary>
    public TradeItem Item { get; init; } = null!;

    /// <summary>The counterparty character name.</summary>
    public string PartnerName { get; init; } = null!;

    /// <summary>
    ///     The gold price. <c>null</c> for a giveaway or a swap, neither of which carries one.
    /// </summary>
    public long? Price { get; init; }

    /// <summary>
    ///     If populated, the item this merchant received on a swap.
    /// </summary>
    public TradeItem? Received { get; init; }
}