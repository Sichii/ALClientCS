using System;
using AL.Data;
using AL.Data.Monsters;
using AL.SocketClient.Model;

namespace AL.Client.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="AL.SocketClient.Model.Monster" />s.
    /// </summary>
    public static class MonsterExtensions
    {
        /// <summary>
        ///     Gets the "G" data for this monster.
        /// </summary>
        /// <param name="monster">The monster to get the data for.</param>
        /// <returns>
        ///     <see cref="GMonster" /> <br />
        ///     The "G" data for this monster from <see cref="GameData" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">monster</exception>
        public static GMonster GetData(this Monster monster)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));

            return GameData.Monsters[monster.Name]!;
        }
    }
}