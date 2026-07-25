namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound magiport offer (node/server.js:9697). <see cref="Name" /> is the caster offering
///     the teleport; pass it to <c>AcceptMagiportAsync</c> to accept.
/// </summary>
public sealed record MagiportData
{
    /// <summary>
    ///     The character name of the mage offering the magiport.
    /// </summary>
    public string Name { get; init; } = null!;
}
