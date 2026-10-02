# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ALClientCS is a headless C# client library for the browser MMO **Adventure.Land**. It is deliberately *not* a 1:1 port
of the official client — objects, properties, and methods are renamed to be more descriptive, and because the original
game is weakly typed, a single API/Socket/Data type here often merges the fields of several original objects. All
projects target net10.0. The libraries build on each other and can be consumed separately; each ships as a NuGet package
(`GeneratePackageOnBuild`, symbols as `.snupkg`).

### The server is open source — use it as the spec

Adventure.Land's server is published at **<https://github.com/kaansoral/adventureland_mongodb>** (cloned locally at `D:\repos\kaansoral\adventureland_mongodb`). It is the live game's actual source, not a reimplementation, so for any protocol question it is an exact answer rather than an approximation. Never guess at a payload shape or a `game_response` code — read the handler.

| File                            | What                                          |
|---------------------------------|-----------------------------------------------|
| `node/server.js`                | Every `socket.on` handler plus the game logic |
| `node/server_functions.js`      | Shared server helpers                         |
| `api.js`                        | REST surface                                  |
| `js/functions.js`, `js/game.js` | The official browser client                   |
| `node/precomputed_map_data.js`  | Very large G data dump — never read whole     |

Things that will waste your time if you don't know them:

- **The published repo is incomplete.** `/common` is not in it, and `api_call` lives there. Fetch it from `https://adventure.land/js/common_functions.js` — the REST calling convention changed and the repo cannot tell you that.
- **Not every emit is a `socket.emit`.** Helpers `xy_emit`, `party_emit`, `instance_emit`, and `notify_friends_emit` carry events like `hit`, `action`, `chat_log`, and `ui`. Grep the quoted event name, not `emit(`.
- **An NPC's entity id is its display name, not its `G.npcs` key.** NPCs are in `Players` under that name (`newupgrade`
  is `Cue`, `secondhands` is `Ponty`); the key rides in the entity's `npc` field (`Player.NPCName`).
  `Players.TryGetValue(npcKey, …)` always misses, and whatever it guards is silently dead. Locate an NPC through
  `GameData.NPCs[key].Locations`, as `CanBuy` and `WithinRangeOfNPC` do. Whether the id carries a `$` prefix is
  unsettled: `Fixtures/snapshots/t11-start-frame.json` holds `"id":"Ace"`, but the clone's `create_npc` would send
  `$Ace` (`node/server_functions.js:1495`). Don't key off an NPC's id, and where a captured frame and the clone
  disagree, treat it as an open question.
- **`ObtainableFromNPC` is first-writer-wins in `NPCsDatum`'s declaration order, which is alphabetical — not `G.npcs`
  wire order.** `EnrichItems` iterates sellers through `DatumBase.BuildLookupTable`, which reflects over
  `GetProperties()`; that order is not contractual. It prefers a seller with placements, which only works because
  `EnrichNPCs` runs before `EnrichItems` in `Populate` — `GNPC.Locations` is empty until then. **Keep that order.**
  `EnrichNPCs` skips `ignore: true` maps, so a seller placed only there (`pots`) has no locations, and an item resolving
  to it would fail `CanBuy` silently. `T1_ObtainableFromNPC_ResolvesToAPlacedSeller` checks the data; it cannot pin the
  ordering itself.
- **A bank pack's array length is not its slot count.** Every pack has 42 slots (`BANK_PACK_SIZE`; `can_add_item`,
  `js/old_common_functions.js:412`). The server fills a pack by pushing and never pads it (`bank_add_item`,
  `node/server.js:2018`), and a newly unlocked pack arrives as `[]` (`:8403`). An index past the array's end is an empty
  slot. Sizing off `.Count` reads a new pack as having no slots and makes `DepositItemAsync` throw "no space" against a
  near-empty bank. Bank maps serve packs 8 / 16 / 24 across `bank` / `bank_b` / `bank_u` (`bank_packs`,
  `js/old_common_functions.js:54`).
- **Nothing read out of `design/*.js` is evidence about `G` until that file's trailing passes have been applied.**
  `design/items.js` writes many stack sizes as `"s":true`, then its last pass rewrites every one to `9999`
  (`design/items.js:7441`) before `node/server.js` evals it (`:373`, `:556`). `G` only ever carries an integer `s`:
  `AL.Core/data.json` holds no boolean one, and `GItem_StackSize_EveryWireSpellingIsNumeric` pins it.

