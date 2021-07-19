using System.Collections.Generic;
using AL.APIClient.Model;

namespace AL.APIClient.Response
{
    /// <summary>
    ///     Represents the data received when requesting character and server info from the gameserver.
    /// </summary>
    public record ServersAndCharactersResponse
    {
        /// <summary>
        ///     A list of characters this user owns.
        /// </summary>
        public IReadOnlyList<Character> Characters { get; init; } = new List<Character>();

        /// <summary>
        ///     The amount of mail in this user's inbox.
        /// </summary>
        public int Mail { get; init; }

        /// <summary>
        ///     A list of servers available to log onto.
        /// </summary>
        public IReadOnlyList<Server> Servers { get; init; } = new List<Server>();
    }
}