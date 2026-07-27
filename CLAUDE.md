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

Two things that will waste your time if you don't know them:

- **The published repo is incomplete.** `/common` is not in it, and `api_call` lives there. Fetch it from `https://adventure.land/js/common_functions.js` — the REST calling convention changed and the repo cannot tell you that.
- **Not every emit is a `socket.emit`.** Helpers `xy_emit`, `party_emit`, `instance_emit`, and `notify_friends_emit` carry events like `hit`, `action`, `chat_log`, and `ui`. Grep the quoted event name, not `emit(`.

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
├── AL.Visualizer      — standalone CLI that renders navmeshes/paths to PNG via ImageSharp (not shipped)
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

Mid-refactor and **the solution does not currently build**: `AL.Client/Ranger.cs(704,18): error CS1643: Not all code paths return a value in lambda expression of type 'Func<GameResponseData, Task<bool>>'`, plus 53 warnings (mostly CS8604 nullable-argument in `Ranger.cs`). There are ~19 uncommitted modified files. The last commit describes the state plainly: *".net 9 update - remove overuse of async/await - added a proper deltaTime usage pattern using PeriodicTimer - updated G.Data - updated enums - likely still unusable, more updates to come"*.

Because `ALBot` builds these projects from source, that error surfaces there too — a `dotnet build` failure in ALBot is often this, not ALBot's own code.

## Credits

[Earthiverse](https://github.com/earthiverse/ALClient) — typings, callbacks. [Spadar](https://github.com/Spadar/AdventureLandService) — mesh generation.
