#region
using System.Text.Json;
using AL.Core.Json;
using AL.Data.Events;
using FluentAssertions;
#endregion

namespace AL.Tests.Data.Tests;

/// <summary>
///     The daily dungeon's encounter table rides events.dreams.encounters: one entry per encounter, each with the replies
///     the party can vote for and what each reply does. The vote picker matches an open choice against it by option ids.
/// </summary>
public class GEventEncountersTests
{
    private const string DREAMS = """
        {
          "name": "Cave of Many Dreams",
          "type": "daily",
          "duration": 1440,
          "encounters": [
            {
              "id": "e01",
              "name": "Dice in a Tin Cup",
              "actor": "dice_operator",
              "group": "mixed",
              "kind": "dice",
              "text": "Pick a bet.",
              "options": [
                { "id": "small", "label": "Bet 2,000 shared gold", "effect": "dice", "cost": 2000, "win": 4000, "offer": true },
                { "id": "die", "label": "Bet 6 Amber for a Loaded Die", "effect": "die", "amber": 6 },
                { "id": "leave", "label": "Walk on", "effect": "leave" }
              ]
            },
            {
              "id": "e30",
              "name": "A Tool Left Behind",
              "actor": "smith",
              "group": "left",
              "kind": "tool",
              "options": [
                { "id": "use", "label": "Use the tool", "effect": "use_tool", "needs": "tool" },
                { "id": "leave", "label": "Leave it", "effect": "leave" }
              ]
            }
          ]
        }
        """;

    [Test]
    public void AnEventWithoutEncountersHasNone()
    {
        var gEvent = JsonSerializer.Deserialize<GEvent>("""{ "name": "Egg Hunt", "type": "event" }""", ALJson.Options)!;

        gEvent.Encounters
              .Should()
              .BeNull();
    }

    [Test]
    public void ParsesEveryEncounterAndItsOptions()
    {
        var gEvent = JsonSerializer.Deserialize<GEvent>(DREAMS, ALJson.Options)!;

        gEvent.Encounters
              .Should()
              .HaveCount(2);

        var dice = gEvent.Encounters![0];

        dice.Id
            .Should()
            .Be("e01");

        dice.Kind
            .Should()
            .Be("dice");

        dice.Name
            .Should()
            .Be("Dice in a Tin Cup");

        dice.Actor
            .Should()
            .Be("dice_operator");

        dice.Options
            .Select(option => option.Id)
            .Should()
            .Equal("small", "die", "leave");

        var small = dice.Options[0];

        small.Effect
             .Should()
             .Be("dice");

        small.Cost
             .Should()
             .Be(2000);

        small.Amber
             .Should()
             .Be(0);

        small.Offer
             .Should()
             .BeTrue();

        dice.Options[1]
            .Amber
            .Should()
            .Be(6);

        gEvent.Encounters[1]
              .Options[0]
              .Needs
              .Should()
              .Be("tool");
    }
}
