#region
using AL.Client.Helpers;
using AL.SocketClient.Model;
using AL.SocketClient.SocketModel;
using FluentAssertions;
#endregion

namespace AL.Tests.SocketClient.Tests;

/// <summary>
///     The daily dungeon's run state rides the cave event. Every frame but "ended" carries the whole state; the frames
///     here are authored from what the game's client reads off it, since the server source for the dungeon is not public.
/// </summary>
public class CaveDataTests
{
    private const string CHOICE_FRAME = """
        {
          "type": "choice",
          "state": {
            "run": "6f1e2d3c4b5a69788796a5b4",
            "floor": 1,
            "server_time": 1789000000000,
            "expires": 1789001440000,
            "paused": true,
            "paused_at": 1789000300000,
            "remaining_ms": 1140000,
            "gold": 8500,
            "amber": 3,
            "doors": [
              { "to": "zone_6f1e2d3c4b5a69788796a5b4_2", "locked": true, "down": true, "x": 380, "y": 200 },
              { "to": "main", "locked": false, "down": false, "x": 200, "y": 380 }
            ],
            "objectives": [
              { "name": "Bat Roost", "kind": "farm", "floor": 1, "required": true, "done": false, "waves": 1, "x": 120, "y": 90 },
              { "name": "Lockbreaker", "kind": "boss", "floor": 1, "required": true, "done": true, "x": 300, "y": 300 }
            ],
            "supplies": ["tool", "lamp"],
            "rewards": [
              { "id": 4, "where": "purse", "gold": 1500, "amber": 1 },
              { "id": 5, "where": "inventory", "item": { "name": "cave_locktooth", "q": 1 }, "recipient": "Ranger", "slot": 12 }
            ],
            "hunts": [ { "kills": 2, "count": 6, "deadline": 1789000500000 } ],
            "practice": [],
            "choice": {
              "id": "c07",
              "resolved": false,
              "deadline": 1789000360000,
              "title": "The Unclaimed Parcel",
              "text": "This box has been here for days.",
              "options": [
                { "id": "e02_0", "label": "Cut the seal.", "cost": 0 },
                { "id": "e02_1", "label": "Pry the hinges off quietly.", "unavailable": "Needs a tool" },
                { "id": "die", "label": "Bet 6 Amber", "cost": 2000, "amber": 6 }
              ],
              "votes": { "Ranger": "e02_0" },
              "people": [ { "name": "Ilex", "hp": 1200, "attack": 35, "cargo": [ { "name": "cave_parcel", "q": 1 } ] } ],
              "fallback": "Leave it.",
              "shop": { "name": "cave_loaded_die", "price": 10000, "room": "r3", "sold": false, "nearby": true }
            }
          }
        }
        """;

    [Test]
    public void AChoiceFrameCarriesTheWholeState()
    {
        var data = TestJson.Socket<CaveData>(CHOICE_FRAME)!;

        data.Type
            .Should()
            .Be("choice");

        data.Ended
            .Should()
            .BeFalse();

        var state = data.State!;

        state.Run
             .Should()
             .Be("6f1e2d3c4b5a69788796a5b4");

        state.Floor
             .Should()
             .Be(1);

        state.Paused
             .Should()
             .BeTrue();

        state.RemainingMs
             .Should()
             .Be(1140000);

        state.Expires
             .Should()
             .Be(1789001440000);

        state.Gold
             .Should()
             .Be(8500);

        state.Amber
             .Should()
             .Be(3);

        state.Doors
             .Should()
             .HaveCount(2);

        state.Doors[0]
             .Should()
             .BeEquivalentTo(
                 new CaveDoor
                 {
                     To = "zone_6f1e2d3c4b5a69788796a5b4_2",
                     Locked = true,
                     Down = true,
                     X = 380,
                     Y = 200
                 });

        state.Objectives[0]
             .Kind
             .Should()
             .Be("farm");

        state.Objectives[1]
             .Done
             .Should()
             .BeTrue();

        state.Supplies
             .Should()
             .Equal("tool", "lamp");

        state.Rewards[1]
             .Item!
             .Name
             .Should()
             .Be("cave_locktooth");

        state.Rewards[1]
             .Recipient
             .Should()
             .Be("Ranger");

        state.Hunts[0]
             .Count
             .Should()
             .Be(6);

        var choice = state.Choice!;

        choice.Id
              .Should()
              .Be("c07");

        choice.Resolved
              .Should()
              .BeFalse();

        choice.Deadline
              .Should()
              .Be(1789000360000);

        choice.Options
              .Should()
              .HaveCount(3);

        choice.Options[1]
              .Unavailable
              .Should()
              .Be("Needs a tool");

        choice.Options[2]
              .Amber
              .Should()
              .Be(6);

        choice.Votes
              .Should()
              .ContainKey("Ranger")
              .WhoseValue
              .Should()
              .Be("e02_0");

        choice.People[0]
              .Cargo![0]
              .Name
              .Should()
              .Be("cave_parcel");

        choice.Shop!
              .Room
              .Should()
              .Be("r3");
    }

