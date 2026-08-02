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
    ///     Seeds a freshly-sighted monster's soft properties from its G data. The server omits a soft property equal to the
    ///     monster's def, so without this a new monster reports 0 for hp/speed/attack/etc. until it takes damage. Only fills
    ///     fields the frame did not carry. Covers the numeric stats named in the phase goal plus level; the non-numeric soft
    ///     flags the browser also seeds (
    ///     <c>
    ///         1hp
    ///     </c>
    ///     ,
    ///     <c>
    ///         cooperative
    ///     </c>
    ///     ,
    ///     <c>
    ///         drops
    ///     </c>
    ///     ,
    ///     <c>
    ///         skin
    ///     </c>
    ///     ,
    ///     <c>
    ///         js/game.js:762-771
    ///     </c>
    ///     ) are not backfilled - no bot decision reads them.
    /// </summary>
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

        //never on the wire at all, unlike every other line here: monster_to_client sends neither a soft nor a hard
        //'range', so without this every live monster reads a reach of zero. Silent everywhere it matters - a kite
        //sizes its stand-off against nothing, and a lap scores being stood on as perfectly safe
        monster.BackfillSoftDefault(EntityUpdateField.Range, def.Range);

        //the server sends level only when > 1 (node/server.js:922), so an absent level means 1, not the int default 0
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
}