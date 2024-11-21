#region
using System;
using AL.Data;
using AL.Data.Classes;
using AL.SocketClient.Interfaces;
#endregion

namespace AL.Client.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="AL.SocketClient.Interfaces.ISimplePlayer" />s and
///     <see cref="AL.SocketClient.Model.Player" />s.
/// </summary>
public static class PlayerExtensions
{
    /// <summary>
    ///     Gets the "G" data for this player's class.
    /// </summary>
    /// <param name="player">
    ///     The player to get the data for.
    /// </param>
    /// <returns>
    ///     <see cref="GClass" />
    ///     <br />
    ///     The "G" data for this player's class from <see cref="GameData" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     player
    /// </exception>
    public static GClass? GetData(this ISimplePlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);

        return GameData.Classes[player.Class];
    }
}