using System.Collections.Generic;
using System.Linq;
using AL.Core.Definitions;
using AL.Data;
using AL.Data.Maps;
using Common.Logging;

namespace AL.Pathfinding.Objects
{
    public class RouteFinder
    {
        private static readonly ILog Logger = LogManager.GetLogger<RouteFinder>();
        // ReSharper disable once NotAccessedField.Local
        private static WorldMesh WorldMesh;

        public static void Initialize()
        {
            Logger.Info("Preparing world navigation");
            var nodes = new Dictionary<Map, GraphNode<Map>>();
            var index = 0;

            foreach ((_, var map) in GameData.Maps)
                nodes.Add(map, new GraphNode<Map>(map, index++));

            foreach ((_, var node) in nodes)
            {
                foreach (var door in node.Edge.Doors)
                    if (nodes.TryGetValue(GameData.Maps[door.DestinationMap], out var neighborNode))
                        node.Neighbors.Add(neighborNode);

                foreach (var npc in node.Edge.NPCs.Select(npc => GameData.NPCs[npc.Id])
                    .Where(npc => npc != null && npc.Role == NPCRole.Transport))
                    foreach (var place in npc.Places)
                        if (nodes.TryGetValue(GameData.Maps[place.Key], out var transportNode))
                            node.Neighbors.Add(transportNode);

                node.Neighbors = node.Neighbors.Distinct().ToList();
            }

            WorldMesh = new WorldMesh(nodes);
        }
    }
}