<https://github.com/earthiverse/ALClient> is a maintained TypeScript client; its `source/definitions/*.d.ts` is a useful cross-check, but it is a third-party client — where it disagrees with the server, the server wins.

## Build Commands

```powershell
# Build the solution
dotnet build ALClientCS.slnx

# Run all tests. TUnit on Microsoft.Testing.Platform, so the test binary IS the runner.
# `dotnet test` reports "Zero tests ran" — the csproj does not opt into
# TestingPlatformDotnetTestSupport. Run the project instead.
dotnet run --project AL.Tests -c Debug

# Run a single test class, then a single test method
dotnet run --project AL.Tests -c Debug -- --treenode-filter "/*/*/PathfindingTests/*"
dotnet run --project AL.Tests -c Debug -- --treenode-filter "/*/*/PathfindingTests/MethodName"

# Regenerate strongly-typed data members from the game's G data
dotnet run --project AL.MemberGenerator

# Render navmesh images (standalone tool)
dotnet run --project AL.Visualizer -- dump-maps
```

Any test deriving from `APITestBed` logs into the live API and needs `TestCredentials.txt` beside the test binary — account email on line 1, password on line 2. `AssemblyInit` repoints `Environment.CurrentDirectory` at the test output directory.

Shared build properties (TFM, nullable, implicit usings, packaging metadata) live in `Directory.Build.props` at the repo root, so a new project inherits them. The three tool projects opt out of packaging with `IsPackable=false`.

**Downstream consumer:** `ALBot` vendors this repo as a git submodule and builds every project here from source through
its own `ALBot.slnx`. Changing a public signature here breaks that build with no compile-time warning on this side.

## Solution Structure

```
ALClientCS.slnx (net10.0)
├── AL.Core            — base types, geometry, comparers, JSON converters, enums, extensions
├── AL.Data            — static 'G' game data (GameData), enriched with derived members
├── AL.APIClient       — REST layer: login, server list, character list (RestSharp)
├── AL.SocketClient    — socket.io transport and the raw socket event model (SocketIOClient)
├── AL.Pathfinding     — exact server movement rules, triangle mesh + funnel (Poly2Tri at build), portal graph
├── AL.Client          — ALClient and per-class subclasses; the public entry point
├── AL.MemberGenerator — codegen console tool, emits a `dataMembers` folder and keeps the previous G fetch under `snapshots/` for a value diff (not shipped)
├── AL.Visualizer      — standalone CLI that renders navmeshes/paths to PNG via SkiaSharp (not shipped)
└── AL.Tests           — TUnit + FluentAssertions
```

**Dependency flow** (from each `.csproj`):

```
        ┌──> AL.Data ───────> AL.Pathfinding ──┐
AL.Core─┤                                      ├──> AL.Client
        └──> AL.APIClient ──> AL.SocketClient ─┘

AL.MemberGenerator -> AL.APIClient + AL.Data
AL.Visualizer      -> AL.APIClient + AL.Pathfinding
AL.Tests           -> AL.Client + AL.MemberGenerator
```

The six library projects are packed on build; `AL.MemberGenerator`, `AL.Visualizer` and `AL.Tests` are `Exe` tools and set `IsPackable=false`.

`AL.Core` is the only project with no project references. Every NuGet dependency the whole stack gets for free flows from it: `Chaos.Time` and `Common.Logging.NLogNetStandard` (which is what puts NLog on the graph — nothing else references it directly).

## Architecture

### Initialization

`ALClient.InitializeAsync()` must run before anything else. It builds the pathfinding nav mesh, loads and enriches game
data, and sets up a few other statics. It is CPU-heavy and takes several seconds. `Pathfinder.Initialize()` does the
pathfinding half alone.

Login is API-first, then per-character: `ALAPIClient.LoginAsync(email, pw)` produces the API client, then `Warrior.StartAsync(name, region, id, apiClient)` (or `Ranger`/`Priest`/`Merchant`) connects one character.

### Client Layer (`AL.Client`)

