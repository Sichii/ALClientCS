# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ALClientCS is a headless C# client library for the browser MMO **Adventure.Land**. It is deliberately *not* a 1:1 port of the official client — objects, properties, and methods are renamed to be more descriptive, and because the original game is weakly typed, a single API/Socket/Data type here often merges the fields of several original objects. Nine projects, all net10.0. The libraries build on each other and can be consumed separately; each ships as a NuGet package (`GeneratePackageOnBuild`, symbols as `.snupkg`).

### The server is open source — use it as the spec

Adventure.Land's server is published at **<https://github.com/kaansoral/adventureland_mongodb>** (cloned locally at `D:\repos\kaansoral\adventureland_mongodb`). It is the live game's actual source, not a reimplementation, so for any protocol question it is an exact answer rather than an approximation. Never guess at a payload shape or a `game_response` code — read the handler.

| File | What |
|---|---|
| `node/server.js` | Every `socket.on` handler plus the game logic (~14.5k lines) |
| `node/server_functions.js` | Shared server helpers |
| `api.js` | REST surface |
| `js/functions.js`, `js/game.js` | The official browser client |
| `node/precomputed_map_data.js` | 13MB of G data — never read whole |

Six things that will waste your time if you don't know them:

- **The published repo is incomplete.** `/common` is not in it, and `api_call` lives there. Fetch it from `https://adventure.land/js/common_functions.js` — the REST calling convention changed and the repo cannot tell you that.
- **Not every emit is a `socket.emit`.** Helpers `xy_emit`, `party_emit`, `instance_emit`, and `notify_friends_emit` carry events like `hit`, `action`, `chat_log`, and `ui`. Grep the quoted event name, not `emit(`.
- **An NPC's entity id is its display name, not its `G.npcs` key.** NPCs are in `Players` — they always have been — but `newupgrade` is filed under `Cue` and `secondhands` under `Ponty`; the key you looked it up by rides in the entity's `npc` field (`Player.NPCName`) instead. So `Players.TryGetValue(npcId, …)` misses every time and whatever it guards is silently dead. That is what `WithinRangeOfNPC` did, disabling both q-benches and Ponty in the downstream bot for as long as anyone had been looking; it now measures against `GameData.NPCs[id].Locations`, the way `CanBuy` always has. **Whether that id carries a `$` prefix is unsettled, and the disagreement is worth knowing about before trusting either source.** `Fixtures/snapshots/t11-start-frame.json` holds `{"npc":"pvp","id":"Ace"}` against `G.npcs.pvp.name == "Ace"`; the cloned server's `create_npc` would make that `$Ace` (`node/server_functions.js:1495`), and `player_to_client` carries a commented-out `data.id="$"+data.id` from some earlier arrangement (`node/server.js:803`). The capture also lacks the `name` field that the same `is_npc` branch sets, so it did not come through that branch as the clone writes it, and the capture's own age is unknown. The display name is the constant; the prefix is not. Don't key off an NPC's id — and where a real frame and the clone disagree about anything, treat it as an open question rather than assuming which is stale.
- **Which NPC an item is `ObtainableFromNPC` is decided by a first-writer race, and the order is the *datum's declaration* order — not the wire order.** `EnrichItems` writes only `if (item is { ObtainableFromNPC: null })`, iterating `NPCs.Values`; that comes from `DatumBase.BuildLookupTable`, which reflects over `GetType().GetProperties()`, so the winner is whichever seller its generated datum declares first — and `NPCsDatum` declares alphabetically. Reasoning about this from `G.npcs`'s JSON order gets the wrong answer, which is a mistake worth naming because it was made here and it produced a confident false bug report about potions. The four basic potions are sold by `fancypots` (`NPCsDatum.cs:91`), `pots` (`:322`) and `wbartender` (`:382`), so **`fancypots` wins and they are perfectly buyable** — it stands on `main`, which is where a merchant parks. In wire order `pots` would have won, and `pots` would have been fatal: its only placements are `old_main` and `original_main`, both `ignore: true`, and `EnrichNPCs` skips ignored maps when filling `Locations`, so `CanBuy`'s closing `ObtainableFromNPC.Locations.Any(…)` could never pass and every potion purchase would fail silently. **So the hazard is real and currently latent**: nothing makes the race prefer a seller you can reach, an item whose alphabetically-first seller lives only on ignored maps is unbuyable with no line anywhere, and `GetProperties` order is not contractual. `basics` (`:22`) beating `weapons` (`:385`) is what keeps `blade`/`claw`/`staff`/`bow` reachable on the same luck. **Closed now, and the enabling half is the part to not undo:** the race orders placed sellers first, which only works because `EnrichNPCs` was moved ahead of `EnrichItems` in `Populate` — `GNPC.Locations` is empty until that pass runs, so with the old order the preference would have compared all-zero counts and silently done nothing. It was a **no-op on the current table** (every item's first seller was already placed), so nothing about which NPC anything resolves to changed; what went away is the silent-unbuyable failure mode. `T1_ObtainableFromNPC_ResolvesToAPlacedSeller` holds it, and it cannot pin the ordering itself for that same reason — it is a canary on the data, and inverting the ordering is how it was checked.
- **A bank pack's array length is not its slot count, and a pack you own but have never used arrives as `[]`.** Every pack is 42 slots and the server checks all 42 whatever it sent (`can_add_item`'s `for(var i=0;i<42;i++)`, `js/old_common_functions.js:412`), but it fills a pack by *pushing* onto the array (`bank_add_item`, `node/server.js:2018`) and never pads it - so the array runs to the highest occupied slot and stops, and a freshly unlocked pack is written `player.user[pack] = []` (`:8403`). Anything sizing a pack off `.Count` therefore reads a densely-filled pack as full and a brand new one as having no slots at all. `FindOptimalBankIndex` did, which made `DepositItemAsync` throw "no space" against a bank with hundreds of free slots - and it reads as the bank genuinely being full, because that is the same message. The slot count is a constant (`BANK_PACK_SIZE`), and an index past the array's end is an empty slot rather than no slot. Which packs a bank map serves is the neighbouring trap: `bank_packs` (`js/old_common_functions.js:54`) splits them 8 / 16 / 24 across `bank` / `bank_b` / `bank_u`, and `GetAvailableBankPacks` was one short on each.
- **`G` never carries a boolean `s`, however `design/items.js` reads.** Many stackable items are *written* `"s":true` in that file, and the file's own trailing pass rewrites every one of them to `9999` (`design/items.js:7441`) before `node/server.js` evals it whole (`:373`, `:556`). So the literal source says `StackSize` is `bool|int` on the wire when the wire only ever carries an integer, and `js/old_common_functions.js:393` reading `s===true && 9999 || s` is the client defending against a shape the server cannot send. `Fixtures/data.json` is the check — 218 items carry `s`, not one of them a boolean — and `GItem_StackSize_EveryWireSpellingIsNumeric` pins it. Worth the space because it reads exactly like a live deserialization bug: a converter accepting both spellings looks like dead code for an impossible case, and here it cost a fix round that was dispatched and then withdrawn, on a measurement that had read the file's data without applying the file's last statement. **Nothing read out of `design/*.js` is evidence about `G` until that file's trailing passes have been applied.**

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

