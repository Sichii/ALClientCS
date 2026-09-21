#region
using AL.Core.Definitions;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The <c>ui</c> frame carries a dozen unrelated casts under one event, each named by a <c>type</c> string the server
///     spells its own way. A member whose spelling is wrong parses as the numeric fallback and draws the wrong thing; a
///     field whose shape is wrong throws, and a throw discards the whole frame in <c>ALSocketClient.OnMessage</c> .
/// </summary>
public class UIDataWireTests
{
    /// <summary>
    ///     The five conditions that can be shrugged off share one shout and differ only by which condition it was.
    /// </summary>
    [Test]
    public void EveryResistSpellingParses()
    {
        var spellings = new Dictionary<string, UIDataType>(StringComparer.Ordinal)
        {
            ["poisoned_resist"] = UIDataType.PoisonedResist,
            ["frozen_resist"] = UIDataType.FrozenResist,
            ["deepfreezed_resist"] = UIDataType.DeepFreezedResist,
            ["burned_resist"] = UIDataType.BurnedResist,
            ["stunned_resist"] = UIDataType.StunnedResist
        };

        foreach ((var wire, var expected) in spellings)
            TestJson.Socket<UIData>($@"{{""type"":""{wire}"",""id"":""a""}}") !.UIDataType
                    .Should()
                    .Be(expected, $"'{wire}' is what the server sends");
    }

    /// <summary>
    ///     The money and transfer types are punctuation on the wire, so none of them can fall back to a member name.
    /// </summary>
    [Test]
    public void TheMoneyAndTransferSpellingsParse()
    {
        var spellings = new Dictionary<string, UIDataType>(StringComparer.Ordinal)
        {
            ["+$$"] = UIDataType.PlayerTrade,
            ["+$p"] = UIDataType.SecondhandsBuy,
            ["+$f"] = UIDataType.LostAndFoundBuy,
            ["+M"] = UIDataType.MerchantSale,
            ["cx_sent"] = UIDataType.CxSent,
            ["4fingers"] = UIDataType.FourFingers,
            ["level_up"] = UIDataType.LevelUp
        };

        foreach ((var wire, var expected) in spellings)
            TestJson.Socket<UIData>($@"{{""type"":""{wire}"",""name"":""Sichi""}}") !.UIDataType
                    .Should()
                    .Be(expected, $"'{wire}' is what the server sends");
    }

    /// <summary>
    ///     A rogue's <c>throw</c> names the item it threw as a bare string (node/server.js:9754), where every other frame that
    ///     carries an item sends an object.
    /// </summary>
    [Test]
    public void TheThrowFrameNamesItsItemAsABareString()
    {
        var data = TestJson.Socket<UIData>(@"{""type"":""throw"",""from"":""Sichi"",""to"":""goo1"",""item"":""shadowstone""}");

        data.Should()
            .NotBeNull();

        data.UIDataType
            .Should()
            .Be(UIDataType.Throw);

        data.Item !.Name
            .Should()
            .Be("shadowstone");

        data.Item
            .ContainsData
            .Should()
            .BeFalse();
    }

    /// <summary>
    ///     The same field as an object, which is the shape every other item-carrying frame uses.
    /// </summary>
    [Test]
    public void TheSameFieldStillParsesAsAnObject()
    {
        var data = TestJson.Socket<UIData>(@"{""type"":""throw"",""item"":{""name"":""shadowstone"",""q"":3}}");

        data.Should()
            .NotBeNull();

        data.Item !.Name
            .Should()
            .Be("shadowstone");

        data.Item
            .Quantity
            .Should()
            .Be(3);

        data.Item
            .ContainsData
            .Should()
            .BeTrue();
    }

    /// <summary>
    ///     <c>mult</c> and <c>amount</c> are whole numbers on the server today. Read as floats anyway, because a fractional
    ///     one landing on an integer property throws and the frame is discarded rather than drawn.
    /// </summary>
    [Test]
    public void AFractionalCountDoesNotDropTheFrame()
    {
        var level = TestJson.Socket<UIData>(@"{""type"":""mlevel"",""id"":""goo1"",""mult"":-1.5}");

        level.Should()
             .NotBeNull();

        level.Mult
             .Should()
             .Be(-1.5f);

        var restore = TestJson.Socket<UIData>(@"{""type"":""restore_mp"",""name"":""Sichi"",""amount"":22.5}");

        restore.Should()
               .NotBeNull();

        restore.Amount
               .Should()
               .Be(22.5f);
    }

    /// <summary>
    ///     The crowd casts, which name the caster in <c>name</c> and everything the swing reached in <c>ids</c> .
    /// </summary>
    [Test]
    public void TheCrowdCastSpellingsParse()
    {
        var spellings = new Dictionary<string, UIDataType>(StringComparer.Ordinal)
        {
            ["cleave"] = UIDataType.Cleave,
            ["agitate"] = UIDataType.Agitate,
            ["shadowstrike"] = UIDataType.ShadowStrike,
            ["fanofknives"] = UIDataType.FanOfKnives,
            ["track"] = UIDataType.Track
        };

        foreach ((var wire, var expected) in spellings)
        {
            var data = TestJson.Socket<UIData>($@"{{""type"":""{wire}"",""name"":""Sichi"",""ids"":[""a"",""b""]}}");

            data!.UIDataType
                 .Should()
                 .Be(expected, $"'{wire}' is what the server sends");

            data.Ids
                .Should()
                .BeEquivalentTo("a", "b");
        }
    }

    /// <summary>
    ///     The single-receiver casts the bench and the monster loops send.
    /// </summary>
    [Test]
    public void TheSingleReceiverSpellingsParse()
    {
        var spellings = new Dictionary<string, UIDataType>(StringComparer.Ordinal)
        {
            ["disengage"] = UIDataType.Disengage,
            ["mlevel"] = UIDataType.MLevel,
            ["mheal"] = UIDataType.MHeal,
            ["magiport"] = UIDataType.Magiport,
            ["reflection"] = UIDataType.Reflection,
            ["alchemy"] = UIDataType.Alchemy,
            ["mcourage"] = UIDataType.MCourage,
            ["mfrenzy"] = UIDataType.MFrenzy,
            ["huntersmark"] = UIDataType.HuntersMark,
            ["dampened"] = UIDataType.Dampened,
            ["restore_mp"] = UIDataType.RestoreMP,
            ["throw"] = UIDataType.Throw
        };

        foreach ((var wire, var expected) in spellings)
            TestJson.Socket<UIData>($@"{{""type"":""{wire}"",""id"":""a""}}") !.UIDataType
                    .Should()
                    .Be(expected, $"'{wire}' is what the server sends");
    }
}