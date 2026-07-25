#region
using AL.Core.Definitions;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Covers the response/completion contract - the discriminators an awaiting method uses to tell success from
///     failure. Every frame here is authored from the server source; there is no captured game_response corpus.
/// </summary>
[TestClass]
public class ResponseContractTests
{
   [TestMethod]
   public void CEventParsesBothTheBooleanAndTheNamedShape()
   {
      //cevent is true on dismantle/upgrade but a name on sell/buy/respawn - one shape must not throw the other away
      var flag = TestJson.Socket<GameResponseData>(@"{ ""response"":""dismantle"", ""cevent"":true }");
      var named = TestJson.Socket<GameResponseData>(@"{ ""response"":""gold_received"", ""cevent"":""sell"" }");

      Assert.IsNotNull(flag);
      Assert.IsNotNull(flag.CEvent);
      Assert.IsNotNull(named);
      Assert.AreEqual("sell", named.CEvent);
   }

   [TestMethod]
   public void FailedAndSuccessAreIndependentSoAnExplicitFalseSurvives()
   {
      //success_response only defaults success to true when it is not already false, so in_progress frames keep it
      const string IN_PROGRESS = @"{ ""response"":""data"", ""place"":""fishing"", ""success"":false, ""in_progress"":true }";

      var data = TestJson.Socket<GameResponseData>(IN_PROGRESS);

      Assert.IsNotNull(data);
      Assert.IsFalse(data.Success);
      Assert.IsFalse(data.Failed);
      Assert.IsTrue(data.InProgress);
   }

   [TestMethod]
   public void SkillSuccessIsCarriedByPlaceAndSuccess()
   {
      const string RESPONSE = @"{ ""response"":""data"", ""place"":""massproduction"", ""success"":true }";

      var data = TestJson.Socket<GameResponseData>(RESPONSE);

      Assert.IsNotNull(data);
      Assert.AreEqual(GameResponseType.Data, data.ResponseType);
      Assert.IsTrue(data.Success);
      Assert.IsFalse(data.Failed);
      Assert.AreEqual("massproduction", data.Place);
   }

   [TestMethod]
   public void SkillFailureIsCarriedByFailedAndReason()
   {
      const string RESPONSE = @"{ ""response"":""data"", ""failed"":true, ""reason"":""no_target"", ""place"":""3shot"" }";

      var data = TestJson.Socket<GameResponseData>(RESPONSE);

      Assert.IsNotNull(data);
      Assert.IsTrue(data.Failed);
      Assert.IsFalse(data.Success);
      Assert.AreEqual("no_target", data.Reason);
      Assert.AreEqual("3shot", data.Place);
   }

   /// <summary>
   ///     The multishot success frame is the collapsed action object, which never carries success. Identifying it
   ///     by Place is the only thing that works - gating on Success here would leave the skill permanently broken.
   /// </summary>
   [TestMethod]
   public void MultishotSuccessIsIdentifiedByPlaceBecauseItCarriesNoSuccessFlag()
   {
      const string RESPONSE = @"{
   ""response"":""data"",
   ""place"":""3shot"",
   ""pids"":[""aB3dEf"",""Gh7jKl""],
   ""targets"":[""someMonster"",""otherMonster""]
}";

      var data = TestJson.Socket<GameResponseData>(RESPONSE);