    /// <summary>
    ///     A character frame lands on the persistent Character by shallow merge, which assigns every writable property off a
    ///     freshly deserialized frame - so a property the frame never carries would be nulled on every tick unless the merge
    ///     is told to leave it.
    /// </summary>
    [Test]
    public void ACharacterFrameMergeLeavesTheCaveStateInPlace()
    {
        var character = TestJson.Socket<Character>("""{ "id": "Ranger", "x": 0, "y": 0 }""")!;
        character.Cave = TestJson.Socket<CaveData>(CHOICE_FRAME)!.State;

        ShallowMerge<Character>.Merge(TestJson.Socket<Character>("""{ "id": "Ranger", "x": 5, "y": 5 }""")!, character);

        character.Cave
                 .Should()
                 .NotBeNull();
    }

    [Test]
    public void AChatFrameCarriesTheLineAndTheState()
    {
        var data = TestJson.Socket<CaveData>(
            """{ "type": "chat", "chat": { "name": "Pip", "text": "Excuse me! Heavy bag." }, "state": { "run": "6f1e2d3c4b5a69788796a5b4", "floor": 0, "paused": false, "gold": 0, "amber": 0 } }""")!;

        data.Chat!
            .Name
            .Should()
            .Be("Pip");

        data.State!
            .Paused
            .Should()
            .BeFalse();
    }

    [Test]
    public void AnEndedFrameNeedNotCarryAState()
    {
        var data = TestJson.Socket<CaveData>("""{ "type": "ended" }""")!;

        data.Ended
            .Should()
            .BeTrue();

        data.State
            .Should()
            .BeNull();
    }

    [Test]
    public void TheInfoReplyCarriesTheVisit()
    {
        var data = TestJson.Socket<GameResponseData>(
            """{ "place": "interaction", "request_id": "abc", "visit": { "available": false, "unlimited": false, "resets": 1789056000000, "home": "US II", "server_time": 1789000000000 } }""")!;

        data.Visit!
            .Available
            .Should()
            .BeFalse();

        data.Visit
            .Resets
            .Should()
            .Be(1789056000000);

        data.Visit
            .Home
            .Should()
            .Be("US II");
    }

    [Test]
    public void ATalkReplyCarriesTheChat()
    {
        var data = TestJson.Socket<GameResponseData>("""{ "place": "interaction", "request_id": "abc", "chat": { "name": "Bram", "text": "Mind the bats." } }""")!;

        data.Chat!
            .Text
            .Should()
            .Be("Mind the bats.");
    }

    [Test]
    public void ACaveChestReceiptCarriesThePurseDelta()
    {
        var data = TestJson.Socket<ChestOpenedData>("""{ "id": "x", "opener": "Ranger", "gold": 0, "items": [], "cave": { "gold": 1500, "amber": 1 } }""")!;

        data.Cave!
            .Gold
            .Should()
            .Be(1500);

        data.Cave
            .Amber
            .Should()
            .Be(1);
    }

    [Test]
    public void ATravelerCarriesItsCaveRole()
    {
        var monster = TestJson.Socket<Monster>("""{ "id": "12", "type": "cave_npc", "x": 1, "y": 2, "hp": 1200, "level": 3, "cave": { "side": "neutral", "room": "r2", "citizen": true } }""")!;

        monster.Cave!
               .Side
               .Should()
               .Be("neutral");

        monster.Cave
               .Room
               .Should()
               .Be("r2");

        monster.Cave
               .Citizen
               .Should()
               .BeTrue();
    }
}
