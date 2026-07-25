#region
using AL.Core.Definitions;
using AL.SocketClient.SocketModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The "hit" event has six mutually incompatible payload shapes (normal, reflect, evade, miss, avoid and
///     burn tick). Only hid/id/source are near-universal - even pid is absent from a burn tick - so every shape
///     must deserialize without throwing, because a throw escapes into ALSocketClient.OnAny and discards the frame.
/// </summary>
[TestClass]
public class HitDataTests
{
   [TestMethod]
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

      Assert.IsNotNull(obj);
      Assert.AreEqual(DamageType.Physical, obj.DamageType);
      Assert.AreEqual(2.35f, obj.Crit);
      Assert.AreEqual(140f, obj.Heal);
      Assert.AreEqual(7f, obj.GoldSteal);
      Assert.AreEqual(300f, obj.MPDamage);
      Assert.AreEqual(4, obj.Mobbing);
      Assert.AreEqual(true, obj.AOE);
      Assert.AreEqual(true, obj.Splash);
      Assert.AreEqual(2, obj.Stacked!.Length);
      Assert.IsTrue(obj.Kill);
   }

   [TestMethod]
   public void AGoldStealAgainstAPlayerArrivesNegative()
   {
      //server.js:3569 - the victim GAINS the gold, so the key is signed
      var obj = TestJson.Socket<HitData>(@"{ ""hid"":""a"", ""id"":""b"", ""goldsteal"":-88 }");

      Assert.IsNotNull(obj);
      Assert.AreEqual(-88f, obj.GoldSteal);
   }

   [TestMethod]
   public void TheReflectShapeOmitsEverythingButFourKeys()
   {
      //server.js:3373 - no source, no damage_type, no crit, and damage is an explicit 0
      const string HIT = @"{ ""pid"":""wMhQBT"", ""hid"":""attackerId"", ""id"":""targetId"", ""damage"":0, ""reflect"":412 }";

      var obj = TestJson.Socket<HitData>(HIT);

      Assert.IsNotNull(obj);
      Assert.AreEqual(412f, obj.Reflect);
      Assert.AreEqual(0, obj.Damage);
      Assert.IsNull(obj.DamageType);
      Assert.IsNull(obj.Crit);
      Assert.IsNull(obj.Stacked);
   }

   [TestMethod]
   public void TheEvadeAndMissShapesDeserializeWithoutTheirOptionalKeys()
   {
      //server.js:3385 evade, :3397 miss
      var evaded = TestJson.Socket<HitData>(
         @"{ ""pid"":""p"", ""hid"":""a"", ""id"":""b"", ""damage"":0, ""evade"":true, ""source"":""attack"" }");

      var missed = TestJson.Socket<HitData>(
         @"{ ""pid"":""p"", ""hid"":""a"", ""id"":""b"", ""damage"":0, ""miss"":true, ""source"":""attack"" }");

      Assert.IsNotNull(evaded);
      Assert.IsTrue(evaded.Evade);
      Assert.IsNull(evaded.AOE);
      Assert.IsNotNull(missed);
      Assert.IsTrue(missed.Miss);
      Assert.IsNull(missed.MPDamage);
   }

   [TestMethod]
   public void ABurnTickHasNoProjectileIdAtAll()
   {
      //server.js:12533/:13145 - five keys, and pid is not one of them
      const string HIT = @"{ ""source"":""burn"", ""hid"":""burnerId"", ""id"":""monsterId"", ""damage"":31, ""kill"":false }";

      var obj = TestJson.Socket<HitData>(HIT);

      Assert.IsNotNull(obj);
      Assert.IsNull(obj.ProjectileId);
      Assert.AreEqual("burn", obj.Source);
      Assert.AreEqual(31, obj.Damage);
      Assert.IsFalse(obj.Kill);
   }

   [TestMethod]
   public void AnUnknownDamageTypeDegradesRatherThanKillingTheFrame()
   {
      var obj = TestJson.Socket<HitData>(
         @"{ ""hid"":""a"", ""id"":""b"", ""damage_type"":""not_a_real_type"", ""damage"":9 }");

      Assert.IsNotNull(obj);
      Assert.AreEqual(DamageType.None, obj.DamageType);
      Assert.AreEqual(9, obj.Damage);
   }

   [TestMethod]
   public void TheAttackMsCorrectionArrivesNegativeAndFractional()
   {
      //server.js:1467-1471 - attack_ms is rounded but mssince is not, so ms is neither integral nor positive
      var correction = TestJson.Socket<SkillTimeoutData>(
         @"{ ""name"":""attack"", ""ms"":-43.75, ""reason"":""attack_ms"" }");

      var cooldown = TestJson.Socket<SkillTimeoutData>(@"{ ""name"":""attack"", ""ms"":500, ""penalty"":0 }");

      Assert.IsNotNull(correction);
      Assert.AreEqual(-43.75f, correction.TimeoutMs);
      Assert.AreEqual("attack_ms", correction.Reason);
      Assert.IsNotNull(cooldown);
      Assert.AreEqual(500f, cooldown.TimeoutMs);
      Assert.IsNull(cooldown.Reason);
   }
}
