#region
using System.Text.Json.Nodes;
using AL.SocketClient;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

public class EntityDeltaDecoderTests
{
    private const string FULL = @"{""type"":""xy"",""players"":[],""monsters"":[{""id"":""1"",""x"":5,""hp"":100,""target"":""bob""}]}";

    [Test]
    public void DeltaRebuildsTheFullPacket()
    {
        var decoder = new EntityDeltaDecoder();

        decoder.Decode("entities", JsonNode.Parse(FULL))
               .Should()
               .BeTrue();

        var delta = JsonNode.Parse(@"{""type"":""xy"",""players"":[],""monsters"":[{""id"":""1"",""~"":[""target""],""x"":9}]}");

        decoder.Decode("entities", delta)
               .Should()
               .BeTrue();

        delta!["monsters"]![0]!.ToJsonString()
                               .Should()
                               .Be(@"{""id"":""1"",""x"":9,""hp"":100}");
    }

    [Test]
    public void DeltaWithoutBaseIsDropped()
    {
        var decoder = new EntityDeltaDecoder();
        var delta = JsonNode.Parse(@"{""type"":""xy"",""players"":[],""monsters"":[{""id"":""1"",""~"":[],""x"":9}]}");

        decoder.Decode("entities", delta)
               .Should()
               .BeFalse();

        delta!["monsters"]!.AsArray()
                           .Should()
                           .BeEmpty();
    }

    [Test]
    public void DisappearForgetsTheBase()
    {
        var decoder = new EntityDeltaDecoder();

        decoder.Decode("entities", JsonNode.Parse(FULL));
        decoder.Decode("disappear", JsonNode.Parse(@"{""id"":""1"",""outside"":true}"));

        decoder.Decode("entities", JsonNode.Parse(@"{""players"":[],""monsters"":[{""id"":""1"",""~"":[],""x"":9}]}"))
               .Should()
               .BeFalse();
    }
}