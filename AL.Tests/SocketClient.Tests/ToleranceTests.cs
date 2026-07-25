#region
using AL.Core.Definitions;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Condition = AL.Core.Definitions.Condition;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Covers the deserialization tolerance work. Every case here is a real wire shape that previously threw,
///     and a throw is not a local failure - it escapes into ALSocketClient.OnAny, which swallows it and
///     discards the entire socket frame.
/// </summary>
[TestClass]
public class ToleranceTests
{
   [TestMethod]
   public void UnknownConditionKeyIsSkippedAndKnownOnesSurvive()
   {
      //patronsgrace is stamped on every player on a blessed server, and used to kill the start payload
      const string ENTITY = @"{
   ""id"":""someMonster"",
   ""s"":{ ""stone"":{ ""ms"":5000 }, ""patronsgrace"":{ ""ms"":1000 }, ""__nope"":{ ""ms"":1 } }
}";

      var obj = TestJson.Socket<Monster>(ENTITY);

      Assert.IsNotNull(obj);
      Assert.IsTrue(obj.Conditions.ContainsKey(Condition.Stone));
      Assert.IsTrue(obj.Conditions.ContainsKey(Condition.PatronsGrace));
      Assert.AreEqual(2, obj.Conditions.Count);
   }

   [TestMethod]
   public void UnknownGameResponseCodeDegradesToUnknown()
   {
      var fromObject = TestJson.Socket<GameResponseData>(@"{ ""response"":""not_a_real_code_at_all"" }");
      var fromString = TestJson.Socket<GameResponseData>(@"""distance""");

      Assert.IsNotNull(fromObject);
      Assert.AreEqual(GameResponseType.Unknown, fromObject.ResponseType);
      Assert.IsNotNull(fromString);
   }

   [TestMethod]
   public void PartyListArrivesAsFalseWhenThePartyDissolves()
   {
      var dissolved = TestJson.Socket<PartyUpdateData>(@"{ ""list"":false }");
      var populated = TestJson.Socket<PartyUpdateData>(@"{ ""list"":[""Alice"",""Bob""] }");

      Assert.IsNotNull(dissolved);
      Assert.AreEqual(0, dissolved.MemberNames.Count);
      Assert.IsNotNull(populated);
      Assert.AreEqual(2, populated.MemberNames.Count);
   }

   [TestMethod]
   public void ItemTitleArrivesAsAStringOrFalseRatherThanAPrediction()
   {
      //server.js:1868/:6689 set p="shiny", :11521 sets p="legacy", :5953 can yield false
      var shiny = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":""shiny"" }");
      var plain = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":false }");

      var upgrading = TestJson.Socket<Item>(
         @"{ ""name"":""placeholder"", ""p"":{ ""chance"":0.6, ""name"":""fireblade"", ""level"":2, ""scroll"":""scroll1"" } }");

      Assert.IsNotNull(shiny);
      Assert.AreEqual("shiny", shiny.Prediction?.Title);
      Assert.IsNotNull(plain);
      Assert.IsNull(plain.Prediction);
      Assert.IsNotNull(upgrading);
      Assert.IsTrue(upgrading.Prediction!.ContainsData);
      Assert.AreEqual("fireblade", upgrading.Prediction.Name);
   }

   [TestMethod]
   public void DeadPlayerRipIsEitherTrueOrAGravestoneName()
   {
      var boolean = TestJson.Socket<Player>(@"{ ""id"":""a"", ""rip"":true }");
      var cosmetic = TestJson.Socket<Player>(@"{ ""id"":""b"", ""rip"":""grave1"" }");
      var alive = TestJson.Socket<Player>(@"{ ""id"":""c"", ""rip"":false }");

      Assert.IsNotNull(boolean);
      Assert.IsTrue(boolean.RIP);
      Assert.IsNotNull(cosmetic);
      Assert.IsTrue(cosmetic.RIP);
      Assert.IsNotNull(alive);
      Assert.IsFalse(alive.RIP);
   }

   [TestMethod]
   public void EvalArrivesAsABareString()
   {
      var bare = TestJson.Socket<EvalData>(@"""egg_splash(0,0)""");
      var wrapped = TestJson.Socket<EvalData>(@"{ ""code"":""egg_splash(0,0)"" }");

      Assert.IsNotNull(bare);
      Assert.AreEqual("egg_splash(0,0)", bare.Code);
      Assert.IsNotNull(wrapped);
      Assert.AreEqual("egg_splash(0,0)", wrapped.Code);
   }

   [TestMethod]
   public void UnknownChestSpriteDoesNotDropTheDrop()
   {
      var known = TestJson.Socket<DropData>(@"{ ""id"":""1"", ""chest"":""chest8"" }");
      var unknown = TestJson.Socket<DropData>(@"{ ""id"":""2"", ""chest"":""chest_from_the_future"" }");

      Assert.IsNotNull(known);
      Assert.AreEqual(ChestType.Chest8, known.ChestType);
      Assert.IsNotNull(unknown);
      Assert.AreEqual(ChestType.Unknown, unknown.ChestType);
   }

   [TestMethod]
   public void AnUnknownEnumDoesNotTruncateTheRestOfTheObject()
   {
      //the tolerant converter must leave the reader aligned or every field after the bad one is lost
      const string DROP = @"{ ""id"":""theId"", ""chest"":""nonexistent"", ""x"":123.5, ""y"":456.5 }";

      var obj = TestJson.Socket<DropData>(DROP);

      Assert.IsNotNull(obj);
      Assert.AreEqual(ChestType.Unknown, obj.ChestType);
      Assert.AreEqual("theId", obj.Id);
      Assert.AreEqual(123.5f, obj.X);
      Assert.AreEqual(456.5f, obj.Y);
   }

   [TestMethod]
   public void EveryEditedEnumParsesWithoutAKeyCollision()
   {
      //EnumHelper builds a case-insensitive dictionary with .Add, which throws on a duplicate key,
      //and Heal/Healed/Healing and Stone/Stoned are one typo away from colliding
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<Condition>("patronsgrace", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<Condition>("damage_received", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<Condition>("healing", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<Condition>("healed", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<Condition>("stone", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<Condition>("stoned", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<ChestType>("chestp", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<KeyType>("spiderkey", out _));
      Assert.IsTrue(AL.Core.Helpers.EnumHelper.TryParse<Stand>("stand1", out _));

      //distinct members, not aliases of one another
      Assert.AreNotEqual(Condition.Healing, Condition.Healed);
      Assert.AreNotEqual(Condition.Stone, Condition.Stoned);
      Assert.AreNotEqual(Condition.Tangle, Condition.Tangled);
   }

   [TestMethod]
   public void AuthCookieWithAPrefixedUserIdParses()
   {
      const string COOKIE =
         "auth=US_abcdefghijklmnopqrstuvwxyz123-tok12345678901234567; Max-Age=157680000; Domain=.adventure.land; Path=/; Expires=Sat, 21 Jul 2035 12:00:00 GMT";

      var authUser = typeof(AL.APIClient.Model.AuthUser).Assembly
                                                        .GetType("AL.APIClient.Model.AuthUser");

      Assert.IsNotNull(authUser);

      var instance = System.Activator.CreateInstance(
         authUser,
         System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
         null,
         [null, COOKIE],
         null);

      Assert.IsNotNull(instance);

      var userId = authUser.GetProperty("UserID")!.GetValue(instance) as string;
      var authKey = authUser.GetProperty("AuthKey")!.GetValue(instance) as string;

      Assert.AreEqual("US_abcdefghijklmnopqrstuvwxyz123", userId);
      Assert.AreEqual("tok12345678901234567", authKey);
   }
}
