#region
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The entity merge is a key-presence merge: a delta only overwrites the fields it actually carried. The
///     server's encoder is sparse (a soft property equal to the G default is omitted), so a whitelist merge that
///     copies every property unconditionally zeros a live monster's hp/speed on the next position-only frame.
/// </summary>
[TestClass]
public class EntityMergeTests
{
   [TestMethod]
   public void PresentFieldsRecordsExactlyTheKeysTheFrameCarried()
   {
      const string FULL = @"{ ""id"":""goo1"", ""type"":""goo"", ""x"":10, ""y"":20, ""hp"":500, ""max_hp"":500, ""speed"":40, ""attack"":18 }";

      var monster = TestJson.Socket<Monster>(FULL);

      Assert.IsNotNull(monster);
      Assert.IsTrue((monster.PresentFields & EntityUpdateField.HP) != 0);
      Assert.IsTrue((monster.PresentFields & EntityUpdateField.Speed) != 0);
      Assert.IsTrue((monster.PresentFields & EntityUpdateField.X) != 0);
      //keys the frame did not carry must not be flagged
      Assert.IsFalse((monster.PresentFields & EntityUpdateField.MP) != 0);
      Assert.IsFalse((monster.PresentFields & EntityUpdateField.Level) != 0);
   }

   /// <summary>
   ///     The regression the unconditional whitelist caused: a bare position delta must leave hp/speed intact.
   /// </summary>
   [TestMethod]
   public void PositionOnlyDeltaLeavesLiveStatsUntouched()
   {
      var live = TestJson.Socket<Monster>(
         @"{ ""id"":""goo1"", ""type"":""goo"", ""x"":10, ""y"":20, ""hp"":500, ""max_hp"":500, ""speed"":40, ""attack"":18 }");
      var delta = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""x"":12, ""y"":20 }");

      Assert.IsNotNull(live);
      Assert.IsNotNull(delta);

      live.Update(delta);

      //position adopted from the delta
      Assert.AreEqual(12f, live.X);
      //stats the delta omitted are preserved, not zeroed
      Assert.AreEqual(500f, live.HP);
      Assert.AreEqual(40f, live.Speed);
      Assert.AreEqual(18f, live.Attack);
      Assert.AreEqual(500f, live.MaxHP);
   }

   [TestMethod]
   public void PresentStatIsOverwrittenByTheDelta()
   {
      var live = TestJson.Socket<Monster>(
         @"{ ""id"":""goo1"", ""type"":""goo"", ""x"":10, ""y"":20, ""hp"":500 }");
      var delta = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""hp"":320 }");

      Assert.IsNotNull(live);
      Assert.IsNotNull(delta);

      live.Update(delta);

      //hp WAS in the delta, so it is adopted
      Assert.AreEqual(320f, live.HP);
   }

   [TestMethod]
   public void BackfillFillsAStatTheFrameOmitted()
   {
      //a freshly-sighted, undamaged monster carries no hp key because it equals the def
      var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""x"":0, ""y"":0 }");

      Assert.IsNotNull(monster);
      Assert.AreEqual(0f, monster.HP);

      monster.BackfillSoftDefault(EntityUpdateField.HP, 500);
      monster.BackfillSoftDefault(EntityUpdateField.Speed, 40);

      Assert.AreEqual(500f, monster.HP);
      Assert.AreEqual(40f, monster.Speed);
   }

   [TestMethod]
   public void BackfillNeverOverridesAValueTheFrameCarried()
   {
      //a damaged monster DOES carry hp; the def must not clobber it back to full
      var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""hp"":137 }");

      Assert.IsNotNull(monster);

      monster.BackfillSoftDefault(EntityUpdateField.HP, 500);

      Assert.AreEqual(137f, monster.HP);
   }

   /// <summary>
   ///     The server sends level only when it exceeds 1, so an undamaged level-1 monster omits it. Absent must
   ///     resolve to 1 (the monster level floor), not the int default 0 - the phase goal names level explicitly.
   /// </summary>
   [TestMethod]
   public void AbsentLevelBackfillsToOneNotZero()
   {
      var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""x"":0, ""y"":0 }");

      Assert.IsNotNull(monster);
      Assert.AreEqual(0, monster.Level);

      monster.BackfillSoftDefault(EntityUpdateField.Level, 1);

      Assert.AreEqual(1, monster.Level);
   }

   [TestMethod]
   public void PresentLevelIsNotOverwrittenByTheBackfillFloor()
   {
      //a level-8 monster sends level (>1), so the floor must not stomp it back to 1
      var monster = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"", ""level"":8 }");

      Assert.IsNotNull(monster);

      monster.BackfillSoftDefault(EntityUpdateField.Level, 1);

      Assert.AreEqual(8, monster.Level);
   }

   [TestMethod]
   public void MergingAcrossIdsThrows()
   {
      var a = TestJson.Socket<Monster>(@"{ ""id"":""goo1"", ""type"":""goo"" }");
      var b = TestJson.Socket<Monster>(@"{ ""id"":""goo2"", ""type"":""goo"" }");

      Assert.IsNotNull(a);
      Assert.IsNotNull(b);
      Assert.ThrowsException<System.InvalidOperationException>(() => a.Update(b));
   }
}
