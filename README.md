# ALClientCS

A headless C# client for the browser game [Adventure.Land](https://adventure.land).

It is not a one-to-one copy of the official client. Many names are changed to say what they hold. A monster's `type` is
`Monster.Name`, `G` is `GameData`, and `smart_move` is `SmartMoveAsync`. The game's JavaScript is loosely typed, so one
class here often holds the fields of several of the game's objects.

The client is split into libraries that build on each other. You can use each one on its own.

```
        ┌────> AL.Data ─────> AL.Pathfinding ───┐
AL.Core─┤                                       ├─> AL.Client
        └─> AL.APIClient ───> AL.SocketClient ──┘
```

## Install

The packages are not on nuget.org yet. Clone this repo and reference `AL.Client/AL.Client.csproj`, which brings in the
other libraries.

## Quick start

```c#
using AL.APIClient;
using AL.APIClient.Definitions;
using AL.Client;
using AL.Core.Definitions;
using AL.Data;

ALClientSettings.UseDefaultLoggingConfiguration();

//this is processing intensive, and may take several seconds to run depending on your CPU.
await ALClient.InitializeAsync();

var apiClient = await AlApiClient.LoginAsync("yourEmail", "yourPassword");

await using var warrior = await Warrior.StartAsync("makiz", ServerRegion.US, ServerId.III, apiClient);
await using var merchant = await Merchant.StartAsync("sichi", ServerRegion.US, ServerId.III, apiClient);
```

There is one class per character class: `Mage`, `Merchant`, `Paladin`, `Priest`, `Ranger`, `Rogue` and `Warrior`.
Disposing a client disconnects it.

Once a client is started, you can act with it:

```c#
//walk to the nearest potion seller and buy 10 health potions
await merchant.SmartMoveToNPCAsync("fancypots");
await merchant.BuyAsync("hpot0", 10);
```

```c#
//walk to the nearest area where goos spawn
await warrior.SmartMoveToMonsterAsync("goo");

//attack one goo until it is gone, then pick the next one
string? targetId = null;

while (true)
{
    await Task.Delay(100);

    if (targetId is null || !warrior.Monsters.ContainsKey(targetId))
        targetId = warrior.Monsters.Values.FirstOrDefault(monster => monster.Name == "goo")?.Id;

    if (targetId is null || !warrior.Monsters.TryGetValue(targetId, out var goo))
        continue;

    if (!warrior.WithinSkillRange(goo, "attack"))
    {
        await warrior.SmartMoveAsync(goo, warrior.Character.Range);

        continue;
    }

    if (!warrior.CanUseSkill("attack"))
        continue;

    try
    {
        await warrior.AttackAsync(goo.Id);
    } catch (Exception e)
    {
        //the goo can die or move away between the checks and the attack
        Console.WriteLine(e.Message);
    }
}
```

## Game data

`GameData` holds the game's static data (`G`). `ALClient.InitializeAsync` fills it. Some extra fields are added that the
game does not have:

```c#
var item = GameData.Items["someItemName"]!;
var source = item.ObtainableFromNPC;        //the NPC you get it from, or null
var sourceType = item.ObtainType;           //whether that NPC sells it, crafts it, exchanges for it or gives it for a quest
var exchanger = item.ExchangeAtNPC;
var exchangeRewards = item.ExchangeRewards; //what exchanging it can give you, by item level
var craftedAt = item.Recipe?.NPC;

var npcPlaces = GameData.NPCs["someNpcName"]!.Locations;   //every place the NPC stands, on every map
var questGiver = GameData.Quests[Quest.Witch];
var classOnly = GameData.Classes["someClassName"]!.ExclusiveCosmetics;

var map = GameData.Maps["someMapName"]!;
var exits = map.Exits;                      //doors and teleports
var mapDrops = map.Drops;                   //drops shared by every monster on the map
var monster = map.Monsters.FirstOrDefault()?.Data;
var npc = map.NPCs.FirstOrDefault()?.Data;

var spawnAreas = monster?.SpawnAreas;       //every area it spawns in, on every map
var roams = monster?.SpawnRoams;            //whether it wanders out of those areas

//a monster's footprint for movement, and its hit box for range checks
var footprint = monster?.BoundingBase;
var hitBox = monster?.HitBox;
```

## Pathfinding

Routes are found over a triangulated navigation mesh and straightened with the funnel algorithm. Routes can use `town`
and `blink`. Pathfinding also works inside the Cave of Many Dreams.

`ALClient.InitializeAsync` sets up the pathfinder. To use the pathfinder without a client, call
`Pathfinder.Initialize()` instead.

Routes skip doors that need a key, and the locked bank floors (`bank_b` and `bank_u`). The server never tells the client
which bank floors your account has unlocked. If you have unlocked them, pass their door locations to
`ALClient.InitializeAsync(unlockedDoors)`.

## Objects that change under you

- `Character` stays the same object for the whole life of the client. Keeping a reference to it is safe.
- **Every object property of `Character` is replaced on each update.** Read it again each time; don't keep it.
- Players, NPCs and monsters stay valid until the server drops them. That happens when the entity dies, when you move
  too far from it, or when you change maps.
- `Bank` is `null` until you first enter the bank. It updates while you are inside, and keeps the last contents you saw
  after you leave.

## Settings and logging

Logging uses `Common.Logging`, so you can plug in any logging library.
`ALClientSettings.UseDefaultLoggingConfiguration()` logs through NLog.
`ALClientSettings.SetLogLevel(NLog.LogLevel.Debug)` changes how much it logs.

Other options in `ALClientSettings`:

| Setting                  | Default | What it does                                                                             |
|--------------------------|---------|------------------------------------------------------------------------------------------|
| `NetworkTimeoutMS`       | 1500    | How long to wait for the server to answer a request                                      |
| `PositionPollingRate`    | 30      | How many times per second positions update                                               |
| `BankCrossingTimeoutMS`  | 10000   | How long to wait when entering or leaving the bank                                       |
| `ReceiveGeneratedMapArt` | true    | Whether daily dungeon floors arrive with their art. Turn it off if nothing draws the map |

## Tools and tests

- `AL.MemberGenerator` rebuilds the typed game data classes when the game's data changes:
  `dotnet run --project AL.MemberGenerator`
- `AL.Visualizer` draws a map's walkable ground and routes to PNG files:
  `dotnet run --project AL.Visualizer -- dump-maps`
- Tests: `dotnet run --project AL.Tests -c Debug`. `dotnet test` finds no tests. Tests that log in need a
  `TestCredentials.txt` file beside the test binary, with your email on line 1 and your password on line 2.

## Credits

- [Earthiverse](https://github.com/earthiverse/ALClient): typings, callbacks
- [Spadar](https://github.com/Spadar/AdventureLandService): mesh generation

## License

MIT