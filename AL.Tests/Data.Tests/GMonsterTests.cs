#region
using AL.Data.Monsters;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The shape a monster keeps whatever the payload leaves out or blanks.
/// </summary>
public class GMonsterTests
{
    /// <summary>
    ///     The cave monsters arrive with the abilities key present and set to null. Every reader indexes the dictionary
    ///     without a null check, so the record must keep an empty one rather than let the deserializer write null over
    ///     the default.
    /// </summary>
    [Test]
    public void AnAbilitiesKeySetToNullReadsAsNoAbilities()
    {
        var monster = TestJson.Data<GMonster>("""{"name":"Cave Bat","abilities":null,"spawns":null}""")!;

        monster.Abilities
               .Should()
               .NotBeNull()
               .And
               .BeEmpty();
    }
}
