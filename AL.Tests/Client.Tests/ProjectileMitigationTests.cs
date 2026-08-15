#region
using AL.Client.Extensions;
using AL.Data;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using AL.Tests.Characterization;
using FluentAssertions;
#endregion

namespace AL.Tests.Client.Tests;

/// <summary>
///     Restates the server's mitigation rule for in-flight damage rather than calling a production probe: a
///     projectile's damage is reduced by the target's defense less TWICE the shooter's pierce, because the server
///     subtracts both the attacker's live stat and the copy stamped onto the projectile at creation
///     (node/server.js:3179, :3611).
/// </summary>
[NotInParallel(ParallelKeys.GAME_DATA)]
public class ProjectileMitigationTests
{
    [Before(Class)]
    public static void EnsureGameData()
    {
        //same guard as GameDataTestBed, but from the committed snapshot so no credentials are needed. the
        //assertions below survive either data source - a warrior deals physical damage in both
        if (GameData.Version == 0)
            GameData.Populate(Fixture.GameDataJson);
    }

    [Test]
    public void WillDieToProjectilesCountsPierceTwice()
    {
        var attacker = TestJson.Socket<Player>(@"{""id"":""a1"",""ctype"":""warrior"",""apiercing"":100}")!;

        static ActionData Shot(string source = "attack")
            => new()
            {
                Target = "t1",
                AttackerId = "a1",
                Damage = 100,
                Source = source
            };

        EntityBase? FindAttacker(string id)
            => id == "a1" ? attacker : null;

        //armor 400 less twice the 100 pierce is 200, a 0.80 multiplier: 100 * 0.80 * 0.95 = 76 clears 70.
        //subtracting pierce once would leave armor 300, a 0.705 multiplier, and 66.98 would not
        var dying = TestJson.Socket<Player>(@"{""id"":""t1"",""hp"":70,""armor"":400}")!;

        dying.WillDieToProjectiles([Shot()], FindAttacker)
             .Should()
             .BeTrue();

        //and 76 does not clear 80, where unmitigated damage (95) would - so the armor is really being read
        var surviving = TestJson.Socket<Player>(@"{""id"":""t1"",""hp"":80,""armor"":400}")!;

        surviving.WillDieToProjectiles([Shot()], FindAttacker)
                 .Should()
                 .BeFalse();

        //piercingshot stamps 500 extra pierce onto its projectile on top of the doubled stat
        //(node/server.js:3049, :3179): armor 400 - 200 - 500 is -300, a 1.15 multiplier, and 109.25 clears 80
        surviving.WillDieToProjectiles([Shot("piercingshot")], FindAttacker)
                 .Should()
                 .BeTrue();

        //no resolver, no mitigation - the raw 95 still clears 80, the plain overload's original arithmetic
        surviving.WillDieToProjectiles([Shot()])
                 .Should()
                 .BeTrue();
    }
}
