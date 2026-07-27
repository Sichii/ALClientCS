#region
using AL.Core.Definitions;
using AL.Core.Helpers;
using AL.SocketClient.Definitions;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
using Condition = AL.Core.Definitions.Condition;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     Covers the deserialization tolerance work. Every case here is a real wire shape that previously threw, and a throw
///     is not a local failure - it escapes into ALSocketClient.OnAny, which swallows it and discards the entire socket
///     frame.
/// </summary>
public class ToleranceTests
{
    [Test]
    public void AnUnknownEnumDoesNotTruncateTheRestOfTheObject()
    {
        //the tolerant converter must leave the reader aligned or every field after the bad one is lost
        const string DROP = @"{ ""id"":""theId"", ""chest"":""nonexistent"", ""x"":123.5, ""y"":456.5 }";

        var obj = TestJson.Socket<DropData>(DROP);

        obj.Should()
           .NotBeNull();

        obj.ChestType
           .Should()
           .Be(ChestType.Unknown);

        obj.Id
           .Should()
           .Be("theId");

        obj.X
           .Should()
           .Be(123.5f);

        obj.Y
           .Should()
           .Be(456.5f);
    }

    [Test]
    public void DeadPlayerRipIsEitherTrueOrAGravestoneName()
    {
        var boolean = TestJson.Socket<Player>(@"{ ""id"":""a"", ""rip"":true }");
        var cosmetic = TestJson.Socket<Player>(@"{ ""id"":""b"", ""rip"":""grave1"" }");
        var alive = TestJson.Socket<Player>(@"{ ""id"":""c"", ""rip"":false }");

        boolean.Should()
               .NotBeNull();

        boolean.RIP
               .Should()
               .BeTrue();

        cosmetic.Should()
                .NotBeNull();

        cosmetic.RIP
                .Should()
                .BeTrue();

        alive.Should()
             .NotBeNull();

        alive.RIP
             .Should()
             .BeFalse();
    }

    [Test]
    public void EvalArrivesAsABareString()
    {
        var bare = TestJson.Socket<EvalData>(@"""egg_splash(0,0)""");
        var wrapped = TestJson.Socket<EvalData>(@"{ ""code"":""egg_splash(0,0)"" }");

        bare.Should()
            .NotBeNull();

        bare.Code
            .Should()
            .Be("egg_splash(0,0)");

        wrapped.Should()
               .NotBeNull();

        wrapped.Code
               .Should()
               .Be("egg_splash(0,0)");
    }

    [Test]
    public void EveryEditedEnumParsesWithoutAKeyCollision()
    {
        //EnumHelper builds a case-insensitive dictionary with .Add, which throws on a duplicate key,
        //and Heal/Healed/Healing and Stone/Stoned are one typo away from colliding
        EnumHelper.TryParse<Condition>("patronsgrace", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<Condition>("damage_received", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<Condition>("healing", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<Condition>("healed", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<Condition>("stone", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<Condition>("stoned", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<ChestType>("chestp", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<KeyType>("spiderkey", out _)
                  .Should()
                  .BeTrue();

        EnumHelper.TryParse<Stand>("stand1", out _)
                  .Should()
                  .BeTrue();

        //distinct members, not aliases of one another
        Condition.Healed
                 .Should()
                 .NotBe(Condition.Healing);

        Condition.Stoned
                 .Should()
                 .NotBe(Condition.Stone);

        Condition.Tangled
                 .Should()
                 .NotBe(Condition.Tangle);
    }

    [Test]
    public void ItemTitleArrivesAsAStringOrFalseRatherThanAPrediction()
    {
        //server.js:1868/:6689 set p="shiny", :11521 sets p="legacy", :5953 can yield false
        var shiny = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":""shiny"" }");
        var plain = TestJson.Socket<Item>(@"{ ""name"":""fireblade"", ""p"":false }");

        var upgrading = TestJson.Socket<Item>(
            @"{ ""name"":""placeholder"", ""p"":{ ""chance"":0.6, ""name"":""fireblade"", ""level"":2, ""scroll"":""scroll1"" } }");

        shiny.Should()
             .NotBeNull();

        (shiny.Prediction?.Title).Should()
                                 .Be("shiny");

        plain.Should()
             .NotBeNull();

        plain.Prediction
             .Should()
             .BeNull();

        upgrading.Should()
                 .NotBeNull();

        upgrading.Prediction!.ContainsData
                 .Should()
                 .BeTrue();

        upgrading.Prediction
                 .Name
                 .Should()
                 .Be("fireblade");
    }

    [Test]
    public void PartyListArrivesAsFalseWhenThePartyDissolves()
    {
        var dissolved = TestJson.Socket<PartyUpdateData>(@"{ ""list"":false }");
        var populated = TestJson.Socket<PartyUpdateData>(@"{ ""list"":[""Alice"",""Bob""] }");

        dissolved.Should()
                 .NotBeNull();

        dissolved.MemberNames
                 .Count
                 .Should()
                 .Be(0);

        populated.Should()
                 .NotBeNull();

        populated.MemberNames
                 .Count
                 .Should()
                 .Be(2);
    }

    [Test]
    public void UnknownChestSpriteDoesNotDropTheDrop()
    {
        var known = TestJson.Socket<DropData>(@"{ ""id"":""1"", ""chest"":""chest8"" }");
        var unknown = TestJson.Socket<DropData>(@"{ ""id"":""2"", ""chest"":""chest_from_the_future"" }");

        known.Should()
             .NotBeNull();

        known.ChestType
             .Should()
             .Be(ChestType.Chest8);

        unknown.Should()
               .NotBeNull();

        unknown.ChestType
               .Should()
               .Be(ChestType.Unknown);
    }

    [Test]
    public void UnknownConditionKeyIsSkippedAndKnownOnesSurvive()
    {
        //patronsgrace is stamped on every player on a blessed server, and used to kill the start payload
        const string ENTITY = @"{
   ""id"":""someMonster"",
   ""s"":{ ""stone"":{ ""ms"":5000 }, ""patronsgrace"":{ ""ms"":1000 }, ""__nope"":{ ""ms"":1 } }
}";

        var obj = TestJson.Socket<Monster>(ENTITY);

        obj.Should()
           .NotBeNull();

        obj.Conditions
           .ContainsKey(Condition.Stone)
           .Should()
           .BeTrue();

        obj.Conditions
           .ContainsKey(Condition.PatronsGrace)
           .Should()
           .BeTrue();

        obj.Conditions
           .Count
           .Should()
           .Be(2);
    }

    [Test]
    public void UnknownGameResponseCodeDegradesToUnknown()
    {
        var fromObject = TestJson.Socket<GameResponseData>(@"{ ""response"":""not_a_real_code_at_all"" }");
        var fromString = TestJson.Socket<GameResponseData>(@"""distance""");

        fromObject.Should()
                  .NotBeNull();

        fromObject.ResponseType
                  .Should()
                  .Be(GameResponseType.Unknown);

        fromString.Should()
                  .NotBeNull();
    }
}