**Downstream consumer:** `D:\repos\Sichii\ALBot` references six of these projects *by relative path* from its own `ALBot.slnx`. Changing a public signature here breaks that build with no compile-time warning on this side.

## Solution Structure

```
ALClientCS.slnx (net10.0)
├── AL.Core            — base types, geometry, comparers, JSON converters, enums, extensions
├── AL.Data            — static 'G' game data (GameData), enriched with derived members
├── AL.APIClient       — REST layer: login, server list, character list (RestSharp)
├── AL.SocketClient    — socket.io transport and the raw socket event model (SocketIOClient)
├── AL.Pathfinding     — nav mesh triangulation (Poly2Tri) + A* (OptimizedPriorityQueue)
├── AL.Client          — ALClient and per-class subclasses; the public entry point
├── AL.MemberGenerator — codegen console tool, emits a `dataMembers` folder (not shipped)
├── AL.Visualizer      — standalone CLI that renders navmeshes/paths to PNG via SkiaSharp (not shipped)
└── AL.Tests           — TUnit + FluentAssertions
```

**Dependency flow** (verified against each `.csproj`):

```
        ┌──> AL.Data ───────> AL.Pathfinding ──┐
AL.Core─┤                                      ├──> AL.Client
        └──> AL.APIClient ──> AL.SocketClient ─┘

AL.MemberGenerator -> AL.APIClient + AL.Data
AL.Visualizer      -> AL.APIClient + AL.Pathfinding
AL.Tests           -> AL.Client
```

