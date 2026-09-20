#region
using System.Text.Json;
using AL.Core.Json;
using AL.Data.Events;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The daily dungeon's camp table rides events.dreams.camps: one list per floor, each camp with its packs in spawn
///     order, a pack being the monsters one wave holds. A reply's outcomes ride beside it on the encounter table, and the
///     fight an outcome starts is the same [monster, count] pair a pack entry is.
/// </summary>
public class GEventCampsTests
{
    private const string DREAMS = """
                                  {
                                    "name": "Cave of Many Dreams",
                                    "type": "daily",
                                    "duration": 1440,
                                    "camps": [
                                      [
                                        { "name": "Amber Nest", "packs": [[["cave_rat", 6], ["cave_spider", 2]], [["cave_bat", 6]]] },
                                        { "name": "Bat Roost", "packs": [[["cave_bat", 8]]] }
                                      ],
                                      [
                                        { "name": "Guard Outpost", "packs": [[["cave_guard", 4], ["cave_wolf", 2]]] }
                                      ]
                                    ],
                                    "encounters": [
                                      {
                                        "id": "e07",
                                        "name": "Borrow a Uniform",
                                        "kind": "disguise",
                                        "options": [
                                          {
                                            "id": "e07_1",
                                            "effect": "venture",
                                            "label": "Steal the captain's coat",
                                            "outcomes": [
                                              { "weight": 1, "text": "The captain has not noticed. Yet.", "flags": ["truce"], "gold": 2000 },
                                              { "weight": 1, "text": "That coat has a bell sewn into it.", "fight": ["cave_guard", 3] }
                                            ]
                                          },
                                          { "id": "e07_3", "effect": "leave", "label": "Walk on" }
                                        ]
                                      }
                                    ]
                                  }
                                  """;

    [Test]
    public void AMalformedPairReadsAsNothing()
    {
        var json = """{ "name": "x", "packs": [[["cave_rat"], 7, ["cave_bat", "two"]]] }""";
        var camp = JsonSerializer.Deserialize<GCamp>(json, ALJson.Options)!;

        camp.Packs[0]
            .Should()
            .HaveCount(3)
            .And
            .AllSatisfy(
                entry => entry.Should()
                              .BeNull(),
                "a pair missing its count, a bare number and a count that is not a number all read as nothing rather than throwing");
    }

    [Test]
    public void AMonsterCountWritesBackAsThePair()
    {
        var json = JsonSerializer.Serialize(
            new GMonsterCount
            {
                Monster = "cave_wolf",
                Count = 5
            },
            ALJson.Options);

        json.Should()
            .Be("""["cave_wolf",5]""");
    }

    [Test]
    public void APackIsTheMonstersOneWaveHolds()
    {
        var amberNest = Dreams()
            .Camps![0][0];

        amberNest.Packs
                 .Should()
                 .HaveCount(2, "two waves were given");

        amberNest.Packs[0]
                 .Select(entry => (entry!.Monster, entry.Count))
                 .Should()
                 .Equal(("cave_rat", 6), ("cave_spider", 2));

        amberNest.Packs[1]
                 .Single()
                 .Should()
                 .BeEquivalentTo(
                     new GMonsterCount
                     {
                         Monster = "cave_bat",
                         Count = 6
                     });
    }

    [Test]
    public void AReplyWithoutOutcomesHasNone()
        => Dreams()
           .Encounters![0]
           .Options[1]
           .Outcomes
           .Should()
           .BeEmpty();

    [Test]
    public void AReplysOutcomesCarryTheFightTheyCanStart()
    {
        var steal = Dreams()
                    .Encounters![0]
                    .Options[0];

        steal.Outcomes
             .Should()
             .HaveCount(2);

        steal.Outcomes[0]
             .Fight
             .Should()
             .BeNull("the first outcome starts nothing");

        steal.Outcomes[0]
             .Gold
             .Should()
             .Be(2000);

        steal.Outcomes[1]
             .Fight
             .Should()
             .BeEquivalentTo(
                 new GMonsterCount
                 {
                     Monster = "cave_guard",
                     Count = 3
                 });
    }

    [Test]
    public void AnEventWithoutCampsHasNone()
    {
        var gEvent = JsonSerializer.Deserialize<GEvent>("""{ "name": "Egg Hunt", "type": "event" }""", ALJson.Options)!;

        gEvent.Camps
              .Should()
              .BeNull();
    }

    [Test]
    public void CampsComeOneListPerFloorInWireOrder()
    {
        var camps = Dreams()
            .Camps!;

        camps.Should()
             .HaveCount(2);

        camps[0]
            .Select(camp => camp.Name)
            .Should()
            .Equal("Amber Nest", "Bat Roost");

        camps[1][0]
            .Name
            .Should()
            .Be("Guard Outpost");
    }

    private static GEvent Dreams() => JsonSerializer.Deserialize<GEvent>(DREAMS, ALJson.Options)!;
}