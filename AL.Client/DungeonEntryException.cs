#region
using AL.SocketClient.Definitions;
#endregion

namespace AL.Client;

/// <summary>
///     The server refused a dungeon entry. <see cref="Reason" /> is what it said.
/// </summary>
/// <remarks>
///     Typed rather than a message, because one refusal is worth acting on:
///     <see cref="GameResponseType.TransportCantInvalid" /> means the copy no longer exists, which a bot wants to write
///     down rather than retry.
/// </remarks>
public sealed class DungeonEntryException(GameResponseType reason, string message) : InvalidOperationException(message)
{
    public GameResponseType Reason { get; } = reason;
}