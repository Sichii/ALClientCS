namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound code-manager message - the channel AL bots use to coordinate a multi-character party
///     (node/server.js:4502). The outbound half is
///     <c>
///         SendCmAsync
///     </c>
///     .
/// </summary>
public sealed record CmData
{
    /// <summary>
    ///     The message payload. Senders typically JSON-encode a structured object into this string; the server forwards it
    ///     verbatim as an opaque string.
    /// </summary>
    public string Message { get; init; } = null!;

    /// <summary>
    ///     The character name of the sender.
    /// </summary>
    public string Name { get; init; } = null!;
}