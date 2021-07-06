using System;
using System.Collections.Generic;
using System.Linq;
using AL.Core.Definitions;
using AL.Core.Json.Converters;
using AL.Core.Model;
using Newtonsoft.Json;

namespace AL.Data.Maps
{
    public record Map
    {
        [JsonProperty("no_bounds")]
        public bool Boundless { get; init; }

        [JsonProperty("burn_multiplier")]
        public float BurnMultiplier { get; init; }

        [JsonProperty(ItemConverterType = typeof(ArrayToObjectConverter<Door>))]
        public Door[] Doors { get; init; }

        [JsonProperty("drop_norm")]
        public float DropNorm { get; init; }

        public string Event { get; init; }

        [JsonProperty("freeze_multiplier")]
        public float FreezeMultiplier { get; init; }

        public string FX { get; init; }
        public bool Ignore { get; init; }
        public bool Instance { get; init; }
        public bool Irregular { get; init; }
        public string Key { get; init; }
        public bool Loss { get; init; }
        public Monster[] Monsters { get; init; }
        public bool Mount { get; init; }
        public string Name { get; init; }
        public NPC[] NPCs { get; init; }

        [JsonProperty("on_death"), JsonConverter(typeof(ArrayToTupleConverter<string, float>))]
        public (string Map, float Spawn) OnDeath { get; init; }

        [JsonProperty("on_exit"), JsonConverter(typeof(ArrayToTupleConverter<string, float>))]
        public (string Map, float Spawn) OnExit { get; init; }

        public bool PvP { get; init; }
        public bool Safe { get; init; }

        [JsonProperty("safe_pvp")]
        public bool SafePvP { get; init; }

        [JsonProperty(ItemConverterType = typeof(ArrayToObjectConverter<Spawn>))]
        public Spawn[] Spawns { get; init; }

        public Trap[] Traps { get; init; }
        public bool Unlist { get; init; }
        public string Weather { get; init; }
        public string World { get; init; }
        public Zone[] Zones { get; init; }

        [JsonIgnore]
        public Lazy<List<Exit>> Exits => new(GenerateExits);
        //quirks
        //animatables obj
        //machines obj[]
        //ref obj
        //old_monsters obj[]

        public virtual bool Equals(Map other) => Key.Equals(other?.Key);

        private List<Exit> GenerateExits()
        {
            var exits = new List<Exit>();

            foreach (var door in Doors)
                exits.Add(new Exit(door, door.DestinationMap, door.DestinationSpawnId, ExitType.Door));

            var npcSets = NPCs.Select(npc => new { MapNPC = npc, GameNPC = GameData.NPCs[npc.Id] })
                .Where(set => set.GameNPC != null && set.GameNPC.Role == NPCRole.Transport);

            foreach (var set in npcSets)
                foreach (var position in set.MapNPC.Positions)
                    foreach ((var toMap, var toSpawnId) in set.GameNPC.Places)
                        exits.Add(new Exit(position, toMap, toSpawnId, ExitType.NPC));

            return exits;
        }

        public override int GetHashCode() => Key.GetHashCode();
    }
}