#region
using AL.Client.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

public class GameResponseLedgerFieldsTests
{
    [Test]
    public void ExactGoldSurvivesAboveFloatPrecision()
    {
        var data = TestJson.Socket<GameResponseData>(@"{ ""response"":""gold_received"", ""gold"":20000003 }");

        data.Should()
            .NotBeNull();

        data.Gold
            .Should()
            .Be(20000003d);
    }

    [Test]
    public void SoldQuantityIsRead()
    {
        var data = TestJson.Socket<GameResponseData>(
            @"{ ""response"":""gold_received"", ""gold"":300, ""item"":{ ""name"":""beewings"", ""q"":3 }, ""cevent"":""sell"" }");

        data.Should()
            .NotBeNull();

        data.Item!.Quantity
            .Should()
            .Be(3);
    }

    [Test]
    [Arguments("donate_thx", GameResponseType.DonateThx)]
    [Arguments("donate_gum", GameResponseType.DonateGum)]
    [Arguments("donate_low", GameResponseType.DonateLow)]
    public void DonationRepliesParse(string wireName, GameResponseType expected)
    {
        var data = TestJson.Socket<GameResponseData>($@"""{wireName}""");

        data.Should()
            .NotBeNull();

        data.ResponseType
            .Should()
            .Be(expected);
    }
}