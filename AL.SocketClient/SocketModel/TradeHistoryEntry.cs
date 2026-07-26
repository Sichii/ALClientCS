#region
using AL.APIClient.Model;
#endregion

namespace AL.SocketClient.SocketModel;

/// <summary>
///     One entry in a merchant's trade history (node/server.js:8298). On the wire each entry is a 4-element positional
///     array
///     <c>
///         [event, name, item, price]
///     </c>
///     , not a keyed object - see <see cref="AL.SocketClient.Json.SystemTextJson.TradeHistoryEntryConverter" />.
/// </summary>
public sealed record TradeHistoryEntry
{
    /// <summary>
    ///     The trade type: "sell", "buy" or "giveaway".
    /// </summary>
    public string Event { get; init; } = null!;

    /// <summary>
    ///     The item traded.
    /// </summary>
    public TradeItem Item { get; init; } = null!;

    /// <summary>
    ///     The counterparty character name.
    /// </summary>
    public string PartnerName { get; init; } = null!;

    /// <summary>
    ///     The gold price.
    ///     <c>
    ///         null
    ///     </c>
    ///     for a giveaway, which carries no price.
    /// </summary>
    public long? Price { get; init; }
}