The six library projects are packed on build; `AL.MemberGenerator`, `AL.Visualizer` and `AL.Tests` are `Exe` tools and set `IsPackable=false`.

`AL.Core` is the only project with no project references. Every NuGet dependency the whole stack gets for free flows from it: `Chaos.Time` and `Common.Logging.NLogNetStandard` (which is what puts NLog on the graph — nothing else references it directly).

## Architecture

### Initialization

`ALClient.InitializeAsync()` must run before anything else. It builds the pathfinding nav mesh, loads and enriches game data, and sets up a few other statics. It is CPU-heavy and takes several seconds. `Pathfinder.InitializeAsync()` exists to do the pathfinding half alone.

Login is API-first, then per-character: `ALAPIClient.LoginAsync(email, pw)` produces the API client, then `Warrior.StartAsync(name, region, id, apiClient)` (or `Ranger`/`Priest`/`Merchant`) connects one character.

### Client Layer (`AL.Client`)

- **`ALClient`** -- `abstract class ALClient : IAsyncDisposable, IDeltaUpdatable`. Holds the socket, the API handle, the persistent `Character`, and the live entity collections. Owns a private `EntityManager` and `PingManager`.
- **`Merchant` / `Ranger` / `Priest` / `Warrior`** -- concrete subclasses adding class-specific skills. `Warrior` is `sealed`; the other three are not.
- **`AsyncDeltaLoop`** (`Abstractions/`) -- base for rate-limited internal loops. `PeriodicTimer(1000 / PollingRate)` plus `Chaos.Time.DeltaTime`, serialized through a `FifoAutoReleasingSemaphoreSlim`. Per-iteration exceptions are caught and logged so a bad tick never kills the loop. Note `Start()` is `async void` by design — it is fire-and-forget; use `StopAsync()` to cancel.
- **`ALClientSettings`** -- static config. `NetworkTimeoutMS` (default 1500), `PositionPollingRate` (default 30), `SetLogLevel()`, `UseDefaultLoggingConfiguration()`.
- **Helpers** (`Helpers/`) -- `DynamicDelay`, `Expectation` (await a specific server response), `RegexCache`, `ShallowMerge`, `SimpleCache`.
- **Extensions** (`Extensions/`) -- `Bank`, `Entity`, `Enumerable`, `Inventory`, `Item`, `Monster`, `Player`, `Task`. Most general-purpose utility lives one layer down in `AL.Core.Extensions`.

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

Two structures, both built at init: a **triangulated navigation mesh** for complex movement, and a **2D byte array** for simple movement. Poly2Tri does the triangulation, OptimizedPriorityQueue backs the search. `GraphBase.OpenNode` uses its `Contains` + `UpdatePriority` (decrease-key), which the BCL `PriorityQueue<T,TP>` has no equivalent for — swapping to the BCL type means restructuring the search around lazy deletion (enqueue duplicates, skip stale pops). That is a normal transformation, not a blocker; which is actually faster here has not been measured. `AL.Visualizer` renders the results to PNG; run it by hand to eyeball a mesh, since no test asserts visually.

## Entity Persistence Rules

Getting these wrong produces stale reads and NREs, because "the object I'm holding" and "the object the server knows about" diverge silently.

| Object | Lifetime |
|---|---|
| `Client.Character` | **Fully persistent and mutable.** A reference stays valid for the client's lifetime. |
| Properties *of* `Character` | **Non-persistent.** Every object property is replaced, not mutated — re-read it, never cache it. |
| Players / NPCs / Monsters | **Semi-persistent.** Valid until the server invalidates the entity. |
| `Client.Bank` | Overwritten wholesale each time the character enters the bank; `null` until the first visit. |

The server invalidates an entity when it dies, when the client travels too far from it, or when the client changes maps.

**An awaited emit's confirmation predicate is the thing to check first when a call "times out", and two of them were unsatisfiable.** Both failed the same way - the full `NetworkTimeoutMS` burned, then a throw naming the network, for an operation the server had already performed or already answered.

