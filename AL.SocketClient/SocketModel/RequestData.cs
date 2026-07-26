namespace AL.SocketClient.SocketModel;

/// <summary>
///     Represents an inbound request from another character to join your party - the counterpart of
///     <see cref="InviteData" /> (node/server.js:10945).
/// </summary>
public sealed record RequestData
{
    /// <summary>
    ///     The character name requesting to join your party.
    /// </summary>
    public string Name { get; init; } = null!;
}