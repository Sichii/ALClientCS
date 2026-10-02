namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound code-manager message, the channel bots use to coordinate a party. The outbound half is
///     <c>SendCmAsync</c>.
/// </summary>
public sealed record CmData
{
    /// <summary>
    ///     The message payload, forwarded verbatim. Senders typically JSON-encode an object into it.
    /// </summary>
    public string Message { get; init; } = null!;

    /// <summary>The character name of the sender.</summary>
    public string Name { get; init; } = null!;
}