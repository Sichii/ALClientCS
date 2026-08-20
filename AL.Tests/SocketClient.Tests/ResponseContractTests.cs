#region
using AL.Core.Definitions;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Covers the response/completion contract - the discriminators an awaiting method uses to tell success from failure.
///     Every frame here is authored from the server source; there is no captured game_response corpus.
/// </summary>
public class ResponseContractTests
{
    [Test]
    public void AlchemyGoldIsFractional()
    {
        var data = TestJson.Socket<GameResponseData>(@"{ ""response"":""data"", ""gold"":1290.6 }");

        data.Should()
            .NotBeNull();

        data.Gold
            .Should()
            .BeApproximately(1290.6f, 0.001f);
    }

    /// <summary>
    ///     Nine distance emits, plus not_ready/inv_size and others, arrive as a bare JSON string rather than an object. Only
    ///     the code survives, so no guard may require an object-only field to recognise them.
    /// </summary>
    [Test]
    public void BareStringFrameYieldsTheCodeAndLeavesEveryDiscriminatorFalse()
    {
        var data = TestJson.Socket<GameResponseData>(@"""distance""");

        data.Should()
            .NotBeNull();

        data.Failed
            .Should()
            .BeFalse();

        data.Success
            .Should()
            .BeFalse();

        data.Place
            .Should()
            .BeNull();
    }

    [Test]
    public void CEventParsesBothTheBooleanAndTheNamedShape()
    {
        //cevent is true on dismantle/upgrade but a name on sell/buy/respawn - one shape must not throw the other away
        var flag = TestJson.Socket<GameResponseData>(@"{ ""response"":""dismantle"", ""cevent"":true }");
        var named = TestJson.Socket<GameResponseData>(@"{ ""response"":""gold_received"", ""cevent"":""sell"" }");

        flag.Should()
            .NotBeNull();

        flag.CEvent
            .Should()
            .NotBeNull();

        named.Should()
             .NotBeNull();

        named.CEvent
             .Should()
             .Be("sell");
    }

    /// <summary>
    ///     A distance frame names the operation that was too far away. Un-gated, one in-flight call resolves on another's
    ///     failure and short-circuits the frame before the real caller sees it.
    /// </summary>
    [Test]
    public void DistanceFrameNamesTheOperationThatWasTooFar()
    {
        var sell = TestJson.Socket<GameResponseData>(@"{ ""response"":""distance"", ""place"":""sell"", ""failed"":true }");

        sell.Should()
            .NotBeNull();

        sell.ResponseType
            .Should()
            .Be(GameResponseType.Distance);

        sell.Place
            .Should()
            .Be("sell");

        sell.Place
            .Should()
            .NotBe("buy");
    }

    /// <summary>
    ///     The two refusals the lost and found can produce, in the two different shapes the server builds them in.
    ///     Both used to arrive as an unrecognised code, which is a caller waiting out the whole network timeout at
    ///     the counter rather than being told no.
    /// </summary>
    [Test]
    public void LostAndFoundRefusalsAreBothRecognised()
    {
        //the listing's refusal is emitted by hand (node/server.js:6891), so it is a bare string like distance's
        var locked = TestJson.Socket<GameResponseData>(@"""lostandfound_donate""");

        locked.Should()
              .NotBeNull();

        locked.ResponseType
              .Should()
              .Be(GameResponseType.LostAndFoundDonate);

        //the buy's goes through fail_response with an object second argument (node/server.js:7134), so place falls
        //back to the handler's own name and failed is set
        var sick = TestJson.Socket<GameResponseData>(@"{ ""goblin"":true, ""response"":""cant_when_sick"", ""place"":""sbuy"", ""failed"":true }");

        sick.Should()
            .NotBeNull();

        sick.ResponseType
            .Should()
            .Be(GameResponseType.CantWhenSick);

        sick.Failed
            .Should()
            .BeTrue();
    }

    [Test]
    public void FailedAndSuccessAreIndependentSoAnExplicitFalseSurvives()
    {
        //success_response only defaults success to true when it is not already false, so in_progress frames keep it
        const string IN_PROGRESS = @"{ ""response"":""data"", ""place"":""fishing"", ""success"":false, ""in_progress"":true }";

        var data = TestJson.Socket<GameResponseData>(IN_PROGRESS);

        data.Should()
            .NotBeNull();

        data.Success
            .Should()
            .BeFalse();

        data.Failed
            .Should()
            .BeFalse();

        data.InProgress
            .Should()
            .BeTrue();
    }

