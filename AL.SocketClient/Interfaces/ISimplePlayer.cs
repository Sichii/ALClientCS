using AL.Core.Definitions;

namespace AL.SocketClient.Interfaces
{
    public interface ISimplePlayer
    {
        /// <summary>
        ///     Whether or not the player is AFK. "CODE" results in <c>true</c> here.
        /// </summary>
        bool AFK { get; }

        /// <summary>
        ///     The age of the character in days.
        /// </summary>
        int Age { get; }

        ALClass Class { get; }

        int Level { get; }

        /// <summary>
        ///     The map this player is on.
        /// </summary>
        string Map { get; }

        string Name { get; }

        /// <summary>
        ///     If populated, this player is in a party. <br />
        ///     This is the name of the player who created the party.
        /// </summary>
        string? PartyLeader { get; }
    }
}