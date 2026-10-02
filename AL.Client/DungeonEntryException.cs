#region
using AL.SocketClient.Definitions;
#endregion

namespace AL.Client;

/// <summary>
///     Represents a dungeon entry the server refused, with what it said in <see cref="Reason" />.
/// </summary>
/// <remarks>
///     <see cref="GameResponseType.TransportCantInvalid" /> means the copy no longer exists, so retrying is pointless.
/// </remarks>
public sealed class DungeonEntryException(GameResponseType reason, string message) : InvalidOperationException(message)
{
    public GameResponseType Reason { get; } = reason;
}