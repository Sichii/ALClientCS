#region
using AL.Core.Definitions;
#endregion

namespace AL.SocketClient.Interfaces;

public interface ISimplePlayer
{
    /// <summary>
    ///     The server's last word on whether this player is away. A character running CODE carries the flag for its whole
    ///     session, and a frame that omits it says nothing either way.
    /// </summary>
    AfkState AFK { get; }

    /// <summary>The age of the character in days.</summary>
    int Age { get; }

    ALClass Class { get; }

    int Level { get; }

    /// <summary>The map this player is on.</summary>
    string Map { get; }

    string Name { get; }

    /// <summary>
    ///     If populated, this player is in a party.
    ///     <br />
    ///     This is the name of the player who created the party.
    /// </summary>
    string? PartyLeader { get; }
}