- `SwapInventorySlotsAsync` waited for a character frame where the two slots held each other's `Item`, compared with `==`. `Item` is a record, so its synthesized equality folds in `PossiblePrefixes`, an `IReadOnlyList<string>` that `EqualityComparer<T>.Default` compares **by reference** - and two deserializations never share the instance. So the predicate was false for every pair except two nulls, and the call could only ever fail. **Do not compare two `Item`s with `==`**; compare name, level and quantity. The same trap is loaded in `SlotItem`, `Monster.Drops` and `Prediction.Nums`.
- `ConsumeAsync` (every potion and regen) watched for a `"not ready"` *disappearing text*, a string that does not appear anywhere in the server. The shared potion cooldown answers `fail_response("not_ready", {ms})` (`node/server.js:7202`), which is a **game_response** - so the single most common outcome there is, using a potion a moment early, matched nothing and spent the whole timeout.

The lesson generalises past those two: `fail_response` is the server's universal refusal and it always lands on `game_response` with `failed: true`, so an awaited call whose only listeners are `eval`, `disappearing_text` or a character frame has no arm for the ordinary refusal at all.
**The same equality mistake has a quieter form that does not time out, and it is still in the client.** Five awaited emits identify *which* item changed by set difference — `Character.Inventory.AsIndexed().Except(snapshot)` — and `.Except` folds `InventoryIndexer`'s record equality into `Item`'s, so it rests on the reference comparison above. A character frame is applied by `ShallowMerge`, which assigns every property and therefore *replaces* `Inventory` with freshly deserialized `Item`s, so nothing captured before the emit can equal anything after it and `.Except` subtracts nothing at all. The three sites ending `.First()` (`ALClient.cs:1624`, `:1718`, and `ExchangeAsync` at `:2399`) hand back the lowest occupied slot on every call; `CraftAsync` (`:2085`) ends `FirstOrDefault(name…)` and hands back the lowest slot holding that name, which is right only while no other copy sits below it. `UnequipAsync` used to be the fifth and is fixed: it snapshots which *indexes* were occupied and resolves the landing slot through `InventoryExtensions.FindLandedItem`, preferring a previously-empty slot — the server's `add_item` puts a non-stackable in the first free slot, while the twin a concurrent equip displaces lands on an occupied one. `FindLandedItemTests` pins the rule; the pattern is what the four remaining sites want too. Nothing throws and nothing waits, which is why this reads as working — the downstream bot's exchange errand was logging the lowest occupied slot as its reward. **Giving `Item` real value equality does not fix these.** It turns "lowest occupied slot" into "lowest *changed* slot", which on an exchange is as likely to be the consumed input as the prize, and it introduces an empty-sequence throw that the `.First()` three do not currently risk. Whatever replaces them has to identify the item by what the caller already expects — a name, a level, a slot that gained quantity — rather than by set difference.

**Some state reaches the inventory on a frame that carries nothing else, and folding it in is the handler's job.** `q_data` is the one instance and the shape is worth recognising: it carries a `q` half (the queued-action timers) and a `p` half (the prediction under the placeholder occupying an in-progress upgrade or compound), plus the `num` naming the slot the second belongs to. Nothing restates that prediction - no character frame, no inventory frame - so a handler that merges only the first leaves `Inventory[slot].Prediction` holding whatever it deserialized with, for the whole operation, and the consumer reads a plausible absence rather than an error. `OnQueuedActionAsync` did exactly that until the roll's digits turned out to be the only observable a downstream feature needed. The digits themselves are the reason to care: they are the actual random number the server upgraded against, published a few at a time as the animation runs out (`node/server.js:13215-13230`), and they are legible only while the placeholder is still there.

## Code Style

### Inline Comments
- lowercase
- no space after `//`
- example: `//this is a comment`

### XML Doc Summaries
- Keep `/// <summary>` blocks on public members — `GenerateDocumentationFile` is on in Release, and these ship in the NuGet packages
- These are separate from inline comments

### Regions
Files open with a `#region` / `#endregion` wrapped using block. Keep that shape when adding files.

## Current State

**The solution builds clean** — 0 warnings, 0 errors, both standalone and as part of `ALBot.slnx`. The `Ranger.cs`
CS1643 and the ~53 nullable warnings this section used to describe are fixed. There is still a body of uncommitted
work in the tree.

Because `ALBot` builds these projects from source, a `dotnet build` failure over there may still originate here —
check the project path in the error before debugging ALBot.

## Credits

[Earthiverse](https://github.com/earthiverse/ALClient) — typings, callbacks. [Spadar](https://github.com/Spadar/AdventureLandService) — mesh generation.
