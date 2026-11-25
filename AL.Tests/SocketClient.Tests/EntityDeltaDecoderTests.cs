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

    private const string PLAYER_AND_MONSTER
        = @"{""type"":""xy"",""players"":[{""id"":""p1"",""x"":1}],""monsters"":[{""id"":""m1"",""x"":5,""hp"":100}]}";

    private static string[] GetIds(JsonObject snapshot, string key)
        => snapshot[key]!.AsArray()
                         .Select(node => (string)node!["id"]!)
                         .ToArray();

    [Test]
    public void Snapshot_SplitsPlayersAndMonsters()
    {
        var decoder = new EntityDeltaDecoder();

        decoder.Decode("entities", JsonNode.Parse(PLAYER_AND_MONSTER));

        var snapshot = decoder.Snapshot();

        ((string)snapshot["type"]!).Should()
                                   .Be("all");

        GetIds(snapshot, "players")
            .Should()
            .Equal("p1");

        GetIds(snapshot, "monsters")
            .Should()
            .Equal("m1");
    }

    [Test]
    public void Snapshot_ReturnsClones()
    {
        var decoder = new EntityDeltaDecoder();

        decoder.Decode("entities", JsonNode.Parse(PLAYER_AND_MONSTER));
        decoder.Snapshot()["monsters"]![0]!["hp"] = 1;

        var delta = JsonNode.Parse(@"{""players"":[],""monsters"":[{""id"":""m1"",""~"":[],""x"":9}]}");

        decoder.Decode("entities", delta);

        delta!["monsters"]![0]!.ToJsonString()
                               .Should()
                               .Be(@"{""id"":""m1"",""x"":9,""hp"":100}");
    }

    [Test]
    public void Snapshot_DropsDisappeared()
    {
        var decoder = new EntityDeltaDecoder();

        decoder.Decode("entities", JsonNode.Parse(PLAYER_AND_MONSTER));
        decoder.Decode("disappear", JsonNode.Parse(@"{""id"":""m1""}"));

        decoder.Snapshot()["monsters"]!.AsArray()
               .Should()
               .BeEmpty();
    }

    [Test]
    public void Snapshot_AllFrameReplaces()
    {
        var decoder = new EntityDeltaDecoder();

        decoder.Decode("entities", JsonNode.Parse(PLAYER_AND_MONSTER));
        decoder.Decode("entities", JsonNode.Parse(@"{""type"":""all"",""players"":[{""id"":""p2""}],""monsters"":[]}"));

        var snapshot = decoder.Snapshot();

        GetIds(snapshot, "players")
            .Should()
            .Equal("p2");

        snapshot["monsters"]!.AsArray()
                             .Should()
                             .BeEmpty();
    }
}