- **`ALClient`** -- `abstract class ALClient : IAsyncDisposable, IDeltaUpdatable`. Holds the socket, the API handle, the persistent `Character`, and the live entity collections. Owns a private `EntityManager` and `PingManager`.
- **`Merchant` / `Ranger` / `Priest` / `Warrior`** -- concrete subclasses adding class-specific skills. `Warrior` is `sealed`; the other three are not.
- **`AsyncDeltaLoop`** (`Abstractions/`) -- base for rate-limited internal loops. `PeriodicTimer(1000 / PollingRate)` plus `Chaos.Time.DeltaTime`, serialized through a `FifoAutoReleasingSemaphoreSlim`. Per-iteration exceptions are caught and logged so a bad tick never kills the loop. Note `Start()` is `async void` by design — it is fire-and-forget; use `StopAsync()` to cancel.
- **`ALClientSettings`** -- static config. `NetworkTimeoutMS` (default 1500), `PositionPollingRate` (default 30), `SetLogLevel()`, `UseDefaultLoggingConfiguration()`.
- **`Helpers/`, `Extensions/`** -- client-level utilities. Most general-purpose utility lives one layer down in
  `AL.Core.Extensions`.

Several live collections on `ALClient` (`AchievementProgress`, `Chests`, `Cooldowns`, and siblings) carry an XML doc warning verbatim: **"THIS COLLECTION IS SYNCHRONIZED, DO NOT DO LONG RUNNING OPERATIONS WHILE ITERATING IT."** Materialize with `.ToList()` before doing anything slow.

### Logging

`Common.Logging`, so a consumer can plug in any factory adapter. `ALClientSettings.UseDefaultLoggingConfiguration()` installs the NLog adapter; `SetLogLevel()` adjusts it. Every client exposes a `Logger`.

### Data Layer (`AL.Data`)

`GameData` is the static accessor for the game's 'G' objects, populated during `InitializeAsync`. Beyond the raw data it carries *enriched* members that the original does not have:

```csharp
var gItem = GameData.Items["someItemName"];
var obtainAt = gItem.ObtainableFromNPC;
var exchangeAt = gItem.ExchangeAtNPC;
var craftAtNpc = gItem.Recipe.NPC;

var gMap = GameData.Maps["someMapName"];
var exits = gMap.Exits;              // doors and transports
var monsters = gMap.Monsters;        // each has .Data -> the G monster

// inserted so rectangle calculations work against a monster entity
var bounds = gMonster.BoundingBase;
```

`AL.MemberGenerator` is what produces the strongly-typed members over this data — rerun it when the game's G data changes.

### Pathfinding (`AL.Pathfinding`)

Built once by `Pathfinder.Initialize()`; every query after that is lock-free. The geometry queries (`CanMove`, `IsWall`, `IsWalkable`, `TryFindNearestWalkable`) are allocation-free once warm; a search allocates only its result legs.

- **`WallLines`** is the server's own `can_move`, line for line: sorted line arrays, four corner tracks of the collision base plus two fence tracks at the destination, `EPS`/`REPS` as the server has them. `Pathfinder.CanMove` and `IsWall` are answered from it. The lines carry the local ice golem corridor carve.
- **`TriangleMesh`** is the walkable ground per map, from the raster flood, vertex trace and Poly2Tri triangulation at build. Flat arrays, neighbour ids, a uniform grid for `TriangleAt`. `IsWalkable` and `TryFindNearestWalkable` are containment in it: the flood fill's answer without the raster. The server's move-endpoint grid is not modelled; where the two floods disagree, `GameData.CarveCorridors` closes the gap.
- **A walk on one map** is Dijkstra over the mesh vertices along triangle edges into `[ThreadStatic]` scratch, the vertex path turned into a triangle corridor by rotating each vertex's fan, then `Funnel` (simple stupid funnel) over the corridor, a farthest-first straightening pass with the exact line test, then a trim to the goal's `Reach` (a rectangle band plus a range: a door is the real rounded rectangle the server opens from, a destination a circle).
- **`PortalGraph`** joins maps: arrival nodes (spawns something lands on), departure nodes (exits), static walk costs
  funnelled at build, town and leave edges, and a blink-only edge wherever no walk joins an arrival to an exit. A search
  adds a virtual start and its ends, runs Dijkstra over arrivals with `PathOptions` pricing recall and blink against the
  walks (a walk at least `BlinkCost` long is offered both walked and cast, the cast priced at its real time from the
  cooldown, `penalty_cd` and optionally the bar, never under `BlinkCost`; that least price is what keeps a short walk
  from being split into casts by a recall or a door and back. A node keeps every arrival no other beats on cost and on
  readiness, both now and at the moment of its next cast, since a wait the least price absorbs lets the penalty run
  down. With blink on, an A* lower bound orders the queue), and expands the winner into `PathEdge`s. Any of N ends:
  the first taken wins. Blink off leaves the route untouched.

