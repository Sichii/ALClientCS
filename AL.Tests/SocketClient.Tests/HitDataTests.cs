#region
using AL.Core.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The "hit" event has six mutually incompatible payload shapes (normal, reflect, evade, miss, avoid and burn tick).
///     Only hid/id/source are near-universal - even pid is absent from a burn tick - so every shape must deserialize
///     without throwing, because a throw escapes into ALSocketClient.OnAny and discards the frame.
/// </summary>
public class HitDataTests
{
    [Test]
    public void ABurnTickHasNoProjectileIdAtAll()
    {
        //server.js:12533/:13145 - five keys, and pid is not one of them
        const string HIT = @"{ ""source"":""burn"", ""hid"":""burnerId"", ""id"":""monsterId"", ""damage"":31, ""kill"":false }";

        var obj = TestJson.Socket<HitData>(HIT);

        obj.Should()
           .NotBeNull();

        obj.ProjectileId
           .Should()
           .BeNull();

        obj.Source
           .Should()
           .Be("burn");

        obj.Damage
           .Should()
           .Be(31);

        obj.Kill
           .Should()
           .BeFalse();
    }

    [Test]
    public void AGoldStealAgainstAPlayerArrivesNegative()
    {
        //server.js:3569 - the victim GAINS the gold, so the key is signed
        var obj = TestJson.Socket<HitData>(@"{ ""hid"":""a"", ""id"":""b"", ""goldsteal"":-88 }");

        obj.Should()
           .NotBeNull();

        obj.GoldSteal
           .Should()
           .Be(-88f);
    }

    [Test]
    public void ANormalHitCarriesEveryFieldTheServerCanAttach()
    {
        //server.js:3216,3434,3475,3542,3561,3569,3579,3749 - every optional key on the "def" shape at once
        const string HIT = @"{
   ""hid"":""attackerId"",
   ""id"":""targetId"",
   ""pid"":""wMhQBT"",
   ""source"":""attack"",
   ""projectile"":""stone"",
   ""damage"":25,
   ""damage_type"":""physical"",
   ""crit"":2.35,
   ""heal"":140,
   ""goldsteal"":7,
   ""mp_damage"":300,
   ""mobbing"":4,
   ""aoe"":true,
   ""splash"":true,
   ""stacked"":[""one"",""two""],
   ""kill"":true
}";

        var obj = TestJson.Socket<HitData>(HIT);

        obj.Should()
           .NotBeNull();

        obj.DamageType
           .Should()
           .Be(DamageType.Physical);

        obj.Crit
           .Should()
           .Be(2.35f);

        obj.Heal
           .Should()
           .Be(140f);

        obj.GoldSteal
           .Should()
           .Be(7f);

        obj.MPDamage
           .Should()
           .Be(300f);

        obj.Mobbing
           .Should()
           .Be(4);

        obj.AOE
           .Should()
           .Be(true);

        obj.Splash
           .Should()
           .Be(true);

        obj.Stacked!.Length
           .Should()
           .Be(2);

        obj.Kill
           .Should()
           .BeTrue();
    }

    [Test]
    public void AnUnknownDamageTypeDegradesRatherThanKillingTheFrame()
    {
        var obj = TestJson.Socket<HitData>(@"{ ""hid"":""a"", ""id"":""b"", ""damage_type"":""not_a_real_type"", ""damage"":9 }");

        obj.Should()
           .NotBeNull();

        obj.DamageType
           .Should()
           .Be(DamageType.None);

        obj.Damage
           .Should()
           .Be(9);
    }

    [Test]
    public void TheAttackMsCorrectionArrivesNegativeAndFractional()
    {
        //server.js:1467-1471 - attack_ms is rounded but mssince is not, so ms is neither integral nor positive
        var correction = TestJson.Socket<SkillTimeoutData>(@"{ ""name"":""attack"", ""ms"":-43.75, ""reason"":""attack_ms"" }");

        var cooldown = TestJson.Socket<SkillTimeoutData>(@"{ ""name"":""attack"", ""ms"":500, ""penalty"":0 }");

        correction.Should()
                  .NotBeNull();

        correction.TimeoutMs
                  .Should()
                  .Be(-43.75f);

        correction.Reason
                  .Should()
                  .Be("attack_ms");

        cooldown.Should()
                .NotBeNull();

        cooldown.TimeoutMs
                .Should()
                .Be(500f);

        cooldown.Reason
                .Should()
                .BeNull();
    }

    [Test]
    public void TheEvadeAndMissShapesDeserializeWithoutTheirOptionalKeys()
    {
        //server.js:3385 evade, :3397 miss
        var evaded = TestJson.Socket<HitData>(
            @"{ ""pid"":""p"", ""hid"":""a"", ""id"":""b"", ""damage"":0, ""evade"":true, ""source"":""attack"" }");

        var missed = TestJson.Socket<HitData>(
            @"{ ""pid"":""p"", ""hid"":""a"", ""id"":""b"", ""damage"":0, ""miss"":true, ""source"":""attack"" }");

        evaded.Should()
              .NotBeNull();

        evaded.Evade
              .Should()
              .BeTrue();

        evaded.AOE
              .Should()
              .BeNull();

        missed.Should()
              .NotBeNull();

        missed.Miss
              .Should()
              .BeTrue();

        missed.MPDamage
              .Should()
              .BeNull();
    }

    [Test]
    public void TheReflectShapeOmitsEverythingButFourKeys()
    {
        //server.js:3373 - no source, no damage_type, no crit, and damage is an explicit 0
        const string HIT = @"{ ""pid"":""wMhQBT"", ""hid"":""attackerId"", ""id"":""targetId"", ""damage"":0, ""reflect"":412 }";

        var obj = TestJson.Socket<HitData>(HIT);

        obj.Should()
           .NotBeNull();

        obj.Reflect
           .Should()
           .Be(412f);

        obj.Damage
           .Should()
           .Be(0);

        obj.DamageType
           .Should()
           .BeNull();

        obj.Crit
           .Should()
           .BeNull();

        obj.Stacked
           .Should()
           .BeNull();
    }
}