#region
using AL.Data;
using AL.Data.Monsters;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
#endregion

namespace AL.Client.Extensions;

/// <summary>
///     Provides a set of extensions for <see cref="AL.SocketClient.Model.Monster" />s.
/// </summary>
public static class MonsterExtensions
{
    /// <summary>
    ///     Fills each soft property a freshly sighted monster's frame did not carry from its G data.
    /// </summary>
    /// <remarks>
    ///     The server omits a soft property equal to the monster's definition, so without this a new monster reads 0 for hp,
    ///     speed, attack and the rest until it takes damage.
    /// </remarks>
    /// <param name="monster">
    ///     The monster to fill.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     monster
    /// </exception>
    public static void BackfillSoftDefaults(this Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        var def = monster.GetData();

        monster.BackfillSoftDefault(EntityUpdateField.HP, def.HP);

        //the server sends max_hp only when it differs from def.hp, so full health backfills off def.hp
        monster.BackfillSoftDefault(EntityUpdateField.MaxHP, def.HP);
        monster.BackfillSoftDefault(EntityUpdateField.MP, def.MP);
        monster.BackfillSoftDefault(EntityUpdateField.MaxMP, def.MP);
        monster.BackfillSoftDefault(EntityUpdateField.Attack, def.Attack);
        monster.BackfillSoftDefault(EntityUpdateField.Speed, def.Speed);
        monster.BackfillSoftDefault(EntityUpdateField.XP, def.XP);
        monster.BackfillSoftDefault(EntityUpdateField.Frequency, def.Frequency);
        monster.BackfillSoftDefault(EntityUpdateField.Armor, def.Armor);
        monster.BackfillSoftDefault(EntityUpdateField.Resistance, def.Resistance);

        //the server never sends range, so without this every monster reads a reach of zero
        monster.BackfillSoftDefault(EntityUpdateField.Range, def.Range);

        //the server sends level only when above 1, so an absent level means 1, not the int default 0
        monster.BackfillSoftDefault(EntityUpdateField.Level, 1);
    }

    /// <summary>
    ///     Gets the "G" data for this monster.
    /// </summary>
    /// <param name="monster">
    ///     The monster to get the data for.
    /// </param>
    /// <returns>
    ///     <see cref="GMonster" />
    ///     <br />
    ///     The "G" data for this monster from <see cref="GameData" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     monster
    /// </exception>
    public static GMonster GetData(this Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        return GameData.Monsters[monster.Name]!;
    }

    /// <summary>
    ///     Determines whether kills on this monster count for every attacker.
    /// </summary>
    /// <remarks>
    ///     The frame carries the flag only when the instance differs from its definition.
    /// </remarks>
    /// <param name="monster">
    ///     The monster to check.
    /// </param>
    /// <returns>
    ///     true if kills count for every attacker; otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     monster
    /// </exception>
    public static bool IsCooperative(this Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        return monster.Cooperative
               ?? monster.GetData()
                         .Cooperative;
    }
}