`PathEdge(Type, Start, End, Cost)` is the whole public shape of a route; on a `Door`/`Transport` leg `Start` is the `Exit`. `AL.Visualizer` renders meshes and paths to PNG; run it by hand to eyeball one, since no test asserts visually.

**A daily-dungeon floor is not in G and never in the world graph.** The server generates a run's floors and streams them over `map_chunk`; `Pathfinder.RegisterGeneratedRun` files them into `GameData.Maps`/`Geometry` (copy-on-write, so readers need no lock), builds their meshes, and gives the run a portal graph of its own that a route starting on a floor uses. Nothing routes from the world into a run - the keeper pulls the party in over an `interaction` - so a `FindPath` from `main` to a floor finds nothing, by design. `GMap.Generated` is how to tell a floor from a map; a run's floors leave the tables two hours after a later run replaces them.

## Entity Persistence Rules

Getting these wrong produces stale reads and NREs, because "the object I'm holding" and "the object the server knows about" diverge silently.

| Object | Lifetime |
|---|---|
| `Client.Character` | **Fully persistent and mutable.** A reference stays valid for the client's lifetime. |
| Properties *of* `Character` | **Non-persistent.** Every object property is replaced, not mutated — re-read it, never cache it. |
| Players / NPCs / Monsters | **Semi-persistent.** Valid until the server invalidates the entity. |
| `Client.Bank` | Overwritten wholesale each time the character enters the bank; `null` until the first visit. |

The server invalidates an entity when it dies, when the client travels too far from it, or when the client changes maps.

**When an awaited emit times out, check its confirmation predicate first.** A predicate that can never match burns the
full `NetworkTimeoutMS`, then throws a network error for an operation the server already performed or already refused.

- **Never compare two `Item`s with `==`.** `Item` is a record, and its `PossiblePrefixes` list compares by reference, so
  two deserialized copies are never equal. Compare name, level and quantity. `SlotItem`, `Monster.Drops` and
  `Prediction.Nums` carry the same trap.
- **`fail_response` is the server's universal refusal, and it always arrives as `game_response` with `failed: true`.**
  An awaited call that listens only on `eval`, `disappearing_text` or a character frame never sees an ordinary refusal.
  The shared potion cooldown is the common case: `fail_response("not_ready", {ms})` (`node/server.js:7202`).
- **Set difference on the inventory finds nothing.** `ShallowMerge` replaces `Inventory` with freshly deserialized
  `Item`s on every character frame, so `Character.Inventory.AsIndexed().Except(snapshot)` subtracts nothing. A site
  ending `.First()` gets the lowest occupied slot; `CraftAsync`'s `FirstOrDefault(name…)` gets the lowest slot holding
  that name. Identify the changed item by what the caller already expects — a name, a level, a slot that gained
  quantity — as `FindLandedItem` (unequip) and `FindExchangePrize` (exchange, `FindExchangePrizeTests`) do. Giving
  `Item` value equality does not fix this: it returns the lowest *changed* slot and adds an empty-sequence throw.

**Some state arrives on a frame of its own, and folding it into the inventory is the handler's job.** `q_data` carries
the queued-action timers (`q`), the prediction for the placeholder occupying an in-progress upgrade or compound slot
(`p`), and that slot's index (`num`). No other frame restates the prediction, so a handler that drops `p` leaves
`Inventory[slot].Prediction` stale for the whole operation. The prediction's digits are the actual random number the
server upgraded against, published a few at a time as the animation runs (`node/server.js:13215-13230`), and readable
only while the placeholder is there.