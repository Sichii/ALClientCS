using System;
using AL.Client.Helpers;
using AL.Core.Geometry;
using AL.Data;
using AL.Data.Classes;
using AL.SocketClient.Interfaces;
using AL.SocketClient.Model;

namespace AL.Client.Extensions
{
    /// <summary>
    ///     Provides a set of extensions for <see cref="AL.SocketClient.Interfaces.ISimplePlayer" />s and
    ///     <see cref="AL.SocketClient.Model.Player" />s.
    /// </summary>
    public static class PlayerExtensions
    {
        /// <summary>
        ///     Gets the "G" data for this player's class.
        /// </summary>
        /// <param name="player">The player to get the data for.</param>
        /// <returns>
        ///     <see cref="GClass" /> <br />
        ///     The "G" data for this player's class from <see cref="GameData" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">player</exception>
        public static GClass? GetData(this ISimplePlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            return GameData.Classes[player.Class];
        }
    }
}