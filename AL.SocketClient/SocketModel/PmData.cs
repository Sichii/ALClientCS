namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound private message (node/server.js:4601). The outbound half is
///     <c>
///         WhisperAsync
///     </c>
///     .
/// </summary>
/// <remarks>
///     The server sends this to both ends of a whisper, and <see cref="To" /> is the only thing that tells them
///     apart: it is populated on the copy echoed back to the sender and absent on the copy the recipient receives.
///     So a sent message needs no local echo - it arrives here like any other.
/// </remarks>
public sealed record PmData
{
    /// <summary>
    ///     The entity id of the sender. Carries the sender's account owner id rather than a character id on a
    ///     cross-server whisper (node/server.js:4562).
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    ///     The message text, stripped and truncated to 1200 characters by the server.
    /// </summary>
    public string Message { get; init; } = null!;

    /// <summary>
    ///     The character name of the sender.
    /// </summary>
    public string Owner { get; init; } = null!;

    /// <summary>
    ///     The character name the message was addressed to, populated only on the copy echoed back to the sender.
    ///     Null identifies a message this character received.
    /// </summary>
    public string? To { get; init; }
}
