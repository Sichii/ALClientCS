namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound magiport offer. Pass <see cref="Name" /> to <c>AcceptMagiportAsync</c> to accept.
/// </summary>
public sealed record MagiportData
{
    /// <summary>
    ///     The character name of the mage offering the magiport.
    /// </summary>
    public string Name { get; init; } = null!;
}