      Assert.IsNotNull(data);
      Assert.AreEqual(GameResponseType.Data, data.ResponseType);
      Assert.IsFalse(data.Success);
      Assert.IsFalse(data.Failed);
      Assert.AreEqual("3shot", data.Place);
      Assert.IsNotNull(data.Targets);
      Assert.AreEqual(2, data.Targets.Length);
      Assert.IsNotNull(data.Pids);
      Assert.AreEqual(2, data.Pids.Length);
   }

   [TestMethod]
   public void StaleUpgradeFrameIsFlaggedSoItCanBeRefused()
   {
      const string RESPONSE = @"{ ""response"":""upgrade_success"", ""stale"":true, ""level"":5, ""num"":3, ""stat_type"":""int"" }";

      var data = TestJson.Socket<GameResponseData>(RESPONSE);

      Assert.IsNotNull(data);
      Assert.IsTrue(data.Stale);
      Assert.AreEqual(5, data.Level);
      Assert.AreEqual(ALAttribute.Int, data.StatType);
   }

   /// <summary>
   ///     Nine distance emits, plus not_ready/inv_size and others, arrive as a bare JSON string rather than an
   ///     object. Only the code survives, so no guard may require an object-only field to recognise them.
   /// </summary>
   [TestMethod]
   public void BareStringFrameYieldsTheCodeAndLeavesEveryDiscriminatorFalse()
   {
      var data = TestJson.Socket<GameResponseData>(@"""distance""");

      Assert.IsNotNull(data);
      Assert.IsFalse(data.Failed);
      Assert.IsFalse(data.Success);
      Assert.IsNull(data.Place);
   }

   /// <summary>
   ///     Turning in a finished monster hunt answers with this instead of monsterhunt_started, and it is the only
   ///     game_response on that path - so nothing else can complete the await.
   /// </summary>
   [TestMethod]
   public void MonsterHuntTurnInIsIdentifiedBySuccessAndPlace()
   {
      const string RESPONSE = @"{ ""response"":""data"", ""place"":""monsterhunt"", ""success"":true, ""completed"":true }";

      var data = TestJson.Socket<GameResponseData>(RESPONSE);

      Assert.IsNotNull(data);
      Assert.AreEqual(GameResponseType.Data, data.ResponseType);
      Assert.IsTrue(data.Success);
      Assert.IsFalse(data.Failed);
      Assert.IsTrue(data.Completed);
      Assert.AreEqual("monsterhunt", data.Place);
   }

   /// <summary>
   ///     Both arrive as bare strings, so they carry no Failed and no Place and the shared default failure arm
   ///     cannot see them. They need an arm keyed on the code alone or the call runs to its 60s timeout.
   /// </summary>
   [TestMethod]
   public void ScrollShortageCodesArriveWithNothingButTheCode()
   {
      var noScroll = TestJson.Socket<GameResponseData>(@"""compound_no_scroll""");
      var scrollQ = TestJson.Socket<GameResponseData>(@"""upgrade_scroll_q""");

      Assert.IsNotNull(noScroll);
      Assert.AreEqual(GameResponseType.CompoundNoScroll, noScroll.ResponseType);
      Assert.IsFalse(noScroll.Failed);
      Assert.IsNull(noScroll.Place);

      Assert.IsNotNull(scrollQ);
      Assert.AreEqual(GameResponseType.UpgradeScrollQ, scrollQ.ResponseType);
      Assert.IsFalse(scrollQ.Failed);
   }

   /// <summary>
   ///     A distance frame names the operation that was too far away. Un-gated, one in-flight call resolves on
   ///     another's failure and short-circuits the frame before the real caller sees it.
   /// </summary>
   [TestMethod]
   public void DistanceFrameNamesTheOperationThatWasTooFar()
   {
      var sell = TestJson.Socket<GameResponseData>(
         @"{ ""response"":""distance"", ""place"":""sell"", ""failed"":true }");

      Assert.IsNotNull(sell);
      Assert.AreEqual(GameResponseType.Distance, sell.ResponseType);
      Assert.AreEqual("sell", sell.Place);
      Assert.AreNotEqual("buy", sell.Place);
   }

   [TestMethod]
   public void AlchemyGoldIsFractional()
   {
      var data = TestJson.Socket<GameResponseData>(@"{ ""response"":""data"", ""gold"":1290.6 }");

      Assert.IsNotNull(data);
      Assert.AreEqual(1290.6f, data.Gold, 0.001f);
   }
}
