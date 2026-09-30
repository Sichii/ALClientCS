namespace AL.Client.Model;

/// <summary>
///     Represents what one exchange paid out, as the server announced it.
/// </summary>
/// <param name="Items">
///     Each item received, with <see cref="SocketClient.Model.Item.Quantity" /> set to the count received. More than one
///     when the table rolled a bonus drop beside the main one.
/// </param>
/// <param name="Gold">The gold received.</param>
public sealed record ExchangeResult(IReadOnlyList<InventoryIndexer> Items, long Gold);