    /// <summary>
    ///     Turning in a finished monster hunt answers with this instead of monsterhunt_started, and it is the only
    ///     game_response on that path - so nothing else can complete the await.
    /// </summary>
    [Test]
    public void MonsterHuntTurnInIsIdentifiedBySuccessAndPlace()
    {
        const string RESPONSE = @"{ ""response"":""data"", ""place"":""monsterhunt"", ""success"":true, ""completed"":true }";

        var data = TestJson.Socket<GameResponseData>(RESPONSE);

        data.Should()
            .NotBeNull();

        data.ResponseType
            .Should()
            .Be(GameResponseType.Data);

        data.Success
            .Should()
            .BeTrue();

        data.Failed
            .Should()
            .BeFalse();

        data.Completed
            .Should()
            .BeTrue();

        data.Place
            .Should()
            .Be("monsterhunt");
    }

    /// <summary>
    ///     The multishot success frame is the collapsed action object, which never carries success. Identifying it by Place is
    ///     the only thing that works - gating on Success here would leave the skill permanently broken.
    /// </summary>
    [Test]
    public void MultishotSuccessIsIdentifiedByPlaceBecauseItCarriesNoSuccessFlag()
    {
        const string RESPONSE = @"{
   ""response"":""data"",
   ""place"":""3shot"",
   ""pids"":[""aB3dEf"",""Gh7jKl""],
   ""targets"":[""someMonster"",""otherMonster""]
}";

        var data = TestJson.Socket<GameResponseData>(RESPONSE);

        data.Should()
            .NotBeNull();

        data.ResponseType
            .Should()
            .Be(GameResponseType.Data);

        data.Success
            .Should()
            .BeFalse();

        data.Failed
            .Should()
            .BeFalse();

        data.Place
            .Should()
            .Be("3shot");

        data.Targets
            .Should()
            .NotBeNull();

        data.Targets
            .Length
            .Should()
            .Be(2);

        data.Pids
            .Should()
            .NotBeNull();

        data.Pids
            .Length
            .Should()
            .Be(2);
    }

    /// <summary>
    ///     Both arrive as bare strings, so they carry no Failed and no Place and the shared default failure arm cannot see
    ///     them. They need an arm keyed on the code alone or the call runs to its 60s timeout.
    /// </summary>
    [Test]
    public void ScrollShortageCodesArriveWithNothingButTheCode()
    {
        var noScroll = TestJson.Socket<GameResponseData>(@"""compound_no_scroll""");
        var scrollQ = TestJson.Socket<GameResponseData>(@"""upgrade_scroll_q""");

        noScroll.Should()
                .NotBeNull();

        noScroll.ResponseType
                .Should()
                .Be(GameResponseType.CompoundNoScroll);

        noScroll.Failed
                .Should()
                .BeFalse();

        noScroll.Place
                .Should()
                .BeNull();

        scrollQ.Should()
               .NotBeNull();

        scrollQ.ResponseType
               .Should()
               .Be(GameResponseType.UpgradeScrollQ);

        scrollQ.Failed
               .Should()
               .BeFalse();
    }

    [Test]
    public void SkillFailureIsCarriedByFailedAndReason()
    {
        const string RESPONSE = @"{ ""response"":""data"", ""failed"":true, ""reason"":""no_target"", ""place"":""3shot"" }";

        var data = TestJson.Socket<GameResponseData>(RESPONSE);

        data.Should()
            .NotBeNull();

        data.Failed
            .Should()
            .BeTrue();

        data.Success
            .Should()
            .BeFalse();

        data.Reason
            .Should()
            .Be("no_target");

        data.Place
            .Should()
            .Be("3shot");
    }

    [Test]
    public void SkillSuccessIsCarriedByPlaceAndSuccess()
    {
        const string RESPONSE = @"{ ""response"":""data"", ""place"":""massproduction"", ""success"":true }";

        var data = TestJson.Socket<GameResponseData>(RESPONSE);

        data.Should()
            .NotBeNull();

        data.ResponseType
            .Should()
            .Be(GameResponseType.Data);

        data.Success
            .Should()
            .BeTrue();

        data.Failed
            .Should()
            .BeFalse();

        data.Place
            .Should()
            .Be("massproduction");
    }

    [Test]
    public void StaleUpgradeFrameIsFlaggedSoItCanBeRefused()
    {
        const string RESPONSE = @"{ ""response"":""upgrade_success"", ""stale"":true, ""level"":5, ""num"":3, ""stat_type"":""int"" }";

        var data = TestJson.Socket<GameResponseData>(RESPONSE);

        data.Should()
            .NotBeNull();

        data.Stale
            .Should()
            .BeTrue();

        data.Level
            .Should()
            .Be(5);

        data.StatType
            .Should()
            .Be(ALAttribute.Int);
    }
}