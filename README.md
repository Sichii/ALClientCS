# ALClientCS
This is a headless client for the game "Adventure.Land". Using this client will not be a 1 to 1 with any other client. Many objects, properties, methods, etc have been renamed to be more descriptive than the original.
Due to the difference in weak vs strongly typed language... many API, Socket, and Data objects contain the equivalent properties of multiple objects from the original. <br/>
This project consists of multiple libraries that build on eachother, and so can be used seperately if required. <br/>
<br/>
```
        ┌────>AL.Data─────>AL.Pathfinding───┐
AL.Core─┤                                   ├─AL.Client
        └─>AL.APIClient───>AL.SocketClient──┘
```

# Logging
This client uses `Common.Logging`, allowing users implement their own logging solution if so desired, or any number of factory adapters from nuget. <br/>
<br/>
Alternatively, the default logging configuration can be enabled by adding `ALClientSettings.UseDefaultLoggingConfiguration();` somewhere near the beginning of the application. <br/>
The default logging configuration utilizes an `NLog` factory adapter. The logging level can be changed in the settings. <br/>

# Settings
Aside from logging, there are some other options available in `ALClientSettings` that may be worth checking out. <br/>

# Pathfinding
This client uses a triangulated navigation mesh for complex movement, and a 2d array of bytes for simple movement. <br/>
Both of these are generated when the pathfinder is initialized. This is done during client initialization, or can be done manually.
```c#
	//use this, it initializes the pathfinder, game data, and a few other key things
	ALClient.InitializeAsync();
	
	//manual initialization
	Pathfinder.InitializeAsync();
```

# General usage
```c#
	private static async Task Main()
	{
		ALClientSettings.UseDefaultLoggingConfiguration();
		//ALClientSettings.SetLogLevel(LogLevel.Debug);

		//this is processing intensive, and may take several seconds to run depending on your CPU.
		await ALClient.InitializeAsync();

		var apiClient = await ALAPIClient.LoginAsync("yourEmail", "yourPassword");
		var makiz = await Warrior.StartAsync("makiz", ServerRegion.US, ServerId.III, apiClient);
		var ragnah = await Priest.StartAsync("ragnah", ServerRegion.US, ServerId.III, apiClient);
	}
```

# Tips
### GameData
`GameData` can be used for static access to 'G' objects. This is populated when `ALClient.InitializeAsync` is called.
Extra data elements have been enriched into this data. A few such examples are...

```c#
	var gItem = GameData.Items["someItemName"];
	var obtainAtGNpc = gItem.ObtainableFromNPC;
	var exchangeAtGNpc = gItem.ExchangeAtNPC;
	var craftRecipe = gItem.Recipe;
	var craftAtNPC = craftRecipe.NPC;
	
	var gMap = GameData.Maps["someMapName"];
	var doorsAndTransports = gMap.Exits;
	
	var gMapMonsters = gMap.Monsters;	
	var gMapMonster = gMapMonsters.FirstOrDefault();
	var gMonster = gMapMonster.Data;
	
	var gMapNpcs = gMap.NPCs;
	var gMapNpc = gMapNPCs.FirstOrDefault();
	var gNpc = gMapNpc.Data;
	
	//this enriched element is inserted into the corresponding Monster entity to allow rectangle calculations
	var boundingBase = gMonster.BoundingBase;
```

### Extensions
Many utility methods are contained within `Extensions` classes, most of those being under the `AL.Core.Extensions` namespace. <br/>
Some of these extensions overlap with eachother intentionally, such as doing distance calculations. <br/>
In this case, it is intentional to force the user to consider what type of distance they want to use. <br/>

```c#
	//potential ambiguous reference
	SomeEntity.Distance(otherEntity);
	
	//center to center distance
	SomeEntity.Distance((IPoint) otherEntity);
	
	//center to center distance, but include a map check
	SomeEntity.Distance((ILocation) otherEntity);
	
	//center to center distance, but include an instance AND map check
	SomeEntity.Distance((IInstancedLocation) otherEntity);
	
	//edge to edge distance
	SomeEntity.Distance((IRectangle) otherEntity);
	
	//ICircle inherits from IPoint
	//edge to edge distance
	SomeCircle.Distance((ICircle) otherCircle);
```

### Persistence
1. Each time you enter the `Bank`, new bank information overwrites `Client.Bank`. <br/>
2. The `Character` object for each client is fully persistent and mutable. <br/>
	a. In other words, a reference to this object will be valid for the lifetime of the client. <br/>
3. **All object properties of `Character` are non-persistent.**
4. Other entities (players, npcs, monsters) are semi-persistent. References are valid until the server invalidates an entity <br/>
	a. The entity died <br/>
	b. The client traveled too far from the entity <br/>
	c. The client changed maps <br/>
	
# Credits
[Earthiverse](https://github.com/earthiverse/ALClient): typings, callbacks <br/>
[Spadar](https://github.com/Spadar/AdventureLandService): mesh generation
